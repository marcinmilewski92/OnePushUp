using Android.App;
using Android.Runtime;
using Android.Content;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using OnePushUp.Services;
using Android.OS;

namespace OnePushUp;

[Application]
public class MainApplication : MauiApplication
{
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
                    bool notificationsEnabled = Preferences.Default.Get(NotificationConstants.NotificationsEnabledKey, false);
                    if (notificationsEnabled)
                    {
                        System.Console.WriteLine("Restoring notifications on app startup");
                        
                        // Send a broadcast to restore notifications
                        Intent restoreIntent = new Intent(this, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
                        restoreIntent.SetAction(NotificationConstants.ActionRestoreNotifications);
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
                                            System.Console.WriteLine($"Restored notification for {settings.Time.Value:hh\\:mm} via service");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) {
                                System.Console.WriteLine($"Error restoring notifications via service: {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error in RestoreNotificationsOnStartup: {ex.Message}");
                }
            }), 3000); // Delay for 3 seconds to ensure app is fully initialized
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Failed to schedule restoration: {ex.Message}");
        }
    }
    
    private void SendRestoreNotificationsIntent()
    {
        try
        {
            Intent intent = new Intent(ApplicationContext, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
            intent.SetAction(NotificationConstants.ActionRestoreNotifications);
            intent.PutExtra("source", "application_startup");
            SendBroadcast(intent);
            System.Console.WriteLine("Sent restore notifications intent to receiver");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error sending restore intent: {ex.Message}");
        }
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

[BroadcastReceiver(Enabled = true)]
public class PackageReplacedReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == Intent.ActionMyPackageReplaced)
        {
            System.Console.WriteLine("App was updated, restoring notifications");
            
            // Send broadcast to our notification receiver
            Intent notificationIntent = new Intent(context, Java.Lang.Class.FromType(typeof(Platforms.Android.NotificationReceiver)));
            notificationIntent.SetAction(NotificationConstants.ActionRestoreNotifications);
            notificationIntent.PutExtra("source", "package_replaced");
            context.SendBroadcast(notificationIntent);
        }
    }
}
