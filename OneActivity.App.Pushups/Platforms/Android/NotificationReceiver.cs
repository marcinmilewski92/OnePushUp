using Android.App;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OneActivity.App.Pushups.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] {
    Intent.ActionBootCompleted,
    Intent.ActionLockedBootCompleted,
    "android.intent.action.QUICKBOOT_POWERON",
    "android.intent.action.MY_PACKAGE_REPLACED"
})]
public class NotificationReceiver : BroadcastReceiver
{
    private ILogger<NotificationReceiver> Logger => _logger ??=
        MauiApplication.Current?.Services?.GetService<ILogger<NotificationReceiver>>()
        ?? NullLogger<NotificationReceiver>.Instance;
    private ILogger<NotificationReceiver>? _logger;

    private IAlarmScheduler? AlarmScheduler => _alarmScheduler ??=
        MauiApplication.Current?.Services?.GetService<IAlarmScheduler>();
    private IAlarmScheduler? _alarmScheduler;

    private INotificationDisplayer? NotificationDisplayer => _displayer ??=
        MauiApplication.Current?.Services?.GetService<INotificationDisplayer>();
    private INotificationDisplayer? _displayer;

    public override void OnReceive(Context context, Intent intent)
    {
        try
        {
            var action = intent.Action;
            if (action == Intent.ActionBootCompleted ||
                action == Intent.ActionLockedBootCompleted ||
                action == "android.intent.action.QUICKBOOT_POWERON" ||
                action == "android.intent.action.MY_PACKAGE_REPLACED" ||
                action == NotificationIntentConstants.ActionRestoreNotifications)
            {
                AlarmScheduler?.RestoreNotificationsAfterReboot(context);
                return;
            }

            bool isTestNotification = intent.GetBooleanExtra(NotificationIntentConstants.ExtraTestNotification, false);
            if (isTestNotification)
            {
                NotificationDisplayer?.ShowTestNotification(context, intent);
                return;
            }

            if (action == NotificationIntentConstants.ActionDailyNotification ||
                action == NotificationIntentConstants.ActionTestNotificationAlarm)
            {
                NotificationDisplayer?.ShowActivityNotification(context, intent);
                AlarmScheduler?.RescheduleForTomorrow(context);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "NotificationReceiver: Error in OnReceive");
            try { AlarmScheduler?.RescheduleForTomorrow(context); } catch {}
        }
    }
}

