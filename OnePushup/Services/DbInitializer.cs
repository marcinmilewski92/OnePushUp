using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnePushUp.Data;

namespace OnePushUp.Services;

public class DbInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OnePushUpDbContext>();
            
            _logger.LogInformation("Starting database initialization...");
            
            // Apply any pending migrations
            // This will create the database if it doesn't exist
            // and bring it up to date with the latest schema
            dbContext.Database.Migrate();
            
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            // In a production app, you might want to handle this more gracefully
            // For example, showing a friendly error message to the user
        }
    }
}
