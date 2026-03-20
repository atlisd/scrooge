using Microsoft.EntityFrameworkCore;
using Scrooge.Api.Data;
using Scrooge.Api.Models;
using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _db.Users
            .OrderBy(u => u.Id)
            .Select(u => new UserDto(u.Id, u.Name))
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        return user is null ? null : new UserDto(user.Id, user.Name);
    }

    public async Task<List<UserDto>> CreateUsersAsync(string name1, string name2)
    {
        var user1 = new User { Name = name1 };
        var user2 = new User { Name = name2 };
        _db.Users.AddRange(user1, user2);
        await _db.SaveChangesAsync();
        return [new UserDto(user1.Id, user1.Name), new UserDto(user2.Id, user2.Name)];
    }

    public async Task<UserDto> UpdateNameAsync(int id, string name)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found");
        user.Name = name;
        await _db.SaveChangesAsync();
        return new UserDto(user.Id, user.Name);
    }

    public async Task<bool> IsSetupCompleteAsync()
    {
        return await _db.Users.CountAsync() >= 2;
    }
}
