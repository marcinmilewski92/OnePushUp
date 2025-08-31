using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Reading.Flavors.Reading;

// EN
public class ReadingContentEn : IActivityContent
{
    private User? _user;
    public void SetUser(User user) => _user = user;
    public string AppName => "OneBookPage";
    public string UnitSingular => "page";
    public string UnitPlural => "pages";
    public string Verb => "read";
    public int MinimalQuantity => 3;
    public string FormatQuantity(int quantity) => $"{quantity} {(quantity == 1 ? UnitSingular : UnitPlural)}";
    public string DailyTitle => "Daily Reading";
    public string PromptToday => "Did you read your 3 pages today?";
    public string AlreadyCompletedTitle => "Nice work!";
    public string AlreadyCompletedMessage(int quantity) => quantity <= 0
        ? "You've recorded 0 pages for today. There's always tomorrow!"
        : $"You read {FormatQuantity(quantity)} today!";
    public string RecordedZeroTitle => "Entry recorded";
    public string RecordedZeroMessage => "You've recorded 0 pages for today. There's always tomorrow!";
    public string EditEntryTitle => "Edit Today's Reading Entry";
    public string EditEntryPrompt => "Update your pages for today";
    public string UpdateSuccessMessage(bool isZero) => isZero
        ? "Your record has been updated to 0 pages for today."
        : "Great job! Your reading record has been updated.";
    public string SaveSuccessMessage(bool isZero) => isZero
        ? "We've recorded your response. Even a few pages help build the habit!"
        : "Nice! Your reading has been recorded.";

    // Navigation + layout
    public string NavDailyGoal => "Daily Goal";
    public string NavStats => "Stats";
    public string NavSettings => "Settings";
    public string CurrentTimeLabel => "Current Time:";

    // Common UI text
    public string LoadingText => "Loading...";
    public string SaveText => "Save";
    public string UpdateText => "Update";
    public string CancelText => "Cancel";
    public string SavingText => "Saving...";

    // Daily activity options/labels
    public string EditButtonText => "Edit Today's Entry";
    public string YesText => "Yes";
    public string YesMoreText => "Yes, I did even more";
    public string NoText => "No";
    public string EditOptionYesLabel => $"Yes, I did {FormatQuantity(MinimalQuantity)}";
    public string EditOptionYesMoreLabel => "Yes, I did more than that";
    public string EditOptionNoLabel => $"No, I didn't {Verb} any";
    public string HowManyLabel => $"How many {UnitPlural} did you {Verb}?";
    public string GetPleaseEnterValidNumberText(int min) => $"Please enter a valid number (minimum {min})";

    // Errors
    public string ErrorCheckingStatus => "Error checking your status for today.";
    public string ErrorEntryNotFound => "Error: Could not find today's entry.";
    public string ErrorUpdatingRecord => "Error updating your reading record.";
    public string ErrorSavingRecord => "Error saving your reading record.";

    // Streak labels/messages
    public string StreakDayStreakLabel => "Day Streak";
    public string StreakUnitsInCurrentStreakLabel => "Units in Current Streak";
    public string StreakTotalUnitsOverallLabel => "Total Units Overall";
    public string StreakMsgLegendary(int days) => $"Incredible! {days} days is legendary!";
    public string StreakMsgImpressive(int days) => $"Amazing! {days} days is impressive!";
    public string StreakMsgReached(int days) => $"Great job! You've reached {days} days!";
    public string StreakMsgBuilding => "Keep going! Your streak is building!";
    public string StreakStartToday => "Start your streak today!";

    // Language settings card
    public string LanguageCardTitle => "Language";
    public string LanguageChooseLabel => "Choose your language";
    public string LanguageCurrentPrefix => "Current:";
}

// PL
public class ReadingContentPl : IActivityContent
{
    private readonly IGenderService _gender;
    private User? _user;
    public ReadingContentPl(IGenderService gender) => _gender = gender;
    public void SetUser(User user) => _user = user;
    public string AppName => "OneBookPage";
    public string UnitSingular => "strona";
    public string UnitPlural => "strony"; // used for 2-4
    public string Verb => "przeczytać";
    public int MinimalQuantity => 3;
    private static string PluralizeStrony(int n) => n == 1 ? "strona" : (n % 10 is >= 2 and <= 4 && (n % 100 < 10 || n % 100 >= 20) ? "strony" : "stron");
    public string FormatQuantity(int quantity) => $"{quantity} {PluralizeStrony(quantity)}";
    private bool IsFemale => _gender.Current == Gender.Female;
    private string ReadPast => IsFemale ? "przeczytałaś" : "przeczytałeś";
    private string RecordedPast => IsFemale ? "zapisałaś" : "zapisałeś";
    public string DailyTitle => "Dzienne czytanie";
    public string PromptToday => $"Czy {ReadPast} dziś swoje {MinimalQuantity} strony?";
    public string AlreadyCompletedTitle => "Super!";
    public string AlreadyCompletedMessage(int quantity) => quantity <= 0
        ? $"{RecordedPast} 0 stron na dziś. Spokojnie, jutro też jest dzień!"
        : $"{(IsFemale ? "Przeczytałaś" : "Przeczytałeś")} {FormatQuantity(quantity)} dziś!";
    public string RecordedZeroTitle => "Wpis zapisany";
    public string RecordedZeroMessage => $"{RecordedPast} 0 stron na dziś. Spokojnie, jutro też jest dzień!";
    public string EditEntryTitle => "Edytuj dzisiejszy wpis czytania";
    public string EditEntryPrompt => "Zaktualizuj liczbę stron na dziś";
    public string UpdateSuccessMessage(bool isZero) => isZero
        ? "Twój wpis został zaktualizowany na 0 stron na dziś."
        : "Świetnie! Zaktualizowaliśmy Twój wpis dotyczący czytania.";
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
    public string EditOptionYesLabel => IsFemale ? $"Tak, przeczytałam {FormatQuantity(MinimalQuantity)}" : $"Tak, przeczytałem {FormatQuantity(MinimalQuantity)}";
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

// Composite
public class ReadingContentLocalized : IActivityContent
{
    private readonly ILanguageService _lang;
    private readonly ReadingContentEn _en;
    private readonly ReadingContentPl _pl;
    public ReadingContentLocalized(ILanguageService lang, ReadingContentEn en, ReadingContentPl pl)
    { _lang = lang; _en = en; _pl = pl; }
    private IActivityContent Current => _lang.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant() == "pl" ? _pl : _en;
    public void SetUser(User user)
    { _en.SetUser(user); _pl.SetUser(user); }
    public string AppName => Current.AppName;
    public string UnitSingular => Current.UnitSingular;
    public string UnitPlural => Current.UnitPlural;
    public string Verb => Current.Verb;
    public int MinimalQuantity => Current.MinimalQuantity;
    public string FormatQuantity(int quantity) => Current.FormatQuantity(quantity);
    public string DailyTitle => Current.DailyTitle;
    public string PromptToday => Current.PromptToday;
    public string AlreadyCompletedTitle => Current.AlreadyCompletedTitle;
    public string AlreadyCompletedMessage(int quantity) => Current.AlreadyCompletedMessage(quantity);
    public string RecordedZeroTitle => Current.RecordedZeroTitle;
    public string RecordedZeroMessage => Current.RecordedZeroMessage;
    public string EditEntryTitle => Current.EditEntryTitle;
    public string EditEntryPrompt => Current.EditEntryPrompt;
    public string UpdateSuccessMessage(bool isZero) => Current.UpdateSuccessMessage(isZero);
    public string SaveSuccessMessage(bool isZero) => Current.SaveSuccessMessage(isZero);
    public string NavDailyGoal => Current.NavDailyGoal;
    public string NavStats => Current.NavStats;
    public string NavSettings => Current.NavSettings;
    public string CurrentTimeLabel => Current.CurrentTimeLabel;
    public string LoadingText => Current.LoadingText;
    public string SaveText => Current.SaveText;
    public string UpdateText => Current.UpdateText;
    public string CancelText => Current.CancelText;
    public string SavingText => Current.SavingText;
    public string EditButtonText => Current.EditButtonText;
    public string YesText => Current.YesText;
    public string YesMoreText => Current.YesMoreText;
    public string NoText => Current.NoText;
    public string EditOptionYesLabel => Current.EditOptionYesLabel;
    public string EditOptionYesMoreLabel => Current.EditOptionYesMoreLabel;
    public string EditOptionNoLabel => Current.EditOptionNoLabel;
    public string HowManyLabel => Current.HowManyLabel;
    public string GetPleaseEnterValidNumberText(int min) => Current.GetPleaseEnterValidNumberText(min);
    public string ErrorCheckingStatus => Current.ErrorCheckingStatus;
    public string ErrorEntryNotFound => Current.ErrorEntryNotFound;
    public string ErrorUpdatingRecord => Current.ErrorUpdatingRecord;
    public string ErrorSavingRecord => Current.ErrorSavingRecord;
    public string StreakDayStreakLabel => Current.StreakDayStreakLabel;
    public string StreakUnitsInCurrentStreakLabel => Current.StreakUnitsInCurrentStreakLabel;
    public string StreakTotalUnitsOverallLabel => Current.StreakTotalUnitsOverallLabel;
    public string StreakMsgLegendary(int days) => Current.StreakMsgLegendary(days);
    public string StreakMsgImpressive(int days) => Current.StreakMsgImpressive(days);
    public string StreakMsgReached(int days) => Current.StreakMsgReached(days);
    public string StreakMsgBuilding => Current.StreakMsgBuilding;
    public string StreakStartToday => Current.StreakStartToday;
    public string LanguageCardTitle => Current.LanguageCardTitle;
    public string LanguageChooseLabel => Current.LanguageChooseLabel;
    public string LanguageCurrentPrefix => Current.LanguageCurrentPrefix;
}
