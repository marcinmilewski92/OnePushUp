using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OneActivity.Data;
using OneActivity.Core.Models.Dtos;

namespace OneActivity.Core.Components.SettingsComponents;

public partial class EditNickname
{
    [Inject]
    private ILogger<EditNickname> Logger { get; set; } = default!;

    private string _nickname = string.Empty;
    private bool _isLoading = true;
    private bool _isSaving;
    private string _message = string.Empty;
    private bool _isError;
    private User? _currentUser;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserData();
    }

    private async Task LoadUserData()
    {
        try
        {
            _isLoading = true;
            _message = string.Empty;
            
            _currentUser = await UserService.GetCurrentUserAsync();
            
            if (_currentUser != null)
            {
                _nickname = _currentUser.NickName;
                Logger.LogInformation("User loaded: {UserId}, Nickname: {Nickname}", _currentUser.Id, _currentUser.NickName);
            }
            else
            {
                _message = "No user found. Please create a user first.";
                _isError = true;
                Logger.LogWarning("No user found in the database");
            }
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error loading user data: {ex.Message}";
            Logger.LogError(ex, "Error loading user data");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SaveNickname()
    {
        try
        {
            _isSaving = true;
            _message = string.Empty;
            _isError = false;
            
            if (_currentUser == null)
            {
                _isError = true;
                _message = "No user found to update.";
                Logger.LogWarning("Attempted to save nickname but no user was found");
                return;
            }

            if (string.IsNullOrWhiteSpace(_nickname))
            {
                _isError = true;
                _message = "Nickname cannot be empty.";
                Logger.LogWarning("Attempted to save empty nickname");
                return;
            }

            // Log before updating
            Logger.LogInformation("Updating nickname from '{OldNickname}' to '{NewNickname}'", _currentUser.NickName, _nickname);

            var userDto = new UserDto
            {
                Id = _currentUser.Id,
                NickName = _nickname
            };

            var success = await UserService.UpdateUserAsync(userDto);
            if (success)
            {
                _message = "Nickname updated successfully!";
                _isError = false;
            }
            else
            {
                _message = "Failed to update nickname. Please try again.";
                _isError = true;
            }
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error updating nickname: {ex.Message}";
            Logger.LogError(ex, "Error updating nickname");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
}
