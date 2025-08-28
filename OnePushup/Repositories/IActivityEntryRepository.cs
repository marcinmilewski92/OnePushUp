using OnePushUp.Data;

namespace OnePushUp.Repositories;

public interface IActivityEntryRepository
{
    Task<Guid> CreateAsync(ActivityEntry entry);
    Task<ActivityEntry?> GetEntryByIdAsync(Guid entryId);
    Task<bool> UpdateAsync(ActivityEntry entry);
    Task<bool> HasEntryForTodayAsync(Guid userId);
    Task<ActivityEntry?> GetEntryForTodayAsync(Guid userId);
    Task<List<ActivityEntry>> GetEntriesForUserAsync(Guid userId);
    Task<int> GetCurrentStreakAsync(Guid userId);
    Task<int> GetTotalQuantityInCurrentStreakAsync(Guid userId);
    Task<int> GetTotalQuantityAsync(Guid userId);
    Task<bool> DeleteEntryForTodayAsync(Guid userId);
}
