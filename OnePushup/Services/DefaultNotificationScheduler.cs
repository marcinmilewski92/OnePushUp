using Microsoft.Extensions.Logging;

namespace OnePushUp.Services;

public class DefaultNotificationScheduler : INotificationScheduler
{
    private readonly ILogger<DefaultNotificationScheduler> _logger;

    public DefaultNotificationScheduler(ILogger<DefaultNotificationScheduler> logger)
    {
        _logger = logger;
    }

    public Task ScheduleAsync(TimeSpan time)
    {
        _logger.LogInformation("Notification scheduling is not supported on this platform");
        return Task.CompletedTask;
    }

    public Task CancelAsync()
    {
        _logger.LogInformation("Notification cancellation is not supported on this platform");
        return Task.CompletedTask;
    }

    public Task SendTestAsync()
    {
        _logger.LogInformation("Test notifications are not supported on this platform");
        return Task.CompletedTask;
    }
}

