namespace Cover.Shared.DTOs;

public record UpdateExpenseRequest(
    string? Merchant,
    string? Description,
    long Amount,
    SplitType SplitType,
    int PaidById,
    DateOnly Date);
