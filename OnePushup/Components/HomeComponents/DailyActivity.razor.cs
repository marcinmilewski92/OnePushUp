using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OnePushUp.Data;
using OnePushUp.Models.Dtos;
using OnePushUp.Services;

namespace OnePushUp.Components.HomeComponents;

public enum ActivityOption
{
    Yes,
    YesMore,
    No
}

public partial class DailyActivity
{
    [Inject]
    private IActivityContent Content { get; set; } = default!;
    [Inject]
    private ActivityService ActivityService { get; set; } = default!;

    [Inject]
    private ILogger<DailyActivity> Logger { get; set; } = default!;
    
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
    private ActivityOption _selectedOption = ActivityOption.Yes;
    private int _repetitions = 2;
    private bool _repetitionError;
    private ActivityEntryDto? _lastEntry;
    private int _minMoreQuantity => Math.Max(Content.MinimalQuantity + 1, Content.MinimalQuantity == int.MaxValue ? int.MaxValue : Content.MinimalQuantity + 1);
    
    protected override async Task OnInitializedAsync()
    {
        _repetitions = Math.Max(Content.MinimalQuantity + 1, 2);
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
            _hasCompletedToday = await ActivityService.HasEntryForTodayAsync(CurrentUser.Id);
            
            if (_hasCompletedToday)
            {
                // Get today's entry for display and potential editing
                _lastEntry = await ActivityService.GetTodayEntryAsync(CurrentUser.Id);
                
                // Pre-set the form values based on today's entry
                if (_lastEntry != null)
                {
                    SetSelectedOptionFromEntry(_lastEntry);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking today's status");
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
            SetSelectedOptionFromEntry(_lastEntry);
        }
    }

    private void SetSelectedOptionFromEntry(ActivityEntryDto entry)
    {
        if (entry.Quantity == 0)
        {
            _selectedOption = ActivityOption.No;
        }
        else if (entry.Quantity == Content.MinimalQuantity)
        {
            _selectedOption = ActivityOption.Yes;
        }
        else
        {
            _selectedOption = ActivityOption.YesMore;
            _repetitions = entry.Quantity;
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

        if (_selectedOption == ActivityOption.Yes)
        {
            repetitions = Content.MinimalQuantity;
        }
        else if (_selectedOption == ActivityOption.YesMore)
        {
            if (_repetitions < _minMoreQuantity)
            {
                _repetitionError = true;
                _isError = true;
                _message = $"Please enter a valid number (minimum {_minMoreQuantity}).";
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
            bool success = await ActivityService.UpdateEntryAsync(_lastEntry.Id, repetitions);
            
            if (success)
            {
                // Refresh last entry
                _lastEntry = await ActivityService.GetTodayEntryAsync(CurrentUser.Id);
                
                var isZero = _selectedOption == ActivityOption.No;
                _message = Content.UpdateSuccessMessage(isZero);
                _isError = false;
                
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
            Logger.LogError(ex, "Error updating training entry");
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
            await ActivityService.CreateEntryAsync(CurrentUser.Id, repetitions);
            
            // Refresh last entry
            _lastEntry = await ActivityService.GetTodayEntryAsync(CurrentUser.Id);
            var isZero = _selectedOption == ActivityOption.No;
            _message = Content.SaveSuccessMessage(isZero);
            _isError = false;
            
            _hasCompletedToday = true;
            
            await OnEntryAdded.InvokeAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving training entry");
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
