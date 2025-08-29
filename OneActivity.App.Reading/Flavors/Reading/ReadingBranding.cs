using OneActivity.Core.Services;

namespace OneActivity.App.Reading.Flavors.Reading;

public class ReadingBranding : IActivityBranding
{
    public string AppDisplayName => "OneReading";
    public string LogoPath => "/FrotkaReading.svg";
    public string SplashPath => "/FrotkaReading.svg";
    public string? PrimaryColor => "#ff0000";
    public string? SecondaryColor => "#ff0000";
    public string? AccentColor => "#fdb127";
}
