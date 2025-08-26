using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace OnePushUp.Data;

public class OnePushUpDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TrainingEntry> TrainingEntries => Set<TrainingEntry>();

    public OnePushUpDbContext(DbContextOptions<OnePushUpDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OnePushUp.db");
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}
