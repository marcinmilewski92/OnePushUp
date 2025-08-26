using Android.App;
using Android.Content;
using Microsoft.Maui.Storage;
using System;

namespace OnePushUp.Platforms.Android;

public static class AlarmScheduler
{
    internal const string EnabledKey = "notifications_enabled";
    internal const string TimeKey = "notification_time";

    public const string ACTION_DAILY_NOTIFICATION = "com.onepushup.DAILY_NOTIFICATION";
    public const string ACTION_RESTORE_NOTIFICATIONS = "RESTORE_NOTIFICATIONS";

    public static void RestoreNotificationsAfterReboot(Context context)
    {
        try
        {
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled)
            {
                Console.WriteLine("NotificationReceiver: Notifications were disabled, not restoring");
                return;
            }

            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0)
            {
                Console.WriteLine("NotificationReceiver: No notification time stored, using default");
                timeTicks = new TimeSpan(8, 0, 0).Ticks;
            }

            var time = TimeSpan.FromTicks(timeTicks);
            Console.WriteLine($"NotificationReceiver: Restoring notification for {time:hh\\:mm}");

            var now = DateTime.Now;
            var notificationTime = new DateTime(now.Year, now.Month, now.Day, time.Hours, time.Minutes, 0);
            if (notificationTime <= now)
            {
                notificationTime = notificationTime.AddDays(1);
                Console.WriteLine("NotificationReceiver: Time already passed today, scheduling for tomorrow");
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
            Console.WriteLine($"NotificationReceiver: Will trigger at {calendar.Time}");

            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                Console.WriteLine("NotificationReceiver: Failed to get AlarmManager service");
                return;
            }

            var exactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            exactIntent.SetAction(ACTION_DAILY_NOTIFICATION);
            exactIntent.PutExtra("notification_id", 1);
            exactIntent.PutExtra("notification_time", $"{calendar.Time}");
            exactIntent.PutExtra("approach", "exact");

            var exactPendingIntent = PendingIntent.GetBroadcast(
                context,
                1,
                exactIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var inexactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            inexactIntent.SetAction(ACTION_DAILY_NOTIFICATION);
            inexactIntent.PutExtra("notification_id", 2);
            inexactIntent.PutExtra("notification_time", $"{calendar.Time}");
            inexactIntent.PutExtra("approach", "inexact");

            var inexactPendingIntent = PendingIntent.GetBroadcast(
                context,
                2,
                inexactIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var repeatingIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            repeatingIntent.SetAction(ACTION_DAILY_NOTIFICATION);
            repeatingIntent.PutExtra("notification_id", 3);
            repeatingIntent.PutExtra("notification_time", $"{calendar.Time}");
            repeatingIntent.PutExtra("approach", "repeating");

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

                    Console.WriteLine("NotificationReceiver: Exact alarm set after boot");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting exact alarm: {ex.Message}");
                }

                try
                {
                    alarmManager.Set(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        inexactPendingIntent);

                    Console.WriteLine("NotificationReceiver: Inexact alarm set after boot");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting inexact alarm: {ex.Message}");
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

                    Console.WriteLine("NotificationReceiver: Exact alarm set for older Android");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting exact alarm on older Android: {ex.Message}");
                }
            }

            try
            {
                alarmManager.SetRepeating(
                    AlarmType.RtcWakeup,
                    triggerAtMillis,
                    AlarmManager.IntervalDay,
                    repeatingPendingIntent);

                Console.WriteLine("NotificationReceiver: Repeating alarm set as backup");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NotificationReceiver: Error setting repeating alarm: {ex.Message}");
            }

            SetWindowAlarm(context, alarmManager, time, -2, 101);
            SetWindowAlarm(context, alarmManager, time, -1, 102);
            SetWindowAlarm(context, alarmManager, time, 1, 103);
            SetWindowAlarm(context, alarmManager, time, 2, 104);

            Console.WriteLine("NotificationReceiver: Multiple alarms scheduled after boot/restore");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error restoring notifications: {ex.Message}");
            Console.WriteLine($"NotificationReceiver: Stack trace: {ex.StackTrace}");
        }
    }

    public static void RescheduleForTomorrow(Context context)
    {
        try
        {
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled)
            {
                Console.WriteLine("NotificationReceiver: Notifications are disabled, not rescheduling");
                return;
            }

            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0)
            {
                Console.WriteLine("NotificationReceiver: No notification time stored, using default");
                timeTicks = new TimeSpan(8, 0, 0).Ticks;
            }

            var scheduledTime = TimeSpan.FromTicks(timeTicks);
            Console.WriteLine($"NotificationReceiver: Rescheduling for tomorrow at {scheduledTime:hh\\:mm}");

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
                Console.WriteLine("NotificationReceiver: Failed to get AlarmManager service for rescheduling");
                return;
            }

            var exactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            exactIntent.SetAction(ACTION_DAILY_NOTIFICATION);
            exactIntent.PutExtra("notification_id", 1);
            exactIntent.PutExtra("notification_time", $"{calendar.Time}");
            exactIntent.PutExtra("approach", "exact_tomorrow");

            var exactPendingIntent = PendingIntent.GetBroadcast(
                context,
                1,
                exactIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var inexactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            inexactIntent.SetAction(ACTION_DAILY_NOTIFICATION);
            inexactIntent.PutExtra("notification_id", 2);
            inexactIntent.PutExtra("notification_time", $"{calendar.Time}");
            inexactIntent.PutExtra("approach", "inexact_tomorrow");

            var inexactPendingIntent = PendingIntent.GetBroadcast(
                context,
                2,
                inexactIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var repeatingIntent = new Intent(context, Java.Lang.Class.FromType(typeof(NotificationReceiver)));
            repeatingIntent.SetAction(ACTION_DAILY_NOTIFICATION);
            repeatingIntent.PutExtra("notification_id", 3);
            repeatingIntent.PutExtra("notification_time", $"{calendar.Time}");
            repeatingIntent.PutExtra("approach", "repeating_tomorrow");

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

                    Console.WriteLine($"NotificationReceiver: Exact alarm rescheduled for tomorrow at {calendar.Time}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting exact alarm for tomorrow: {ex.Message}");
                }

                try
                {
                    alarmManager.Set(
                        AlarmType.RtcWakeup,
                        triggerAtMillis,
                        inexactPendingIntent);

                    Console.WriteLine("NotificationReceiver: Inexact alarm set for tomorrow");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting inexact alarm for tomorrow: {ex.Message}");
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

                    Console.WriteLine($"NotificationReceiver: Alarm rescheduled for tomorrow at {calendar.Time}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting exact alarm for older Android: {ex.Message}");
                }
            }

            try
            {
                alarmManager.SetRepeating(
                    AlarmType.RtcWakeup,
                    triggerAtMillis,
                    AlarmManager.IntervalDay,
                    repeatingPendingIntent);

                Console.WriteLine($"NotificationReceiver: Repeating alarm set for tomorrow at {calendar.Time}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NotificationReceiver: Error setting repeating alarm: {ex.Message}");
            }

            SetWindowAlarm(context, alarmManager, scheduledTime, -2, 101);
            SetWindowAlarm(context, alarmManager, scheduledTime, -1, 102);
            SetWindowAlarm(context, alarmManager, scheduledTime, 1, 103);
            SetWindowAlarm(context, alarmManager, scheduledTime, 2, 104);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error rescheduling for tomorrow: {ex.Message}");
        }
    }

    private static void SetWindowAlarm(Context context, AlarmManager alarmManager, TimeSpan targetTime, int minuteOffset, int requestCode)
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
            intent.SetAction(ACTION_DAILY_NOTIFICATION);
            intent.PutExtra("notification_id", requestCode);
            intent.PutExtra("window_alarm", true);
            intent.PutExtra("minute_offset", minuteOffset);
            intent.PutExtra("approach", $"window_{minuteOffset}");

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

                    Console.WriteLine($"NotificationReceiver: Window alarm set for {(minuteOffset > 0 ? "+" : "")}{minuteOffset} minute(s) offset");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting window alarm: {ex.Message}");
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

                    Console.WriteLine($"NotificationReceiver: Window alarm set for {(minuteOffset > 0 ? "+" : "")}{minuteOffset} minute(s) offset");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"NotificationReceiver: Error setting window alarm for older Android: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error setting window alarm with offset {minuteOffset}: {ex.Message}");
        }
    }
}

