using Microsoft.Extensions.Logging;
using OnePushUp.Services;

namespace OneActivity.App.Pushups.Platforms.Android;

public class AndroidNotificationScheduler : INotificationScheduler
{
    private readonly ILogger<AndroidNotificationScheduler> _logger;
    public AndroidNotificationScheduler(ILogger<AndroidNotificationScheduler> logger) { _logger = logger; }
    public Task SendTestAsync() { _logger.LogInformation("Android test notification placeholder"); return Task.CompletedTask; }
    public Task ScheduleAsync(TimeSpan time) { _logger.LogInformation("Android schedule at {Time}", time); return Task.CompletedTask; }
    public Task CancelAsync() { _logger.LogInformation("Android cancel notifications"); return Task.CompletedTask; }
}

