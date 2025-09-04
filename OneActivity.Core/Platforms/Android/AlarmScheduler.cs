#if ANDROID
using global::Android.Content;
using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OneActivity.Core.Platforms.Android;

public class AlarmScheduler(ILogger<AlarmScheduler> logger, INotificationScheduler scheduler) : IAlarmScheduler
{
    private readonly ILogger<AlarmScheduler> _logger = logger;
    private readonly NotificationService? _notificationService = MauiApplication.Current?.Services?.GetService<NotificationService>();
    private readonly INotificationScheduler _scheduler = scheduler;

    public void RestoreNotificationsAfterReboot(Context context)
    {
        try
        {
            _logger.LogInformation("Restoring notifications after device reboot or app update");
            
            // Use Task.Run to avoid blocking the main thread
            Task.Run(async () => {
                try
                {
                    if (_notificationService == null)
                    {
                        _logger.LogError("Failed to get NotificationService from DI container");
                        return;
                    }
                    
                    var settings = await _notificationService.GetNotificationSettingsAsync();
                    if (settings.Enabled && settings.Time.HasValue)
                    {
                        await _scheduler.ScheduleAsync(settings.Time.Value);
                        _logger.LogInformation("Successfully restored notification for {Time}", settings.Time.Value);
                    }
                    else
                    {
                        _logger.LogInformation("Notifications not enabled, skipping restoration");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RestoreNotificationsAfterReboot");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore notifications after reboot");
        }
    }

    public void RescheduleForTomorrow(Context context)
    {
        try
        {
            _logger.LogInformation("Rescheduling notifications for tomorrow");
            
            // Use Task.Run to avoid blocking the main thread
            Task.Run(async () => {
                try
                {
                    if (_notificationService == null)
                    {
                        _logger.LogError("Failed to get NotificationService from DI container");
                        return;
                    }
                    
                    var settings = await _notificationService.GetNotificationSettingsAsync();
                    if (settings.Enabled && settings.Time.HasValue)
                    {
                        await _scheduler.ScheduleAsync(settings.Time.Value);
                        _logger.LogInformation("Successfully rescheduled notification for tomorrow at {Time}", settings.Time.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RescheduleForTomorrow");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reschedule notification for tomorrow");
        }
    }
}
#endif
