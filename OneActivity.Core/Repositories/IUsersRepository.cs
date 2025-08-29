using OneActivity.Data;

namespace OneActivity.Core.Repositories;

public interface IUsersRepository
{
    Task<Guid> CreateAsync(User user);
    Task<User?> GetAsync();
    Task<bool> UpdateAsync(User user);
}
