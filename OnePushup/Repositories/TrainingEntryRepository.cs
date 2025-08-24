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
        var entryResult = await _db.TrainingEntries.AddAsync(entry);
        await _db.SaveChangesAsync();
        return entryResult.Entity.Id;
    }
    
    public async Task<bool> HasEntryForTodayAsync(Guid userId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        return await _db.TrainingEntries
            .AnyAsync(e => e.UserId == userId && 
                           e.DateTime >= today && 
                           e.DateTime < tomorrow);
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
        var entries = await _db.TrainingEntries
            .Where(e => e.UserId == userId && e.NumberOfRepetitions > 0)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();
        
        if (!entries.Any())
        {
            return 0;
        }
        
        var streak = 1;
        var currentDate = entries.First().DateTime.Date;
        
        for (int i = 1; i < entries.Count; i++)
        {
            var previousDate = entries[i].DateTime.Date;
            
            // If the previous entry was made exactly one day before the current one
            if (currentDate.AddDays(-1) == previousDate)
            {
                streak++;
                currentDate = previousDate;
            }
            else
            {
                // Streak is broken
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
        
        var entries = await _db.TrainingEntries
            .Where(e => e.UserId == userId && e.NumberOfRepetitions > 0)
            .OrderByDescending(e => e.DateTime)
            .Take(streak)
            .ToListAsync();
        
        return entries.Sum(e => e.NumberOfRepetitions);
    }
    
    public async Task<int> GetTotalPushupsAsync(Guid userId)
    {
        return await _db.TrainingEntries
            .Where(e => e.UserId == userId)
            .SumAsync(e => e.NumberOfRepetitions);
    }
}
