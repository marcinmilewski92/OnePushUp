using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using OnePushUp.Data;
using OnePushUp.Repositories;
using OnePushUp.Services;
using OneActivity.App.Reading.Flavors.Reading;
#if ANDROID
using OneActivity.App.Reading.Platforms.Android;
#endif

namespace OneActivity.App.Reading;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddDbContext<OnePushUpDbContext>(options =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "OnePushUp.db");
            options.UseSqlite($"Data Source={dbPath}");
        });
        builder.Services.AddTransient<IUsersRepository, UsersRepository>();
        builder.Services.AddTransient<IActivityEntryRepository, ActivityEntryRepository>();

        builder.Services.AddTransient<ActivityService>();
        builder.Services.AddSingleton<IActivityContent, ReadingContent>();
        builder.Services.AddSingleton<IActivityBranding, ReadingBranding>();
        builder.Services.AddTransient<UserService>();
        builder.Services.AddSingleton<INotificationScheduler, DefaultNotificationScheduler>();
        builder.Services.AddTransient<NotificationService>();
        builder.Services.AddSingleton<DbInitializer>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            dbInitializer.Initialize();
        }

        return app;
    }
}
