using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OneActivity.Data;
using OneActivity.Core.Repositories;
using Xunit;

namespace OneActivity.Tests;

public class ActivityEntryRepositoryTests
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
    public async Task HasEntryForTodayAsync_True_WhenEntryInLocalDay()
    {
        using var db = CreateDbContext();
        var repo = new ActivityEntryRepository(db);
        var userId = Guid.NewGuid();

        db.Users.Add(new User { Id = userId, NickName = "Test" });
        db.ActivityEntries.Add(new ActivityEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Quantity = 1,
            DateTime = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var hasToday = await repo.HasEntryForTodayAsync(userId);
        Assert.True(hasToday);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ReturnsZero_WhenNoRecentEntries()
    {
        using var db = CreateDbContext();
        var repo = new ActivityEntryRepository(db);
        var userId = Guid.NewGuid();

        db.Users.Add(new User { Id = userId, NickName = "Test" });
        db.ActivityEntries.Add(new ActivityEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Quantity = 10,
            DateTime = DateTimeOffset.UtcNow.AddDays(-2)
        });
        await db.SaveChangesAsync();

        var streak = await repo.GetCurrentStreakAsync(userId);

        Assert.Equal(0, streak);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_Persists_WhenTrainingYesterday()
    {
        using var db = CreateDbContext();
        var repo = new ActivityEntryRepository(db);
        var userId = Guid.NewGuid();

        db.Users.Add(new User { Id = userId, NickName = "Test" });
        db.ActivityEntries.AddRange(
            new ActivityEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Quantity = 10,
                DateTime = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new ActivityEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Quantity = 20,
                DateTime = DateTimeOffset.UtcNow.AddDays(-1)
            }
        );
        await db.SaveChangesAsync();

        var streak = await repo.GetCurrentStreakAsync(userId);

        Assert.Equal(2, streak);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_IncludesToday()
    {
        using var db = CreateDbContext();
        var repo = new ActivityEntryRepository(db);
        var userId = Guid.NewGuid();

        db.Users.Add(new User { Id = userId, NickName = "Test" });
        db.ActivityEntries.AddRange(
            new ActivityEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Quantity = 15,
                DateTime = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new ActivityEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Quantity = 25,
                DateTime = DateTimeOffset.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var streak = await repo.GetCurrentStreakAsync(userId);

        Assert.Equal(2, streak);
    }
}
