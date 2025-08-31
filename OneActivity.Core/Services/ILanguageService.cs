using System.Globalization;

namespace OneActivity.Core.Services;

public interface ILanguageService
{
    CultureInfo CurrentCulture { get; }
    void SetCulture(string cultureName);
    event Action? CultureChanged;
}

