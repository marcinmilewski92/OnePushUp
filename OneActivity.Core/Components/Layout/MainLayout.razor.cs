using Microsoft.AspNetCore.Components;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.Layout;

public partial class MainLayout
{
    [Inject] public IActivityBranding Branding { get; set; } = default!;
    [Inject] public IActivityContent Content { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public ILanguageService Language { get; set; } = default!;
    [Inject] public IGenderService Gender { get; set; } = default!;

    private Timer? _timer;
    private DateTime _currentTime;

    private string GetPageTitle()
    {
        var uri = new Uri(Navigation.Uri);
        var path = uri.AbsolutePath;

        return path switch
        {
            "/" => Content.DailyTitle,
            "/stats" => Content.NavStats,
            "/settings" => Content.NavSettings,
            _ => Branding.AppDisplayName
        };
    }

    protected override void OnInitialized()
    {
        _currentTime = DateTime.Now;
        _timer = new Timer(UpdateTime, null, 0, 1000);
        Language.CultureChanged += OnCultureChanged;
        Gender.GenderChanged += OnGenderChanged;
    }

    private void UpdateTime(object? state)
    {
        _currentTime = DateTime.Now;
        InvokeAsync(StateHasChanged);
    }

    private void OnCultureChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnGenderChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        Language.CultureChanged -= OnCultureChanged;
        Gender.GenderChanged -= OnGenderChanged;
    }
}
