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
            dbContext.Database.Migrate();
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
        }
    }
}

