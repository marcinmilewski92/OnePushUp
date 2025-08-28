using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OneActivity.Data;

public class OneActivityDbContextFactory : IDesignTimeDbContextFactory<OneActivityDbContext>
{
    public OneActivityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OneActivityDbContext>();
        optionsBuilder.UseSqlite("Data Source=OnePushUp.db");

        return new OneActivityDbContext(optionsBuilder.Options);
    }
}
