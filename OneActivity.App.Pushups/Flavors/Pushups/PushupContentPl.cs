using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Pushups.Flavors.Pushups;

public class PushupContentPl : IActivityContent
{
    private readonly IGenderService _genderService;
    private User? _user;
    private string _userName = "Kolego";

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
    public string UnitSingular => "pompka";
    public string UnitPlural => "pompki";
    public string Verb => "zrobić";
    public int MinimalQuantity => 1;

    public string FormatQuantity(int quantity)
    {
        var unit = quantity == 1 ? UnitSingular : UnitPlural;
        return $"{quantity} {unit}";
    }

    public string DailyTitle => "Dzienne wyzwanie";
    private bool IsFemale => _genderService.Current == Gender.Female;
    private string DidVerbPast => IsFemale ? "zrobiłaś" : "zrobiłeś";
    private string RecordedPast => IsFemale ? "zapisałaś" : "zapisałeś";
    public string PromptToday => $"Czy {DidVerbPast} dziś jedną {UnitSingular}, {_user?.NickName}?";
    public string AlreadyCompletedTitle => $"Świetna robota, {_userName}! 💪";
    public string AlreadyCompletedMessage(int quantity)
    {
        if (quantity <= 0) return $"{_userName}, {RecordedPast} 0 pompek na dziś. Spokojnie, jutro też jest dzień!";
        if (quantity == 1) return $"{(IsFemale ? "Zrobiłaś" : "Zrobiłeś")} 1 {UnitSingular} dziś!";
        return $"{(IsFemale ? "Zrobiłaś" : "Zrobiłeś")} {quantity} pompek dziś!";
    }
    public string RecordedZeroTitle => "Wyzwanie zapisane";
    public string RecordedZeroMessage => $"{_userName}, {RecordedPast} 0 pompek na dziś. Spokojnie, jutro też jest dzień!";
    public string EditEntryTitle => $"Edytuj dzisiejszy wpis: {UnitPlural}";
    public string EditEntryPrompt => $"Zaktualizuj dzisiejszą liczbę: {UnitSingular}";
    public string UpdateSuccessMessage(bool isZero) => isZero
        ? $"Twój wpis został zaktualizowany na 0 {UnitPlural} na dziś."
        : $"Świetnie! Zaktualizowaliśmy Twój wpis dotyczący {UnitSingular}.";
    public string SaveSuccessMessage(bool isZero) => isZero
        ? $"Zapisaliśmy Twoją odpowiedź. Pamiętaj, nawet jedna {UnitSingular} jest lepsza niż żadna!"
        : $"Świetnie! Zapisaliśmy Twoją {UnitSingular}.";

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
    public string EditOptionYesLabel => IsFemale ? $"Tak, zrobiłam {FormatQuantity(MinimalQuantity)}" : $"Tak, zrobiłem {FormatQuantity(MinimalQuantity)}";
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
    public string StreakUnitsInCurrentStreakLabel => "Jednostki w bieżącej serii";
    public string StreakTotalUnitsOverallLabel => "Łącznie jednostek";
    public string StreakMsgLegendary(int days) => $"Niesamowite! {days} dni to legenda!";
    public string StreakMsgImpressive(int days) => $"Świetnie! {days} dni to imponujący wynik!";
    public string StreakMsgReached(int days) => $"Brawo! Masz już {days} dni!";
    public string StreakMsgBuilding => "Tak trzymaj! Twoja seria rośnie!";
    public string StreakStartToday => "Zacznij swoją serię już dziś!";

    // Language settings card
    public string LanguageCardTitle => "Język";
    public string LanguageChooseLabel => "Wybierz język";
    public string LanguageCurrentPrefix => "Aktualny:";
}
