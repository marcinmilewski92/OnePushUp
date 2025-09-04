#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using OneActivity.Core.Services;
using OneActivity.Data;
using System;

namespace OneActivity.Core.Platforms.Android;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
public class NotificationForegroundService : Service
{
    private ILogger<NotificationForegroundService> Logger => _logger ??=
        MauiApplication.Current.Services?.GetService<ILogger<NotificationForegroundService>>() ??
        Microsoft.Extensions.Logging.Abstractions.NullLogger<NotificationForegroundService>.Instance;
    private ILogger<NotificationForegroundService>? _logger;
    private readonly IActivityContent? _activityContent;

    public override IBinder? OnBind(Intent? intent) => null;
    
    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        try
        {
            var notificationBuilder = new NotificationCompat.Builder(this, NotificationDisplayer.ChannelId)
                .SetContentTitle("Preparing your reminder")
                .SetContentText("Ensuring your daily notification is delivered")
                .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                .SetOngoing(true);
                
            // Create notification channel if needed
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(NotificationDisplayer.ChannelId, 
                    $"Daily Reminders", NotificationImportance.High)
                {
                    Description = $"Daily activity reminders",
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channel.EnableVibration(true);
                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);
            }
                
            StartForeground(42, notificationBuilder.Build());
            
            // Schedule actual notification delivery with a slight delay
            var handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(() => {
                try
                {
                    var notificationDisplayer = MauiApplication.Current.Services?.GetService<INotificationDisplayer>();
                    var newIntent = new Intent();
                    newIntent.PutExtra(NotificationIntentConstants.ExtraApproach, "foreground_service");
                    notificationDisplayer?.ShowActivityNotification(this, newIntent);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error showing notification from foreground service");
                }
                finally
                {
                    StopSelf();
                }
            }, 5000);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in foreground service");
            StopSelf();
        }
        
        return StartCommandResult.Sticky;
    }
}
#endif
