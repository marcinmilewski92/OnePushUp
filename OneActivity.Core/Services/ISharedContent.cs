namespace OneActivity.Core.Services;

public interface ISharedContent
{
    // Settings general
    string NoUserProfileMessage { get; }

    // Language card
    string LanguageCardTitle { get; }
    string LanguageChooseLabel { get; }
    string CurrentPrefix { get; }

    // Gender card
    string GenderCardTitle { get; }
    string GenderChooseLabel { get; }
    string GenderUnspecified { get; }
    string GenderMale { get; }
    string GenderFemale { get; }

    // Notification testing
    string NotificationTestingTitle { get; }
    string NotificationTestingDescription { get; }
    string NotificationTestingButtonIdle { get; }
    string NotificationTestingButtonTesting { get; }
    string LastNotificationScheduledLabel { get; }
    string NotificationTestSuccess(string time);
    string NotificationTestErrorPrefix { get; }
}

