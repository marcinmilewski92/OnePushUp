using Microsoft.EntityFrameworkCore;
using OnePushUp.Data;

namespace OnePushUp;

public class DbInitializer
{
    private readonly IServiceProvider _serviceProvider;

    public DbInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OnePushUpDbContext>();
        
        // Create database if it doesn't exist
        dbContext.Database.EnsureCreated();
        
        // Alternative approach using migrations
        // Uncomment this if you prefer using migrations instead of EnsureCreated
        // dbContext.Database.Migrate();
    }
}
