using Microsoft.EntityFrameworkCore;
using OneActivity.Data;

namespace OneActivity.Core.Repositories;

public class ActivityEntryRepository(OneActivityDbContext db) : IActivityEntryRepository
{
    private readonly OneActivityDbContext _db = db;
    private readonly record struct DailyTotal(DateTime Date, int Total);

    public async Task<Guid> CreateAsync(ActivityEntry entry)
    {
        if (entry.DateTime == default) entry.DateTime = DateTimeOffset.UtcNow;
        else if (entry.DateTime.Offset != TimeSpan.Zero) entry.DateTime = entry.DateTime.ToUniversalTime();
        var er = await _db.ActivityEntries.AddAsync(entry);
        await _db.SaveChangesAsync();
        return er.Entity.Id;
    }

    public Task<ActivityEntry?> GetEntryByIdAsync(Guid entryId) => _db.ActivityEntries.FindAsync(entryId).AsTask();

    public async Task<bool> UpdateAsync(ActivityEntry entry)
    {
        _db.ActivityEntries.Update(entry);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> HasEntryForTodayAsync(Guid userId)
    {
        var (start, end) = ActivityEntryRepository.GetUserLocalDateRange();
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();
        var entries = await _db.ActivityEntries.AsNoTracking().Where(e => e.UserId == userId).Select(e => e.DateTime).ToListAsync();
        return entries.Any(d => d >= startUtc && d < endUtc);
    }

    public async Task<ActivityEntry?> GetEntryForTodayAsync(Guid userId)
    {
        var (start, end) = ActivityEntryRepository.GetUserLocalDateRange();
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();
        var entries = await _db.ActivityEntries.AsNoTracking().Where(e => e.UserId == userId).ToListAsync();
        return entries.Where(e => e.DateTime >= startUtc && e.DateTime < endUtc).OrderByDescending(e => e.DateTime).FirstOrDefault();
    }

    public Task<List<ActivityEntry>> GetEntriesForUserAsync(Guid userId) =>
        _db.ActivityEntries.AsNoTracking().Where(e => e.UserId == userId).OrderByDescending(e => e.DateTime).ToListAsync();

    private async Task<List<DailyTotal>> GetEntriesByLocalDateAsync(Guid userId)
    {
        var entries = await _db.ActivityEntries.AsNoTracking().Where(e => e.UserId == userId && e.Quantity > 0).ToListAsync();
        return
        [
            .. entries.GroupBy(e => e.DateTime.ToLocalTime().Date).Select(g => new DailyTotal(g.Key, g.Sum(e => e.Quantity))).OrderByDescending(x => x.Date),
        ];
    }

    private async Task<(List<DateTime> streakDates, List<DailyTotal> entries)> GetOrderedStreakDatesAsync(Guid userId)
    {
        var today = await GetEntryForTodayAsync(userId);
        if (today != null && today.Quantity == 0) return (new List<DateTime>(), new List<DailyTotal>());
        var entries = await GetEntriesByLocalDateAsync(userId);
        if (entries.Count == 0) return (new List<DateTime>(), entries);
        var (start, _) = ActivityEntryRepository.GetUserLocalDateRange();
        var gap = (start.Date - entries[0].Date).Days;
        if (today == null && gap > 1) return (new List<DateTime>(), entries);
        var dates = entries.Select(e => e.Date).ToList();
        var streak = 1;
        for (int i = 0; i < dates.Count - 1; i++)
        {
            var days = (dates[i] - dates[i + 1]).Days;
            if (days == 1) streak++; else break;
        }
        return (dates.Take(streak).ToList(), entries);
    }

    public async Task<int> GetCurrentStreakAsync(Guid userId) => (await GetOrderedStreakDatesAsync(userId)).streakDates.Count;

    public async Task<int> GetTotalQuantityInCurrentStreakAsync(Guid userId)
    {
        var (dates, entries) = await GetOrderedStreakDatesAsync(userId);
        var dict = entries.ToDictionary(e => e.Date, e => e.Total);
        return dates.Sum(d => dict.GetValueOrDefault(d, 0));
    }

    public Task<int> GetTotalQuantityAsync(Guid userId) => _db.ActivityEntries.AsNoTracking().Where(e => e.UserId == userId).SumAsync(e => e.Quantity);

    public async Task<bool> DeleteEntryForTodayAsync(Guid userId)
    {
        var (start, end) = ActivityEntryRepository.GetUserLocalDateRange();
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();
        var candidates = await _db.ActivityEntries.Where(e => e.UserId == userId).ToListAsync();
        var today = candidates.FirstOrDefault(e => e.DateTime >= startUtc && e.DateTime < endUtc);
        if (today == null) return false;
        _db.ActivityEntries.Remove(today);
        return await _db.SaveChangesAsync() > 0;
    }

    private static (DateTimeOffset start, DateTimeOffset end) GetUserLocalDateRange()
    {
        var now = DateTimeOffset.Now;
        var start = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        return (start, start.AddDays(1));
    }
}
