using Microsoft.EntityFrameworkCore;

namespace OneActivity.Data;

public class OneActivityDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ActivityEntry> ActivityEntries => Set<ActivityEntry>();

    public OneActivityDbContext(DbContextOptions<OneActivityDbContext> options) : base(options) {}
}
