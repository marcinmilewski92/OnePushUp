using OneActivity.Core.Services;

namespace OneActivity.App.Reading.Flavors.Reading;

public class ReadingBranding : IActivityBranding
{
    public string AppDisplayName => "OneBookPage";
    public string LogoPath => "/splash.svg";
    public string SplashPath => "/splash.svg";
    public string? PrimaryColor => "#cc5500";
    public string? SecondaryColor => "#9b0000";
    public string? AccentColor => "#ffc300";
}
