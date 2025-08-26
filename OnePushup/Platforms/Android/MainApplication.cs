using Android.App;
using Android.Runtime;
using Android.Content;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using OnePushUp.Services;
using Android.OS;

namespace OnePushUp;

[Application]
public class MainApplication : MauiApplication
{
    private ILogger<MainApplication> Logger => _logger ??=
        Services.GetService<ILogger<MainApplication>>() ?? NullLogger<MainApplication>.Instance;
    private ILogger<MainApplication>? _logger;
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
        
        // Register for package replaced events (app updates)
        RegisterForPackageReplaced();
        
        // Restore notifications on app startup
        RestoreNotificationsOnStartup();
    }
    
    private void RegisterForPackageReplaced()
    {
        IntentFilter filter = new IntentFilter();
        filter.AddAction(Intent.ActionMyPackageReplaced);
        RegisterReceiver(new PackageReplacedReceiver(), filter);
    }
    
    private void RestoreNotificationsOnStartup()
    {
        try
        {
            var handler = new Handler(Looper.MainLooper);
            
            // Delay to ensure app is fully initialized
            handler.PostDelayed(new Java.Lang.Runnable(() => 
            {
                try
                {
                    bool notificationsEnabled = Preferences.Default.Get("notifications_enabled", false);
                    if (notificationsEnabled)
                    {
                        Logger.LogInformation("Restoring notifications on app startup");
                        
                        // Send a broadcast to restore notifications
                        Intent restoreIntent = new Intent(this, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
                        restoreIntent.SetAction("RESTORE_NOTIFICATIONS");
                        SendBroadcast(restoreIntent);
                        
                        // Also try to schedule directly through the service for redundancy
                        Task.Run(async () => {
                            try {
                                // Get notification service from DI
                                using (var scope = Services.CreateScope())
                                {
                                    var notificationService = scope.ServiceProvider.GetService<NotificationService>();
                                    if (notificationService != null)
                                    {
                                        var settings = await notificationService.GetNotificationSettingsAsync();
                                        if (settings.Enabled && settings.Time.HasValue)
                                        {
                                            await notificationService.UpdateNotificationSettingsAsync(settings);
                                            Logger.LogInformation("Restored notification for {Time} via service", settings.Time.Value.ToString("hh\\:mm"));
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) {
                                Logger.LogError(ex, "Error restoring notifications via service");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error in RestoreNotificationsOnStartup");
                }
            }), 3000); // Delay for 3 seconds to ensure app is fully initialized
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to schedule restoration");
        }
    }
    
    private void SendRestoreNotificationsIntent()
    {
        try
        {
            Intent intent = new Intent(ApplicationContext, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
            intent.SetAction("RESTORE_NOTIFICATIONS");
            intent.PutExtra("source", "application_startup");
            SendBroadcast(intent);
            Logger.LogInformation("Sent restore notifications intent to receiver");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending restore intent");
        }
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

[BroadcastReceiver(Enabled = true)]
public class PackageReplacedReceiver : BroadcastReceiver
{
    private ILogger<PackageReplacedReceiver> Logger => _logger ??=
        MauiApplication.Current?.Services?.GetService<ILogger<PackageReplacedReceiver>>()
        ?? NullLogger<PackageReplacedReceiver>.Instance;
    private ILogger<PackageReplacedReceiver>? _logger;
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == Intent.ActionMyPackageReplaced)
        {
            Logger.LogInformation("App was updated, restoring notifications");
            
            // Send broadcast to our notification receiver
            Intent notificationIntent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
            notificationIntent.SetAction("RESTORE_NOTIFICATIONS");
            notificationIntent.PutExtra("source", "package_replaced");
            context.SendBroadcast(notificationIntent);
        }
    }
}
