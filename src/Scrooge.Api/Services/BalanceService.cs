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

        var summary = await _db.Expenses
            .GroupBy(e => e.PaidById)
            .Select(g => new
            {
                PaidById = g.Key,
                TotalPaid = g.Sum(e => e.Amount),
                Credit = g.Sum(e =>
                    e.SplitType == SplitType.Equal     ? e.Amount / 2 :
                    e.SplitType == SplitType.FullOther ? e.Amount     : 0L)
            })
            .ToListAsync();

        var u1 = summary.FirstOrDefault(s => s.PaidById == user1.Id);
        var u2 = summary.FirstOrDefault(s => s.PaidById == user2.Id);

        long user1TotalPaid = u1?.TotalPaid ?? 0;
        long user1Credit    = u1?.Credit    ?? 0;
        long user2TotalPaid = u2?.TotalPaid ?? 0;
        long user2Credit    = u2?.Credit    ?? 0;

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
