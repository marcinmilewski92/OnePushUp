using Microsoft.AspNetCore.Components;
using OnePushUp.Data;
using OnePushUp.Models.Dtos;
using OnePushUp.Services;

namespace OnePushUp.Components.HomeComponents;

public enum PushupOption
{
    Yes,
    YesMore,
    No
}

public partial class DailyPushup
{
    [Inject]
    private TrainingService TrainingService { get; set; } = default!;
    
    [Parameter]
    public User CurrentUser { get; set; } = default!;
    
    [Parameter]
    public EventCallback OnEntryAdded { get; set; }
    
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _hasCompletedToday;
    private string _message = string.Empty;
    private bool _isError;
    private PushupOption _selectedOption = PushupOption.Yes;
    private int _repetitions = 2;
    private bool _repetitionError;
    private TrainingEntryDto? _lastEntry;
    
    protected override async Task OnInitializedAsync()
    {
        await CheckTodayStatus();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (CurrentUser != null && CurrentUser.Id != Guid.Empty && !_isLoading)
        {
            await CheckTodayStatus();
        }
    }
    
    private async Task CheckTodayStatus()
    {
        try
        {
            _isLoading = true;
            _hasCompletedToday = await TrainingService.HasEntryForTodayAsync(CurrentUser.Id);
            
            if (_hasCompletedToday)
            {
                var entries = await TrainingService.GetEntriesForUserAsync(CurrentUser.Id);
                if (entries.Any())
                {
                    _lastEntry = entries.First(); // Get the most recent entry
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking today's status: {ex.Message}");
            _isError = true;
            _message = "Error checking your status for today.";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
    
    private async Task SaveTrainingEntry()
    {
        try
        {
            _isSaving = true;
            _message = string.Empty;
            _isError = false;
            _repetitionError = false;
            
            // Skip if user selected "No"
            if (_selectedOption == PushupOption.No)
            {
                _message = "No problem. Try again tomorrow!";
                return;
            }
            
            int repetitions = 1; // Default for "Yes" option
            
            if (_selectedOption == PushupOption.YesMore)
            {
                if (_repetitions < 2)
                {
                    _repetitionError = true;
                    _isError = true;
                    _message = "Please enter a valid number of pushups (minimum 2).";
                    return;
                }
                
                repetitions = _repetitions;
            }
            
            await TrainingService.CreateEntryAsync(CurrentUser.Id, repetitions);
            
            // Refresh last entry
            var entries = await TrainingService.GetEntriesForUserAsync(CurrentUser.Id);
            if (entries.Any())
            {
                _lastEntry = entries.First();
            }
            
            _message = "Great job! Your pushup has been recorded.";
            _hasCompletedToday = true;
            
            await OnEntryAdded.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving training entry: {ex.Message}");
            _isError = true;
            _message = "Error saving your pushup record.";
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
}
