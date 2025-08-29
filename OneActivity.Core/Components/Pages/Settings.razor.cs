using OneActivity.Data;

namespace OneActivity.Core.Components.Pages;

public partial class Settings
{
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }

    private User? CurrentUser;

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await UserService.GetCurrentUserAsync();
    }
}
