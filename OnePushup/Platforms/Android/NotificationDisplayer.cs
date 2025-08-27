using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;

namespace OnePushUp.Platforms.Android;

public interface INotificationDisplayer
{
    void ShowPushupNotification(Context context, Intent intent);
    void ShowTestNotification(Context context, Intent intent);
}

public class NotificationDisplayer : INotificationDisplayer
{
    private readonly ILogger<NotificationDisplayer> _logger;

    public NotificationDisplayer(ILogger<NotificationDisplayer> logger)
    {
        _logger = logger;
    }

    public void ShowPushupNotification(Context context, Intent intent)
    {
        try
        {
            var notificationManager = NotificationManager.FromContext(context);

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
                _logger.LogInformation("Notification channel created");
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

            string timeString = intent.GetStringExtra(NotificationIntentConstants.ExtraNotificationTime) ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _logger.LogInformation("Notification time from intent: {Time}", timeString);

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
                    iconId = global::Android.Resource.Drawable.IcDialogInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding custom icon");
                iconId = global::Android.Resource.Drawable.IcDialogInfo;
            }

            string approach = intent.GetStringExtra(NotificationIntentConstants.ExtraApproach) ?? "default";

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

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Q)
            {
                builder.SetFullScreenIntent(pendingIntent, true);
            }

            int notificationId = intent.GetIntExtra(NotificationIntentConstants.ExtraNotificationId, 1);
            notificationManager.Notify(notificationId, builder.Build());

            _logger.LogInformation("Pushup notification displayed with ID {NotificationId} at {Time}", notificationId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            WakeDeviceScreen(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing notification");
        }
    }

    public void ShowTestNotification(Context context, Intent intent)
    {
        try
        {
            var notificationManager = NotificationManager.FromContext(context);

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

            int notificationId = intent.GetIntExtra(NotificationIntentConstants.ExtraNotificationId, 999);
            notificationManager.Notify(notificationId, builder.Build());

            _logger.LogInformation("Test notification displayed at {Time}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing test notification");
        }
    }

    private void WakeDeviceScreen(Context context)
    {
        try
        {
            var powerManager = context.GetSystemService(Context.PowerService) as PowerManager;
            if (powerManager == null) return;

            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenBright | WakeLockFlags.AcquireCausesWakeup, "OnePushUp::NotificationWakeLock");
            wakeLock.Acquire(5000);

            _logger.LogInformation("Acquired wake lock to ensure notification visibility");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waking device screen");
        }
    }
}
