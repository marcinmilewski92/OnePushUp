using Android.App;
using Android.Content;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using OnePushUp.Services;

namespace OnePushUp.Platforms.Android;

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

    public AlarmScheduler(ILogger<AlarmScheduler> logger)
    {
        _logger = logger;
    }

    public void RestoreNotificationsAfterReboot(Context context)
    {
        try
        {
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled)
            {
                _logger.LogInformation("Notifications were disabled, not restoring");
                return;
            }

            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0)
            {
                _logger.LogInformation("No notification time stored, using default");
                timeTicks = NotificationService.DefaultNotificationTime.Ticks;
            }

            var time = TimeSpan.FromTicks(timeTicks);
            _logger.LogInformation("Restoring notification for {Time}", time.ToString("hh\\:mm"));

            var now = DateTime.Now;
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
            _logger.LogInformation("Will trigger at {CalendarTime}", calendar.Time);

            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                _logger.LogError("Failed to get AlarmManager service");
                return;
            }

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

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
            {
                try
                {
                    alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        exactPendingIntent);
                    _logger.LogInformation("Exact alarm set after boot");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting exact alarm");
                }

                try
                {
                    alarmManager.Set(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        inexactPendingIntent);
                    _logger.LogInformation("Inexact alarm set after boot");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting inexact alarm");
                }
            }
            else
            {
                try
                {
                    alarmManager.SetExact(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        exactPendingIntent);
                    _logger.LogInformation("Exact alarm set for older Android");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting exact alarm on older Android");
                }
            }

            try
            {
                alarmManager.SetRepeating(
                    AlarmType.RtcWakeup,
                    triggerAtMillis,
                    AlarmManager.IntervalDay,
                    repeatingPendingIntent);
                _logger.LogInformation("Repeating alarm set as backup");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting repeating alarm");
            }

            SetWindowAlarm(context, alarmManager, time, -2, 101);
            SetWindowAlarm(context, alarmManager, time, -1, 102);
            SetWindowAlarm(context, alarmManager, time, 1, 103);
            SetWindowAlarm(context, alarmManager, time, 2, 104);

            _logger.LogInformation("Multiple alarms scheduled after boot/restore");
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
            if (!enabled)
            {
                _logger.LogInformation("Notifications are disabled, not rescheduling");
                return;
            }

            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0)
            {
                _logger.LogInformation("No notification time stored, using default");
                timeTicks = NotificationService.DefaultNotificationTime.Ticks;
            }

            TimeSpan scheduledTime = TimeSpan.FromTicks(timeTicks);
            _logger.LogInformation("Rescheduling for tomorrow at {Time}", scheduledTime.ToString("hh\\:mm"));

            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, scheduledTime.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, scheduledTime.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            calendar.Add(Java.Util.CalendarField.DayOfYear, 1);

            var triggerAtMillis = calendar.TimeInMillis;
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                _logger.LogError("Failed to get AlarmManager service for rescheduling");
                return;
            }

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

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
            {
                try
                {
                    alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        exactPendingIntent);
                    _logger.LogInformation("Alarm rescheduled for tomorrow at {CalendarTime}", calendar.Time);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting exact alarm for tomorrow");
                }

                try
                {
                    alarmManager.Set(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        inexactPendingIntent);
                    _logger.LogInformation("Inexact alarm rescheduled for tomorrow");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting inexact alarm for tomorrow");
                }
            }
            else
            {
                try
                {
                    alarmManager.SetExact(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        exactPendingIntent);
                    _logger.LogInformation("Alarm rescheduled for tomorrow at {CalendarTime}", calendar.Time);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting exact alarm for older Android");
                }
            }

            try
            {
                alarmManager.SetRepeating(
                    AlarmType.RtcWakeup,
                    triggerAtMillis,
                    AlarmManager.IntervalDay,
                    repeatingPendingIntent);
                _logger.LogInformation("Repeating alarm set for tomorrow at {CalendarTime}", calendar.Time);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting repeating alarm");
            }

            SetWindowAlarm(context, alarmManager, scheduledTime, -2, 101);
            SetWindowAlarm(context, alarmManager, scheduledTime, -1, 102);
            SetWindowAlarm(context, alarmManager, scheduledTime, 1, 103);
            SetWindowAlarm(context, alarmManager, scheduledTime, 2, 104);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling for tomorrow");
        }
    }

    private void SetWindowAlarm(Context context, AlarmManager alarmManager, TimeSpan targetTime, int minuteOffset, int requestCode)
    {
        try
        {
            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, targetTime.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, targetTime.Minutes + minuteOffset);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);

            if (calendar.TimeInMillis <= Java.Lang.JavaSystem.CurrentTimeMillis())
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

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
            {
                try
                {
                    alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        windowTriggerAtMillis,
                        pendingIntent);
                    _logger.LogInformation("Window alarm set for {Offset} minute(s) offset", (minuteOffset > 0 ? "+" : "") + minuteOffset);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting window alarm");
                }
            }
            else
            {
                try
                {
                    alarmManager.SetExact(
                        AlarmType.RtcWakeup,
                        windowTriggerAtMillis,
                        pendingIntent);
                    _logger.LogInformation("Window alarm set for {Offset} minute(s) offset", (minuteOffset > 0 ? "+" : "") + minuteOffset);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting window alarm for older Android");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting window alarm with offset {Offset}", minuteOffset);
        }
    }
}
