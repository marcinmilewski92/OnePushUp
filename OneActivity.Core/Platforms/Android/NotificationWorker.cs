#if ANDROID
using global::Android.Content;
using AndroidX.Work;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;
using Java.Util.Concurrent;

namespace OneActivity.Core.Platforms.Android;

public class NotificationWorker : Worker
{
    private readonly ILogger<NotificationWorker> _logger;

    private INotificationDisplayer? NotificationDisplayer => _displayer ??=
        MauiApplication.Current?.Services?.GetService<INotificationDisplayer>();
    private INotificationDisplayer? _displayer;
    
    public NotificationWorker(Context context, WorkerParameters workerParams) 
        : base(context, workerParams) 
    {
        _logger = MauiApplication.Current?.Services?.GetService<ILogger<NotificationWorker>>() ??
                  Microsoft.Extensions.Logging.Abstractions.NullLogger<NotificationWorker>.Instance;
    }
    
    public override Result DoWork()
    {
        try
        {
            _logger.LogInformation("NotificationWorker executing");
            var intent = new Intent();
            intent.PutExtra(NotificationIntentConstants.ExtraApproach, "workmanager");
            NotificationDisplayer?.ShowActivityNotification(ApplicationContext, intent);
            return Result.InvokeSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NotificationWorker");
            return Result.InvokeFailure();
        }
    }
}

public static class WorkManagerExtensions
{
    public static void ScheduleWorkManagerBackup(this INotificationScheduler scheduler, TimeSpan time)
    {
        var delay = CalculateDelayToTime(time);
        var workRequest = OneTimeWorkRequest.Builder
            .From<NotificationWorker>()
            .SetInitialDelay(delay, TimeUnit.Milliseconds)
            .AddTag("daily_notification")
            .Build();

        var context = global::Android.App.Application.Context;
        WorkManager.GetInstance(context)
            .EnqueueUniqueWork("daily_notification", ExistingWorkPolicy.Replace, (OneTimeWorkRequest)workRequest);
    }
    
    private static long CalculateDelayToTime(TimeSpan targetTime)
    {
        var now = DateTime.Now;
        var target = new DateTime(now.Year, now.Month, now.Day, 
            targetTime.Hours, targetTime.Minutes, targetTime.Seconds);
        
        if (target <= now)
            target = target.AddDays(1);
            
        var delay = target - now;
        return (long)delay.TotalMilliseconds;
    }
}
#endif
