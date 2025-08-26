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
    private bool _isEditing = false;
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
                // Get today's entry for display and potential editing
                _lastEntry = await TrainingService.GetTodayEntryAsync(CurrentUser.Id);
                
                // Pre-set the form values based on today's entry
                if (_lastEntry != null)
                {
                    if (_lastEntry.NumberOfRepetitions == 0)
                    {
                        _selectedOption = PushupOption.No;
                    }
                    else if (_lastEntry.NumberOfRepetitions == 1)
                    {
                        _selectedOption = PushupOption.Yes;
                    }
                    else
                    {
                        _selectedOption = PushupOption.YesMore;
                        _repetitions = _lastEntry.NumberOfRepetitions;
                    }
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
    
    private void EnableEditMode()
    {
        _isEditing = true;
        _message = string.Empty;
        _isError = false;
        
        // Pre-set the form values based on the current entry
        if (_lastEntry != null)
        {
            if (_lastEntry.NumberOfRepetitions == 0)
            {
                _selectedOption = PushupOption.No;
            }
            else if (_lastEntry.NumberOfRepetitions == 1)
            {
                _selectedOption = PushupOption.Yes;
            }
            else
            {
                _selectedOption = PushupOption.YesMore;
                _repetitions = _lastEntry.NumberOfRepetitions;
            }
        }
    }
    
    private void CancelEdit()
    {
        _isEditing = false;
        _message = string.Empty;
    }

    private bool TryGetRepetitions(out int repetitions)
    {
        repetitions = 0;

        if (_selectedOption == PushupOption.Yes)
        {
            repetitions = 1;
        }
        else if (_selectedOption == PushupOption.YesMore)
        {
            if (_repetitions < 2)
            {
                _repetitionError = true;
                _isError = true;
                _message = "Please enter a valid number of pushups (minimum 2).";
                return false;
            }

            repetitions = _repetitions;
        }

        // For PushupOption.No, repetitions remains 0
        return true;
    }

    private async Task UpdateTrainingEntry()
    {
        if (_lastEntry == null)
        {
            _isError = true;
            _message = "Error: Could not find today's entry.";
            return;
        }
        
        try
        {
            _isSaving = true;
            _message = string.Empty;
            _isError = false;
            _repetitionError = false;

            if (!TryGetRepetitions(out var repetitions))
            {
                return;
            }

            // Update the existing entry
            bool success = await TrainingService.UpdateEntryAsync(_lastEntry.Id, repetitions);
            
            if (success)
            {
                // Refresh last entry
                _lastEntry = await TrainingService.GetTodayEntryAsync(CurrentUser.Id);
                
                if (_selectedOption == PushupOption.No)
                {
                    _message = "Your record has been updated to 0 pushups for today.";
                    _isError = false; // Not really an error, just a different status
                }
                else
                {
                    _message = "Great job! Your pushup record has been updated.";
                }
                
                _isEditing = false; // Exit edit mode
                
                await OnEntryAdded.InvokeAsync(); // Refresh streak data
            }
            else
            {
                _isError = true;
                _message = "Error updating your pushup record.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating training entry: {ex.Message}");
            _isError = true;
            _message = "Error updating your pushup record.";
        }
        finally
        {
            _isSaving = false;
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

            if (!TryGetRepetitions(out var repetitions))
            {
                return;
            }

            // Always save an entry, even for "No" responses
            await TrainingService.CreateEntryAsync(CurrentUser.Id, repetitions);
            
            // Refresh last entry
            _lastEntry = await TrainingService.GetTodayEntryAsync(CurrentUser.Id);
            
            if (_selectedOption == PushupOption.No)
            {
                _message = "We've recorded your response. Remember, even one pushup is better than none!";
                _isError = false; // Not really an error, but we'll show it differently
            }
            else
            {
                _message = "Great job! Your pushup has been recorded.";
            }
            
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
