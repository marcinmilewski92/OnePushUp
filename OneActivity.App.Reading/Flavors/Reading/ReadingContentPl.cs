using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Reading.Flavors.Reading;

public class ReadingContentPl(IGenderService gender) : IActivityContent
{
    private readonly IGenderService _gender = gender;
    private User? _user;

    public void SetUser(User user) => _user = user;
    public string AppName => "OneBookPage";
    public string UnitSingular => "strona"; // mianownik
    public string UnitPlural => "strony";   // mianownik (2–4)
    public string Verb => "przeczytać";
    public int MinimalQuantity => 1;
    private static string PluralizeStrona(int n) => n == 1 ? "strona" : (n % 10 is >= 2 and <= 4 && (n % 100 < 10 || n % 100 >= 20) ? "strony" : "stron");

    public string FormatQuantity(int quantity) => $"{quantity} {PluralizeStrona(quantity)}";
    private bool IsFemale => _gender.Current == Gender.Female;
    private string ReadPast => IsFemale ? "przeczytałaś" : "przeczytałeś";
    private string RecordedPast => IsFemale ? "zapisałaś" : "zapisałeś";
    public string DailyTitle => "Dzienne czytanie";
    public string PromptToday => $"Czy {ReadPast} dziś jedną stronę?";
    public string AlreadyCompletedTitle => "Świetna robota!";
    public string AlreadyCompletedMessage(int quantity) => quantity <= 0
        ? $"{RecordedPast} dziś 0 stron. Spokojnie, jutro też jest dzień!"
        : $"{(IsFemale ? "Przeczytałaś" : "Przeczytałeś")} dziś {FormatQuantityAcc(quantity)}!";
    public string RecordedZeroTitle => "Wpis zapisany";
    public string RecordedZeroMessage => $"{RecordedPast} 0 stron na dziś. Spokojnie, jutro też jest dzień!";
    public string EditEntryTitle => "Edytuj dzisiejszy wpis czytania";
    public string EditEntryPrompt => "Zaktualizuj dzisiejszą liczbę stron";
    public string UpdateSuccessMessage(bool isZero) => isZero
        ? "Twój dzisiejszy wpis został zaktualizowany na 0 stron."
        : "Świetnie! Zaktualizowaliśmy Twój dzisiejszy wynik czytania.";
    public string SaveSuccessMessage(bool isZero) => isZero
        ? "Zapisaliśmy Twoją odpowiedź. Nawet kilka stron buduje nawyk!"
        : "Super! Zapisaliśmy Twoje czytanie.";

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
    public string YesMoreText => IsFemale ? "Tak, przeczytałam więcej" : "Tak, przeczytałem więcej";
    public string NoText => "Nie";
    public string EditOptionYesLabel => IsFemale ? "Tak, przeczytałam jedną stronę" : "Tak, przeczytałem jedną stronę";
    public string EditOptionYesMoreLabel => IsFemale ? "Tak, przeczytałam więcej" : "Tak, przeczytałem więcej";
    public string EditOptionNoLabel => "Nie";
    public string HowManyLabel => IsFemale ? "Ile stron przeczytałaś?" : "Ile stron przeczytałeś?";
    public string GetPleaseEnterValidNumberText(int min) => $"Wpisz poprawną liczbę (minimum {min})";

    // Errors
    public string ErrorCheckingStatus => "Błąd sprawdzania stanu na dziś.";
    public string ErrorEntryNotFound => "Błąd: Nie znaleziono dzisiejszego wpisu.";
    public string ErrorUpdatingRecord => "Błąd aktualizacji Twojego rekordu czytania.";
    public string ErrorSavingRecord => "Błąd zapisywania Twojego rekordu czytania.";

    // Streak labels/messages
    public string StreakDayStreakLabel => "Seria dni";
    public string StreakUnitsInCurrentStreakLabel => "Strony w bieżącej serii";
    public string StreakTotalUnitsOverallLabel => "Łącznie stron";
    public string StreakMsgLegendary(int days) => $"Niesamowicie! {days} dni – to już legenda!";
    public string StreakMsgImpressive(int days) => $"Świetna robota! {days} dni – imponujący wynik!";
    public string StreakMsgReached(int days) => $"Brawo! Masz już {days} dni!";
    public string StreakMsgBuilding => "Tak trzymaj! Twoja seria rośnie!";
    public string StreakStartToday => "Zacznij swoją serię już dziś!";

    // Pomocnicze: biernik w zdaniach (1 stronę, 2–4 strony, 5+ stron)
    private static string FormatQuantityAcc(int quantity)
    {
        string form = quantity == 1
            ? "stronę"
            : (quantity % 10 is >= 2 and <= 4 && (quantity % 100 < 10 || quantity % 100 >= 20) ? "strony" : "stron");
        return $"{quantity} {form}";
    }

    // Language settings card
    public string LanguageCardTitle => "Język";
    public string LanguageChooseLabel => "Wybierz język";
    public string LanguageCurrentPrefix => "Aktualny:";
}
