#if ANDROID
using global::Android.App;
using global::Android.Content;
using global::Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;
using Preferences = Microsoft.Maui.Storage.Preferences;
using AndroidApp = global::Android.App.Application;

namespace OneActivity.Core.Platforms.Android;

public class AndroidNotificationScheduler(ILogger<AndroidNotificationScheduler> logger, IActivityContent content) : INotificationScheduler
{
    private readonly ILogger<AndroidNotificationScheduler> _logger = logger;
    private readonly IActivityContent _content = content;
    private readonly BatteryOptimizationHelper? _batteryHelper = new BatteryOptimizationHelper(logger);
    private const string LastScheduledKey = "last_notification_scheduled";

    public async Task<bool> RequestBatteryOptimizationExemptionAsync()
    {
        return await _batteryHelper!.RequestBatteryOptimizationExemptionAsync();
    }

    public bool IsIgnoringBatteryOptimizations()
    {
        return BatteryOptimizationHelper.IsIgnoringBatteryOptimizations();
    }


    public Task ScheduleAsync(TimeSpan time) => ScheduleAndroidNotificationAsync(time);
    public Task CancelAsync() => CancelAndroidNotificationsAsync();

    private static async Task<bool> CheckAndRequestNotificationPermissionAsync()
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

    private static void RequestExactAlarmPermissionIfNeeded(Context context)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            if (context.GetSystemService(Context.AlarmService) is AlarmManager alarmManager && !alarmManager.CanScheduleExactAlarms())
            {
                try
                {
                    var intent = new Intent(global::Android.Provider.Settings.ActionRequestScheduleExactAlarm);
                    intent.SetData(global::Android.Net.Uri.Parse($"package:{context.PackageName}"));
                    intent.SetFlags(ActivityFlags.NewTask);
                    context.StartActivity(intent);
                }
                catch { }
            }
        }
    }

    private static (AlarmManager? alarmManager, PendingIntent? exactPendingIntent, PendingIntent? inexactPendingIntent, PendingIntent? repeatingPendingIntent, long triggerAtMillis, long delayMs, bool canUseExactAlarms, string calendarTime)
        CreateAlarmIntents(Context context, TimeSpan time)
    {
        if (context.GetSystemService(Context.AlarmService) is not AlarmManager alarmManager) return (null, null, null, null, 0, 0, false, string.Empty);

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
        try { if (_batteryHelper != null) await _batteryHelper.RequestBatteryOptimizationExemptionAsync(); } catch { }
        var context = AndroidApp.Context;
        try { RequestExactAlarmPermissionIfNeeded(context); } catch { }
        await CancelAndroidNotificationsAsync();

        var alarmData = CreateAlarmIntents(context, time);
        if (alarmData.alarmManager == null) return;
        var am = alarmData.alarmManager;
        var triggerAtMillis = alarmData.triggerAtMillis;

        // Use AlarmClock API for higher priority alarm
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            try
            {
                // Use the package's launch intent for both apps
                var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName);
                if (launchIntent != null)
                {
                    var pendingShowIntent = PendingIntent.GetActivity(
                        context,
                        0,
                        launchIntent,
                        PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                    // Use AlarmClock API which has higher priority
                    var alarmInfo = new AlarmManager.AlarmClockInfo(triggerAtMillis, pendingShowIntent);
                    am.SetAlarmClock(alarmInfo, alarmData.exactPendingIntent!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AlarmClock intent, falling back to standard alarms");
            }
            
            // Also set exact alarm as a fallback
            if (alarmData.canUseExactAlarms)
                am.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, alarmData.exactPendingIntent!);
            
            // Set inexact alarm as another fallback
            am.Set(AlarmType.RtcWakeup, triggerAtMillis, alarmData.inexactPendingIntent!);
        }
        else
        {
            if (alarmData.canUseExactAlarms)
                am.SetExact(AlarmType.RtcWakeup, triggerAtMillis, alarmData.exactPendingIntent!);
            am.Set(AlarmType.Rtc, triggerAtMillis, alarmData.inexactPendingIntent!);
        }

        // Set repeating alarm as yet another fallback
        am.SetRepeating(AlarmType.RtcWakeup, triggerAtMillis, AlarmManager.IntervalDay, alarmData.repeatingPendingIntent!);
        
        // Schedule WorkManager backup (fires around the same time)
        this.ScheduleWorkManagerBackup(time);
        
        // Schedule foreground service to start 5 minutes before notification time
        ScheduleForegroundServiceBackup(time);
        
        Preferences.Default.Set(LastScheduledKey, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Preferences.Default.Set("notification_target_time", alarmData.calendarTime);
    }
    
    private void ScheduleForegroundServiceBackup(TimeSpan targetTime)
    {
        try
        {
            var context = AndroidApp.Context;
            var now = DateTime.Now;
            var target = new DateTime(now.Year, now.Month, now.Day, 
                targetTime.Hours, targetTime.Minutes, targetTime.Seconds);
                
            // If target time is in the past, schedule for tomorrow
            if (target <= now)
                target = target.AddDays(1);
                
            // Schedule foreground service to start 5 minutes before actual notification time
            var serviceStartTime = target.AddMinutes(-5);
            var delay = serviceStartTime - now;
            
            // Don't schedule if it's too far in the future (Android may kill this alarm)
            if (delay.TotalHours > 23)
                return;
                
            var serviceIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationForegroundService)));
            PendingIntent pendingServiceIntent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                pendingServiceIntent = PendingIntent.GetForegroundService(
                    context,
                    42,
                    serviceIntent,
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            }
            else
            {
                pendingServiceIntent = PendingIntent.GetService(
                    context,
                    42,
                    serviceIntent,
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            }

            if (context.GetSystemService(Context.AlarmService) is AlarmManager alarmManager)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        Java.Lang.JavaSystem.CurrentTimeMillis() + (long)delay.TotalMilliseconds,
                        pendingServiceIntent);
                }
                else
                {
                    alarmManager.SetExact(
                        AlarmType.RtcWakeup,
                        Java.Lang.JavaSystem.CurrentTimeMillis() + (long)delay.TotalMilliseconds,
                        pendingServiceIntent);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule foreground service backup");
        }
    }
    
    public async Task<bool> VerifyNotificationScheduledAsync()
    {
        var context = AndroidApp.Context;
        var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        var pending = PendingIntent.GetBroadcast(
            context, 
            1, 
            intent, 
            PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);
        
        // No pending intent found - alarm was canceled or never set
        if (pending == null)
        {
            _logger.LogWarning("Notification alarm not found - checking if it needs to be rescheduled");
            var settingsService = MauiApplication.Current?.Services?.GetService<NotificationService>();
            if (settingsService != null)
            {
                var settings = await settingsService.GetNotificationSettingsAsync();
                if (settings.Enabled && settings.Time.HasValue)
                {
                    _logger.LogWarning("Notification alarm not found - rescheduling");
                    await ScheduleAsync(settings.Time.Value);
                    return true;
                }
            }
            return false;
        }
        return true;
    }

    private static Task CancelAndroidNotificationsAsync()
    {
        var context = AndroidApp.Context;
        var am = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (am == null) return Task.CompletedTask;

        try
        {
            // Cancel all pending alarm variants we create (exact=1, inexact=2, repeating=3)
            var brType = Java.Lang.Class.FromType(typeof(NotificationReceiver));
            for (int requestCode = 1; requestCode <= 3; requestCode++)
            {
                var intent = new Intent(context, brType).SetAction(NotificationIntentConstants.ActionDailyNotification);
                var pending = PendingIntent.GetBroadcast(context, requestCode, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                am.Cancel(pending);
            }

            // Cancel the foreground-service prewarm alarm (requestCode=42)
            var serviceIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationForegroundService)));
            var pendingService = (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                ? PendingIntent.GetForegroundService(context, 42, serviceIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)
                : PendingIntent.GetService(context, 42, serviceIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            am.Cancel(pendingService);

            // Cancel the WorkManager backup job by unique name
            try
            {
                AndroidX.Work.WorkManager.GetInstance(context).CancelUniqueWork("daily_notification");
            }
            catch { }
        }
        catch { }

        return Task.CompletedTask;
    }
}
#endif
