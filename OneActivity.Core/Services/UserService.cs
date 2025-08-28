using Microsoft.EntityFrameworkCore;
using OnePushUp.Data;
using OnePushUp.Models.Dtos;

namespace OnePushUp.Services;

public class UserService
{
    private readonly OnePushUpDbContext _dbContext;

    public UserService(OnePushUpDbContext dbContext)
    {
        _dbContext = dbContext;
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
            NickName = nickName
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
        await _dbContext.SaveChangesAsync();
        return true;
    }
}

