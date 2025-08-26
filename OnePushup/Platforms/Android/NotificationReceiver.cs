using Android.App;
using Android.Content;
using Microsoft.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
    private ILogger<NotificationReceiver> Logger => _logger ??=
        MauiApplication.Current?.Services?.GetService<ILogger<NotificationReceiver>>()
        ?? NullLogger<NotificationReceiver>.Instance;
    private ILogger<NotificationReceiver>? _logger;

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
                action == AlarmScheduler.ACTION_RESTORE_NOTIFICATIONS)
            {
                AlarmScheduler.RestoreNotificationsAfterReboot(context);
                return;
            }

            if (intent.GetBooleanExtra("test_notification", false))
            {
                NotificationBuilder.ShowTestNotification(context, intent);
                return;
            }

            if (action == AlarmScheduler.ACTION_DAILY_NOTIFICATION ||
                action == "DAILY_NOTIFICATION_EXACT" ||
                action == "DAILY_NOTIFICATION_INEXACT" ||
                action == "DAILY_NOTIFICATION_REPEAT" ||
                action == "WINDOW_NOTIFICATION_ALARM" ||
                action == "TEST_NOTIFICATION_ALARM")
            {
                NotificationBuilder.ShowPushupNotification(context, intent);
                AlarmScheduler.RescheduleForTomorrow(context);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "NotificationReceiver: Error in OnReceive");
            try
            {
                AlarmScheduler.RescheduleForTomorrow(context);
            }
            catch
            {
            }
        }
    }
}

