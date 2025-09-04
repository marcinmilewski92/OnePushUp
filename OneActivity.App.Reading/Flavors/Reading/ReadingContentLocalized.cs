using OneActivity.Core.Services;
using OneActivity.Data;

namespace OneActivity.App.Reading.Flavors.Reading;
public class ReadingContentLocalized(ILanguageService lang, ReadingContentEn en, ReadingContentPl pl) : IActivityContent
{
    private readonly ILanguageService _lang = lang;
    private readonly ReadingContentEn _en = en;
    private readonly ReadingContentPl _pl = pl;

    private IActivityContent Current => _lang.CurrentCulture.TwoLetterISOLanguageName.Equals("pl", StringComparison.InvariantCultureIgnoreCase) ? _pl : _en;
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
