namespace OnePushUp.Services;

public class ReadingBranding : IActivityBranding
{
    public string AppDisplayName => "OneReading";
    public string LogoPath => "/splash.svg"; // Replace with reading logo when provided
    public string SplashPath => "/splash.svg";
    public string? PrimaryColor => "#1e88e5"; // Sample blue theme
    public string? SecondaryColor => "#1565c0";
    public string? AccentColor => "#fbc02d";
}

