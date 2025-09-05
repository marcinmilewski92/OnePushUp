using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;
using System.Globalization;

namespace OneActivity.Core.Components.SettingsComponents;

public partial class NotificationSettings : IDisposable
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
        Language.CultureChanged += OnCultureChanged;
        await LoadNotificationSettings();
    }

    [Inject]
    private ILanguageService Language { get; set; } = default!;

    private void OnCultureChanged() => InvokeAsync(StateHasChanged);

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
            if (_notificationsEnabled)
            {
                await LoadDiagnostics();
            }
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"{Shared.ErrorLoadingNotificationsPrefix} {ex.Message}";
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
                        normalized = normalized[..5];
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
                ? Shared.NotificationsEnabledSuccess
                : Shared.NotificationsDisabledSuccess;
            _isError = false;

            if (_notificationsEnabled)
            {
                await LoadDiagnostics();
            }
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"{Shared.ErrorUpdatingNotificationsPrefix} {ex.Message}";
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
                _message = Shared.InvalidTimeFormat;
                return;
            }

            await NotificationService.UpdateNotificationSettingsAsync(new Models.NotificationSettings
            {
                Enabled = true,
                Time = parsed
            });

            _message = Shared.NotificationTimeUpdated(_notificationTimeText);
            _isError = false;
            await LoadDiagnostics();
        }
        catch (Exception ex)
        {
            _isError = true;
            _message = $"{Shared.ErrorUpdatingNotificationTimePrefix} {ex.Message}";
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
            s = s[..5];

        if (TimeSpan.TryParseExact(s, formats, CultureInfo.InvariantCulture, out var ts))
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

    [Inject] private INotificationDiagnostics NotificationDiagnostics { get; set; } = default!;
    protected bool? ExactAlarmAllowed { get; set; }
    protected bool? IgnoringBatteryOptimizations { get; set; }

    private static readonly string[] formats = new[] { "HH\\:mm", "H\\:mm" };

    private async Task LoadDiagnostics()
    {
        try
        {
            ExactAlarmAllowed = await NotificationDiagnostics.IsExactAlarmAllowedAsync();
            IgnoringBatteryOptimizations = await NotificationDiagnostics.IsIgnoringBatteryOptimizationsAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed loading notification diagnostics");
        }
    }

    public void Dispose()
    {
        Language.CultureChanged -= OnCultureChanged;
    }

    private async Task OpenExactAlarmSettings()
    {
        try
        {
            await NotificationDiagnostics.OpenExactAlarmSettingsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to open exact alarm settings");
        }
    }

    private async Task OpenBatterySettings()
    {
        try
        {
            await NotificationDiagnostics.OpenBatteryOptimizationSettingsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to open battery optimization settings");
        }
    }
}
