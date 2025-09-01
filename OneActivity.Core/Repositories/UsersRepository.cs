using Microsoft.EntityFrameworkCore;
using OneActivity.Data;

namespace OneActivity.Core.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly OneActivityDbContext _db;
    public UsersRepository(OneActivityDbContext db)
    {
        _db = db;
    }
    public async Task<Guid> CreateAsync(User user)
    {
        var userEntry = await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        return userEntry.Entity.Id;
    }
    
    public async Task<User?> GetAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var existingUser = await _db.Users.FindAsync(user.Id);
        if (existingUser is null)
        {
            return false;
        }

        existingUser.NickName = user.NickName;
        existingUser.Gender = user.Gender;
        await _db.SaveChangesAsync();
        return true;
    }
}
