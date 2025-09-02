namespace OneActivity.Core.Services;

public class SharedContentEn : ISharedContent
{
    public string NoUserProfileMessage => "No user profile found. Please create a user profile from the home page first.";

    public string LanguageCardTitle => "Language";
    public string LanguageChooseLabel => "Choose your language";
    public string CurrentPrefix => "Current:";

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

