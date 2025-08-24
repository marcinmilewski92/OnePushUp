using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;

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
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OnePushUp.db");
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}