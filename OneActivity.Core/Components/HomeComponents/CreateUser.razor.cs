using Microsoft.AspNetCore.Components;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.HomeComponents;

public partial class CreateUser : ComponentBase
{
    [Inject]
    private UserService UserService { get; set; } = default!;
    [Inject]
    private ISharedContent Shared { get; set; } = default!;
    [Inject]
    private IActivityContent Content { get; set; } = default!;
    
    public string NickName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<Guid> OnUserCreated { get; set; }

    private async Task HandleSubmit()
    {
        ErrorMessage = string.Empty;
        var trimmedNickName = NickName.Trim();
        
        if (string.IsNullOrWhiteSpace(trimmedNickName))
        {
            ErrorMessage = Shared.NicknameEmptyError;
            return;
        }

        if (trimmedNickName.Length > 15)
        {
            ErrorMessage = Shared.NicknameTooLongError;
            return;
        }
        
        try
        {
            var userId = await UserService.CreateUserAsync(trimmedNickName);
            
            if (userId != Guid.Empty)
            {
                NickName = string.Empty;
                await OnUserCreated.InvokeAsync(userId);
            }
            else
            {
                ErrorMessage = Shared.CreateUserFailed;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{Shared.CreateUserErrorPrefix} {ex.Message}";
        }
    }
}
