namespace Scrooge.Shared.DTOs;

public record ExpenseDto(
    int Id,
    string? Merchant,
    string? Description,
    long Amount,
    SplitType SplitType,
    int PaidById,
    string PaidByName,
    DateOnly Date,
    DateTime CreatedAt);
