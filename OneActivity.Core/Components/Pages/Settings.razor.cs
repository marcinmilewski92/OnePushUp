using OnePushUp.Data;

namespace OnePushUp.Components.Pages;

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

