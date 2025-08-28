using Microsoft.Extensions.Logging;
using OneActivity.Data;
using OnePushUp.Models.Dtos;
using OnePushUp.Repositories;

namespace OnePushUp.Services;

public class UserService
{
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUsersRepository usersRepository, ILogger<UserService> logger)
    {
        _usersRepository = usersRepository;
        _logger = logger;
    }

    public Task<User?> GetCurrentUserAsync() => _usersRepository.GetAsync();

    public async Task<UserDto?> GetCurrentUserDtoAsync()
    {
        var user = await _usersRepository.GetAsync();
        if (user == null)
            return null;
            
        return new UserDto
        {
            Id = user.Id,
            NickName = user.NickName
        };
    }

    public async Task<Guid> CreateUserAsync(string nickname)
    {
        var user = new User
        {
            NickName = nickname
        };
        
        return await _usersRepository.CreateAsync(user);
    }

    public async Task<bool> UpdateUserAsync(UserDto userDto)
    {
        var user = await _usersRepository.GetAsync();
        if (user == null)
        {
            _logger.LogWarning("Failed to update user {UserId}: user not found", userDto.Id);
            return false;
        }

        user.NickName = userDto.NickName;
        var success = await _usersRepository.UpdateAsync(user);
        if (!success)
        {
            _logger.LogWarning("Failed to update user {UserId}: user not found", userDto.Id);
        }

        return success;
    }
}
