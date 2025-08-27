using Microsoft.Extensions.Logging;
using OnePushUp.Data;
using OnePushUp.Repositories;
using OnePushUp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
#if ANDROID
using OnePushUp.Platforms.Android;
#endif

namespace OnePushUp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        
        // Database and repositories
        // Explicitly configure SQLite provider and reliable app data path
        builder.Services.AddDbContext<OnePushUpDbContext>(options =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "OnePushUp.db");
            options.UseSqlite($"Data Source={dbPath}");
        });
        builder.Services.AddTransient<IUsersRepository, UsersRepository>();
        builder.Services.AddTransient<ITrainingEntryRepository, TrainingEntryRepository>();
        
        // Services
        builder.Services.AddTransient<TrainingService>();
        builder.Services.AddTransient<UserService>();
#if ANDROID
        builder.Services.AddSingleton<INotificationScheduler, AndroidNotificationScheduler>();
        builder.Services.AddSingleton<IAlarmScheduler, AlarmScheduler>();
        builder.Services.AddSingleton<INotificationDisplayer, NotificationDisplayer>();
#else
        builder.Services.AddSingleton<INotificationScheduler, DefaultNotificationScheduler>();
#endif
        builder.Services.AddTransient<NotificationService>();
        builder.Services.AddSingleton<DbInitializer>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        
        // Initialize the database on startup
        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            dbInitializer.Initialize();
        }

        return app;
    }
}