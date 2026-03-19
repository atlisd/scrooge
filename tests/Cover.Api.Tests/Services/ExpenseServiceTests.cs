using Microsoft.EntityFrameworkCore;
using Cover.Api.Data;
using Cover.Api.Models;
using Cover.Api.Services;
using Cover.Shared.DTOs;

namespace Cover.Api.Tests.Services;

public class ExpenseServiceTests
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
    public async Task Create_ReturnsExpenseDto()
    {
        var db = CreateDb();
        var service = new ExpenseService(db);

        var result = await service.CreateAsync(new CreateExpenseRequest(
            "Amazon", "Groceries", 10000, SplitType.Equal, 1, DateOnly.FromDateTime(DateTime.Today)));

        Assert.Equal("Amazon", result.Merchant);
        Assert.Equal("Groceries", result.Description);
        Assert.Equal(10000, result.Amount);
        Assert.Equal("Alice", result.PaidByName);
    }

    [Fact]
    public async Task GetById_ReturnsCorrectExpense()
    {
        var db = CreateDb();
        var service = new ExpenseService(db);
        var created = await service.CreateAsync(new CreateExpenseRequest(
            "Test", null, 5000, SplitType.Equal, 1, DateOnly.FromDateTime(DateTime.Today)));

        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result!.Id);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        var db = CreateDb();
        var service = new ExpenseService(db);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_ModifiesExpense()
    {
        var db = CreateDb();
        var service = new ExpenseService(db);
        var created = await service.CreateAsync(new CreateExpenseRequest(
            "OldMerchant", "Old", 5000, SplitType.Equal, 1, DateOnly.FromDateTime(DateTime.Today)));

        var updated = await service.UpdateAsync(created.Id, new UpdateExpenseRequest(
            "NewMerchant", "New", 7000, SplitType.FullOther, 2, DateOnly.FromDateTime(DateTime.Today)));

        Assert.Equal("NewMerchant", updated.Merchant);
        Assert.Equal("New", updated.Description);
        Assert.Equal(7000, updated.Amount);
        Assert.Equal(SplitType.FullOther, updated.SplitType);
        Assert.Equal("Bob", updated.PaidByName);
    }

    [Fact]
    public async Task Delete_RemovesExpense()
    {
        var db = CreateDb();
        var service = new ExpenseService(db);
        var created = await service.CreateAsync(new CreateExpenseRequest(
            "ToDelete", null, 5000, SplitType.Equal, 1, DateOnly.FromDateTime(DateTime.Today)));

        await service.DeleteAsync(created.Id);

        var result = await service.GetByIdAsync(created.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAll_Pagination()
    {
        var db = CreateDb();
        var service = new ExpenseService(db);
        for (int i = 0; i < 5; i++)
        {
            await service.CreateAsync(new CreateExpenseRequest(
                $"Item {i}", null, 1000 * (i + 1), SplitType.Equal, 1,
                DateOnly.FromDateTime(DateTime.Today)));
        }

        var page1 = await service.GetAllAsync(1, 2);
        var page2 = await service.GetAllAsync(2, 2);

        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(2, page2.Items.Count);
    }

    [Fact]
    public async Task GetAll_FilterByPaidById()
    {
        var db = CreateDb();
        var service = new ExpenseService(db);
        await service.CreateAsync(new CreateExpenseRequest(
            "Alice's", null, 5000, SplitType.Equal, 1, DateOnly.FromDateTime(DateTime.Today)));
        await service.CreateAsync(new CreateExpenseRequest(
            "Bob's", null, 3000, SplitType.Equal, 2, DateOnly.FromDateTime(DateTime.Today)));

        var result = await service.GetAllAsync(1, 20, paidById: 1);

        Assert.Single(result.Items);
        Assert.Equal("Alice's", result.Items[0].Merchant);
    }
}
