using Microsoft.AspNetCore.Components;
using OneActivity.Data;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public UserService UserService { get; set; } = default!;
    [Inject]
    public DbReadyService DbReady { get; set; } = default!;

    public User? CurrentUser { get; set; }
    private bool _dbReady;

    protected override async Task OnInitializedAsync()
    {
        // Ensure DB is fully initialized before querying
        await DbReady.WaitUntilReadyAsync();
        _dbReady = true;
        CurrentUser = await UserService.GetCurrentUserAsync();
    }

    public async Task HandleUserCreated(Guid userId)
    {
        CurrentUser = await UserService.GetCurrentUserAsync();
        StateHasChanged();
    }

    public async Task HandleEntryAdded()
    {
        StateHasChanged();
    }
}
