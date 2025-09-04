#if ANDROID
namespace OneActivity.Core.Platforms.Android;

public static class NotificationIntentConstants
{
    public const string ActionDailyNotification = "com.oneactivity.DAILY_NOTIFICATION";
    public const string ActionTestNotificationAlarm = "com.oneactivity.TEST_NOTIFICATION_ALARM";
    public const string ActionRestoreNotifications = "com.oneactivity.RESTORE_NOTIFICATIONS";
    
    public const string ExtraNotificationId = "notification_id";
    public const string ExtraNotificationTime = "notification_time";
    public const string ExtraApproach = "approach";
    public const string ExtraTestNotification = "test_notification";
}
#endif
