using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OneActivity.Data;
using OnePushUp.Models.Dtos;
using OnePushUp.Services;

namespace OnePushUp.Components.HomeComponents;

public partial class Streak
{
    [Inject]
    private ActivityService ActivityService { get; set; } = default!;

    [Inject]
    private ILogger<Streak> Logger { get; set; } = default!;
    
    [Parameter]
    public User CurrentUser { get; set; } = default!;
    
    [Parameter]
    public EventCallback OnDataLoaded { get; set; }
    
    private bool _isLoading = true;
    private StreakDataDto? _streakData;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadStreakData();
    }
    
    public async Task RefreshStreakData()
    {
        await LoadStreakData();
    }
    
    private async Task LoadStreakData()
    {
        try
        {
            _isLoading = true;
            
            if (CurrentUser != null && CurrentUser.Id != Guid.Empty)
            {
                _streakData = await ActivityService.GetStreakDataAsync(CurrentUser.Id);
                await OnDataLoaded.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading streak data");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
    
    private string GetStreakColorClass()
    {
        if (_streakData == null)
            return "text-muted";
            
        if (_streakData.CurrentStreak >= 30)
            return "text-danger"; // Red for 30+ days
        if (_streakData.CurrentStreak >= 14)
            return "text-warning"; // Orange for 14+ days
        if (_streakData.CurrentStreak >= 7)
            return "text-success"; // Green for 7+ days
        if (_streakData.CurrentStreak > 0)
            return "text-primary"; // Blue for any streak
            
        return "text-muted"; // Gray for no streak
    }
}

