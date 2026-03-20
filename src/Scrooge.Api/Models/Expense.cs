using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Models;

public class Expense
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Merchant { get; set; }
    public long Amount { get; set; }
    public int PaidById { get; set; }
    public User PaidBy { get; set; } = null!;
    public SplitType SplitType { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
