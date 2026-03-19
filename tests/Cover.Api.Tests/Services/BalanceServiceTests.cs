using Microsoft.EntityFrameworkCore;
using Cover.Api.Data;
using Cover.Api.Models;
using Cover.Api.Services;
using Cover.Shared.DTOs;

namespace Cover.Api.Tests.Services;

public class BalanceServiceTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Users.AddRange(
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task NoExpenses_ReturnsZeroBalance()
    {
        var db = CreateDb();
        var service = new BalanceService(db);

        var result = await service.GetBalanceAsync();

        Assert.NotNull(result);
        Assert.Equal(0, result!.OwedAmount);
    }

    [Fact]
    public async Task EqualSplit_CorrectBalance()
    {
        var db = CreateDb();
        db.Expenses.Add(new Expense
        {
            Description = "Groceries", Amount = 10000, PaidById = 1,
            SplitType = SplitType.Equal, Date = DateOnly.FromDateTime(DateTime.Today)
        });
        db.SaveChanges();
        var service = new BalanceService(db);

        var result = await service.GetBalanceAsync();

        Assert.Equal(5000, result!.OwedAmount);
        Assert.Equal(2, result.OwesUserId);  // Bob owes
        Assert.Equal(1, result.IsOwedUserId); // Alice is owed
    }

    [Fact]
    public async Task FullOther_CorrectBalance()
    {
        var db = CreateDb();
        db.Expenses.Add(new Expense
        {
            Description = "Bob's item", Amount = 8000, PaidById = 1,
            SplitType = SplitType.FullOther, Date = DateOnly.FromDateTime(DateTime.Today)
        });
        db.SaveChanges();
        var service = new BalanceService(db);

        var result = await service.GetBalanceAsync();

        Assert.Equal(8000, result!.OwedAmount);
        Assert.Equal(2, result.OwesUserId);
    }

    [Fact]
    public async Task BothSides_NetBalance()
    {
        var db = CreateDb();
        db.Expenses.AddRange(
            new Expense
            {
                Description = "Groceries", Amount = 10000, PaidById = 1,
                SplitType = SplitType.Equal, Date = DateOnly.FromDateTime(DateTime.Today)
            },
            new Expense
            {
                Description = "Dinner", Amount = 6000, PaidById = 2,
                SplitType = SplitType.Equal, Date = DateOnly.FromDateTime(DateTime.Today)
            });
        db.SaveChanges();
        var service = new BalanceService(db);

        var result = await service.GetBalanceAsync();

        // Alice credit: 5000, Bob credit: 3000, net = 2000 → Bob owes Alice 2000
        Assert.Equal(2000, result!.OwedAmount);
        Assert.Equal(2, result.OwesUserId);
        Assert.Equal(1, result.IsOwedUserId);
    }

    [Fact]
    public async Task MixedSplitTypes_CorrectBalance()
    {
        var db = CreateDb();
        db.Expenses.AddRange(
            new Expense
            {
                Description = "Groceries", Amount = 10000, PaidById = 1,
                SplitType = SplitType.Equal, Date = DateOnly.FromDateTime(DateTime.Today)
            },
            new Expense
            {
                Description = "Bob's thing", Amount = 8000, PaidById = 1,
                SplitType = SplitType.FullOther, Date = DateOnly.FromDateTime(DateTime.Today)
            });
        db.SaveChanges();
        var service = new BalanceService(db);

        var result = await service.GetBalanceAsync();

        // Alice credit: 5000 (equal) + 8000 (full) = 13000
        Assert.Equal(13000, result!.OwedAmount);
        Assert.Equal(2, result.OwesUserId);
    }
}
