using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Scrooge.Api.Data;
using Scrooge.Api.Models;

namespace Scrooge.Api.Services;

/// <summary>
/// Session lifecycle with sliding expiry: a session stays valid as long as it is used
/// at least once every <see cref="IdleTimeout"/>. LastUsedAt is written at most once per
/// <see cref="TouchInterval"/> to avoid a DB write on every request.
/// </summary>
public class SessionService : ISessionService
{
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromDays(180);
    public static readonly TimeSpan TouchInterval = TimeSpan.FromDays(1);

    private readonly AppDbContext _db;

    public SessionService(AppDbContext db) => _db = db;

    public async Task<string> CreateAsync()
    {
        var now = DateTime.UtcNow;
        var session = new AppSession
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            CreatedAt = now,
            LastUsedAt = now
        };
        _db.AppSessions.Add(session);
        await _db.SaveChangesAsync();
        return session.Token;
    }

    public async Task<SessionValidation> ValidateAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return SessionValidation.Invalid;

        var session = await _db.AppSessions.FirstOrDefaultAsync(s => s.Token == token);
        if (session is null)
            return SessionValidation.Invalid;

        var now = DateTime.UtcNow;
        if (now - session.LastUsedAt > IdleTimeout)
        {
            _db.AppSessions.Remove(session);
            await _db.SaveChangesAsync();
            return SessionValidation.Invalid;
        }

        if (now - session.LastUsedAt > TouchInterval)
        {
            session.LastUsedAt = now;
            await _db.SaveChangesAsync();
            return SessionValidation.Renewed;
        }

        return SessionValidation.Valid;
    }

    public async Task DeleteAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return;

        var session = await _db.AppSessions.FirstOrDefaultAsync(s => s.Token == token);
        if (session is not null)
        {
            _db.AppSessions.Remove(session);
            await _db.SaveChangesAsync();
        }
    }
}
