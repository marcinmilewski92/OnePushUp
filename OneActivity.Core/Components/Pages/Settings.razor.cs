using Microsoft.AspNetCore.Components;
using OneActivity.Data;
using OneActivity.Core.Services;

namespace OneActivity.Core.Components.Pages;

public partial class Settings : IDisposable
{
    [Inject]
    public ILanguageService Language { get; set; } = default!;

    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }

    private User? CurrentUser;

    protected override async Task OnInitializedAsync()
    {
        Language.CultureChanged += OnCultureChanged;
        CurrentUser = await UserService.GetCurrentUserAsync();
    }

    private void OnCultureChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Language.CultureChanged -= OnCultureChanged;
    }
}
