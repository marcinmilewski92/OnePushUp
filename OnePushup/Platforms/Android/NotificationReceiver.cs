using Android.App;
using Android.Content;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    private const string LastFiredDateKey = "last_notification_fired_date"; // yyyy-MM-dd
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
            Logger.LogInformation("NotificationReceiver: Received intent with action: {Action}", action ?? "null");

            if (action == Intent.ActionBootCompleted ||
                action == Intent.ActionLockedBootCompleted ||
                action == "android.intent.action.QUICKBOOT_POWERON" ||
                action == "android.intent.action.MY_PACKAGE_REPLACED" ||
                action == NotificationIntentConstants.ActionRestoreNotifications)
            {
                Logger.LogInformation("NotificationReceiver: System boot or app updated, restoring notifications");
                AlarmScheduler?.RestoreNotificationsAfterReboot(context);
                return;
            }

            bool isTestNotification = intent.GetBooleanExtra(NotificationIntentConstants.ExtraTestNotification, false);
            if (isTestNotification)
            {
                Logger.LogInformation("NotificationReceiver: Handling test notification");
                NotificationDisplayer?.ShowTestNotification(context, intent);
                return;
            }

            if (action == NotificationIntentConstants.ActionDailyNotification ||
                action == "DAILY_NOTIFICATION_EXACT" ||
                action == "DAILY_NOTIFICATION_INEXACT" ||
                action == "DAILY_NOTIFICATION_REPEAT" ||
                action == "WINDOW_NOTIFICATION_ALARM" ||
                action == NotificationIntentConstants.ActionTestNotificationAlarm)
            {
                // Deduplicate: only show one real daily notification per local day
                var todayKey = DateTime.Now.ToString("yyyy-MM-dd");
                var lastFired = Microsoft.Maui.Storage.Preferences.Default.Get(LastFiredDateKey, string.Empty);

                bool isTest = intent.GetBooleanExtra(NotificationIntentConstants.ExtraTestNotification, false);
                if (!isTest && string.Equals(todayKey, lastFired, StringComparison.Ordinal))
                {
                    Logger.LogInformation("Skipping duplicate daily notification for today");
                }
                else
                {
                    NotificationDisplayer?.ShowActivityNotification(context, intent);
                    if (!isTest)
                    {
                        Microsoft.Maui.Storage.Preferences.Default.Set(LastFiredDateKey, todayKey);
                    }
                    AlarmScheduler?.RescheduleForTomorrow(context);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "NotificationReceiver: Error in OnReceive");
            try
            {
                AlarmScheduler?.RescheduleForTomorrow(context);
            }
            catch
            {
            }
        }
    }
}
