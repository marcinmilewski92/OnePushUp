using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OneActivity.Data;
using OneActivity.Core.Models.Dtos;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.HomeComponents;

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
    private bool _showConfetti;
    private static readonly string[] ConfettiColors = new[]
    {
        "#e74c3c", "#f1c40f", "#2ecc71", "#3498db", "#9b59b6", "#e67e22",
        "#1abc9c", "#ff6b6b", "#ffd166", "#06d6a0", "#118ab2", "#ef476f"
    };
    
    protected override async Task OnInitializedAsync()
    {
        Content.SetUser(CurrentUser);
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
            _message = Content.ErrorCheckingStatus;
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
                _message = Content.GetPleaseEnterValidNumberText(_minMoreQuantity);
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
            _message = Content.ErrorEntryNotFound;
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
            var oldQuantity = _lastEntry.Quantity;
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

                // Celebrate only when quantity increased
                if (repetitions > oldQuantity)
                {
                    _ = ShowConfettiAsync();
                }
            }
            else
            {
                _isError = true;
                _message = Content.ErrorUpdatingRecord;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating training entry");
            _isError = true;
            _message = Content.ErrorUpdatingRecord;
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

            // Celebrate non-zero saves
            if (repetitions > 0)
            {
                _ = ShowConfettiAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving training entry");
            _isError = true;
            _message = Content.ErrorSavingRecord;
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private async Task ShowConfettiAsync()
    {
        try
        {
            _showConfetti = true;
            StateHasChanged();
            await Task.Delay(3000);
        }
        finally
        {
            _showConfetti = false;
            StateHasChanged();
        }
    }
}
