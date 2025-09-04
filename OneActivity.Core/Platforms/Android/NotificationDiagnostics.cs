#if ANDROID
using AApp = global::Android.App.Application;
using global::Android.Content;
using global::Android.OS;
using Microsoft.Maui.ApplicationModel;
using OneActivity.Core.Services;

namespace OneActivity.Core.Platforms.Android;

public class mAndroidNotificationDiagnostics : INotificationDiagnostics
{
    public Task<bool> IsExactAlarmAllowedAsync()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.S)
            return Task.FromResult(true);
        var am = (global::Android.App.AlarmManager?)AApp.Context.GetSystemService(Context.AlarmService);
        return Task.FromResult(am?.CanScheduleExactAlarms() == true);
    }

    public Task<bool> IsIgnoringBatteryOptimizationsAsync()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            return Task.FromResult(true);
        var pm = (global::Android.OS.PowerManager?)AApp.Context.GetSystemService(Context.PowerService);
        return Task.FromResult(pm?.IsIgnoringBatteryOptimizations(AApp.Context.PackageName) == true);
    }

    public Task OpenExactAlarmSettingsAsync()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            try
            {
                var ctx = AApp.Context;
                var intent = new Intent(global::Android.Provider.Settings.ActionRequestScheduleExactAlarm);
                intent.SetData(global::Android.Net.Uri.Parse($"package:{ctx.PackageName}"));
                intent.SetFlags(ActivityFlags.NewTask);
                ctx.StartActivity(intent);
            }
            catch { }
        }
        return Task.CompletedTask;
    }

    public Task OpenBatteryOptimizationSettingsAsync()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            try
            {
                var ctx = AApp.Context;
                // First try direct request for exemption dialog
                var intent = new Intent(global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations)
                    .SetData(global::Android.Net.Uri.Parse($"package:{ctx.PackageName}"))
                    .SetFlags(ActivityFlags.NewTask);
                ctx.StartActivity(intent);
            }
            catch
            {
                try
                {
                    // Fall back to the general battery optimization settings list
                    var fallback = new Intent(global::Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings)
                        .SetFlags(ActivityFlags.NewTask);
                    AApp.Context.StartActivity(fallback);
                }
                catch
                {
                    try
                    {
                        // Last resort: open app details page
                        var details = new Intent(global::Android.Provider.Settings.ActionApplicationDetailsSettings)
                            .SetData(global::Android.Net.Uri.Parse($"package:{AApp.Context.PackageName}"))
                            .SetFlags(ActivityFlags.NewTask);
                        AApp.Context.StartActivity(details);
                    }
                    catch { }
                }
            }
        }
        return Task.CompletedTask;
    }
}
#endif
