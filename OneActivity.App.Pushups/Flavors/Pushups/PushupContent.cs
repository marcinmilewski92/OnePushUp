using System.Globalization;
using OnePushUp.Services;

namespace OneActivity.App.Pushups.Flavors.Pushups;

public class PushupContent : IActivityContent
{
    public string AppName => "OnePushUp";
    public string UnitSingular => "pushup";
    public string UnitPlural => "pushups";
    public string Verb => "do";
    public int MinimalQuantity => 1;

    public string FormatQuantity(int quantity)
    {
        var unit = quantity == 1 ? UnitSingular : UnitPlural;
        return $"{quantity} {unit}";
    }

    public string DailyTitle => "Daily Pushup Challenge";
    public string PromptToday => "Did you do your one pushup today?";
    public string AlreadyCompletedTitle => "Great job! 💪";
    public string AlreadyCompletedMessage(int quantity)
    {
        if (quantity <= 0) return "You've recorded 0 pushups for today. Don't worry, there's always tomorrow!";
        if (quantity == 1) return "You did 1 pushup today!";
        return $"You did {quantity} pushups today!";
    }
    public string RecordedZeroTitle => "Challenge recorded";
    public string RecordedZeroMessage => "You've recorded 0 pushups for today. Don't worry, there's always tomorrow!";
    public string EditEntryTitle => "Edit Today's Pushup Entry";
    public string EditEntryPrompt => "Update your pushup count for today";
    public string UpdateSuccessMessage(bool isZero) => isZero
        ? "Your record has been updated to 0 pushups for today."
        : "Great job! Your pushup record has been updated.";
    public string SaveSuccessMessage(bool isZero) => isZero
        ? "We've recorded your response. Remember, even one pushup is better than none!"
        : "Great job! Your pushup has been recorded.";
}

