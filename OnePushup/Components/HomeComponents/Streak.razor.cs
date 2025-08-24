using Microsoft.AspNetCore.Components;
using OnePushUp.Data;

namespace OnePushUp.Components.HomeComponents;

public partial class Streak
{
    [Parameter]
    public User CurrentUser { get; set; } = default!;
    
    [Parameter]
    public EventCallback OnDataLoaded { get; set; }
    
    private bool _isLoading = true;
    private int _currentStreak;
    private int _streakTotal;
    private int _totalPushups;
    
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
                _currentStreak = await TrainingEntryRepository.GetCurrentStreakAsync(CurrentUser.Id);
                _streakTotal = await TrainingEntryRepository.GetTotalPushupsInCurrentStreakAsync(CurrentUser.Id);
                _totalPushups = await TrainingEntryRepository.GetTotalPushupsAsync(CurrentUser.Id);
                
                await OnDataLoaded.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading streak data: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
    
    private string GetStreakColorClass()
    {
        if (_currentStreak >= 30)
            return "text-danger"; // Red for 30+ days
        if (_currentStreak >= 14)
            return "text-warning"; // Orange for 14+ days
        if (_currentStreak >= 7)
            return "text-success"; // Green for 7+ days
        if (_currentStreak > 0)
            return "text-primary"; // Blue for any streak
            
        return "text-muted"; // Gray for no streak
    }
}
