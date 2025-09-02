namespace OneActivity.Core.Services;

public class SharedContentPl : ISharedContent
{
    public string NoUserProfileMessage => "Nie znaleziono profilu użytkownika. Utwórz profil na stronie głównej.";

    public string LanguageCardTitle => "Język";
    public string LanguageChooseLabel => "Wybierz język";
    public string CurrentPrefix => "Aktualny:";

    public string GenderCardTitle => "Forma gramatyczna";
    public string GenderChooseLabel => "Wybierz swoją formę";
    public string GenderUnspecified => "Nieokreślona";
    public string GenderMale => "Męska";
    public string GenderFemale => "Żeńska";

    public string NotificationTestingTitle => "Test powiadomień";
    public string NotificationTestingDescription => "Wyślij powiadomienie testowe, aby sprawdzić działanie powiadomień na urządzeniu.";
    public string NotificationTestingButtonIdle => "Wyślij powiadomienie testowe";
    public string NotificationTestingButtonTesting => "Testowanie...";
    public string LastNotificationScheduledLabel => "Ostatnie zaplanowane powiadomienie:";
    public string NotificationTestSuccess(string time) => $"Powiadomienie testowe wysłane pomyślnie o {time}";
    public string NotificationTestErrorPrefix => "Błąd wysyłania powiadomienia testowego:";
}

