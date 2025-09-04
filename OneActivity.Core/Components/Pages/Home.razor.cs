using Microsoft.AspNetCore.Components;
using OneActivity.Data;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public UserService UserService { get; set; } = default!;

    public User? CurrentUser { get; set; }

    protected override async Task OnInitializedAsync()
    {
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
