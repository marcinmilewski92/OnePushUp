#if ANDROID
namespace OneActivity.Core.Platforms.Android;

public static class NotificationIntentConstants
{
    public const string ActionDailyNotification = "DAILY_NOTIFICATION";
    public const string ActionRestoreNotifications = "RESTORE_NOTIFICATIONS";
    public const string ActionTestNotificationAlarm = "TEST_NOTIFICATION_ALARM";

    public const string ExtraNotificationId = "notification_id";
    public const string ExtraNotificationTime = "notification_time";
    public const string ExtraApproach = "approach";
    public const string ExtraTestNotification = "test_notification";
    public const string ExtraWindowAlarm = "window_alarm";
    public const string ExtraMinuteOffset = "minute_offset";
}
#endif

