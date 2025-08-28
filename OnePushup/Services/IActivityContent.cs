namespace OnePushUp.Services;

public interface IActivityContent
{
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
}

