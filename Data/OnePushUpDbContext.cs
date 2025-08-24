using Microsoft.EntityFrameworkCore;
using OnePushUp.Domain;

namespace OnePushUp.Data;

public class OnePushUpDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<TrainingEntry> TrainingEntries { get; set; }
    
    public OnePushUpDbContext()
    {
    }
    
    public OnePushUpDbContext(DbContextOptions<OnePushUpDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite($"Data Source=OnePushUp.db");
        }
    }
}
