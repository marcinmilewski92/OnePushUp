using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using OneActivity.Data;
using OneActivity.Core.Repositories;
using OneActivity.Core.Services;
using OneActivity.App.Reading.Flavors.Reading;
using OneActivity.Core.Hosting;
#if ANDROID
using OneActivity.Core.Platforms.Android;
#endif
#if IOS || MACCATALYST
using OneActivity.Core.Platforms.Apple;
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

        builder.UseOneActivityCore(() => Path.Combine(FileSystem.AppDataDirectory, "OneReading.db"));

        // Language-aware content providers
        builder.Services.AddSingleton<ReadingContentEn>();
        builder.Services.AddSingleton<ReadingContentPl>();
        builder.Services.AddSingleton<IActivityContent, ReadingContentLocalized>();
        // Shared content providers (Settings)
        builder.Services.AddSingleton<SharedContentEn>();
        builder.Services.AddSingleton<SharedContentPl>();
        builder.Services.AddSingleton<ISharedContent, SharedContentLocalized>();
        builder.Services.AddSingleton<IActivityBranding, ReadingBranding>();

        #if ANDROID
        // Register notification reliability components
        builder.Services.AddSingleton<INotificationDisplayer, NotificationDisplayer>();
        builder.Services.AddSingleton<IAlarmScheduler, AlarmScheduler>();
        #endif

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
