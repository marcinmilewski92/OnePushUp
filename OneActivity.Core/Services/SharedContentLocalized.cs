namespace OneActivity.Core.Services;

public class SharedContentLocalized : ISharedContent
{
    private readonly ILanguageService _lang;
    private readonly SharedContentEn _en;
    private readonly SharedContentPl _pl;
    public SharedContentLocalized(ILanguageService lang, SharedContentEn en, SharedContentPl pl)
    { _lang = lang; _en = en; _pl = pl; }
    private ISharedContent Cur => _lang.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant() == "pl" ? _pl : _en;

    public string NoUserProfileMessage => Cur.NoUserProfileMessage;
    public string LanguageCardTitle => Cur.LanguageCardTitle;
    public string LanguageChooseLabel => Cur.LanguageChooseLabel;
    public string CurrentPrefix => Cur.CurrentPrefix;
    public string GenderCardTitle => Cur.GenderCardTitle;
    public string GenderChooseLabel => Cur.GenderChooseLabel;
    public string GenderUnspecified => Cur.GenderUnspecified;
    public string GenderMale => Cur.GenderMale;
    public string GenderFemale => Cur.GenderFemale;
    public string NotificationTestingTitle => Cur.NotificationTestingTitle;
    public string NotificationTestingDescription => Cur.NotificationTestingDescription;
    public string NotificationTestingButtonIdle => Cur.NotificationTestingButtonIdle;
    public string NotificationTestingButtonTesting => Cur.NotificationTestingButtonTesting;
    public string LastNotificationScheduledLabel => Cur.LastNotificationScheduledLabel;
    public string NotificationTestSuccess(string time) => Cur.NotificationTestSuccess(time);
    public string NotificationTestErrorPrefix => Cur.NotificationTestErrorPrefix;
}

