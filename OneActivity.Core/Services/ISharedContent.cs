namespace OneActivity.Core.Services;

public interface ISharedContent
{
    // Create user
    string CreateUserPrompt { get; }
    string CreateUserNicknamePlaceholder { get; }
    string NicknameEmptyError { get; }
    string NicknameTooLongError { get; }
    string CreateUserFailed { get; }
    string CreateUserErrorPrefix { get; }

    // Edit nickname
    string EditNicknameTitle { get; }
    string NicknameLabel { get; }
    string NicknameUpdateSuccess { get; }
    string NicknameUpdateFailed { get; }
    string ErrorLoadingUserDataPrefix { get; }
    string ErrorUpdatingNicknamePrefix { get; }

    // Settings general
    string NoUserProfileMessage { get; }

    // Language card
    string LanguageCardTitle { get; }
    string LanguageChooseLabel { get; }
    string CurrentPrefix { get; }
    string LanguageEnglishOption { get; }
    string LanguagePolishOption { get; }

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

