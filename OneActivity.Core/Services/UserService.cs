using Microsoft.EntityFrameworkCore;
using OneActivity.Data;
using OneActivity.Core.Models.Dtos;

namespace OneActivity.Core.Services;

public class UserService
{
    private readonly OneActivityDbContext _dbContext;
    private readonly IGenderService _genderService;

    public UserService(OneActivityDbContext dbContext, IGenderService genderService)
    {
        _dbContext = dbContext;
        _genderService = genderService;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        return await _dbContext.Users.FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateUserAsync(string nickName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            NickName = nickName,
            Gender = (int)_genderService.Current
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> UpdateUserAsync(UserDto userDto)
    {
        var user = await _dbContext.Users.FindAsync(userDto.Id);
        if (user == null) return false;

        user.NickName = userDto.NickName;
        if (userDto is { Gender: not null })
        {
            user.Gender = (int)userDto.Gender.Value;
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateGenderAsync(Guid userId, Gender gender)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;
        user.Gender = (int)gender;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
