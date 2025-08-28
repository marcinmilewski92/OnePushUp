using Microsoft.AspNetCore.Components;
using OneActivity.Data;
using OnePushUp.Services;
using OnePushUp.Components.HomeComponents;

namespace OnePushUp.Components.Pages;

public partial class Stats : ComponentBase
{
    [Inject]
    public UserService UserService { get; set; } = default!;

    public User? CurrentUser { get; set; }
    
    private Streak? _streakComponent;

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await UserService.GetCurrentUserAsync();
    }
    
    public async Task HandleStreakDataLoaded()
    {
        // This method is called when streak data is loaded
        StateHasChanged();
    }
}
