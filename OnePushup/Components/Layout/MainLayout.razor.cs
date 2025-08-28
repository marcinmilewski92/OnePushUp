using Microsoft.AspNetCore.Components;
using OnePushUp.Services;

namespace OnePushUp.Components.Layout;

public partial class MainLayout
{
    [Inject] public IActivityBranding Branding { get; set; } = default!;
    [Inject] public IActivityContent Content { get; set; } = default!;
    private string GetPageTitle()
    {
        var uri = new Uri(Navigation.Uri);
        var path = uri.AbsolutePath;

        return path switch
        {
            "/" => Content.DailyTitle,
            "/stats" => "Stats",
            "/settings" => "Settings",
            _ => Branding.AppDisplayName
        };
    }

    [Inject] public NavigationManager Navigation { get; set; } = default!;
    private Timer? _timer;
    private DateTime _currentTime;

    protected override void OnInitialized()
    {
        _currentTime = DateTime.Now;
        _timer = new Timer(UpdateTime, null, 0, 1000);
    }

    private void UpdateTime(object? state)
    {
        _currentTime = DateTime.Now;
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
