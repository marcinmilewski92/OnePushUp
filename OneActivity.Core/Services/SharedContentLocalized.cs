namespace OneActivity.Core.Services;

public class SharedContentLocalized : ISharedContent
{
    private readonly ILanguageService _lang;
    private readonly SharedContentEn _en;
    private readonly SharedContentPl _pl;
    public SharedContentLocalized(ILanguageService lang, SharedContentEn en, SharedContentPl pl)
    { _lang = lang; _en = en; _pl = pl; }
    private ISharedContent Cur => _lang.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant() == "pl" ? _pl : _en;

    public string CreateUserPrompt => Cur.CreateUserPrompt;
    public string CreateUserNicknamePlaceholder => Cur.CreateUserNicknamePlaceholder;
    public string NicknameEmptyError => Cur.NicknameEmptyError;
    public string NicknameTooLongError => Cur.NicknameTooLongError;
    public string CreateUserFailed => Cur.CreateUserFailed;
    public string CreateUserErrorPrefix => Cur.CreateUserErrorPrefix;

    public string EditNicknameTitle => Cur.EditNicknameTitle;
    public string NicknameLabel => Cur.NicknameLabel;
    public string NicknameUpdateSuccess => Cur.NicknameUpdateSuccess;
    public string NicknameUpdateFailed => Cur.NicknameUpdateFailed;
    public string ErrorLoadingUserDataPrefix => Cur.ErrorLoadingUserDataPrefix;
    public string ErrorUpdatingNicknamePrefix => Cur.ErrorUpdatingNicknamePrefix;

    public string NoUserProfileMessage => Cur.NoUserProfileMessage;

    public string LanguageCardTitle => Cur.LanguageCardTitle;
    public string LanguageChooseLabel => Cur.LanguageChooseLabel;
    public string CurrentPrefix => Cur.CurrentPrefix;
    public string LanguageEnglishOption => Cur.LanguageEnglishOption;
    public string LanguagePolishOption => Cur.LanguagePolishOption;

    public string GenderCardTitle => Cur.GenderCardTitle;
    public string GenderChooseLabel => Cur.GenderChooseLabel;
    public string GenderUnspecified => Cur.GenderUnspecified;
    public string GenderMale => Cur.GenderMale;
    public string GenderFemale => Cur.GenderFemale;

    public string NotificationReliabilityInfo => Cur.NotificationReliabilityInfo;
    public string NotificationDiagnosticsTitle => Cur.NotificationDiagnosticsTitle;
    public string ExactAlarmsAllowedLabel => Cur.ExactAlarmsAllowedLabel;
    public string OpenExactAlarmSettingsButton => Cur.OpenExactAlarmSettingsButton;
    public string IgnoringBatteryOptimizationsLabel => Cur.IgnoringBatteryOptimizationsLabel;
    public string OpenBatteryOptimizationSettingsButton => Cur.OpenBatteryOptimizationSettingsButton;
}
