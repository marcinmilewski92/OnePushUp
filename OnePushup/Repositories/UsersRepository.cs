using Microsoft.EntityFrameworkCore;
using OnePushUp.Data;

namespace OnePushUp.Repositories;

public class UsersRepository
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
        return await _db.Users.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var existingUser = await _db.Users.FindAsync(user.Id);
        if (existingUser is not null)
        {
            existingUser.NickName = user.NickName;
            await _db.SaveChangesAsync();
        }
        
        
        
        
        
    }
}