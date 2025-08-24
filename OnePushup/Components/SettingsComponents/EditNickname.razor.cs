using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OnePushUp.Data;

namespace OnePushUp.Components.SettingsComponents;

public partial class EditNickname
{
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
            
            _currentUser = await UsersRepository.GetAsync();
            
            if (_currentUser != null)
            {
                _nickname = _currentUser.NickName;
                Console.WriteLine($"User loaded: {_currentUser.Id}, Nickname: {_currentUser.NickName}");
                Logger.LogInformation("User loaded: {UserId}, Nickname: {Nickname}", _currentUser.Id, _currentUser.NickName);
            }
            else
            {
                _message = "No user found. Please create a user first.";
                _isError = true;
                Console.WriteLine("No user found in the database.");
                Logger.LogWarning("No user found in the database");
            }
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error loading user data: {ex.Message}";
            Console.WriteLine($"Error loading user data: {ex.Message}");
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
                Console.WriteLine("No user found to update.");
                Logger.LogWarning("Attempted to save nickname but no user was found");
                return;
            }

            if (string.IsNullOrWhiteSpace(_nickname))
            {
                _isError = true;
                _message = "Nickname cannot be empty.";
                Console.WriteLine("Nickname cannot be empty.");
                Logger.LogWarning("Attempted to save empty nickname");
                return;
            }

            // Log before updating
            Console.WriteLine($"Updating nickname from '{_currentUser.NickName}' to '{_nickname}'");
            Logger.LogInformation("Updating nickname from '{OldNickname}' to '{NewNickname}'", _currentUser.NickName, _nickname);
            
            _currentUser.NickName = _nickname;
            await UsersRepository.UpdateAsync(_currentUser);
            
            _message = "Nickname updated successfully!";
            Console.WriteLine("Nickname updated successfully!");
            Logger.LogInformation("Nickname updated successfully for user {UserId}", _currentUser.Id);
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error saving nickname: {ex.Message}";
            Console.WriteLine($"Error saving nickname: {ex.Message}");
            Logger.LogError(ex, "Error saving nickname");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
}
