using OnePushUp.Data;
using OnePushUp.Models.Dtos;
using OnePushUp.Repositories;

namespace OnePushUp.Services;

public class UserService
{
    private readonly UsersRepository _usersRepository;

    public UserService(UsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        return await _usersRepository.GetAsync();
    }

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

    public async Task UpdateUserAsync(UserDto userDto)
    {
        var user = await _usersRepository.GetAsync();
        if (user != null)
        {
            user.NickName = userDto.NickName;
            await _usersRepository.UpdateAsync(user);
        }
    }
}
