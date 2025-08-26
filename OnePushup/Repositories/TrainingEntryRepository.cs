using Microsoft.EntityFrameworkCore;
using OnePushUp.Data;

namespace OnePushUp.Repositories;

public class TrainingEntryRepository : ITrainingEntryRepository
{
    private readonly OnePushUpDbContext _db;
    
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
            entry.DateTime = DateTime.UtcNow;
        }
        else if (entry.DateTime.Kind != DateTimeKind.Utc)
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

    private async Task<Dictionary<DateTime, int>> GetEntriesByLocalDateAsync(Guid userId)
    {
        var entries = await _db.TrainingEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.NumberOfRepetitions > 0)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();

        return entries
            .GroupBy(e => ToUserLocalTime(e.DateTime).Date)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.NumberOfRepetitions));
    }

    private async Task<(List<DateTime> streakDates, Dictionary<DateTime, int> entriesByLocalDate)> GetOrderedStreakDatesAsync(Guid userId)
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
            return (new List<DateTime>(), new Dictionary<DateTime, int>());
        }

        var entriesByLocalDate = await GetEntriesByLocalDateAsync(userId);

        if (!entriesByLocalDate.Any())
        {
            return (new List<DateTime>(), entriesByLocalDate);
        }

        var orderedDates = entriesByLocalDate.Keys
            .OrderByDescending(d => d)
            .ToList();

        var userLocalToday = DateTime.Now.Date;
        var userLocalYesterday = userLocalToday.AddDays(-1);

        var latestEntryDate = orderedDates.First();
        bool isOngoingStreak = latestEntryDate >= userLocalYesterday;

        if (!isOngoingStreak)
        {
            return (new List<DateTime>(), entriesByLocalDate);
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
        return (streakDates, entriesByLocalDate);
    }
    
    public async Task<int> GetCurrentStreakAsync(Guid userId)
    {
        var (streakDates, _) = await GetOrderedStreakDatesAsync(userId);
        return streakDates.Count;
    }
    
    public async Task<int> GetTotalPushupsInCurrentStreakAsync(Guid userId)
    {
        var (streakDates, entriesByLocalDate) = await GetOrderedStreakDatesAsync(userId);
        return streakDates.Sum(date => entriesByLocalDate.GetValueOrDefault(date, 0));
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
    
    private (DateTime start, DateTime end) GetUserLocalDateRange()
    {
        // Get the current user's local date 
        var userLocalToday = DateTime.Now.Date;
        
        // Create date range from midnight to midnight (local time)
        var todayStart = userLocalToday;
        var todayEnd = userLocalToday.AddDays(1);
        
        return (todayStart, todayEnd);
    }
    
    private DateTime ToUserLocalTime(DateTime utcTime)
    {
        if (utcTime.Kind != DateTimeKind.Utc)
        {
            // If not UTC, convert to UTC first (safety check)
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        }
        
        // Convert UTC to local time
        return utcTime.ToLocalTime();
    }
}
