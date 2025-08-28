#if ANDROID
using Android.App;
using Android.Content;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using OnePushUp.Services;

namespace OneActivity.Core.Platforms.Android;

public interface IAlarmScheduler
{
    void RestoreNotificationsAfterReboot(Context context);
    void RescheduleForTomorrow(Context context);
}

public class AlarmScheduler : IAlarmScheduler
{
    private readonly ILogger<AlarmScheduler> _logger;
    private const string EnabledKey = "notifications_enabled";
    private const string TimeKey = "notification_time";

    public AlarmScheduler(ILogger<AlarmScheduler> logger) { _logger = logger; }

    public void RestoreNotificationsAfterReboot(Context context)
    {
        try
        {
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled) return;

            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0) timeTicks = NotificationService.DefaultNotificationTime.Ticks;
            var time = TimeSpan.FromTicks(timeTicks);

            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, time.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            if (calendar.TimeInMillis <= Java.Lang.JavaSystem.CurrentTimeMillis())
                calendar.Add(Java.Util.CalendarField.DayOfYear, 1);

            var am = (AlarmManager?)context.GetSystemService(Context.AlarmService);
            if (am == null) return;

            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            intent.SetAction(NotificationIntentConstants.ActionDailyNotification);
            var pending = PendingIntent.GetBroadcast(context, 1, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
                am.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, calendar.TimeInMillis, pending);
            else
                am.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring notifications");
        }
    }

    public void RescheduleForTomorrow(Context context)
    {
        try
        {
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled) return;

            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0) timeTicks = NotificationService.DefaultNotificationTime.Ticks;
            var t = TimeSpan.FromTicks(timeTicks);

            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, t.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, t.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            calendar.Add(Java.Util.CalendarField.DayOfYear, 1);

            var am = (AlarmManager?)context.GetSystemService(Context.AlarmService);
            if (am == null) return;

            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            intent.SetAction(NotificationIntentConstants.ActionDailyNotification);
            var pending = PendingIntent.GetBroadcast(context, 1, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
                am.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, calendar.TimeInMillis, pending);
            else
                am.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling notification");
        }
    }
}
#endif

