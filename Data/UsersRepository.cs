using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnePushUp.Domain;

namespace OnePushUp.Data;

public class UsersRepository
{
    public async Task<Guid> CreateAsync(User user)
    {
        // ...existing code...
    }
    public async Task<User?> GetUserAsync()
    {
        // ...existing code...
    }
}

