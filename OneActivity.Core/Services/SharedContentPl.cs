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

    // Niezawodność i diagnostyka powiadomień
    public string NotificationReliabilityInfo => "Aby uzyskać najlepszą niezawodność na niektórych urządzeniach (np. Xiaomi, Huawei, Oppo), włącz Autostart / zezwól na działanie w tle, przyznaj uprawnienie Dokładne alarmy i wyłącz optymalizacje baterii dla aplikacji. Skorzystaj z przycisków poniżej, aby otworzyć odpowiednie ustawienia.";
    public string NotificationDiagnosticsTitle => "Diagnostyka powiadomień";
    public string ExactAlarmsAllowedLabel => "Dokładne alarmy dozwolone:";
    public string OpenExactAlarmSettingsButton => "Otwórz ustawienia dokładnych alarmów";
    public string IgnoringBatteryOptimizationsLabel => "Ignorowanie optymalizacji baterii:";
    public string OpenBatteryOptimizationSettingsButton => "Otwórz ustawienia optymalizacji baterii";

    // Ustawienia powiadomień
    public string NotificationsCardTitle => "Powiadomienia push";
    public string NotificationsLoadingText => "Wczytywanie ustawień powiadomień...";
    public string NotificationsEnableLabel => "Włącz codzienne przypomnienia";
    public string NotificationsTimeLabel => "Godzina przypomnienia";
    public string NotificationsTimeHelp => "Użyj formatu 24‑godzinnego HH:mm (np. 08:00). Otrzymasz powiadomienie o tej porze każdego dnia.";
    public string NotificationsEnabledSuccess => "Powiadomienia włączone!";
    public string NotificationsDisabledSuccess => "Powiadomienia wyłączone!";
    public string InvalidTimeFormat => "Nieprawidłowy format godziny. Użyj HH:mm (np. 08:00).";
    public string ErrorLoadingNotificationsPrefix => "Błąd wczytywania ustawień powiadomień:";
    public string ErrorUpdatingNotificationsPrefix => "Błąd aktualizacji ustawień powiadomień:";
    public string ErrorUpdatingNotificationTimePrefix => "Błąd aktualizacji godziny powiadomienia:";
    public string NotificationTimeUpdated(string timeText) => $"Zmieniono godzinę powiadomienia na {timeText}!";
}
