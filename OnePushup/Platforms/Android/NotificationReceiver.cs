using Android.App;
using Android.Content;
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
    public override void OnReceive(Context context, Intent intent)
    {
        try
        {
            var action = intent.Action;
            Console.WriteLine($"NotificationReceiver: Received intent with action: {action ?? "null"}");

            if (action == Intent.ActionBootCompleted ||
                action == Intent.ActionLockedBootCompleted ||
                action == "android.intent.action.QUICKBOOT_POWERON" ||
                action == "android.intent.action.MY_PACKAGE_REPLACED" ||
                action == AlarmScheduler.ACTION_RESTORE_NOTIFICATIONS)
            {
                Console.WriteLine("NotificationReceiver: System boot or app updated, restoring notifications");
                AlarmScheduler.RestoreNotificationsAfterReboot(context);
                return;
            }

            bool isTestNotification = intent.GetBooleanExtra("test_notification", false);
            if (isTestNotification)
            {
                Console.WriteLine("NotificationReceiver: Handling test notification");
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
            Console.WriteLine($"NotificationReceiver: Error in OnReceive: {ex.Message}");
            Console.WriteLine($"NotificationReceiver: Stack trace: {ex.StackTrace}");

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

