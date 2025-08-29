using OneActivity.Core.Services;

namespace OneActivity.App.Pushups.Flavors.Pushups;

public class PushupBranding : IActivityBranding
{
    public string AppDisplayName => "OnePushUp";
    public string LogoPath => "/splash.svg";
    public string SplashPath => "/splash.svg";
    public string? PrimaryColor => "#cc5500";
    public string? SecondaryColor => "#9b0000";
    public string? AccentColor => "#ffc300";
}
