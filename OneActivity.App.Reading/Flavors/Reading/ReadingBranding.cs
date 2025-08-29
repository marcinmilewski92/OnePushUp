using OneActivity.Core.Services;

namespace OneActivity.App.Reading.Flavors.Reading;

public class ReadingBranding : IActivityBranding
{
    public string AppDisplayName => "OneReading";
    public string LogoPath => "/splash.svg";
    public string SplashPath => "/splash.svg";
    public string? PrimaryColor => "#1e88e5";
    public string? SecondaryColor => "#1565c0";
    public string? AccentColor => "#fbc02d";
}
