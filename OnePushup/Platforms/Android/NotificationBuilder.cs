using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System;

namespace OnePushUp.Platforms.Android;

public static class NotificationBuilder
{
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
                Console.WriteLine("NotificationReceiver: Notification channel created");
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

            string timeString = intent.GetStringExtra("notification_time") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"NotificationReceiver: Notification time from intent: {timeString}");

            int iconId;
            try
            {
                iconId = context.Resources.GetIdentifier("notification_icon", "drawable", context.PackageName);
                if (iconId == 0)
                {
                    iconId = context.Resources.GetIdentifier("ic_notification", "drawable", context.PackageName);
                }

                if (iconId == 0)
                {
                    try
                    {
                        iconId = global::Android.Resource.Drawable.IcDialogInfo;
                    }
                    catch
                    {
                    }
                }

                if (iconId == 0)
                {
                    Console.WriteLine("NotificationReceiver: Custom icon not found by any method");
                    iconId = global::Android.Resource.Drawable.IcDialogInfo;
                }
                else
                {
                    Console.WriteLine($"NotificationReceiver: Using custom notification icon with id: {iconId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NotificationReceiver: Error finding custom icon: {ex.Message}");
                iconId = global::Android.Resource.Drawable.IcDialogInfo;
            }

            string approach = intent.GetStringExtra("approach") ?? "default";

            var builder = new NotificationCompat.Builder(context, "pushup_reminders")
                .SetContentTitle("OnePushUp Reminder")
                .SetContentText($"Time to do your daily pushup! ({approach})")
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

            Console.WriteLine($"NotificationReceiver: Pushup notification displayed with ID {notificationId} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            WakeDeviceScreen(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error showing notification: {ex.Message}");
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

            Console.WriteLine($"NotificationReceiver: Test notification displayed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error showing test notification: {ex.Message}");
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

            Console.WriteLine("NotificationReceiver: Acquired wake lock to ensure notification visibility");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationReceiver: Error waking device screen: {ex.Message}");
        }
    }
}

