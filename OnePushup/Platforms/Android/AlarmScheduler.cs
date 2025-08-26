using Android.App;
using Android.Content;
using Microsoft.Maui.Storage;
using Microsoft.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace OnePushUp.Platforms.Android;

public static class AlarmScheduler
{
    internal const string EnabledKey = "notifications_enabled";
    internal const string TimeKey = "notification_time";

    public const string ACTION_DAILY_NOTIFICATION = "com.onepushup.DAILY_NOTIFICATION";
    public const string ACTION_RESTORE_NOTIFICATIONS = "RESTORE_NOTIFICATIONS";

    private static ILogger Logger => _logger ??=
        MauiApplication.Current?.Services?.GetService<ILoggerFactory>()?.CreateLogger(nameof(AlarmScheduler))
        ?? NullLogger.Instance;
    private static ILogger? _logger;

    public static void RestoreNotificationsAfterReboot(Context context)
    {
        try
        {
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled)
            {
                Logger.LogInformation("Notifications were disabled, not restoring");
                return;
            }

            long timeTicks = Preferences.Default.Get(TimeKey, new TimeSpan(8, 0, 0).Ticks);
            var time = TimeSpan.FromTicks(timeTicks);

            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, time.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);

            if (calendar.TimeInMillis <= Java.Lang.JavaSystem.CurrentTimeMillis())
            {
                calendar.Add(Java.Util.CalendarField.DayOfYear, 1);
            }

            Logger.LogInformation("Restoring notification for {time}", calendar.Time);
            SetAlarm(context, calendar.TimeInMillis, 1, "restore");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error restoring notifications");
        }
    }

    public static void RescheduleForTomorrow(Context context)
    {
        try
        {
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled)
            {
                Logger.LogInformation("Notifications are disabled, not rescheduling");
                return;
            }

            long timeTicks = Preferences.Default.Get(TimeKey, new TimeSpan(8, 0, 0).Ticks);
            var time = TimeSpan.FromTicks(timeTicks);

            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, time.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            calendar.Add(Java.Util.CalendarField.DayOfYear, 1);

            Logger.LogInformation("Scheduling next notification for {time}", calendar.Time);
            SetAlarm(context, calendar.TimeInMillis, 1, "reschedule");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rescheduling notification");
        }
    }

    private static void SetAlarm(Context context, long triggerAtMillis, int requestCode, string reason)
    {
        try
        {
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                Logger.LogError("Failed to get AlarmManager service");
                return;
            }

            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            intent.SetAction(ACTION_DAILY_NOTIFICATION);
            intent.PutExtra("notification_id", requestCode);
            intent.PutExtra("reason", reason);

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                requestCode,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
            {
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
            }
            else
            {
                alarmManager.SetExact(AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
            }

            Logger.LogInformation("Alarm scheduled at {time} for {reason}", new Java.Util.Date(triggerAtMillis), reason);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting alarm");
        }
    }
}

