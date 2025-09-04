#if IOS || MACCATALYST
using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;
using UserNotifications;
using Foundation;

namespace OneActivity.Core.Platforms.Apple;

public class AppleNotificationScheduler(ILogger<AppleNotificationScheduler> logger, IActivityContent content) : INotificationScheduler
{
    private readonly ILogger<AppleNotificationScheduler> _logger = logger;
    private readonly IActivityContent _content = content;

    public async Task ScheduleAsync(TimeSpan time) => await ScheduleAsync(new[] { time });

    public async Task ScheduleAsync(IEnumerable<TimeSpan> times)
    {
        if (!await EnsurePermissionAsync()) return;
        UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
        int i = 0;
        foreach (var t in times)
        {
            var dc = new NSDateComponents { Hour = t.Hours, Minute = t.Minutes };
            var trigger = UNCalendarNotificationTrigger.CreateTrigger(dc, true);
            var content = new UNMutableNotificationContent
            {
                Title = $"{_content.AppName} Reminder",
                Body = $"Time to {_content.Verb} your daily {_content.UnitSingular}!",
                Sound = UNNotificationSound.Default
            };
            var id = $"activity_daily_{t.Hours:D2}{t.Minutes:D2}_{i++}";
            var request = UNNotificationRequest.FromIdentifier(id, content, trigger);
            await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
        }
    }

    public Task CancelAsync()
    {
        UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
        UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();
        return Task.CompletedTask;
    }

    private static async Task<bool> EnsurePermissionAsync()
    {
        var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
        if (settings.AuthorizationStatus == UNAuthorizationStatus.Authorized || settings.AuthorizationStatus == UNAuthorizationStatus.Provisional)
            return true;
        var (approved, _) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound);
        return approved;
    }
}
#endif
