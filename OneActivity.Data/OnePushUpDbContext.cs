using Microsoft.EntityFrameworkCore;

namespace OneActivity.Data;

public class OneActivityDbContext(DbContextOptions<OneActivityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ActivityEntry> ActivityEntries => Set<ActivityEntry>();
}
