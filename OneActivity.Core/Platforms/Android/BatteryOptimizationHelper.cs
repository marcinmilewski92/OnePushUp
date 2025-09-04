#if ANDROID
using global::Android.App;
using global::Android.Content;
using global::Android.OS;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;

namespace OneActivity.Core.Platforms.Android;

public class BatteryOptimizationHelper(ILogger logger)
{
    private readonly ILogger _logger = logger;

    public async Task<bool> RequestBatteryOptimizationExemptionAsync()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            return true;
            
        var powerManager = global::Android.App.Application.Context.GetSystemService(Context.PowerService) as PowerManager;
        if (powerManager?.IsIgnoringBatteryOptimizations(global::Android.App.Application.Context.PackageName) == true)
            return true;

        try {
            var intent = new Intent();
            intent.SetAction(global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
            var packageName = global::Android.App.Application.Context.PackageName;
            intent.SetData(global::Android.Net.Uri.Parse($"package:{packageName}"));
            var activity = Platform.CurrentActivity;
            activity?.StartActivity(intent);
            return true;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to request battery optimization exemption");
            return false;
        }
    }

    public static bool IsIgnoringBatteryOptimizations()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            return true;
            
        var powerManager = global::Android.App.Application.Context.GetSystemService(Context.PowerService) as PowerManager;
        return powerManager?.IsIgnoringBatteryOptimizations(global::Android.App.Application.Context.PackageName) == true;
    }
}
#endif
