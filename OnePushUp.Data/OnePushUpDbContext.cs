using Microsoft.EntityFrameworkCore;

namespace OnePushUp.Data;

public class OnePushUpDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ActivityEntry> ActivityEntries => Set<ActivityEntry>();

    public OnePushUpDbContext(DbContextOptions<OnePushUpDbContext> options) : base(options) {}
}

