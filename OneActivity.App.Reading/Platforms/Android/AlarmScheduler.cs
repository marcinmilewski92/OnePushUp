using Android.Content;
using Microsoft.Extensions.Logging;

namespace OneActivity.App.Reading.Platforms.Android;

public interface IAlarmScheduler
{
    void RestoreNotificationsAfterReboot(Context context);
    void RescheduleForTomorrow(Context context);
}

public class AlarmScheduler : IAlarmScheduler
{
    private readonly ILogger<AlarmScheduler> _logger;
    public AlarmScheduler(ILogger<AlarmScheduler> logger) { _logger = logger; }
    public void RestoreNotificationsAfterReboot(Context context) { _logger.LogInformation("Restore notifications no-op"); }
    public void RescheduleForTomorrow(Context context) { _logger.LogInformation("Reschedule no-op"); }
}

