namespace OnePushUp.Services;

public interface INotificationScheduler
{
    Task ScheduleAsync(TimeSpan time);
    Task CancelAsync();
    Task SendTestAsync();
}

