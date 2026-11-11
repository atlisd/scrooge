using Microsoft.EntityFrameworkCore;
using Scrooge.Api.Data;
using Scrooge.Api.Models;
using Scrooge.Api.Services;

namespace Scrooge.Api.Tests.Services;

public class SessionServiceTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<string> AddSession(AppDbContext db, TimeSpan lastUsedAgo)
    {
        var token = Guid.NewGuid().ToString();
        db.AppSessions.Add(new AppSession
        {
            Token = token,
            CreatedAt = DateTime.UtcNow - lastUsedAgo,
            LastUsedAt = DateTime.UtcNow - lastUsedAgo
        });
        await db.SaveChangesAsync();
        return token;
    }

    [Fact]
    public async Task Create_ThenValidate_IsValid()
    {
        var db = CreateDb();
        var service = new SessionService(db);

        var token = await service.CreateAsync();

        Assert.False(string.IsNullOrEmpty(token));
        Assert.Equal(SessionValidation.Valid, await service.ValidateAsync(token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("does-not-exist")]
    public async Task Validate_UnknownOrMissingToken_IsInvalid(string? token)
    {
        var service = new SessionService(CreateDb());
        Assert.Equal(SessionValidation.Invalid, await service.ValidateAsync(token));
    }

    [Fact]
    public async Task Validate_RecentlyUsed_DoesNotTouch()
    {
        var db = CreateDb();
        var token = await AddSession(db, TimeSpan.FromHours(1));
        var before = (await db.AppSessions.SingleAsync()).LastUsedAt;

        var result = await new SessionService(db).ValidateAsync(token);

        Assert.Equal(SessionValidation.Valid, result);
        Assert.Equal(before, (await db.AppSessions.SingleAsync()).LastUsedAt);
    }

    [Fact]
    public async Task Validate_UsedOverADayAgo_RenewsLastUsedAt()
    {
        var db = CreateDb();
        var token = await AddSession(db, TimeSpan.FromDays(30));

        var result = await new SessionService(db).ValidateAsync(token);

        Assert.Equal(SessionValidation.Renewed, result);
        var session = await db.AppSessions.SingleAsync();
        Assert.True(DateTime.UtcNow - session.LastUsedAt < TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Validate_IdleBeyondTimeout_IsInvalidAndDeleted()
    {
        var db = CreateDb();
        var token = await AddSession(db, SessionService.IdleTimeout + TimeSpan.FromDays(1));

        var result = await new SessionService(db).ValidateAsync(token);

        Assert.Equal(SessionValidation.Invalid, result);
        Assert.Empty(db.AppSessions);
    }

    [Fact]
    public async Task Delete_RemovesOnlyThatSession()
    {
        var db = CreateDb();
        var service = new SessionService(db);
        var keep = await service.CreateAsync();
        var remove = await service.CreateAsync();

        await service.DeleteAsync(remove);

        Assert.Equal(SessionValidation.Invalid, await service.ValidateAsync(remove));
        Assert.Equal(SessionValidation.Valid, await service.ValidateAsync(keep));
    }
}
