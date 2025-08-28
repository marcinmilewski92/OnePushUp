using Microsoft.AspNetCore.Components;
using OnePushUp.Data;
using OnePushUp.Services;

namespace OnePushUp.Components.Pages;

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

