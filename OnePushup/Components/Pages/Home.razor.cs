using Microsoft.AspNetCore.Components;
using OnePushUp.Data;
using OnePushUp.Repositories;
using OnePushUp.Components.HomeComponents;

namespace OnePushUp.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public UsersRepository UsersRepository { get; set; } = default!;

    public User? CurrentUser { get; set; }
    
    private Streak? _streakComponent;

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await UsersRepository.GetAsync();
    }

    public async Task HandleUserCreated(Guid userId)
    {
        CurrentUser = await UsersRepository.GetAsync();
        StateHasChanged();
    }
    
    public async Task HandleEntryAdded()
    {
        // When a training entry is added, refresh the streak data
        if (_streakComponent != null)
        {
            await _streakComponent.RefreshStreakData();
        }
        StateHasChanged();
    }
    
    public async Task HandleStreakDataLoaded()
    {
        // This method is called when streak data is loaded
        StateHasChanged();
    }
}