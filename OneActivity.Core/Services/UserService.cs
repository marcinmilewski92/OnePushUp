using Microsoft.EntityFrameworkCore;
using OneActivity.Data;
using OneActivity.Core.Models.Dtos;

namespace OneActivity.Core.Services;

public class UserService
{
    private readonly OneActivityDbContext _dbContext;

    public UserService(OneActivityDbContext dbContext)
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
