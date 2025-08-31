using Microsoft.Maui.Storage;

namespace OneActivity.Core.Services;

public class GenderService : IGenderService
{
    private const string PreferenceKey = "user_gender";
    private Gender _current;

    public GenderService()
    {
        var raw = Preferences.Default.Get(PreferenceKey, (int)Gender.Unspecified);
        _current = Enum.IsDefined(typeof(Gender), raw) ? (Gender)raw : Gender.Unspecified;
    }

    public Gender Current => _current;

    public event Action? GenderChanged;

    public void Set(Gender gender)
    {
        if (_current == gender) return;
        _current = gender;
        Preferences.Default.Set(PreferenceKey, (int)_current);
        GenderChanged?.Invoke();
    }
}

