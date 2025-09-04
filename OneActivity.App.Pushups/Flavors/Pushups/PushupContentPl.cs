using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Pushups.Flavors.Pushups;

public class PushupContentPl : IActivityContent
{
    private readonly IGenderService _genderService;
    private User? _user;
    private string _userName = "Przyjacielu";

    public PushupContentPl(IGenderService genderService)
    {
        _genderService = genderService;
    }

    public void SetUser(User user)
    {
        if (_user != null) return;
        _user = user;
        _userName = user.NickName;
    }

    public string AppName => "OnePushUp";
    public string UnitSingular => "pompka"; // mianownik lp.
    public string UnitPlural => "pompki";   // mianownik l.mn. (dla 2–4)
    public string Verb => "zrobić";
    public int MinimalQuantity => 1;

    // Mianownik (do samodzielnego wyświetlania liczby): 1 pompka, 2/3/4 pompki, 5+ pompek
    public string FormatQuantity(int quantity)
    {
        string form = quantity == 1
            ? "pompka"
            : (quantity % 10 is >= 2 and <= 4 && (quantity % 100 < 10 || quantity % 100 >= 20) ? "pompki" : "pompek");
        return $"{quantity} {form}";
    }

    // Biernik w zdaniach typu "zrobiłem X ...": 1 pompkę, 2–4 pompki, 5+ pompek
    private static string FormatQuantityAcc(int quantity)
    {
        string form = quantity == 1
            ? "pompkę"
            : (quantity % 10 is >= 2 and <= 4 && (quantity % 100 < 10 || quantity % 100 >= 20) ? "pompki" : "pompek");
        return $"{quantity} {form}";
    }

    public string DailyTitle => "Dzienne wyzwanie";
    private bool IsFemale => _genderService.Current == Gender.Female;
    private string DidVerbPast => IsFemale ? "zrobiłaś" : "zrobiłeś";
    private string RecordedPast => IsFemale ? "zapisałaś" : "zapisałeś";
    public string PromptToday => $"Czy {DidVerbPast} dziś jedną pompkę, {_user?.NickName}?";
    public string AlreadyCompletedTitle => $"Świetna robota, {_userName}! 💪";
    public string AlreadyCompletedMessage(int quantity)
    {
        if (quantity <= 0)
            return $"{_userName}, {RecordedPast} 0 pompek na dziś. Spokojnie, jutro też jest dzień!";
        var prefix = IsFemale ? "Zrobiłaś" : "Zrobiłeś";
        return quantity == 1
            ? $"{prefix} 1 pompkę dziś!"
            : $"{prefix} {FormatQuantityAcc(quantity)} dziś!";
    }
    public string RecordedZeroTitle => "Wyzwanie zapisane";
    public string RecordedZeroMessage => $"{_userName}, {RecordedPast} 0 pompek na dziś. Spokojnie, jutro też jest dzień!";
    public string EditEntryTitle => $"Edytuj dzisiejszy wpis: pompki";
    public string EditEntryPrompt => $"Zaktualizuj dzisiejszą liczbę pompek";
    public string UpdateSuccessMessage(bool isZero) => isZero
        ? $"Twój dzisiejszy wpis został zaktualizowany na 0 pompek."
        : $"Świetnie! Zaktualizowaliśmy Twój dzisiejszy wynik.";
    public string SaveSuccessMessage(bool isZero) => isZero
        ? $"Zapisaliśmy Twoją odpowiedź. Pamiętaj – liczy się regularność. Jutro spróbuj znów!"
        : $"Świetnie! Zapisaliśmy Twój wynik.";

    // Navigation + layout
    public string NavDailyGoal => "Dzienne wyzwanie";
    public string NavStats => "Statystyki";
    public string NavSettings => "Ustawienia";
    public string CurrentTimeLabel => "Aktualny czas:";

    // Common UI text
    public string LoadingText => "Ładowanie...";
    public string SaveText => "Zapisz";
    public string UpdateText => "Zaktualizuj";
    public string CancelText => "Anuluj";
    public string SavingText => "Zapisywanie...";

    // Daily activity options/labels
    public string EditButtonText => "Edytuj dzisiejszy wpis";
    public string YesText => "Tak";
    public string YesMoreText => IsFemale ? "Tak, zrobiłam więcej" : "Tak, zrobiłem więcej";
    public string NoText => "Nie";
    public string EditOptionYesLabel => IsFemale ? "Tak, zrobiłam jedną pompkę" : "Tak, zrobiłem jedną pompkę";
    public string EditOptionYesMoreLabel => IsFemale ? "Tak, zrobiłam więcej" : "Tak, zrobiłem więcej";
    public string EditOptionNoLabel => "Nie";
    public string HowManyLabel => IsFemale ? "Ile pompek zrobiłaś?" : "Ile pompek zrobiłeś?";
    public string GetPleaseEnterValidNumberText(int min) => $"Wpisz poprawną liczbę (minimum {min})";

    // Errors
    public string ErrorCheckingStatus => "Błąd sprawdzania stanu na dziś.";
    public string ErrorEntryNotFound => "Błąd: Nie znaleziono dzisiejszego wpisu.";
    public string ErrorUpdatingRecord => "Błąd aktualizacji Twojego rekordu.";
    public string ErrorSavingRecord => "Błąd zapisywania Twojego rekordu.";

    // Streak labels/messages
    public string StreakDayStreakLabel => "Seria dni";
    public string StreakUnitsInCurrentStreakLabel => "Pompki w bieżącej serii";
    public string StreakTotalUnitsOverallLabel => "Łącznie pompek";
    public string StreakMsgLegendary(int days) => $"Niesamowicie! {days} dni – to już legenda!";
    public string StreakMsgImpressive(int days) => $"Świetna robota! {days} dni – imponujący wynik!";
    public string StreakMsgReached(int days) => $"Brawo! Masz już {days} dni!";
    public string StreakMsgBuilding => "Tak trzymaj! Twoja seria rośnie!";
    public string StreakStartToday => "Zacznij swoją serię już dziś!";

    // Language settings card
    public string LanguageCardTitle => "Język";
    public string LanguageChooseLabel => "Wybierz język";
    public string LanguageCurrentPrefix => "Aktualny:";
}
