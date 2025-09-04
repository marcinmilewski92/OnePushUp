namespace OneActivity.Core.Services;

public interface INotificationDiagnostics
{
    Task<bool> IsExactAlarmAllowedAsync();
    Task<bool> IsIgnoringBatteryOptimizationsAsync();
    Task OpenExactAlarmSettingsAsync();
    Task OpenBatteryOptimizationSettingsAsync();
}

