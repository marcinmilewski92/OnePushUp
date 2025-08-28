using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using OneActivity.Data;
using OnePushUp.Repositories;
using OnePushUp.Services;
using OneActivity.App.Pushups.Flavors.Pushups;
using OneActivity.Core.Hosting;
#if ANDROID
using OneActivity.Core.Platforms.Android;
#endif
#if IOS || MACCATALYST
using OneActivity.Core.Platforms.Apple;
#endif

namespace OneActivity.App.Pushups;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.UseOneActivityCore(() => Path.Combine(FileSystem.AppDataDirectory, "OnePushUp.db"));

        
        builder.Services.AddSingleton<IActivityContent, PushupContent>();
        builder.Services.AddSingleton<IActivityBranding, PushupBranding>();
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
