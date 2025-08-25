using Android.App;
using Android.Content;
using Android.OS; 
using AndroidX.Core.App;
using Microsoft.Maui.Storage;
using System;

namespace OnePushUp.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { 
    Intent.ActionBootCompleted, 
    Intent.ActionLockedBootCompleted,
    "android.intent.action.QUICKBOOT_POWERON",
    "android.intent.action.MY_PACKAGE_REPLACED"
})]
public class NotificationReceiver : BroadcastReceiver
{
    private const string EnabledKey = "notifications_enabled";
    private const string TimeKey = "notification_time";
    
    // System alarm actions
    private const string ACTION_DAILY_NOTIFICATION = "com.onepushup.DAILY_NOTIFICATION";
    private const string ACTION_RESTORE_NOTIFICATIONS = "RESTORE_NOTIFICATIONS";

    public override void OnReceive(Context context, Intent intent)
    {
        try
        {
            // Get the action from the intent
            var action = intent.Action;
            
            Console.WriteLine($"NotificationReceiver: Received intent with action: {action ?? "null"}");
            
            // Handle system boot or package replaced
            if (action == Intent.ActionBootCompleted || 
                action == Intent.ActionLockedBootCompleted || 
                action == "android.intent.action.QUICKBOOT_POWERON" || 
                action == "android.intent.action.MY_PACKAGE_REPLACED" ||
                action == ACTION_RESTORE_NOTIFICATIONS)
            {
                Console.WriteLine("NotificationReceiver: System boot or app updated, restoring notifications");
                RestoreNotificationsAfterReboot(context);
                return;
            }
            
            // Handle test notification
            bool isTestNotification = intent.GetBooleanExtra("test_notification", false);
            if (isTestNotification)
            {
                Console.WriteLine("NotificationReceiver: Handling test notification");
                ShowTestNotification(context, intent);
                return;
            }
            
            // For all other notifications, show the regular notification
            if (action == ACTION_DAILY_NOTIFICATION || 
                action == "DAILY_NOTIFICATION_EXACT" || 
                action == "DAILY_NOTIFICATION_INEXACT" || 
                action == "DAILY_NOTIFICATION_REPEAT" ||
                action == "WINDOW_NOTIFICATION_ALARM" ||
                action == "TEST_NOTIFICATION_ALARM")
            {
                ShowPushupNotification(context, intent);
                
                // Reschedule for tomorrow regardless of which type of notification triggered
                RescheduleForTomorrow(context);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error in OnReceive: {ex.Message}");
            Console.WriteLine($"NotificationReceiver: Stack trace: {ex.StackTrace}");
            
            // Try to reschedule anyway if there was an error
            try
            {
                RescheduleForTomorrow(context);
            }
            catch
            {
                // Ignore errors in the rescue attempt
            }
        }
    }
    
    private void ShowPushupNotification(Context context, Intent intent)
    {
        try
        {
            var notificationManager = NotificationManager.FromContext(context);
            
            // Create the notification channel (required for API 26+)
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    "pushup_reminders",
                    "Pushup Reminders",
                    NotificationImportance.High)
                {
                    Description = "Daily reminders to do your pushups",
                    LockscreenVisibility = NotificationVisibility.Public
                };
                
                // Enable vibration separately
                channel.EnableVibration(true);
                
                notificationManager.CreateNotificationChannel(channel);
                Console.WriteLine("NotificationReceiver: Notification channel created");
            }
            
            // Create the notification with an intent that opens the app
            var notificationIntent = new Intent(context, Java.Lang.Class.ForName("onepushup.MainActivity"));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
            notificationIntent.SetAction(Intent.ActionMain);
            notificationIntent.AddCategory(Intent.CategoryLauncher);
            
            var pendingIntent = PendingIntent.GetActivity(
                context, 
                0, 
                notificationIntent, 
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            
            // Get the notification time from intent extras (if available)
            string timeString = intent.GetStringExtra("notification_time") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"NotificationReceiver: Notification time from intent: {timeString}");
            
            // Try to find notification icon by resource ID
            int iconId;
            try {
                // Try multiple ways to get the icon
                iconId = context.Resources.GetIdentifier("notification_icon", "drawable", context.PackageName);
                if (iconId == 0)
                {
                    // Try the original icon name
                    iconId = context.Resources.GetIdentifier("ic_notification", "drawable", context.PackageName);
                }
                
                if (iconId == 0)
                {
                    // If still not found, try a direct reference
                    try {
                        // Remove the direct reference to Resource.Drawable.notification_icon since it doesn't exist
                        // Use system icon as fallback
                        iconId = global::Android.Resource.Drawable.IcDialogInfo;
                    }
                    catch {
                        // Ignore if this fails
                    }
                }
                
                if (iconId == 0)
                {
                    Console.WriteLine("NotificationReceiver: Custom icon not found by any method");
                    iconId = global::Android.Resource.Drawable.IcDialogInfo; // Fallback to system icon
                }
                else
                {
                    Console.WriteLine($"NotificationReceiver: Using custom notification icon with id: {iconId}");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"NotificationReceiver: Error finding custom icon: {ex.Message}");
                iconId = global::Android.Resource.Drawable.IcDialogInfo; // Fallback to system icon
            }
            
            // Get the approach type (for debugging)
            string approach = intent.GetStringExtra("approach") ?? "default";
            
            // Build the main notification
            var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                .SetContentTitle("OnePushUp Reminder")
                .SetContentText($"Time to do your daily pushup! ({approach})")
                .SetSmallIcon(iconId)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic)
                .SetCategory(NotificationCompat.CategoryAlarm)
                .SetDefaults(NotificationCompat.DefaultAll); // Enable sound, vibration and lights
            
            // Add a full screen intent to increase visibility on lock screen
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Q)
            {
                builder.SetFullScreenIntent(pendingIntent, true);
            }
            
            // Show the notification
            int notificationId = intent.GetIntExtra("notification_id", 1);
            notificationManager.Notify(notificationId, builder.Build());
            
            Console.WriteLine($"NotificationReceiver: Pushup notification displayed with ID {notificationId} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            // Wake up the device if screen is off
            WakeDeviceScreen(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error showing notification: {ex.Message}");
        }
    }
    
    private void ShowTestNotification(Context context, Intent intent)
    {
        try
        {
            var notificationManager = NotificationManager.FromContext(context);
            
            // Create the notification channel if needed
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
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
            
            // Create notification intent
            var notificationIntent = new Intent(context, Java.Lang.Class.ForName("onepushup.MainActivity"));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
            notificationIntent.SetAction(Intent.ActionMain);
            notificationIntent.AddCategory(Intent.CategoryLauncher);
            
            var pendingIntent = PendingIntent.GetActivity(
                context, 
                0, 
                notificationIntent, 
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            
            // Get icon
            int iconId = global::Android.Resource.Drawable.IcDialogInfo;
            
            // Build test notification
            var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                .SetContentTitle("OnePushUp Test")
                .SetContentText($"Notification system is working! Current time: {DateTime.Now:HH:mm:ss}")
                .SetSmallIcon(iconId)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic);
            
            // Show the notification
            int notificationId = intent.GetIntExtra("notification_id", 999);
            notificationManager.Notify(notificationId, builder.Build());
            
            Console.WriteLine($"NotificationReceiver: Test notification displayed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error showing test notification: {ex.Message}");
        }
    }
    
    private void WakeDeviceScreen(Context context)
    {
        try
        {
            // Acquire a wake lock to wake up the screen if it's off
            var powerManager = context.GetSystemService(Context.PowerService) as PowerManager;
            if (powerManager == null) return;
            
            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenBright | WakeLockFlags.AcquireCausesWakeup, "OnePushUp::NotificationWakeLock");
            wakeLock.Acquire(5000); // Hold for 5 seconds
            
            Console.WriteLine("NotificationReceiver: Acquired wake lock to ensure notification visibility");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error waking device screen: {ex.Message}");
        }
    }
    
    private void RestoreNotificationsAfterReboot(Context context)
    {
        try
        {
            // Check if notifications were enabled before reboot
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled)
            {
                Console.WriteLine("NotificationReceiver: Notifications were disabled, not restoring");
                return;
            }
            
            // Get the stored notification time
            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0)
            {
                Console.WriteLine("NotificationReceiver: No notification time stored, using default");
                timeTicks = new TimeSpan(8, 0, 0).Ticks; // Default to 8:00 AM
            }
            
            var time = TimeSpan.FromTicks(timeTicks);
            Console.WriteLine($"NotificationReceiver: Restoring notification for {time:hh\\:mm}");
            
            // Calculate when to send the notification
            var now = DateTime.Now;
            
            // Create a time that's specific to today at the desired hour/minute
            var notificationTime = new DateTime(
                now.Year, now.Month, now.Day, 
                time.Hours, time.Minutes, 0);
            
            // If the time has already passed today, schedule for tomorrow
            if (notificationTime <= now)
            {
                notificationTime = notificationTime.AddDays(1);
                Console.WriteLine("NotificationReceiver: Time already passed today, scheduling for tomorrow");
            }
            
            // APPROACH 1: Use Calendar and RTC
            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, time.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            
            // If the time has already passed today, add one day
            var nowMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            if (calendar.TimeInMillis <= nowMillis) 
            {
                calendar.Add(Java.Util.CalendarField.DayOfYear, 1);
            }
            
            var triggerAtMillis = calendar.TimeInMillis;
            Console.WriteLine($"NotificationReceiver: Will trigger at {calendar.Time}");
            
            // Schedule using AlarmManager
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                Console.WriteLine("NotificationReceiver: Failed to get AlarmManager service");
                return;
            }
            
            // IMPORTANT: Set multiple alarms with different intents and request codes
            // to ensure at least one of them works
            
            // 1. Exact alarm
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
            
            // 2. Inexact alarm
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
            
            // 3. Repeating alarm
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
            
            // Set alarms based on API level
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
            {
                // For newer Android versions, use SetExactAndAllowWhileIdle
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
                
                // Also set an inexact alarm as backup
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
                // For older Android versions
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
            
            // Always set a repeating alarm as a reliable backup
            try
            {
                alarmManager.SetRepeating(
                    AlarmType.RtcWakeup,
                    triggerAtMillis,
                    AlarmManager.IntervalDay, // 24 hours in milliseconds
                    repeatingPendingIntent);
                
                Console.WriteLine("NotificationReceiver: Repeating alarm set as backup");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NotificationReceiver: Error setting repeating alarm: {ex.Message}");
            }
            
            // Set window alarms as additional backup (+/- 2 minutes)
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
    
    private void RescheduleForTomorrow(Context context)
    {
        try
        {
            // Make sure notifications are still enabled
            bool enabled = Preferences.Default.Get(EnabledKey, false);
            if (!enabled)
            {
                Console.WriteLine("NotificationReceiver: Notifications are disabled, not rescheduling");
                return;
            }
            
            // Get the notification time preference
            long timeTicks = Preferences.Default.Get(TimeKey, 0L);
            if (timeTicks == 0)
            {
                Console.WriteLine("NotificationReceiver: No notification time stored, using default");
                timeTicks = new TimeSpan(8, 0, 0).Ticks; // Default to 8:00 AM
            }
            
            TimeSpan scheduledTime = TimeSpan.FromTicks(timeTicks);
            Console.WriteLine($"NotificationReceiver: Rescheduling for tomorrow at {scheduledTime:hh\\:mm}");
            
            // Calculate next notification time using Calendar
            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, scheduledTime.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, scheduledTime.Minutes);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            
            // Add one day to ensure it's tomorrow
            calendar.Add(Java.Util.CalendarField.DayOfYear, 1);
            
            var triggerAtMillis = calendar.TimeInMillis;
            
            // Get AlarmManager
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                Console.WriteLine("NotificationReceiver: Failed to get AlarmManager service for rescheduling");
                return;
            }
            
            // Create multiple intents with different actions/request codes
            
            // 1. Exact alarm for tomorrow
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
            
            // 2. Inexact alarm for tomorrow
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
            
            // 3. Repeating alarm
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
            
            // Set all alarm types based on API level
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
            
            // Set repeating alarm
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
            
            // Also set window alarms for tomorrow
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
    
    private void SetWindowAlarm(Context context, AlarmManager alarmManager, TimeSpan targetTime, int minuteOffset, int requestCode)
    {
        try
        {
            // Create a calendar for the offset time
            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
            calendar.Set(Java.Util.CalendarField.HourOfDay, targetTime.Hours);
            calendar.Set(Java.Util.CalendarField.Minute, targetTime.Minutes + minuteOffset);
            calendar.Set(Java.Util.CalendarField.Second, 0);
            calendar.Set(Java.Util.CalendarField.Millisecond, 0);
            
            // If the time has already passed today, add one day
            if (calendar.TimeInMillis <= Java.Lang.JavaSystem.CurrentTimeMillis()) 
            {
                calendar.Add(Java.Util.CalendarField.DayOfYear, 1);
            }
            
            var windowTriggerAtMillis = calendar.TimeInMillis;
            
            // Create intent with unique request code for this window alarm
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
            
            // Set the alarm
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
