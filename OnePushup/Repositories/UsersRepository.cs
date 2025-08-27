using Microsoft.EntityFrameworkCore;
using OnePushUp.Data;

namespace OnePushUp.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly OnePushUpDbContext _db;
    public UsersRepository(OnePushUpDbContext db)
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
        await _db.SaveChangesAsync();
        return true;
    }
}
