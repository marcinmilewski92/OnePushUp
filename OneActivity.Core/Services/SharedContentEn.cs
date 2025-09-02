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

    public string NotificationTestingTitle => "Notification Testing";
    public string NotificationTestingDescription => "Send a test notification to verify that notifications are working on your device.";
    public string NotificationTestingButtonIdle => "Send Test Notification";
    public string NotificationTestingButtonTesting => "Testing...";
    public string LastNotificationScheduledLabel => "Last notification scheduled:";
    public string NotificationTestSuccess(string time) => $"Test notification sent successfully at {time}";
    public string NotificationTestErrorPrefix => "Error sending test notification:";
}

