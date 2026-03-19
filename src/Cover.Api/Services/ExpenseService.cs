using Microsoft.EntityFrameworkCore;
using Cover.Api.Data;
using Cover.Api.Models;
using Cover.Shared.DTOs;

namespace Cover.Api.Services;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _db;

    public ExpenseService(AppDbContext db) => _db = db;

    public async Task<PagedResult<ExpenseDto>> GetAllAsync(int page, int pageSize, int? paidById = null)
    {
        var query = _db.Expenses.Include(e => e.PaidBy).AsQueryable();

        if (paidById.HasValue)
            query = query.Where(e => e.PaidById == paidById.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExpenseDto(
                e.Id, e.Merchant, e.Description, e.Amount, e.SplitType,
                e.PaidById, e.PaidBy.Name, e.Date, e.CreatedAt))
            .ToListAsync();

        return new PagedResult<ExpenseDto>(items, totalCount, page, pageSize);
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        return await _db.Expenses
            .Include(e => e.PaidBy)
            .Where(e => e.Id == id)
            .Select(e => new ExpenseDto(
                e.Id, e.Merchant, e.Description, e.Amount, e.SplitType,
                e.PaidById, e.PaidBy.Name, e.Date, e.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request)
    {
        var user = await _db.Users.FindAsync(request.PaidById)
            ?? throw new KeyNotFoundException($"User {request.PaidById} not found");

        var expense = new Expense
        {
            Merchant = request.Merchant,
            Description = request.Description,
            Amount = request.Amount,
            SplitType = request.SplitType,
            PaidById = request.PaidById,
            Date = request.Date
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return new ExpenseDto(
            expense.Id, expense.Merchant, expense.Description, expense.Amount, expense.SplitType,
            expense.PaidById, user.Name, expense.Date, expense.CreatedAt);
    }

    public async Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseRequest request)
    {
        var expense = await _db.Expenses.Include(e => e.PaidBy)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Expense {id} not found");

        var user = await _db.Users.FindAsync(request.PaidById)
            ?? throw new KeyNotFoundException($"User {request.PaidById} not found");

        expense.Merchant = request.Merchant;
        expense.Description = request.Description;
        expense.Amount = request.Amount;
        expense.SplitType = request.SplitType;
        expense.PaidById = request.PaidById;
        expense.Date = request.Date;
        expense.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new ExpenseDto(
            expense.Id, expense.Merchant, expense.Description, expense.Amount, expense.SplitType,
            expense.PaidById, user.Name, expense.Date, expense.CreatedAt);
    }

    public async Task DeleteAsync(int id)
    {
        var expense = await _db.Expenses.FindAsync(id)
            ?? throw new KeyNotFoundException($"Expense {id} not found");

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
    }

    public async Task<List<string>> GetMerchantsAsync(string query)
        => await _db.Expenses
            .Where(e => e.Merchant != null && EF.Functions.ILike(e.Merchant, $"{query}%"))
            .Select(e => e.Merchant!)
            .Distinct()
            .OrderBy(m => m)
            .Take(10)
            .ToListAsync();
}
