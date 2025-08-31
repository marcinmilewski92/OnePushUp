using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Pushups.Flavors.Pushups;

public class PushupContentEn : IActivityContent
{
    private User? _user;
    private string _userName = "Mate";

    public void SetUser(User user)
    {
        if (_user != null) return;
        _user = user;
        _userName = user.NickName;
    }

    public string AppName => "OnePushUp";
    public string UnitSingular => "push-up";
    public string UnitPlural => "push-ups";
    public string Verb => "do";
    public int MinimalQuantity => 1;

    public string FormatQuantity(int quantity)
    {
        var unit = quantity == 1 ? UnitSingular : UnitPlural;
        return $"{quantity} {unit}";
    }

    public string DailyTitle => "Daily Challenge";
    public string PromptToday => $"Did you do your one {UnitSingular} today, {_user?.NickName}?";
    public string AlreadyCompletedTitle => $"Great job, {_userName}! 💪";
    public string AlreadyCompletedMessage(int quantity)
    {
        if (quantity <= 0) return $"{_userName}, you've recorded 0 {UnitPlural} for today. Don't worry, there's always tomorrow!";
        if (quantity == 1) return $"You did 1 {UnitSingular} today!";
        return $"You did {quantity} {UnitPlural} today!";
    }
    public string RecordedZeroTitle => "Challenge recorded";
    public string RecordedZeroMessage => $"{_userName}, you've recorded 0 pushups for today. Don't worry, there's always tomorrow!";
    public string EditEntryTitle => $"Edit Today's {UnitPlural} Entry";
    public string EditEntryPrompt => $"Update your {UnitSingular} count for today";
    public string UpdateSuccessMessage(bool isZero) => isZero
        ? $"Your record has been updated to 0 {UnitPlural} for today."
        : $"Great job! Your {UnitSingular} record has been updated.";
    public string SaveSuccessMessage(bool isZero) => isZero
        ? $"We've recorded your response. Remember, even one {UnitSingular} is better than none!"
        : $"Great job! Your {UnitSingular} has been recorded.";

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
    public string ErrorUpdatingRecord => $"Error updating your {UnitSingular} record.";
    public string ErrorSavingRecord => $"Error saving your {UnitSingular} record.";

    // Streak labels/messages
    public string StreakDayStreakLabel => "Day Streak";
    public string StreakUnitsInCurrentStreakLabel => $"Units in Current Streak";
    public string StreakTotalUnitsOverallLabel => $"Total Units Overall";
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