using Microsoft.Extensions.Logging;
using OnePushUp.Models;
#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.ApplicationModel;
#endif
using Preferences = Microsoft.Maui.Storage.Preferences;

namespace OnePushUp.Services;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private const string EnabledKey = "notifications_enabled";
    private const string TimeKey = "notification_time";
    private const string LastScheduledKey = "last_notification_scheduled";

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task<NotificationSettings> GetNotificationSettingsAsync()
    {
        try
        {
            var enabled = Preferences.Default.Get(EnabledKey, false);
            
            // Time is stored as ticks
            var timeTicks = Preferences.Default.Get(TimeKey, 0L);
            TimeSpan? time = timeTicks > 0 ? TimeSpan.FromTicks(timeTicks) : new TimeSpan(8, 0, 0);
            
            return Task.FromResult(new NotificationSettings
            {
                Enabled = enabled,
                Time = time
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification settings");
            return Task.FromResult(new NotificationSettings
            {
                Enabled = false,
                Time = new TimeSpan(8, 0, 0) // Default to 8:00 AM
            });
        }
    }

    public async Task UpdateNotificationSettingsAsync(NotificationSettings settings)
    {
        try
        {
            // Update both enabled state and time in a single method
            Preferences.Default.Set(EnabledKey, settings.Enabled);
            
            if (settings.Time.HasValue)
            {
                Preferences.Default.Set(TimeKey, settings.Time.Value.Ticks);
            }
            
            // Schedule or cancel notifications based on the new settings
            if (settings.Enabled && settings.Time.HasValue)
            {
                await ScheduleNotificationAsync(settings.Time.Value);
                _logger.LogInformation($"Notification settings updated - Enabled: {settings.Enabled}, Time: {settings.Time?.ToString(@"hh\:mm")}");
            }
            else
            {
                await CancelNotificationsAsync();
                _logger.LogInformation($"Notifications disabled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification settings");
            throw;
        }
    }

    public async Task SetNotificationsEnabledAsync(bool enabled)
    {
        try
        {
            var current = await GetNotificationSettingsAsync();
            var updated = new NotificationSettings
            {
                Enabled = enabled,
                Time = current.Time
            };

            await UpdateNotificationSettingsAsync(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting notification enabled state");
            throw;
        }
    }

    public async Task SetNotificationTimeAsync(TimeSpan time)
    {
        try
        {
            var current = await GetNotificationSettingsAsync();
            var updated = new NotificationSettings
            {
                Enabled = current.Enabled,
                Time = time
            };

            await UpdateNotificationSettingsAsync(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting notification time");
            throw;
        }
    }

    public async Task SendTestNotificationAsync()
    {
        _logger.LogInformation("Sending test notification...");
        
#if ANDROID
        try
        {
            // Check if we have notification permission
            if (!await CheckAndRequestNotificationPermissionAsync())
            {
                _logger.LogWarning("Notification permission denied");
                return;
            }
            
            var context = Android.App.Application.Context;
            var notificationManager = NotificationManager.FromContext(context);
            
            // Create notification channel if needed
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
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
                _logger.LogInformation("Notification channel created successfully");
            }
            
            // Create the notification intent
            var notificationIntent = new Intent(context, typeof(MainActivity));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            
            var pendingIntent = PendingIntent.GetActivity(
                context, 
                0, 
                notificationIntent, 
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            
            // Use the default Android system information icon
            int iconId = Android.Resource.Drawable.IcDialogInfo;
            
            // Build the notification
            var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                .SetContentTitle("OnePushUp Test")
                .SetContentText($"This is a test notification - {DateTime.Now:HH:mm:ss}")
                .SetSmallIcon(iconId)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic);
            
            // Send the notification
            notificationManager.Notify(100, builder.Build());
            _logger.LogInformation("Test notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            throw;
        }
#else
        _logger.LogInformation("Test notifications are only supported on Android");
#endif
    }

    private Task ScheduleNotificationAsync(TimeSpan time)
    {
#if ANDROID
        return ScheduleAndroidNotificationAsync(time);
#else
        _logger.LogInformation("Notification scheduling is only supported on Android");
        return Task.CompletedTask;
#endif
    }

    private Task CancelNotificationsAsync()
    {
#if ANDROID
        return CancelAndroidNotificationsAsync();
#else
        _logger.LogInformation("Notification cancellation is only supported on Android");
        return Task.CompletedTask;
#endif
    }

#if ANDROID
    private async Task<bool> CheckAndRequestNotificationPermissionAsync()
    {
        try
        {
            // Only need to request permission on Android 13 (API 33) and above
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                // For Android 13+, we need to request post notification permission
                var status = await Permissions.CheckStatusAsync<PostNotificationsPermission>();
                
                _logger.LogInformation($"Current notification permission status: {status}");
                
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<PostNotificationsPermission>();
                    _logger.LogInformation($"Notification permission request result: {status}");
                }
                
                return status == PermissionStatus.Granted;
            }
            
            // Permission automatically granted on older Android versions
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking notification permission: {Message}", ex.Message);
            return false;
        }
    }

    // Custom permission class for POST_NOTIFICATIONS (Android 13+)
    public class PostNotificationsPermission : Permissions.BasePlatformPermission
    {
#if ANDROID
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new[]
        {
            ("android.permission.POST_NOTIFICATIONS", true)
        };
#endif
    }
    private async Task<bool> EnsureNotificationPermissionAsync()
    {
        return await CheckAndRequestNotificationPermissionAsync();
    }

    private (AlarmManager? alarmManager, PendingIntent? exactPendingIntent, PendingIntent? inexactPendingIntent, PendingIntent? repeatingPendingIntent, long triggerAtMillis, long delayMs, bool canUseExactAlarms, string calendarTime) CreateAlarmIntents(Context context, TimeSpan time)
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

        var exactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
        exactIntent.SetAction("com.onepushup.DAILY_NOTIFICATION");
        exactIntent.PutExtra("notification_id", 1);
        exactIntent.PutExtra("notification_time", $"{calendar.Time}");
        exactIntent.PutExtra("approach", "exact");

        var exactPendingIntent = PendingIntent.GetBroadcast(
            context,
            1,
            exactIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var inexactIntent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
        inexactIntent.SetAction("com.onepushup.DAILY_NOTIFICATION");
        inexactIntent.PutExtra("notification_id", 2);
        inexactIntent.PutExtra("notification_time", $"{calendar.Time}");
        inexactIntent.PutExtra("approach", "inexact");

        var inexactPendingIntent = PendingIntent.GetBroadcast(
            context,
            2,
            inexactIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var repeatingIntent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
        repeatingIntent.SetAction("com.onepushup.DAILY_NOTIFICATION");
        repeatingIntent.PutExtra("notification_id", 3);
        repeatingIntent.PutExtra("notification_time", $"{calendar.Time}");
        repeatingIntent.PutExtra("approach", "repeating");

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
        }

        return (alarmManager, exactPendingIntent, inexactPendingIntent, repeatingPendingIntent, triggerAtMillis, delayMs, canUseExactAlarms, calendar.Time.ToString());
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

            var context = Android.App.Application.Context;

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
    
    // This is a new method for scheduling a direct notification using Handler
    private void ScheduleDirectNotification(Context context, long delayMs)
    {
        try
        {
            // Only schedule if delay is reasonable (less than 24 hours)
            if (delayMs > 0 && delayMs < 24 * 60 * 60 * 1000)
            {
                _logger.LogInformation($"Scheduling direct notification with Handler in {delayMs}ms");
                
                // Create a Handler on the main looper
                var handler = new Android.OS.Handler(Android.OS.Looper.MainLooper);
                
                // Post delayed runnable
                handler.PostDelayed(new Java.Lang.Runnable(() => 
                {
                    try 
                    {
                        _logger.LogInformation("Direct notification handler triggered");
                        
                        // Create and send a direct notification
                        var notificationManager = NotificationManager.FromContext(context);
                        
                        // Ensure channel exists
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
                        
                        // Create notification intent
                        var notificationIntent = new Intent(context, typeof(MainActivity));
                        notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
                        
                        var pendingIntent = PendingIntent.GetActivity(
                            context, 
                            500, 
                            notificationIntent, 
                            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                        
                        // Get icon
                        int iconId = Android.Resource.Drawable.IcDialogInfo;
                        
                        // Build notification
                        var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                            .SetContentTitle("OnePushUp Reminder")
                            .SetContentText("Time to do your daily pushup! Do it now to not lose your streak!")
                            .SetSmallIcon(iconId)
                            .SetContentIntent(pendingIntent)
                            .SetAutoCancel(true)
                            .SetPriority(NotificationCompat.PriorityHigh)
                            .SetVisibility(NotificationCompat.VisibilityPublic)
                            .SetDefaults(NotificationCompat.DefaultAll);
                        
                        // Show notification
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
            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
            intent.SetAction("com.onepushup.DAILY_NOTIFICATION");
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
            var context = Android.App.Application.Context;
            
            // Cancel the alarm
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                _logger.LogError("Failed to get AlarmManager service");
                return Task.CompletedTask;
            }
            
            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
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
            // Create the intent for a test notification with a different request code
            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
            intent.SetAction("TEST_NOTIFICATION_ALARM");
            intent.PutExtra("notification_id", 999);
            intent.PutExtra("test_notification", true);
            intent.PutExtra("notification_time", $"{DateTime.Now.AddMilliseconds(delayMs):yyyy-MM-dd HH:mm:ss}");
            
            var pendingIntent = PendingIntent.GetBroadcast(
                context, 
                999, // Different request code
                intent, 
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            
            // Schedule using AlarmManager
            var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                _logger.LogError("Failed to get AlarmManager service for test notification");
                return;
            }
            
            // Set exact alarm for test notification
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
#endif
}
