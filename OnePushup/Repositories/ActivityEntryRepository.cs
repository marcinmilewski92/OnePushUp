using Microsoft.EntityFrameworkCore;
using OneActivity.Data;

namespace OnePushUp.Repositories;

public class ActivityEntryRepository : IActivityEntryRepository
{
    private readonly OneActivityDbContext _db;

    private readonly record struct DailyTotal(DateTime Date, int Total);
    
    public ActivityEntryRepository(OneActivityDbContext db)
    {
        _db = db;
    }
    
    public async Task<Guid> CreateAsync(ActivityEntry entry)
    {
        if (entry.DateTime == default)
        {
            entry.DateTime = DateTimeOffset.UtcNow;
        }
        else if (entry.DateTime.Offset != TimeSpan.Zero)
        {
            entry.DateTime = entry.DateTime.ToUniversalTime();
        }

        var entryResult = await _db.ActivityEntries.AddAsync(entry);
        await _db.SaveChangesAsync();
        return entryResult.Entity.Id;
    }
    
    public async Task<ActivityEntry?> GetEntryByIdAsync(Guid entryId)
    {
        return await _db.ActivityEntries.FindAsync(entryId);
    }
    
    public async Task<bool> UpdateAsync(ActivityEntry entry)
    {
        _db.ActivityEntries.Update(entry);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }
    
    public async Task<bool> HasEntryForTodayAsync(Guid userId)
    {
        var (start, end) = GetUserLocalDateRange();
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        var entries = await _db.ActivityEntries.AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => new { e.DateTime })
            .ToListAsync();

        return entries.Any(e => e.DateTime >= startUtc && e.DateTime < endUtc);
    }

    public async Task<ActivityEntry?> GetEntryForTodayAsync(Guid userId)
    {
        var (start, end) = GetUserLocalDateRange();
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        var entries = await _db.ActivityEntries.AsNoTracking()
            .Where(e => e.UserId == userId)
            .ToListAsync();

        return entries
            .Where(e => e.DateTime >= startUtc && e.DateTime < endUtc)
            .OrderByDescending(e => e.DateTime)
            .FirstOrDefault();
    }
    
    public async Task<List<ActivityEntry>> GetEntriesForUserAsync(Guid userId)
    {
        return await _db.ActivityEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();
    }

    private async Task<List<DailyTotal>> GetEntriesByLocalDateAsync(Guid userId)
    {
        var entries = await _db.ActivityEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Quantity > 0)
            .ToListAsync();

        return entries
            .GroupBy(e => e.DateTime.ToLocalTime().Date)
            .Select(g => new DailyTotal(g.Key, g.Sum(e => e.Quantity)))
            .OrderByDescending(dt => dt.Date)
            .ToList();
    }

    private async Task<(List<DateTime> streakDates, List<DailyTotal> entriesByLocalDate)> GetOrderedStreakDatesAsync(Guid userId)
    {
        var todayEntry = await GetEntryForTodayAsync(userId);

        if (todayEntry != null && todayEntry.Quantity == 0)
        {
            return (new List<DateTime>(), new List<DailyTotal>());
        }

        var entriesByLocalDate = await GetEntriesByLocalDateAsync(userId);
        if (!entriesByLocalDate.Any())
        {
            return (new List<DateTime>(), entriesByLocalDate);
        }

        var orderedDates = entriesByLocalDate.Select(e => e.Date).ToList();

        var streak = 1;
        for (int i = 0; i < orderedDates.Count - 1; i++)
        {
            var currentDate = orderedDates[i];
            var previousDate = orderedDates[i + 1];
            var daysBetween = (currentDate - previousDate).Days;

            if (daysBetween == 1)
            {
                streak++;
            }
            else
            {
                break;
            }
        }

        var streakDates = orderedDates.Take(streak).ToList();
        return (streakDates, entriesByLocalDate);
    }
    
    public async Task<int> GetCurrentStreakAsync(Guid userId)
    {
        var (streakDates, _) = await GetOrderedStreakDatesAsync(userId);
        return streakDates.Count;
    }
    
    public async Task<int> GetTotalQuantityInCurrentStreakAsync(Guid userId)
    {
        var (streakDates, entriesByLocalDate) = await GetOrderedStreakDatesAsync(userId);
        var lookup = entriesByLocalDate.ToDictionary(e => e.Date, e => e.Total);
        return streakDates.Sum(date => lookup.GetValueOrDefault(date, 0));
    }
    
    public async Task<int> GetTotalQuantityAsync(Guid userId)
    {
        return await _db.ActivityEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .SumAsync(e => e.Quantity);
    }

    public async Task<bool> DeleteEntryForTodayAsync(Guid userId)
    {
        var (start, end) = GetUserLocalDateRange();
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        var candidates = await _db.ActivityEntries
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var todayEntry = candidates
            .FirstOrDefault(e => e.DateTime >= startUtc && e.DateTime < endUtc);

        if (todayEntry == null)
        {
            return false;
        }

        _db.ActivityEntries.Remove(todayEntry);
        var result = await _db.SaveChangesAsync();

        return result > 0;
    }

    private (DateTimeOffset start, DateTimeOffset end) GetUserLocalDateRange()
    {
        var now = DateTimeOffset.Now;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var todayEnd = todayStart.AddDays(1);

        return (todayStart, todayEnd);
    }
}
