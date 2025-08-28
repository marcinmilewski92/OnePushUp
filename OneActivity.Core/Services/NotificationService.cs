using Microsoft.Extensions.Logging;
using System.Globalization;
using OnePushUp.Models;
using Preferences = Microsoft.Maui.Storage.Preferences;

namespace OnePushUp.Services;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly INotificationScheduler _scheduler;
    private const string EnabledKey = "notifications_enabled";
    private const string TimeKey = "notification_time";
    public static readonly TimeSpan DefaultNotificationTime = TimeSpan.FromHours(8);

    public NotificationService(ILogger<NotificationService> logger, INotificationScheduler scheduler)
    {
        _logger = logger;
        _scheduler = scheduler;
    }

    public async Task<NotificationSettings> GetNotificationSettingsAsync()
    {
        try
        {
            var enabled = Preferences.Default.Get(EnabledKey, false);

            var timeTicks = Preferences.Default.Get(TimeKey, 0L);
            TimeSpan? time = timeTicks > 0 ? TimeSpan.FromTicks(timeTicks) : DefaultNotificationTime;

            return new NotificationSettings
            {
                Enabled = enabled,
                Time = time
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification settings");
            return new NotificationSettings
            {
                Enabled = false,
                Time = DefaultNotificationTime
            };
        }
    }

    public async Task UpdateNotificationSettingsAsync(NotificationSettings settings)
    {
        try
        {
            Preferences.Default.Set(EnabledKey, settings.Enabled);

            if (settings.Time.HasValue)
            {
                Preferences.Default.Set(TimeKey, settings.Time.Value.Ticks);
            }

            if (settings.Enabled && settings.Time.HasValue)
            {
                await _scheduler.ScheduleAsync(settings.Time.Value);
                var timeText = settings.Time.HasValue
                    ? settings.Time.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
                    : "None";
                _logger.LogInformation("Notification settings updated - Enabled: {Enabled}, Time: {Time}", settings.Enabled, timeText);
            }
            else
            {
                await _scheduler.CancelAsync();
                _logger.LogInformation("Notifications disabled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification settings");
            throw;
        }
    }

    public async Task SetNotificationsEnabledAsync(bool enabled)
    {
        try
        {
            var current = await GetNotificationSettingsAsync();
            var updated = new NotificationSettings
            {
                Enabled = enabled,
                Time = current.Time
            };

            await UpdateNotificationSettingsAsync(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting notification enabled state");
            throw;
        }
    }

    public async Task SetNotificationTimeAsync(TimeSpan time)
    {
        try
        {
            var current = await GetNotificationSettingsAsync();
            var updated = new NotificationSettings
            {
                Enabled = current.Enabled,
                Time = time
            };

            await UpdateNotificationSettingsAsync(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting notification time");
            throw;
        }
    }

    public async Task SendTestNotificationAsync()
    {
        _logger.LogInformation("Sending test notification...");
        await _scheduler.SendTestAsync();
    }
}

