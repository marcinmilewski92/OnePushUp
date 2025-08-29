using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using OneActivity.Data;
using OnePushUp.Repositories;
using OnePushUp.Services;
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

        OneActivity.Core.Hosting.OneActivityHostExtensions.UseOneActivityCore(builder, () => Path.Combine(FileSystem.AppDataDirectory, "OneBookPage.db"));

        
        builder.Services.AddSingleton<IActivityContent, ReadingContent>();
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
