using Microsoft.AspNetCore.Components;
using OnePushUp.Services;

namespace OnePushUp.Components.HomeComponents;

public partial class CreateUser : ComponentBase
{
    [Inject]
    private UserService UserService { get; set; } = default!;
    
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
            ErrorMessage = "Nickname cannot be empty.";
            return;
        }
        
        if (trimmedNickName.Length > 15)
        {
            ErrorMessage = "Nickname must be at most 15 characters.";
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
                ErrorMessage = "Failed to create user. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating user: {ex.Message}";
        }
    }
}

