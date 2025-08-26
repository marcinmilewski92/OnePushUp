namespace OnePushUp;

public static class NotificationConstants
{
    // Channel identifiers
    public const string ChannelId = "pushup_reminders";
    public const string ChannelName = "Pushup Reminders";
    public const string ChannelDescription = "Daily reminders to do your pushups";

    // Preference keys
    public const string NotificationsEnabledKey = "notifications_enabled";
    public const string NotificationTimeKey = "notification_time";
    public const string LastNotificationScheduledKey = "last_notification_scheduled";
    public const string NotificationTargetTimeKey = "notification_target_time";

    // Intent actions
    public const string ActionDailyNotification = "com.onepushup.DAILY_NOTIFICATION";
    public const string ActionRestoreNotifications = "RESTORE_NOTIFICATIONS";
    public const string ActionDailyNotificationExact = "DAILY_NOTIFICATION_EXACT";
    public const string ActionDailyNotificationInexact = "DAILY_NOTIFICATION_INEXACT";
    public const string ActionDailyNotificationRepeat = "DAILY_NOTIFICATION_REPEAT";
    public const string ActionWindowNotificationAlarm = "WINDOW_NOTIFICATION_ALARM";
    public const string ActionTestNotification = "TEST_NOTIFICATION_ALARM";

    // Request codes
    public const int RequestCodeMainActivity = 0;
    public const int RequestCodeExact = 1;
    public const int RequestCodeInexact = 2;
    public const int RequestCodeRepeating = 3;
    public const int RequestCodeWindowMinus2 = 101;
    public const int RequestCodeWindowMinus1 = 102;
    public const int RequestCodeWindowPlus1 = 103;
    public const int RequestCodeWindowPlus2 = 104;
    public const int RequestCodeDirectNotification = 500;
    public const int RequestCodeTestNotification = 999;
}
