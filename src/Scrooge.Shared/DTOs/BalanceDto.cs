namespace Scrooge.Shared.DTOs;

public record UserBalanceInfo(int UserId, string Name, long TotalPaid, long Credit);

public record BalanceDto(
    UserBalanceInfo User1,
    UserBalanceInfo User2,
    long NetBalance,
    int OwesUserId,
    int IsOwedUserId,
    long OwedAmount);
