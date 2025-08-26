using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnePushUp.Data;

public class OnePushUpDbContextFactory : IDesignTimeDbContextFactory<OnePushUpDbContext>
{
    public OnePushUpDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OnePushUpDbContext>();
        optionsBuilder.UseSqlite("Data Source=OnePushUp.db");

        return new OnePushUpDbContext(optionsBuilder.Options);
    }
}
