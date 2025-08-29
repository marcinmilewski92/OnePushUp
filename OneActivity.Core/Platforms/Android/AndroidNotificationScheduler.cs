#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;
using Preferences = Microsoft.Maui.Storage.Preferences;
using AndroidApp = Android.App.Application;

namespace OneActivity.Core.Platforms.Android;

public class AndroidNotificationScheduler : INotificationScheduler
{
    private readonly ILogger<AndroidNotificationScheduler> _logger;
    private readonly IActivityContent _content;
    private const string LastScheduledKey = "last_notification_scheduled";

    public AndroidNotificationScheduler(ILogger<AndroidNotificationScheduler> logger, IActivityContent content)
    {
        _logger = logger;
        _content = content;
    }

    public async Task SendTestAsync()
    {
        if (!await CheckAndRequestNotificationPermissionAsync()) return;
        var context = AndroidApp.Context;
        var nm = NotificationManager.FromContext(context);
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(NotificationDisplayer.ChannelId, $"{_content.AppName} Reminders", NotificationImportance.High)
            {
                Description = $"Daily reminders to {_content.Verb} your {_content.UnitPlural}",
                LockscreenVisibility = NotificationVisibility.Public
            };
            channel.EnableVibration(true);
            nm.CreateNotificationChannel(channel);
        }
        var launch = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName) ?? new Intent(Intent.ActionMain);
        launch.AddCategory(Intent.CategoryLauncher);
        launch.SetPackage(context.PackageName);
        var pending = PendingIntent.GetActivity(context, 0, launch, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        var builder = new NotificationCompat.Builder(context, NotificationDisplayer.ChannelId)
            .SetContentTitle($"{_content.AppName} Test")
            .SetContentText($"This is a test notification - {DateTime.Now:HH:mm:ss}")
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetAutoCancel(true)
            .SetContentIntent(pending)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetVisibility(NotificationCompat.VisibilityPublic);
        nm.Notify(100, builder.Build());
    }

    public Task ScheduleAsync(TimeSpan time) => ScheduleAndroidNotificationAsync(time);
    public Task CancelAsync() => CancelAndroidNotificationsAsync();

    private async Task<bool> CheckAndRequestNotificationPermissionAsync()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            var status = await Permissions.CheckStatusAsync<PostNotificationsPermission>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<PostNotificationsPermission>();
            return status == PermissionStatus.Granted;
        }
        return true;
    }

    public class PostNotificationsPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new[]
        {
            ("android.permission.POST_NOTIFICATIONS", true)
        };
    }

    private async Task<bool> EnsureNotificationPermissionAsync() => await CheckAndRequestNotificationPermissionAsync();

    private (AlarmManager? alarmManager, PendingIntent? exactPendingIntent, PendingIntent? inexactPendingIntent, PendingIntent? repeatingPendingIntent, long triggerAtMillis, long delayMs, bool canUseExactAlarms, string calendarTime)
        CreateAlarmIntents(Context context, TimeSpan time)
    {
        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
        if (alarmManager == null) return (null, null, null, null, 0, 0, false, string.Empty);

        var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
        calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hours);
        calendar.Set(Java.Util.CalendarField.Minute, time.Minutes);
        calendar.Set(Java.Util.CalendarField.Second, 0);
        calendar.Set(Java.Util.CalendarField.Millisecond, 0);
        var nowMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
        if (calendar.TimeInMillis <= nowMillis) calendar.Add(Java.Util.CalendarField.DayOfYear, 1);

        var triggerAtMillis = calendar.TimeInMillis;
        var delayMs = triggerAtMillis - nowMillis;

        var exactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        exactIntent.SetAction(NotificationIntentConstants.ActionDailyNotification);
        exactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationId, 1);
        exactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationTime, $"{calendar.Time}");
        exactIntent.PutExtra(NotificationIntentConstants.ExtraApproach, "exact");
        var exactPendingIntent = PendingIntent.GetBroadcast(context, 1, exactIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var inexactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        inexactIntent.SetAction(NotificationIntentConstants.ActionDailyNotification);
        inexactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationId, 2);
        inexactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationTime, $"{calendar.Time}");
        inexactIntent.PutExtra(NotificationIntentConstants.ExtraApproach, "inexact");
        var inexactPendingIntent = PendingIntent.GetBroadcast(context, 2, inexactIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var repeatingIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        repeatingIntent.SetAction(NotificationIntentConstants.ActionDailyNotification);
        repeatingIntent.PutExtra(NotificationIntentConstants.ExtraNotificationId, 3);
        repeatingIntent.PutExtra(NotificationIntentConstants.ExtraNotificationTime, $"{calendar.Time}");
        repeatingIntent.PutExtra(NotificationIntentConstants.ExtraApproach, "repeating");
        var repeatingPendingIntent = PendingIntent.GetBroadcast(context, 3, repeatingIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        bool canUseExactAlarms = true;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarmManager.CanScheduleExactAlarms())
        {
            canUseExactAlarms = false;
        }
        return (alarmManager, exactPendingIntent, inexactPendingIntent, repeatingPendingIntent, triggerAtMillis, delayMs, canUseExactAlarms, calendar.Time.ToString());
    }

    private async Task ScheduleAndroidNotificationAsync(TimeSpan time)
    {
        if (!await EnsureNotificationPermissionAsync()) return;
        var context = AndroidApp.Context;
        await CancelAndroidNotificationsAsync();

        var alarmData = CreateAlarmIntents(context, time);
        if (alarmData.alarmManager == null) return;
        var am = alarmData.alarmManager;
        var triggerAtMillis = alarmData.triggerAtMillis;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            if (alarmData.canUseExactAlarms)
                am.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, alarmData.exactPendingIntent!);
            am.Set(AlarmType.RtcWakeup, triggerAtMillis, alarmData.inexactPendingIntent!);
        }
        else
        {
            if (alarmData.canUseExactAlarms)
                am.SetExact(AlarmType.RtcWakeup, triggerAtMillis, alarmData.exactPendingIntent!);
            am.Set(AlarmType.Rtc, triggerAtMillis, alarmData.inexactPendingIntent!);
        }

        am.SetRepeating(AlarmType.RtcWakeup, triggerAtMillis, AlarmManager.IntervalDay, alarmData.repeatingPendingIntent!);
        Preferences.Default.Set(LastScheduledKey, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Preferences.Default.Set("notification_target_time", alarmData.calendarTime);
    }

    private Task CancelAndroidNotificationsAsync()
    {
        var context = AndroidApp.Context;
        var am = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (am == null) return Task.CompletedTask;
        var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        var pending = PendingIntent.GetBroadcast(context, 1, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        am.Cancel(pending);
        return Task.CompletedTask;
    }
}
#endif
