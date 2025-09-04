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

    // Notification reliability and diagnostics
    string NotificationReliabilityInfo { get; }
    string NotificationDiagnosticsTitle { get; }
    string ExactAlarmsAllowedLabel { get; }
    string OpenExactAlarmSettingsButton { get; }
    string IgnoringBatteryOptimizationsLabel { get; }
    string OpenBatteryOptimizationSettingsButton { get; }
}
