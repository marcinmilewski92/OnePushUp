using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace OnePushUp.Platforms.Android;

public static class NotificationBuilder
{
    private static ILogger Logger => _logger ??=
        MauiApplication.Current?.Services?.GetService<ILoggerFactory>()?.CreateLogger(nameof(NotificationBuilder))
        ?? NullLogger.Instance;
    private static ILogger? _logger;

    public static void ShowPushupNotification(Context context, Intent intent)
    {
        try
        {
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

            var notificationIntent = new Intent(context, Java.Lang.Class.ForName("onepushup.MainActivity"));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
            notificationIntent.SetAction(Intent.ActionMain);
            notificationIntent.AddCategory(Intent.CategoryLauncher);

            var pendingIntent = PendingIntent.GetActivity(
                context,
                0,
                notificationIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            int iconId = global::Android.Resource.Drawable.IcDialogInfo;
            var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                .SetContentTitle("OnePushUp Reminder")
                .SetContentText("Time to do your daily pushup!")
                .SetSmallIcon(iconId)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic)
                .SetCategory(NotificationCompat.CategoryAlarm)
                .SetDefaults(NotificationCompat.DefaultAll);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                builder.SetFullScreenIntent(pendingIntent, true);
            }

            int notificationId = intent.GetIntExtra("notification_id", 1);
            notificationManager.Notify(notificationId, builder.Build());
            WakeDeviceScreen(context);
            Logger.LogInformation("Pushup notification displayed with ID {Id}", notificationId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing pushup notification");
        }
    }

    public static void ShowTestNotification(Context context, Intent intent)
    {
        try
        {
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

            var notificationIntent = new Intent(context, Java.Lang.Class.ForName("onepushup.MainActivity"));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
            notificationIntent.SetAction(Intent.ActionMain);
            notificationIntent.AddCategory(Intent.CategoryLauncher);

            var pendingIntent = PendingIntent.GetActivity(
                context,
                0,
                notificationIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            int iconId = global::Android.Resource.Drawable.IcDialogInfo;
            var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                .SetContentTitle("OnePushUp Test")
                .SetContentText($"Notification system is working! Current time: {DateTime.Now:HH:mm:ss}")
                .SetSmallIcon(iconId)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic);

            int notificationId = intent.GetIntExtra("notification_id", 999);
            notificationManager.Notify(notificationId, builder.Build());
            Logger.LogInformation("Test notification displayed at {Time}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing test notification");
        }
    }

    private static void WakeDeviceScreen(Context context)
    {
        try
        {
            var powerManager = context.GetSystemService(Context.PowerService) as PowerManager;
            if (powerManager == null) return;

            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenBright | WakeLockFlags.AcquireCausesWakeup, "OnePushUp::NotificationWakeLock");
            wakeLock.Acquire(5000);
            Logger.LogInformation("Wake lock acquired to display notification");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error waking device screen");
        }
    }
}

