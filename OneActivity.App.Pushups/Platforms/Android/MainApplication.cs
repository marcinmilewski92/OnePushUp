using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OneActivity.App.Pushups;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    private ILogger<MainApplication> Logger => _logger ??=
        Services.GetService<ILogger<MainApplication>>() ?? NullLogger<MainApplication>.Instance;
    private ILogger<MainApplication>? _logger;

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void OnCreate()
    {
        base.OnCreate();
        RegisterForPackageReplaced();
        RestoreNotificationsOnStartup();
    }

    private void RegisterForPackageReplaced()
    {
        IntentFilter filter = new();
        filter.AddAction(Intent.ActionMyPackageReplaced);
        RegisterReceiver(new PackageReplacedReceiver(), filter);
    }

    private void RestoreNotificationsOnStartup()
    {
        try
        {
            var handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(new Java.Lang.Runnable(() =>
            {
                try
                {
                    bool notificationsEnabled = Microsoft.Maui.Storage.Preferences.Default.Get("notifications_enabled", false);
                    if (notificationsEnabled)
                    {
                        Logger.LogInformation("Restoring notifications on app startup");
                        Intent restoreIntent = new(this, Java.Lang.Class.FromType(typeof(OneActivity.Core.Platforms.Android.NotificationReceiver)));
                        restoreIntent.SetAction(OneActivity.Core.Platforms.Android.NotificationIntentConstants.ActionRestoreNotifications);
                        SendBroadcast(restoreIntent);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error scheduling notifications on startup");
                }
            }), 2000);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in RestoreNotificationsOnStartup");
        }
    }

    private class PackageReplacedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            // No-op; NotificationReceiver handles restoration
        }
    }
}
