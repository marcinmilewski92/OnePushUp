using OneActivity.Data;
using OneActivity.Core.Models.Dtos;
using OneActivity.Core.Repositories;

namespace OneActivity.Core.Services;

public class ActivityService
{
    private readonly IActivityEntryRepository _repo;

    public ActivityService(IActivityEntryRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateEntryAsync(Guid userId, int quantity)
    {
        var entry = new ActivityEntry
        {
            DateTime = DateTimeOffset.Now,
            Quantity = quantity,
            UserId = userId
        };
        
        return await _repo.CreateAsync(entry);
    }

    public async Task<bool> UpdateEntryAsync(Guid entryId, int quantity)
    {
        var entry = await _repo.GetEntryByIdAsync(entryId);
        if (entry == null)
        {
            return false;
        }
        
        entry.Quantity = quantity;
        return await _repo.UpdateAsync(entry);
    }

    public Task<bool> HasEntryForTodayAsync(Guid userId) => _repo.HasEntryForTodayAsync(userId);

    public async Task<ActivityEntryDto?> GetTodayEntryAsync(Guid userId)
    {
        var entry = await _repo.GetEntryForTodayAsync(userId);
        return entry == null ? null : MapToDto(entry);
    }

    public async Task<List<ActivityEntryDto>> GetEntriesForUserAsync(Guid userId)
    {
        var entries = await _repo.GetEntriesForUserAsync(userId);
        return entries.Select(MapToDto).ToList();
    }

    public async Task<StreakDataDto> GetStreakDataAsync(Guid userId)
    {
        var currentStreak = await _repo.GetCurrentStreakAsync(userId);
        var streakTotal   = await _repo.GetTotalQuantityInCurrentStreakAsync(userId);
        var total         = await _repo.GetTotalQuantityAsync(userId);

        return new StreakDataDto
        {
            CurrentStreak            = currentStreak,
            QuantityInCurrentStreak  = streakTotal,
            TotalQuantity            = total
        };
    }

    public Task<bool> DeleteTodayEntryAsync(Guid userId) => _repo.DeleteEntryForTodayAsync(userId);

    private static ActivityEntryDto MapToDto(ActivityEntry entry)
    {
        return new ActivityEntryDto
        {
            Id = entry.Id,
            DateTime = entry.DateTime,
            Quantity = entry.Quantity,
            UserId = entry.UserId
        };
    }
}
