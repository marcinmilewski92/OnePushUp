using Microsoft.Extensions.Logging;
using OnePushUp.Data;
using OnePushUp.Repositories;
using OnePushUp.Services;
using Microsoft.EntityFrameworkCore;

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
        builder.Services.AddDbContext<OnePushUpDbContext>();
        builder.Services.AddTransient<UsersRepository>();
        builder.Services.AddTransient<TrainingEntryRepository>();
        
        // Services
        builder.Services.AddTransient<TrainingService>();
        builder.Services.AddTransient<UserService>();
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