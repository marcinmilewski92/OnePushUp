using System.Globalization;
using Microsoft.Maui.Storage;

namespace OneActivity.Core.Services;

public class LanguageService : ILanguageService
{
    private const string PreferenceKey = "culture";
    private CultureInfo _current;

    public LanguageService()
    {
        var name = Preferences.Default.Get(PreferenceKey, "en");
        _current = CreateCulture(name);
        ApplyCulture(_current);
    }

    public CultureInfo CurrentCulture => _current;
    public event Action? CultureChanged;

    public void SetCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName)) return;
        var newCulture = CreateCulture(cultureName);
        if (newCulture.Name.Equals(_current.Name, StringComparison.OrdinalIgnoreCase)) return;

        _current = newCulture;
        Preferences.Default.Set(PreferenceKey, _current.TwoLetterISOLanguageName);
        ApplyCulture(_current);
        CultureChanged?.Invoke();
    }

    private static void ApplyCulture(CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static CultureInfo CreateCulture(string cultureName)
    {
        try
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
        catch
        {
            return CultureInfo.GetCultureInfo("en");
        }
    }
}

