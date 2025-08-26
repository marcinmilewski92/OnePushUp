using OnePushUp.Data;
using OnePushUp.Models.Dtos;
using OnePushUp.Repositories;

namespace OnePushUp.Services;

public class TrainingService
{
    private readonly ITrainingEntryRepository _trainingEntryRepository;

    public TrainingService(ITrainingEntryRepository trainingEntryRepository)
    {
        _trainingEntryRepository = trainingEntryRepository;
    }

    public async Task<Guid> CreateEntryAsync(Guid userId, int repetitions)
    {
        var entry = new TrainingEntry
        {
            DateTime = DateTime.UtcNow,
            NumberOfRepetitions = repetitions,
            UserId = userId
        };
        
        return await _trainingEntryRepository.CreateAsync(entry);
    }

    public async Task<bool> UpdateEntryAsync(Guid entryId, int repetitions)
    {
        // Get the existing entry first
        var entry = await _trainingEntryRepository.GetEntryByIdAsync(entryId);
        if (entry == null)
        {
            return false;
        }
        
        // Update the repetitions count
        entry.NumberOfRepetitions = repetitions;
        
        // Save the changes
        return await _trainingEntryRepository.UpdateAsync(entry);
    }

    public async Task<bool> HasEntryForTodayAsync(Guid userId)
    {
        return await _trainingEntryRepository.HasEntryForTodayAsync(userId);
    }

    public async Task<TrainingEntryDto?> GetTodayEntryAsync(Guid userId)
    {
        var entry = await _trainingEntryRepository.GetEntryForTodayAsync(userId);
        if (entry == null)
        {
            return null;
        }

        return MapToDto(entry);
    }

    public async Task<List<TrainingEntryDto>> GetEntriesForUserAsync(Guid userId)
    {
        var entries = await _trainingEntryRepository.GetEntriesForUserAsync(userId);
        return entries.Select(MapToDto).ToList();
    }

    public async Task<StreakDataDto> GetStreakDataAsync(Guid userId)
    {
        var currentStreakTask = _trainingEntryRepository.GetCurrentStreakAsync(userId);
        var streakTotalTask = _trainingEntryRepository.GetTotalPushupsInCurrentStreakAsync(userId);
        var totalPushupsTask = _trainingEntryRepository.GetTotalPushupsAsync(userId);

        await Task.WhenAll(currentStreakTask, streakTotalTask, totalPushupsTask);

        return new StreakDataDto
        {
            CurrentStreak = await currentStreakTask,
            PushupsInCurrentStreak = await streakTotalTask,
            TotalPushups = await totalPushupsTask
        };
    }

    public async Task<bool> DeleteTodayEntryAsync(Guid userId)
    {
        return await _trainingEntryRepository.DeleteEntryForTodayAsync(userId);
    }

    private static TrainingEntryDto MapToDto(TrainingEntry entry)
    {
        return new TrainingEntryDto
        {
            Id = entry.Id,
            DateTime = entry.DateTime,
            NumberOfRepetitions = entry.NumberOfRepetitions,
            UserId = entry.UserId
        };
    }
}
