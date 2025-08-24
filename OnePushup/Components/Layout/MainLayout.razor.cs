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
            "/" => "Dashboard",
            "/settings" => "Settings",
            _ => "OnePushUp"
        };
    }

    [Inject] public NavigationManager Navigation { get; set; } = default!;
}