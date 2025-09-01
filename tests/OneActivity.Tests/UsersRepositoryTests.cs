using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OneActivity.Core.Repositories;
using OneActivity.Data;
using Xunit;

namespace OneActivity.Tests;

public class UsersRepositoryTests
{
    private static OneActivityDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<OneActivityDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new OneActivityDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task UpdateAsync_PersistsGenderChange()
    {
        using var db = CreateDbContext();
        var repo = new UsersRepository(db);

        var user = new User
        {
            Id = Guid.NewGuid(),
            NickName = "Initial",
            Gender = 0
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var updatedUser = new User
        {
            Id = user.Id,
            NickName = "Updated",
            Gender = 1
        };

        var result = await repo.UpdateAsync(updatedUser);

        Assert.True(result);
        var storedUser = await db.Users.FindAsync(user.Id);
        Assert.NotNull(storedUser);
        Assert.Equal(1, storedUser!.Gender);
    }
}
