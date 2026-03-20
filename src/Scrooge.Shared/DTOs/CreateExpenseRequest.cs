namespace Scrooge.Shared.DTOs;

public record CreateExpenseRequest(
    string? Merchant,
    string? Description,
    long Amount,
    SplitType SplitType,
    int PaidById,
    DateOnly Date);
