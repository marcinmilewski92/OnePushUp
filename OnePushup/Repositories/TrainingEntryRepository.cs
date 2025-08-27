using Microsoft.EntityFrameworkCore;
using OnePushUp.Data;

namespace OnePushUp.Repositories;

public class TrainingEntryRepository : ITrainingEntryRepository
{
    private readonly OnePushUpDbContext _db;

    private readonly record struct DailyTotal(DateTime Date, int TotalReps);
    
    public TrainingEntryRepository(OnePushUpDbContext db)
    {
        _db = db;
    }
    
    public async Task<Guid> CreateAsync(TrainingEntry entry)
    {
        // Always store in UTC, but we'll preserve the original input time
        // This ensures database consistency while respecting user's time zone
        if (entry.DateTime == default)
        {
            entry.DateTime = DateTimeOffset.UtcNow;
        }
        else if (entry.DateTime.Offset != TimeSpan.Zero)
        {
            // Convert to UTC if not already
            entry.DateTime = entry.DateTime.ToUniversalTime();
        }

        var entryResult = await _db.TrainingEntries.AddAsync(entry);
        await _db.SaveChangesAsync();
        return entryResult.Entity.Id;
    }
    
    public async Task<TrainingEntry?> GetEntryByIdAsync(Guid entryId)
    {
        return await _db.TrainingEntries.FindAsync(entryId);
    }
    
    public async Task<bool> UpdateAsync(TrainingEntry entry)
    {
        _db.TrainingEntries.Update(entry);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }
    
    public async Task<bool> HasEntryForTodayAsync(Guid userId) =>
        await GetEntryForTodayAsync(userId) is not null;
    
    public async Task<TrainingEntry?> GetEntryForTodayAsync(Guid userId)
    {
        // Get the user's local date range for "today"
        var userLocalToday = GetUserLocalDateRange();
        
        // Convert the local date range to UTC for database comparison
        var todayStartUtc = userLocalToday.start.ToUniversalTime();
        var todayEndUtc = userLocalToday.end.ToUniversalTime();
        
        return await _db.TrainingEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId &&
                                    e.DateTime >= todayStartUtc &&
                                    e.DateTime < todayEndUtc);
    }
    
    public async Task<List<TrainingEntry>> GetEntriesForUserAsync(Guid userId)
    {
        return await _db.TrainingEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();
    }

    private async Task<List<DailyTotal>> GetEntriesByLocalDateAsync(Guid userId)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.UtcNow);

        return await _db.TrainingEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.NumberOfRepetitions > 0)
            .GroupBy(e => e.DateTime.ToOffset(offset).Date)
            .Select(g => new DailyTotal(g.Key, g.Sum(e => e.NumberOfRepetitions)))
            .ToListAsync();
    }

    private async Task<(List<DateTime> streakDates, List<DailyTotal> entriesByLocalDate)> GetOrderedStreakDatesAsync(Guid userId)
    {
        // Check if there's an entry with 0 pushups for today, which would break the streak
        var dateRange = GetUserLocalDateRange();
        var todayStartUtc = dateRange.start.ToUniversalTime();
        var todayEndUtc = dateRange.end.ToUniversalTime();

        var todayEntry = await _db.TrainingEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId &&
                                    e.DateTime >= todayStartUtc &&
                                    e.DateTime < todayEndUtc);

        if (todayEntry != null && todayEntry.NumberOfRepetitions == 0)
        {
            return (new List<DateTime>(), new List<DailyTotal>());
        }

        var entriesByLocalDate = await GetEntriesByLocalDateAsync(userId);

        if (!entriesByLocalDate.Any())
        {
            return (new List<DateTime>(), entriesByLocalDate);
        }

        var orderedEntries = entriesByLocalDate
            .OrderByDescending(e => e.Date)
            .ToList();

        var orderedDates = orderedEntries.Select(e => e.Date).ToList();

        var userLocalToday = DateTimeOffset.Now.Date;
        var userLocalYesterday = userLocalToday.AddDays(-1);

        var latestEntryDate = orderedDates.First();
        bool isOngoingStreak = latestEntryDate >= userLocalYesterday;

        if (!isOngoingStreak)
        {
            return (new List<DateTime>(), orderedEntries);
        }

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
        return (streakDates, orderedEntries);
    }
    
    public async Task<int> GetCurrentStreakAsync(Guid userId)
    {
        var (streakDates, _) = await GetOrderedStreakDatesAsync(userId);
        return streakDates.Count;
    }
    
    public async Task<int> GetTotalPushupsInCurrentStreakAsync(Guid userId)
    {
        var (streakDates, entriesByLocalDate) = await GetOrderedStreakDatesAsync(userId);
        var lookup = entriesByLocalDate.ToDictionary(e => e.Date, e => e.TotalReps);
        return streakDates.Sum(date => lookup.GetValueOrDefault(date, 0));
    }
    
    public async Task<int> GetTotalPushupsAsync(Guid userId)
    {
        return await _db.TrainingEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .SumAsync(e => e.NumberOfRepetitions);
    }

    public async Task<bool> DeleteEntryForTodayAsync(Guid userId)
    {
        var todayEntry = await GetEntryForTodayAsync(userId);

        if (todayEntry == null)
        {
            return false; // No entry found to delete
        }
        
        // Remove the entry
        _db.TrainingEntries.Remove(todayEntry);
        var result = await _db.SaveChangesAsync();
        
        return result > 0;
    }
    
    // Helper methods for time zone handling
    
    private (DateTimeOffset start, DateTimeOffset end) GetUserLocalDateRange()
    {
        // Get the current user's local date
        var now = DateTimeOffset.Now;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var todayEnd = todayStart.AddDays(1);

        return (todayStart, todayEnd);
    }

    private DateTimeOffset ToUserLocalTime(DateTimeOffset utcTime)
    {
        return utcTime.ToLocalTime();
    }
}
