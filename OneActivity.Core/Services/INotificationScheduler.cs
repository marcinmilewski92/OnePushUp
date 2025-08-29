namespace OneActivity.Core.Services;

public interface INotificationScheduler
{
    Task ScheduleAsync(TimeSpan time);
    Task CancelAsync();
    Task SendTestAsync();
}
