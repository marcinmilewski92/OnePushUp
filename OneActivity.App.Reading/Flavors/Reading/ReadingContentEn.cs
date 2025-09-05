using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Reading.Flavors.Reading;

public class ReadingContentEn : IActivityContent
{
    private User? _user;
    public void SetUser(User user) => _user = user;
    public string AppName => "OneBookPage";
    public string UnitSingular => "page";
    public string UnitPlural => "pages";
    public string Verb => "read";
    public int MinimalQuantity => 1;
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