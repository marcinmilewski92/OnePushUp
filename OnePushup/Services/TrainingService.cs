using OnePushUp.Data;
using OnePushUp.Models.Dtos;
using OnePushUp.Repositories;

namespace OnePushUp.Services;

public class TrainingService
{
    private readonly TrainingEntryRepository _trainingEntryRepository;

    public TrainingService(TrainingEntryRepository trainingEntryRepository)
    {
        _trainingEntryRepository = trainingEntryRepository;
    }

    public async Task<Guid> CreateEntryAsync(Guid userId, int repetitions)
    {
        var entry = new TrainingEntry
        {
            DateTime = DateTime.Now.Kind == DateTimeKind.Local ? 
                DateTime.Now.ToUniversalTime() : 
                DateTime.UtcNow,
            NumberOfRepetitions = repetitions,
            UserId = userId
        };
        
        return await _trainingEntryRepository.CreateAsync(entry);
    }

    public async Task<bool> HasEntryForTodayAsync(Guid userId)
    {
        return await _trainingEntryRepository.HasEntryForTodayAsync(userId);
    }

    public async Task<List<TrainingEntryDto>> GetEntriesForUserAsync(Guid userId)
    {
        var entries = await _trainingEntryRepository.GetEntriesForUserAsync(userId);
        return entries.Select(e => new TrainingEntryDto
        {
            Id = e.Id,
            DateTime = e.DateTime,
            NumberOfRepetitions = e.NumberOfRepetitions,
            UserId = e.UserId
        }).ToList();
    }

    public async Task<StreakDataDto> GetStreakDataAsync(Guid userId)
    {
        var currentStreak = await _trainingEntryRepository.GetCurrentStreakAsync(userId);
        var streakTotal = await _trainingEntryRepository.GetTotalPushupsInCurrentStreakAsync(userId);
        var totalPushups = await _trainingEntryRepository.GetTotalPushupsAsync(userId);

        return new StreakDataDto
        {
            CurrentStreak = currentStreak,
            PushupsInCurrentStreak = streakTotal,
            TotalPushups = totalPushups
        };
    }
}
