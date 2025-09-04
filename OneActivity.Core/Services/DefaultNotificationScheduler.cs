using Microsoft.Extensions.Logging;

namespace OneActivity.Core.Services;

public class DefaultNotificationScheduler(ILogger<DefaultNotificationScheduler> logger) : INotificationScheduler
{
    private readonly ILogger<DefaultNotificationScheduler> _logger = logger;

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
}
