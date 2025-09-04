#if ANDROID
using global::Android.App;
using global::Android.Content;
using global::Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;

namespace OneActivity.Core.Platforms.Android;

public interface INotificationDisplayer
{
    void ShowActivityNotification(Context context, Intent intent);
    void ShowTestNotification(Context context, Intent intent);
}

public class NotificationDisplayer : INotificationDisplayer
{
    private readonly ILogger<NotificationDisplayer> _logger;
    private readonly IActivityContent _content;
    public const string ChannelId = "activity_reminders";

    public NotificationDisplayer(ILogger<NotificationDisplayer> logger, IActivityContent content)
    {
        _logger = logger;
        _content = content;
    }

    public void ShowActivityNotification(Context context, Intent intent)
    {
        try
        {
            var nm = NotificationManager.FromContext(context);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, $"{_content.AppName} Reminders", NotificationImportance.High)
                {
                    Description = $"Daily reminders to {_content.Verb} your {_content.UnitPlural}",
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channel.EnableVibration(true);
                nm.CreateNotificationChannel(channel);
            }

            var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName) ?? new Intent(Intent.ActionMain);
            launchIntent.AddCategory(Intent.CategoryLauncher);
            launchIntent.SetPackage(context.PackageName);
            launchIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
            var pending = PendingIntent.GetActivity(context, 0, launchIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            int iconId = global::Android.Resource.Drawable.IcDialogInfo;
            string approach = intent.GetStringExtra(NotificationIntentConstants.ExtraApproach) ?? "default";

            var builder = new NotificationCompat.Builder(context, ChannelId)
                .SetContentTitle($"{_content.AppName} Reminder")
                .SetContentText($"Time to {_content.Verb} your daily {_content.UnitSingular}! ({approach})")
                .SetSmallIcon(iconId)
                .SetContentIntent(pending)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic)
                .SetCategory(NotificationCompat.CategoryAlarm)
                .SetDefaults(NotificationCompat.DefaultAll);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                builder.SetFullScreenIntent(pending, true);
            }

            int notificationId = intent.GetIntExtra(NotificationIntentConstants.ExtraNotificationId, 1);
            nm.Notify(notificationId, builder.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing activity notification");
        }
    }

    public void ShowTestNotification(Context context, Intent intent)
    {
        try
        {
            var nm = NotificationManager.FromContext(context);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, $"{_content.AppName} Reminders", NotificationImportance.High)
                {
                    Description = $"Daily reminders to {_content.Verb} your {_content.UnitPlural}",
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channel.EnableVibration(true);
                nm.CreateNotificationChannel(channel);
            }

            var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName) ?? new Intent(Intent.ActionMain);
            launchIntent.AddCategory(Intent.CategoryLauncher);
            launchIntent.SetPackage(context.PackageName);
            launchIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
            var pending = PendingIntent.GetActivity(context, 0, launchIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            int iconId = global::Android.Resource.Drawable.IcDialogInfo;
            var builder = new NotificationCompat.Builder(context, ChannelId)
                .SetContentTitle($"{_content.AppName} Test")
                .SetContentText($"Notification system is working! Current time: {DateTime.Now:HH:mm:ss}")
                .SetSmallIcon(iconId)
                .SetContentIntent(pending)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic);

            int notificationId = intent.GetIntExtra(NotificationIntentConstants.ExtraNotificationId, 999);
            nm.Notify(notificationId, builder.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing test notification");
        }
    }
}
#endif
