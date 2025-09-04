using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OneActivity.App.Reading;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    private ILogger<MainApplication>? _logger;

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void OnCreate()
    {
        base.OnCreate();
        IntentFilter filter = new();
        filter.AddAction(Intent.ActionMyPackageReplaced);
        RegisterReceiver(new PackageReplacedReceiver(), filter);

        // Attempt to restore notifications shortly after startup
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
                        var restoreIntent = new Intent(this, Java.Lang.Class.FromType(typeof(OneActivity.Core.Platforms.Android.NotificationReceiver)));
                        restoreIntent.SetAction(OneActivity.Core.Platforms.Android.NotificationIntentConstants.ActionRestoreNotifications);
                        SendBroadcast(restoreIntent);
                    }
                }
                catch (Exception) { }
            }), 2000);
        }
        catch (Exception) { }
    }

    private class PackageReplacedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent) { }
    }
}
