using OneActivity.Core.Services;

namespace OneActivity.App.Reading.Flavors.Reading;

public class ReadingBranding : IActivityBranding
{
    public string AppDisplayName => "OneReading";
    public string LogoPath => "/FrotkaReading.svg";
    public string SplashPath => "/FrotkaReading.svg";
    public string? PrimaryColor => "#cc5500";
    public string? SecondaryColor => "#9b0000";
    public string? AccentColor => "#ffc300";
}
