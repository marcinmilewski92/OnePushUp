using Microsoft.AspNetCore.Components;
using OnePushUp.Data;


namespace OnePushUp.Components.HomeComponents;

public partial class CreateUser : ComponentBase
{
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
        var user = new User { NickName = trimmedNickName };
        var userId = await UsersRepository.CreateAsync(user);
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
}