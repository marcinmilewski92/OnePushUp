using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using OnePushUp.Services;
using Preferences = Microsoft.Maui.Storage.Preferences;
// Fully qualify the Application references to resolve ambiguity
using AndroidApp = Android.App.Application;

namespace OnePushUp.Platforms.Android;

public class AndroidNotificationScheduler : INotificationScheduler
{
    private readonly ILogger<AndroidNotificationScheduler> _logger;
    private const string LastScheduledKey = "last_notification_scheduled";

    public AndroidNotificationScheduler(ILogger<AndroidNotificationScheduler> logger)
    {
        _logger = logger;
    }

    public async Task SendTestAsync()
    {
        try
        {
            if (!await CheckAndRequestNotificationPermissionAsync())
            {
                _logger.LogWarning("Notification permission denied");
                return;
            }

            var context = AndroidApp.Context;
            var notificationManager = NotificationManager.FromContext(context);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    "pushup_reminders",
                    "Pushup Reminders",
                    NotificationImportance.High)
                {
                    Description = "Daily reminders to do your pushups",
                    LockscreenVisibility = NotificationVisibility.Public
                };

                channel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channel);
                _logger.LogInformation("Notification channel created successfully");
            }

            var notificationIntent = new Intent(context, typeof(MainActivity));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(
                context,
                0,
                notificationIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            // Use default system icon
            int iconId = global::Android.Resource.Drawable.IcDialogInfo;

            var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                .SetContentTitle("OnePushUp Test")
                .SetContentText($"This is a test notification - {DateTime.Now:HH:mm:ss}")
                .SetSmallIcon(iconId)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic);

            notificationManager.Notify(100, builder.Build());
            _logger.LogInformation("Test notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            throw;
        }
    }

    public async Task ScheduleAsync(TimeSpan time)
    {
        await ScheduleAndroidNotificationAsync(time);
    }

    public Task CancelAsync()
    {
        return CancelAndroidNotificationsAsync();
    }

    private async Task<bool> CheckAndRequestNotificationPermissionAsync()
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                var status = await Permissions.CheckStatusAsync<PostNotificationsPermission>();
                _logger.LogInformation($"Current notification permission status: {status}");

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<PostNotificationsPermission>();
                    _logger.LogInformation($"Notification permission request result: {status}");
                }

                return status == PermissionStatus.Granted;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking notification permission: {Message}", ex.Message);
            return false;
        }
    }

    public class PostNotificationsPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new[]
        {
            ("android.permission.POST_NOTIFICATIONS", true)
        };
    }

    private async Task<bool> EnsureNotificationPermissionAsync()
    {
        return await CheckAndRequestNotificationPermissionAsync();
    }

    private (AlarmManager? alarmManager,
             PendingIntent? exactPendingIntent,
             PendingIntent? inexactPendingIntent,
             PendingIntent? repeatingPendingIntent,
             long triggerAtMillis,
             long delayMs,
             bool canUseExactAlarms,
             string calendarTime)
        CreateAlarmIntents(Context context, TimeSpan time)
    {
        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
        if (alarmManager == null)
        {
            _logger.LogError("Failed to get AlarmManager service");
            return (null, null, null, null, 0, 0, false, string.Empty);
        }

        var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
        calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hours);
        calendar.Set(Java.Util.CalendarField.Minute, time.Minutes);
        calendar.Set(Java.Util.CalendarField.Second, 0);
        calendar.Set(Java.Util.CalendarField.Millisecond, 0);

        var nowMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
        if (calendar.TimeInMillis <= nowMillis)
        {
            calendar.Add(Java.Util.CalendarField.DayOfYear, 1);
        }

        var triggerAtMillis = calendar.TimeInMillis;
        var delayMs = triggerAtMillis - nowMillis;

        _logger.LogInformation($"Calendar time: {calendar.Time}");
        _logger.LogInformation($"Delay in ms: {delayMs}");

        var exactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        exactIntent.SetAction(NotificationIntentConstants.ActionDailyNotification);
        exactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationId, 1);
        exactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationTime, $"{calendar.Time}");
        exactIntent.PutExtra(NotificationIntentConstants.ExtraApproach, "exact");

        var exactPendingIntent = PendingIntent.GetBroadcast(
            context,
            1,
            exactIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var inexactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        inexactIntent.SetAction(NotificationIntentConstants.ActionDailyNotification);
        inexactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationId, 2);
        inexactIntent.PutExtra(NotificationIntentConstants.ExtraNotificationTime, $"{calendar.Time}");
        inexactIntent.PutExtra(NotificationIntentConstants.ExtraApproach, "inexact");

        var inexactPendingIntent = PendingIntent.GetBroadcast(
            context,
            2,
            inexactIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var repeatingIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
        repeatingIntent.SetAction(NotificationIntentConstants.ActionDailyNotification);
        repeatingIntent.PutExtra(NotificationIntentConstants.ExtraNotificationId, 3);
        repeatingIntent.PutExtra(NotificationIntentConstants.ExtraNotificationTime, $"{calendar.Time}");
        repeatingIntent.PutExtra(NotificationIntentConstants.ExtraApproach, "repeating");

        var repeatingPendingIntent = PendingIntent.GetBroadcast(
            context,
            3,
            repeatingIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        bool canUseExactAlarms = true;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarmManager.CanScheduleExactAlarms())
        {
            _logger.LogWarning("Cannot schedule exact alarms - permission not granted");
            canUseExactAlarms = false;
            TryPromptExactAlarmSettings(context);
        }

        return (alarmManager, exactPendingIntent, inexactPendingIntent, repeatingPendingIntent, triggerAtMillis, delayMs, canUseExactAlarms, calendar.Time.ToString());
    }

    private void TryPromptExactAlarmSettings(Context context)
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                var intent = new Intent(global::Android.Provider.Settings.ActionRequestScheduleExactAlarm);
                intent.SetData(global::Android.Net.Uri.Parse($"package:{context.PackageName}"));
                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
                _logger.LogInformation("Opened exact alarm settings screen");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open exact alarm settings");
        }
    }

    private void SetupWindowAlarms(Context context, AlarmManager alarmManager, TimeSpan time)
    {
        SetWindowAlarm(context, alarmManager, time, -2, 101);
        SetWindowAlarm(context, alarmManager, time, -1, 102);
        SetWindowAlarm(context, alarmManager, time, 1, 103);
        SetWindowAlarm(context, alarmManager, time, 2, 104);
    }

    private async Task ScheduleAndroidNotificationAsync(TimeSpan time)
    {
        try
        {
            if (!await EnsureNotificationPermissionAsync())
            {
                _logger.LogWarning("Cannot schedule notification - permission denied");
                return;
            }

            var context = AndroidApp.Context;

            await CancelAndroidNotificationsAsync();

            var now = DateTime.Now;
            _logger.LogInformation($"Scheduling notification - Current time: {now:yyyy-MM-dd HH:mm:ss}");
            _logger.LogInformation($"Requested notification time: {time.Hours:D2}:{time.Minutes:D2}");

            var alarmData = CreateAlarmIntents(context, time);
            if (alarmData.alarmManager == null)
            {
                return;
            }

            var alarmManager = alarmData.alarmManager;
            var triggerAtMillis = alarmData.triggerAtMillis;
            var delayMs = alarmData.delayMs;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (alarmData.canUseExactAlarms)
                {
                    alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        alarmData.exactPendingIntent!);

                    _logger.LogInformation("Exact alarm set");
                }

                alarmManager.Set(
                    AlarmType.RtcWakeup,
                    triggerAtMillis,
                    alarmData.inexactPendingIntent!);

                _logger.LogInformation("Inexact alarm set");
            }
            else
            {
                if (alarmData.canUseExactAlarms)
                {
                    alarmManager.SetExact(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        alarmData.exactPendingIntent!);
                }

                alarmManager.Set(
                    AlarmType.Rtc,
                    triggerAtMillis,
                    alarmData.inexactPendingIntent!);
            }

            alarmManager.SetRepeating(
                AlarmType.RtcWakeup,
                triggerAtMillis,
                AlarmManager.IntervalDay,
                alarmData.repeatingPendingIntent!);

            _logger.LogInformation("Repeating alarm set");

            SetupWindowAlarms(context, alarmManager, time);

            SendTestNotificationDelayed(context, 10000);

            Preferences.Default.Set(LastScheduledKey, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Preferences.Default.Set("notification_target_time", alarmData.calendarTime);

            _logger.LogInformation($"Multiple notification alarms scheduled for {time.Hours:D2}:{time.Minutes:D2}");

            ScheduleDirectNotification(context, delayMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling Android notification");
            throw;
        }
    }

    private void ScheduleDirectNotification(Context context, long delayMs)
    {
        try
        {
            if (delayMs > 0 && delayMs < 24 * 60 * 60 * 1000)
            {
                _logger.LogInformation($"Scheduling direct notification with Handler in {delayMs}ms");

                var handler = new Handler(Looper.MainLooper);

                handler.PostDelayed(new Java.Lang.Runnable(() =>
                {
                    try
                    {
                        _logger.LogInformation("Direct notification handler triggered");

                        var notificationManager = NotificationManager.FromContext(context);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var channel = new NotificationChannel(
                                "pushup_reminders",
                                "Pushup Reminders",
                                NotificationImportance.High)
                            {
                                Description = "Daily reminders to do your pushups",
                                LockscreenVisibility = NotificationVisibility.Public
                            };

                            channel.EnableVibration(true);
                            notificationManager.CreateNotificationChannel(channel);
                        }

                        var notificationIntent = new Intent(context, typeof(MainActivity));
                        notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);

                        var pendingIntent = PendingIntent.GetActivity(
                            context,
                            500,
                            notificationIntent,
                            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                        // Use default system icon
                        int iconId = global::Android.Resource.Drawable.IcDialogInfo;

                        var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                            .SetContentTitle("OnePushUp Reminder")
                            .SetContentText("Time to do your daily pushup! Do it now to not lose your streak!")
                            .SetSmallIcon(iconId)
                            .SetContentIntent(pendingIntent)
                            .SetAutoCancel(true)
                            .SetPriority(NotificationCompat.PriorityHigh)
                            .SetVisibility(NotificationCompat.VisibilityPublic)
                            .SetDefaults(NotificationCompat.DefaultAll);

                        notificationManager.Notify(500, builder.Build());
                        _logger.LogInformation("Direct notification displayed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in direct notification handler");
                    }
                }), delayMs);

                _logger.LogInformation("Direct notification handler scheduled");
            }
            else
            {
                _logger.LogInformation("Direct notification not scheduled - delay too long or negative");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling direct notification");
        }
    }

    private void SetWindowAlarm(Context context, AlarmManager alarmManager, TimeSpan time, int minuteOffset, int requestCode)
    {
        try
        {
            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, time.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            calendar.Add(Java.Util.CalendarField.Minute, minuteOffset);

            var nowMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            if (calendar.TimeInMillis <= nowMillis)
            {
                calendar.Add(Java.Util.CalendarField.DayOfYear, 1);
            }

            var windowTriggerAtMillis = calendar.TimeInMillis;

            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            intent.SetAction(NotificationIntentConstants.ActionDailyNotification);
            intent.PutExtra(NotificationIntentConstants.ExtraNotificationId, requestCode);
            intent.PutExtra(NotificationIntentConstants.ExtraWindowAlarm, true);
            intent.PutExtra(NotificationIntentConstants.ExtraMinuteOffset, minuteOffset);
            intent.PutExtra(NotificationIntentConstants.ExtraApproach, $"window_{minuteOffset}");

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                requestCode,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                alarmManager.SetExactAndAllowWhileIdle(
                    AlarmType.RtcWakeup,
                    windowTriggerAtMillis,
                    pendingIntent);
            }
            else
            {
                alarmManager.SetExact(
                    AlarmType.RtcWakeup,
                    windowTriggerAtMillis,
                    pendingIntent);
            }

            _logger.LogInformation($"Window alarm set for {(minuteOffset > 0 ? "+" : "")}{minuteOffset} minute(s) offset");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting window alarm with offset {minuteOffset}");
        }
    }

    private Task CancelAndroidNotificationsAsync()
    {
        try
        {
            var context = AndroidApp.Context;

            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                _logger.LogError("Failed to get AlarmManager service");
                return Task.CompletedTask;
            }

            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                1,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            alarmManager.Cancel(pendingIntent);

            _logger.LogInformation("Cancelled all notifications");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling Android notifications");
            throw;
        }
    }

    private void SendTestNotificationDelayed(Context context, long delayMs)
    {
        try
        {
            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            intent.SetAction(NotificationIntentConstants.ActionTestNotificationAlarm);
            intent.PutExtra(NotificationIntentConstants.ExtraNotificationId, 999);
            intent.PutExtra(NotificationIntentConstants.ExtraTestNotification, true);
            intent.PutExtra(NotificationIntentConstants.ExtraNotificationTime, $"{DateTime.Now.AddMilliseconds(delayMs):yyyy-MM-dd HH:mm:ss}");

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                999,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                _logger.LogError("Failed to get AlarmManager service for test notification");
                return;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                alarmManager.SetExactAndAllowWhileIdle(
                    AlarmType.RtcWakeup,
                    Java.Lang.JavaSystem.CurrentTimeMillis() + delayMs,
                    pendingIntent);

                _logger.LogInformation($"Test notification scheduled in {delayMs}ms");
            }
            else
            {
                alarmManager.SetExact(
                    AlarmType.RtcWakeup,
                    Java.Lang.JavaSystem.CurrentTimeMillis() + delayMs,
                    pendingIntent);

                _logger.LogInformation($"Test notification scheduled in {delayMs}ms");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling test notification");
        }
    }
}
