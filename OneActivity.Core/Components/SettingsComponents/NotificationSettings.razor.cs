using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OnePushUp.Services;
using System.Globalization;

namespace OnePushUp.Components.SettingsComponents;

public partial class NotificationSettings
{
    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private ILogger<NotificationSettings> Logger { get; set; } = default!;
    
    private bool _isLoading = true;
    private bool _notificationsEnabled;
    private string _notificationTimeText = "08:00"; // 24h HH:mm
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
                var t = settings.Time.Value;
                _notificationTimeText = t.ToString("hh\\:mm", CultureInfo.InvariantCulture);
            }
            else
            {
                _notificationTimeText = "08:00";
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
            TimeSpan? time = null;
            if (_notificationsEnabled)
            {
                if (!TryParseNotificationTime(out var parsed))
                {
                    var normalized = (_notificationTimeText ?? string.Empty).Trim();
                    if (normalized.Length >= 5)
                    {
                        normalized = normalized.Substring(0, 5);
                        _notificationTimeText = normalized;
                    }

                    if (!TryParseNotificationTime(out parsed))
                    {
                        _isError = true;
                        _message = "Invalid time format. Please use HH:mm (e.g., 08:00).";
                        return;
                    }
                }
                time = parsed;
            }

            await NotificationService.UpdateNotificationSettingsAsync(new Models.NotificationSettings
            {
                Enabled = _notificationsEnabled,
                Time = time
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
            if (!TryParseNotificationTime(out var parsed))
            {
                _isError = true;
                _message = "Invalid time format. Please use HH:mm (e.g., 08:00).";
                return;
            }

            await NotificationService.UpdateNotificationSettingsAsync(new Models.NotificationSettings
            {
                Enabled = true,
                Time = parsed
            });

            _message = $"Notification time updated to {_notificationTimeText}!";
            _isError = false;
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"Error updating notification time: {ex.Message}";
            Logger.LogError(ex, "Error updating notification time");
        }
    }

    private async Task OnTimeInputChanged(ChangeEventArgs e)
    {
        _notificationTimeText = e.Value?.ToString() ?? string.Empty;
        await UpdateNotificationTime();
    }

    private bool TryParseNotificationTime(out TimeSpan time)
    {
        time = default;
        var s = (_notificationTimeText ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(s)) return false;

        if (s.Length >= 8 && s[2] == ':' && s[5] == ':')
            s = s.Substring(0, 5);

        if (TimeSpan.TryParseExact(s, new[] { "HH\\:mm", "H\\:mm" }, CultureInfo.InvariantCulture, out var ts))
        {
            time = ts;
            _notificationTimeText = ts.ToString("hh\\:mm", CultureInfo.InvariantCulture);
            return true;
        }

        if (TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out ts) || TimeSpan.TryParse(s, out ts))
        {
            if (ts < TimeSpan.Zero || ts >= TimeSpan.FromDays(1)) return false;
            time = new TimeSpan(ts.Hours, ts.Minutes, 0);
            _notificationTimeText = time.ToString("hh\\:mm", CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }
}

