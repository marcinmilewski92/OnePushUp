using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OneActivity.Data;
using OneActivity.Core.Models.Dtos;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.SettingsComponents;

public partial class EditNickname : IDisposable
{
    [Inject]
    private ILogger<EditNickname> Logger { get; set; } = default!;
    [Inject]
    private ILanguageService Language { get; set; } = default!;

    private string _nickname = string.Empty;
    private bool _isLoading = true;
    private bool _isSaving;
    private string _message = string.Empty;
    private bool _isError;
    private User? _currentUser;

    protected override async Task OnInitializedAsync()
    {
        Language.CultureChanged += OnCultureChanged;
        await LoadUserData();
    }

    private void OnCultureChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Language.CultureChanged -= OnCultureChanged;
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
                _message = Shared.NoUserProfileMessage;
                _isError = true;
                Logger.LogWarning("No user found in the database");
            }
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"{Shared.ErrorLoadingUserDataPrefix} {ex.Message}";
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
                _message = Shared.NoUserProfileMessage;
                Logger.LogWarning("Attempted to save nickname but no user was found");
                return;
            }

            if (string.IsNullOrWhiteSpace(_nickname))
            {
                _isError = true;
                _message = Shared.NicknameEmptyError;
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
                _message = Shared.NicknameUpdateSuccess;
                _isError = false;
            }
            else
            {
                _message = Shared.NicknameUpdateFailed;
                _isError = true;
            }
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"{Shared.ErrorUpdatingNicknamePrefix} {ex.Message}";
            Logger.LogError(ex, "Error updating nickname");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
}
