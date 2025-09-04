namespace OneActivity.Core.Services;

public class DefaultNotificationDiagnostics : INotificationDiagnostics
{
    public Task<bool> IsExactAlarmAllowedAsync() => Task.FromResult(true);
    public Task<bool> IsIgnoringBatteryOptimizationsAsync() => Task.FromResult(true);
    public Task OpenExactAlarmSettingsAsync() => Task.CompletedTask;
    public Task OpenBatteryOptimizationSettingsAsync() => Task.CompletedTask;
}

