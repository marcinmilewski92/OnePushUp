using Microsoft.AspNetCore.Components;

namespace OnePushUp.Components.Layout;

public partial class MainLayout
{
    private string GetPageTitle()
    {
        var uri = new Uri(Navigation.Uri);
        var path = uri.AbsolutePath;

        return path switch
        {
            "/" => "Daily Goal",
            "/stats" => "Stats",
            "/settings" => "Settings",
            _ => "OnePushUp"
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