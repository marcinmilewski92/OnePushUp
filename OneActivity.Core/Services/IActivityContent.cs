using OneActivity.Data;

namespace OneActivity.Core.Services;

public interface IActivityContent
{
    public void SetUser(User user);
    string AppName { get; }
    string UnitSingular { get; }
    string UnitPlural { get; }
    string Verb { get; }
    int MinimalQuantity { get; }

    // Helpers
    string FormatQuantity(int quantity);
    string DailyTitle { get; }
    string PromptToday { get; }
    string AlreadyCompletedTitle { get; }
    string AlreadyCompletedMessage(int quantity);
    string RecordedZeroTitle { get; }
    string RecordedZeroMessage { get; }
    string EditEntryTitle { get; }
    string EditEntryPrompt { get; }
    string UpdateSuccessMessage(bool isZero);
    string SaveSuccessMessage(bool isZero);

    // Navigation + layout
    string NavDailyGoal { get; }
    string NavStats { get; }
    string NavSettings { get; }
    string CurrentTimeLabel { get; }

    // Common UI text
    string LoadingText { get; }
    string SaveText { get; }
    string UpdateText { get; }
    string CancelText { get; }
    string SavingText { get; }

    // Daily activity options/labels
    string EditButtonText { get; }
    string YesText { get; }
    string YesMoreText { get; }
    string NoText { get; }
    string EditOptionYesLabel { get; }
    string EditOptionYesMoreLabel { get; }
    string EditOptionNoLabel { get; }
    string HowManyLabel { get; }
    string GetPleaseEnterValidNumberText(int min);

    // Errors
    string ErrorCheckingStatus { get; }
    string ErrorEntryNotFound { get; }
    string ErrorUpdatingRecord { get; }
    string ErrorSavingRecord { get; }

    // Streak labels/messages
    string StreakDayStreakLabel { get; }
    string StreakUnitsInCurrentStreakLabel { get; }
    string StreakTotalUnitsOverallLabel { get; }
    string StreakMsgLegendary(int days);
    string StreakMsgImpressive(int days);
    string StreakMsgReached(int days);
    string StreakMsgBuilding { get; }
    string StreakStartToday { get; }

    // Language settings card
    string LanguageCardTitle { get; }
    string LanguageChooseLabel { get; }
    string LanguageCurrentPrefix { get; }
}
