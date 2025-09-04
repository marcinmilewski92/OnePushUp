namespace OneActivity.Core.Services;

public class SharedContentEn : ISharedContent
{
    public string CreateUserPrompt => "How would you like me to call you?";
    public string CreateUserNicknamePlaceholder => "Enter your nickname (you can change it later)";
    public string NicknameEmptyError => "Nickname cannot be empty.";
    public string NicknameTooLongError => "Nickname must be at most 15 characters.";
    public string CreateUserFailed => "Failed to create user. Please try again.";
    public string CreateUserErrorPrefix => "Error creating user:";

    public string EditNicknameTitle => "Edit Nickname";
    public string NicknameLabel => "Nickname";
    public string NicknameUpdateSuccess => "Nickname updated successfully!";
    public string NicknameUpdateFailed => "Failed to update nickname. Please try again.";
    public string ErrorLoadingUserDataPrefix => "Error loading user data:";
    public string ErrorUpdatingNicknamePrefix => "Error updating nickname:";

    public string NoUserProfileMessage => "No user profile found. Please create a user profile from the home page first.";

    public string LanguageCardTitle => "Language";
    public string LanguageChooseLabel => "Choose your language";
    public string CurrentPrefix => "Current:";
    public string LanguageEnglishOption => "English (EN)";
    public string LanguagePolishOption => "Polish (PL)";

    public string GenderCardTitle => "Gender";
    public string GenderChooseLabel => "Choose your form";
    public string GenderUnspecified => "Unspecified";
    public string GenderMale => "Male";
    public string GenderFemale => "Female";

    // Notification reliability and diagnostics
    public string NotificationReliabilityInfo => "For best reliability on some devices (e.g., Xiaomi, Huawei, Oppo), enable Autostart / allow background activity, grant Exact Alarms, and exclude the app from battery optimizations. Use the buttons below to open the right settings screens.";
    public string NotificationDiagnosticsTitle => "Notification Diagnostics";
    public string ExactAlarmsAllowedLabel => "Exact alarms allowed:";
    public string OpenExactAlarmSettingsButton => "Open exact alarm settings";
    public string IgnoringBatteryOptimizationsLabel => "Ignoring battery optimizations:";
    public string OpenBatteryOptimizationSettingsButton => "Open battery optimization settings";
}
