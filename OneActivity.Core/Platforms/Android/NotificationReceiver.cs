#if ANDROID
using global::Android.App;
using global::Android.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OneActivity.Core.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter([
    Intent.ActionBootCompleted,
    Intent.ActionLockedBootCompleted,
    "android.intent.action.QUICKBOOT_POWERON",
    "android.intent.action.MY_PACKAGE_REPLACED"
])]
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

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;
        
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
                var todayKey = DateTime.Now.ToString("yyyy-MM-dd");
                var lastFired = Microsoft.Maui.Storage.Preferences.Default.Get(LastFiredDateKey, string.Empty);
                if (!isTestNotification && string.Equals(todayKey, lastFired, StringComparison.Ordinal))
                {
                    // Deduplicate only real daily notifications
                }
                else
                {
                    NotificationDisplayer?.ShowActivityNotification(context, intent);
                    if (!isTestNotification)
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
            try { AlarmScheduler?.RescheduleForTomorrow(context); } catch {}
        }
    }
}
#endif
