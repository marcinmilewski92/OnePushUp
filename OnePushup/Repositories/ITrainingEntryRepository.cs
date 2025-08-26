using OnePushUp.Data;

namespace OnePushUp.Repositories;

public interface ITrainingEntryRepository
{
    Task<Guid> CreateAsync(TrainingEntry entry);
    Task<TrainingEntry?> GetEntryByIdAsync(Guid entryId);
    Task<bool> UpdateAsync(TrainingEntry entry);
    Task<bool> HasEntryForTodayAsync(Guid userId);
    Task<TrainingEntry?> GetEntryForTodayAsync(Guid userId);
    Task<List<TrainingEntry>> GetEntriesForUserAsync(Guid userId);
    Task<int> GetCurrentStreakAsync(Guid userId);
    Task<int> GetTotalPushupsInCurrentStreakAsync(Guid userId);
    Task<int> GetTotalPushupsAsync(Guid userId);
    Task<bool> DeleteEntryForTodayAsync(Guid userId);
}
