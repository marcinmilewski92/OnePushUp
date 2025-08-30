using System.Globalization;
using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Pushups.Flavors.Pushups;

public class PushupContent : IActivityContent
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
}
