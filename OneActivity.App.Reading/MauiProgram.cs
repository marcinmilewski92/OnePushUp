using Microsoft.Extensions.Logging;
using OneActivity.Core.Services;
using OneActivity.App.Reading.Flavors.Reading;
#if ANDROID
using OneActivity.Core.Platforms.Android;
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

        OneActivity.Core.Hosting.OneActivityHostExtensions.UseOneActivityCore(builder, () => Path.Combine(FileSystem.AppDataDirectory, "OnePushUp.db"));

        // Language-aware content providers
        builder.Services.AddSingleton<ReadingContentEn>();
        builder.Services.AddSingleton<ReadingContentPl>();
        builder.Services.AddSingleton<IActivityContent, ReadingContentLocalized>();
        // Shared content providers (Settings)
        builder.Services.AddSingleton<SharedContentEn>();
        builder.Services.AddSingleton<SharedContentPl>();
        builder.Services.AddSingleton<ISharedContent, SharedContentLocalized>();
        builder.Services.AddSingleton<IActivityBranding, ReadingBranding>();
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
