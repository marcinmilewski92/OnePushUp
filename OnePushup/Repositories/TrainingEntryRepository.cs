using Microsoft.EntityFrameworkCore;
using OnePushUp.Data;

namespace OnePushUp.Repositories;

public class TrainingEntryRepository
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
            .FirstOrDefaultAsync(e => e.UserId == userId && 
                                    e.DateTime >= todayStartUtc && 
                                    e.DateTime < todayEndUtc);
    }
    
    public async Task<List<TrainingEntry>> GetEntriesForUserAsync(Guid userId)
    {
        return await _db.TrainingEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();
    }
    
    public async Task<int> GetCurrentStreakAsync(Guid userId)
    {
        // Check if there's an entry with 0 pushups for today, which would break the streak
        var dateRange = GetUserLocalDateRange();
        var todayStartUtc = dateRange.start.ToUniversalTime();
        var todayEndUtc = dateRange.end.ToUniversalTime();
        
        var todayEntry = await _db.TrainingEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && 
                                    e.DateTime >= todayStartUtc && 
                                    e.DateTime < todayEndUtc);
        
        // If today's entry exists and has 0 pushups, streak is broken
        if (todayEntry != null && todayEntry.NumberOfRepetitions == 0)
        {
            return 0;
        }
        
        // Get all user entries with valid repetitions
        var entries = await _db.TrainingEntries
            .Where(e => e.UserId == userId && e.NumberOfRepetitions > 0)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();
        
        if (!entries.Any())
        {
            return 0;
        }
        
        // Group entries by user's local date (not UTC date)
        var entriesByLocalDate = entries
            .GroupBy(e => ToUserLocalTime(e.DateTime).Date)
            .Select(g => g.Key)
            .OrderByDescending(d => d)
            .ToList();
        
        // Get today and yesterday in user's local time
        var userLocalToday = DateTime.Now.Date;
        var userLocalYesterday = userLocalToday.AddDays(-1);
        
        // Check if user has entry for today or yesterday to maintain streak
        var latestEntryDate = entriesByLocalDate.First();
        bool isOngoingStreak = latestEntryDate >= userLocalYesterday;
        
        if (!isOngoingStreak)
        {
            // Streak is broken - no entry today or yesterday
            return 0;
        }
        
        // Count consecutive days
        var streak = 1;
        for (int i = 0; i < entriesByLocalDate.Count - 1; i++)
        {
            var currentDate = entriesByLocalDate[i];
            var previousDate = entriesByLocalDate[i + 1];
            
            // Calculate days between entries - should be exactly 1 for consecutive days
            var daysBetween = (currentDate - previousDate).Days;
            
            if (daysBetween == 1)
            {
                streak++;
            }
            else
            {
                // Streak is broken - gap in days
                break;
            }
        }
        
        return streak;
    }
    
    public async Task<int> GetTotalPushupsInCurrentStreakAsync(Guid userId)
    {
        var streak = await GetCurrentStreakAsync(userId);
        if (streak == 0)
        {
            return 0;
        }
        
        // Get all user entries with valid repetitions
        var entries = await _db.TrainingEntries
            .Where(e => e.UserId == userId && e.NumberOfRepetitions > 0)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();
            
        if (!entries.Any())
        {
            return 0;
        }
        
        // Group entries by user's local date (not UTC date)
        var entriesByLocalDate = entries
            .GroupBy(e => ToUserLocalTime(e.DateTime).Date)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.NumberOfRepetitions));
        
        // Get today and yesterday in user's local time
        var userLocalToday = DateTime.Now.Date;
        var userLocalYesterday = userLocalToday.AddDays(-1);
        
        // Find the latest entry date in user's local time
        var latestEntryDate = entriesByLocalDate.Keys.Max();
        
        // The streak is valid if the latest entry is from today or yesterday
        bool isOngoingStreak = latestEntryDate >= userLocalYesterday;
        
        if (!isOngoingStreak)
        {
            return 0;
        }
        
        // Calculate the streak start date (earliest date in current streak)
        var streakDates = new List<DateTime>();
        var currentDate = latestEntryDate;
        
        // Add the latest entry date to our streak dates
        streakDates.Add(currentDate);
        
        // Work backwards to find all consecutive days in the streak
        for (int i = 1; i < streak; i++)
        {
            currentDate = currentDate.AddDays(-1);
            
            // If we have an entry for this date, it's part of the streak
            if (entriesByLocalDate.ContainsKey(currentDate))
            {
                streakDates.Add(currentDate);
            }
            else
            {
                // We've reached the end of consecutive entries
                break;
            }
        }
        
        // Sum up pushups for all dates in the streak
        return streakDates.Sum(date => entriesByLocalDate.GetValueOrDefault(date, 0));
    }
    
    public async Task<int> GetTotalPushupsAsync(Guid userId)
    {
        return await _db.TrainingEntries
            .Where(e => e.UserId == userId)
            .SumAsync(e => e.NumberOfRepetitions);
    }
    
    public async Task<bool> DeleteEntryForTodayAsync(Guid userId)
    {
        // Get the user's local date range for "today"
        var dateRange = GetUserLocalDateRange();
        
        // Convert the local date range to UTC for database comparison
        var todayStartUtc = dateRange.start.ToUniversalTime();
        var todayEndUtc = dateRange.end.ToUniversalTime();
        
        // Find today's entry
        var todayEntry = await _db.TrainingEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && 
                                    e.DateTime >= todayStartUtc && 
                                    e.DateTime < todayEndUtc);
        
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
