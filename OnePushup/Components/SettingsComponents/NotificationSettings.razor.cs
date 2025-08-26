using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OnePushUp.Services;

namespace OnePushUp.Components.SettingsComponents;

public partial class NotificationSettings
{
    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private ILogger<NotificationSettings> Logger { get; set; } = default!;
    
    private bool _isLoading = true;
    private bool _notificationsEnabled;
    private TimeOnly _notificationTime = new(8, 0); // Default to 8:00 AM
    private string _message = string.Empty;
    private bool _isError;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadNotificationSettings();
    }
    
    private async Task LoadNotificationSettings()
    {
        try
        {
            _isLoading = true;
            
            var settings = await NotificationService.GetNotificationSettingsAsync();
            _notificationsEnabled = settings.Enabled;
            
            if (settings.Time.HasValue)
            {
                _notificationTime = new TimeOnly(settings.Time.Value.Hours, settings.Time.Value.Minutes);
            }
            
            _isLoading = false;
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error loading notification settings: {ex.Message}";
            Logger.LogError(ex, "Error loading notification settings");
            _isLoading = false;
        }
    }
    
    private async Task ToggleNotifications()
    {
        try
        {
            await NotificationService.UpdateNotificationSettingsAsync(
                new Models.NotificationSettings
                {
                    Enabled = _notificationsEnabled,
                    Time = _notificationsEnabled ? new TimeSpan(_notificationTime.Hour, _notificationTime.Minute, 0) : null
                });
                
            _message = _notificationsEnabled 
                ? "Notifications enabled successfully!" 
                : "Notifications disabled successfully!";
            _isError = false;
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error updating notification settings: {ex.Message}";
            Logger.LogError(ex, "Error updating notification settings");
        }
    }
    
    private async Task UpdateNotificationTime()
    {
        if (!_notificationsEnabled) return;
        
        try
        {
            await NotificationService.UpdateNotificationSettingsAsync(
                new Models.NotificationSettings
                {
                    Enabled = true,
                    Time = new TimeSpan(_notificationTime.Hour, _notificationTime.Minute, 0)
                });
                
            _message = $"Notification time updated to {_notificationTime.ToShortTimeString()}!";
            _isError = false;
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error updating notification time: {ex.Message}";
            Logger.LogError(ex, "Error updating notification time");
        }
    }
}
