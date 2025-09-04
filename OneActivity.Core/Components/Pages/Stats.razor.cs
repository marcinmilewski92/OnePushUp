using Microsoft.AspNetCore.Components;
using OneActivity.Data;
using OneActivity.Core.Services;
using OneActivity.Core.Components.HomeComponents;

namespace OneActivity.Core.Components.Pages;

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
        StateHasChanged();
    }
}
