namespace OneActivity.Core.Services;

public class SharedContentPl : ISharedContent
{
    public string CreateUserPrompt => "Jak mam się do Ciebie zwracać?";
    public string CreateUserNicknamePlaceholder => "Wpisz swój pseudonim (możesz go później zmienić)";
    public string NicknameEmptyError => "Pseudonim nie może być pusty.";
    public string NicknameTooLongError => "Pseudonim może mieć maksymalnie 15 znaków.";
    public string CreateUserFailed => "Nie udało się utworzyć użytkownika. Spróbuj ponownie.";
    public string CreateUserErrorPrefix => "Błąd tworzenia użytkownika:";

    public string EditNicknameTitle => "Edytuj pseudonim";
    public string NicknameLabel => "Pseudonim";
    public string NicknameUpdateSuccess => "Pseudonim zaktualizowany pomyślnie!";
    public string NicknameUpdateFailed => "Nie udało się zaktualizować pseudonimu. Spróbuj ponownie.";
    public string ErrorLoadingUserDataPrefix => "Błąd wczytywania danych użytkownika:";
    public string ErrorUpdatingNicknamePrefix => "Błąd aktualizacji pseudonimu:";

    public string NoUserProfileMessage => "Nie znaleziono profilu użytkownika. Utwórz profil na stronie głównej.";

    public string LanguageCardTitle => "Język";
    public string LanguageChooseLabel => "Wybierz język";
    public string CurrentPrefix => "Aktualny:";
    public string LanguageEnglishOption => "Angielski (EN)";
    public string LanguagePolishOption => "Polski (PL)";

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

