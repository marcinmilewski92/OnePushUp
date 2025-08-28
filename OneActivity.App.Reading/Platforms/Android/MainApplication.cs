using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OneActivity.App.Reading;

[Application]
public class MainApplication : MauiApplication
{
    private ILogger<MainApplication> Logger => _logger ??=
        Services.GetService<ILogger<MainApplication>>() ?? NullLogger<MainApplication>.Instance;
    private ILogger<MainApplication>? _logger;

    public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) {}

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void OnCreate()
    {
        base.OnCreate();
        IntentFilter filter = new IntentFilter();
        filter.AddAction(Intent.ActionMyPackageReplaced);
        RegisterReceiver(new PackageReplacedReceiver(), filter);
    }

    private class PackageReplacedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent) { }
    }
}

