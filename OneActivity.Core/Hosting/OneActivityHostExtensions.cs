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
using OnePushUp.Repositories;
using OnePushUp.Services;

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
            options.UseSqlite($"Data Source={path}");
        });
        builder.Services.AddTransient<IUsersRepository, UsersRepository>();
        builder.Services.AddTransient<IActivityEntryRepository, ActivityEntryRepository>();

        // Services
        builder.Services.AddTransient<ActivityService>();
        builder.Services.AddTransient<UserService>();
        builder.Services.AddTransient<NotificationService>();
        builder.Services.AddSingleton<DbInitializer>();

        // Platform schedulers
#if ANDROID
        builder.Services.AddSingleton<INotificationScheduler, AndroidNotificationScheduler>();
        builder.Services.AddSingleton<IAlarmScheduler, AlarmScheduler>();
        builder.Services.AddSingleton<INotificationDisplayer, NotificationDisplayer>();
#elif IOS || MACCATALYST
        builder.Services.AddSingleton<INotificationScheduler, AppleNotificationScheduler>();
#else
        builder.Services.AddSingleton<INotificationScheduler, DefaultNotificationScheduler>();
#endif

        return builder;
    }
}
