using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using OnePushUp.Services;

namespace OneActivity.App.Pushups.Platforms.Android;

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
        Show(context, $"{_content.AppName} Reminder", $"Time to {_content.Verb} your daily {_content.UnitSingular}!");
    }

    public void ShowTestNotification(Context context, Intent intent)
    {
        Show(context, $"{_content.AppName} Test", $"This is a test notification - {DateTime.Now:HH:mm:ss}");
    }

    private void Show(Context context, string title, string text)
    {
        try
        {
            var notificationManager = NotificationManager.FromContext(context);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, $"{_content.AppName} Reminders", NotificationImportance.High)
                {
                    Description = $"Daily reminders to {_content.Verb} your {_content.UnitPlural}",
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channel);
            }

            var intent = new Intent(context, Java.Lang.Class.FromType(typeof(OneActivity.App.Pushups.MainActivity)));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            int iconId = global::Android.Resource.Drawable.IcDialogInfo;

            var builder = new NotificationCompat.Builder(context, ChannelId)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(iconId)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetVisibility(NotificationCompat.VisibilityPublic);

            notificationManager.Notify(new Random().Next(1000, 9999), builder.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing notification");
        }
    }
}

