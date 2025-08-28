namespace OnePushUp.Services;

public class ReadingContent : IActivityContent
{
    public string AppName => "OneReading";
    public string UnitSingular => "page";
    public string UnitPlural => "pages";
    public string Verb => "read";
    public int MinimalQuantity => 3;

    public string FormatQuantity(int quantity)
    {
        var unit = quantity == 1 ? UnitSingular : UnitPlural;
        return $"{quantity} {unit}";
    }

    public string DailyTitle => "Daily Reading";
    public string PromptToday => "Did you read your 3 pages today?";
    public string AlreadyCompletedTitle => "Nice work!";
    public string AlreadyCompletedMessage(int quantity)
    {
        if (quantity <= 0) return "You've recorded 0 pages for today. There's always tomorrow!";
        return $"You read {quantity} { (quantity == 1 ? UnitSingular : UnitPlural)} today!";
    }
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
}

