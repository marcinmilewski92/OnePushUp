namespace OneActivity.Core.Services;

public interface IActivityBranding
{
    string AppDisplayName { get; }
    // Paths relative to wwwroot
    string LogoPath { get; }
    string SplashPath { get; }
    // Optional CSS variables (hex values)
    string? PrimaryColor { get; }
    string? SecondaryColor { get; }
    string? AccentColor { get; }
}
