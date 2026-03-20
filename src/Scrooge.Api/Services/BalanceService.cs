using Microsoft.EntityFrameworkCore;
using Scrooge.Api.Data;
using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Services;

public class BalanceService : IBalanceService
{
    private readonly AppDbContext _db;

    public BalanceService(AppDbContext db) => _db = db;

    public async Task<BalanceDto?> GetBalanceAsync()
    {
        var users = await _db.Users.OrderBy(u => u.Id).ToListAsync();
        if (users.Count < 2) return null;

        var user1 = users[0];
        var user2 = users[1];

        var expenses = await _db.Expenses.ToListAsync();

        long user1TotalPaid = 0, user2TotalPaid = 0;
        long user1Credit = 0, user2Credit = 0;

        foreach (var expense in expenses)
        {
            if (expense.PaidById == user1.Id)
            {
                user1TotalPaid += expense.Amount;
                user1Credit += expense.SplitType switch
                {
                    SplitType.Equal => expense.Amount / 2,
                    SplitType.FullOther => expense.Amount,
                    _ => 0
                };
            }
            else if (expense.PaidById == user2.Id)
            {
                user2TotalPaid += expense.Amount;
                user2Credit += expense.SplitType switch
                {
                    SplitType.Equal => expense.Amount / 2,
                    SplitType.FullOther => expense.Amount,
                    _ => 0
                };
            }
        }

        var netBalance = user1Credit - user2Credit;
        int owesUserId, isOwedUserId;
        long owedAmount;

        if (netBalance > 0)
        {
            owesUserId = user2.Id;
            isOwedUserId = user1.Id;
            owedAmount = netBalance;
        }
        else
        {
            owesUserId = user1.Id;
            isOwedUserId = user2.Id;
            owedAmount = -netBalance;
        }

        return new BalanceDto(
            new UserBalanceInfo(user1.Id, user1.Name, user1TotalPaid, user1Credit),
            new UserBalanceInfo(user2.Id, user2.Name, user2TotalPaid, user2Credit),
            netBalance, owesUserId, isOwedUserId, owedAmount);
    }
}
