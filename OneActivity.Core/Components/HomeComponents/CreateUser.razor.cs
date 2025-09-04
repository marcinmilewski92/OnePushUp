using Microsoft.AspNetCore.Components;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.HomeComponents;

public partial class CreateUser : ComponentBase
{
    [Inject]
    private UserService UserService { get; set; } = default!;
    [Inject]
    private IActivityContent Content { get; set; } = default!;
    [Inject]
    private ISharedContent Shared { get; set; } = default!;

    public string NickName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<Guid> OnUserCreated { get; set; }

    private async Task HandleSubmit()
    {
        ErrorMessage = string.Empty;

        // Basic validation
        if (string.IsNullOrWhiteSpace(NickName))
        {
            ErrorMessage = Shared.NicknameEmptyError;
            return;
        }
        if (NickName.Length > 15)
        {
            ErrorMessage = Shared.NicknameTooLongError;
            return;
        }

        try
        {
            var userId = await UserService.CreateUserAsync(NickName.Trim());
            if (OnUserCreated.HasDelegate)
            {
                await OnUserCreated.InvokeAsync(userId);
            }
        }
        catch (Exception ex)
        {
            // Provide a simple error surface
            ErrorMessage = $"{Shared.CreateUserErrorPrefix} {ex.Message}";
        }
    }
}
