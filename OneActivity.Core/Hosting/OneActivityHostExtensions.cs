using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;
#if ANDROID
using OneActivity.Core.Platforms.Android;
#endif
#if IOS || MACCATALYST
using OneActivity.Core.Platforms.Apple;
#endif
using OneActivity.Data;
using OneActivity.Core.Repositories;
using OneActivity.Core.Services;

namespace OneActivity.Core.Hosting;

public static class OneActivityHostExtensions
{
    public static MauiAppBuilder UseOneActivityCore(this MauiAppBuilder builder, Func<string>? dbPathFactory = null)
    {
        builder.Services.AddMauiBlazorWebView();

        // Database + repositories
        builder.Services.AddDbContext<OneActivityDbContext>(options =>
        {
            var path = dbPathFactory?.Invoke() ?? Path.Combine(FileSystem.AppDataDirectory, "OnePushUp.db");
            // Specify migrations assembly explicitly to avoid trimming issues on Android
            options.UseSqlite(
                $"Data Source={path}",
                b => b.MigrationsAssembly(typeof(OneActivityDbContext).Assembly.FullName)
            );
        });
        builder.Services.AddTransient<IUsersRepository, UsersRepository>();
        builder.Services.AddTransient<IActivityEntryRepository, ActivityEntryRepository>();

        // Services
        builder.Services.AddTransient<ActivityService>();
        builder.Services.AddTransient<UserService>();
        builder.Services.AddTransient<NotificationService>();
        builder.Services.AddSingleton<DbInitializer>();
        builder.Services.AddSingleton<ILanguageService, LanguageService>();
        builder.Services.AddSingleton<IGenderService, GenderService>();

        // Platform schedulers
#if ANDROID
        builder.Services.AddSingleton<INotificationScheduler, AndroidNotificationScheduler>();
        builder.Services.AddSingleton<IAlarmScheduler, AlarmScheduler>();
        builder.Services.AddSingleton<INotificationDisplayer, NotificationDisplayer>();
        builder.Services.AddSingleton<INotificationDiagnostics, AndroidNotificationDiagnostics>();
#elif IOS || MACCATALYST
        builder.Services.AddSingleton<INotificationScheduler, AppleNotificationScheduler>();
        builder.Services.AddSingleton<INotificationDiagnostics, DefaultNotificationDiagnostics>();
#else
        builder.Services.AddSingleton<INotificationScheduler, DefaultNotificationScheduler>();
        builder.Services.AddSingleton<INotificationDiagnostics, DefaultNotificationDiagnostics>();
#endif

        return builder;
    }
}
