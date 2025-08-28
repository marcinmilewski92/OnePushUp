using OnePushUp.Data;

namespace OnePushUp.Repositories;

public interface IUsersRepository
{
    Task<Guid> CreateAsync(User user);
    Task<User?> GetAsync();
    Task<bool> UpdateAsync(User user);
}

