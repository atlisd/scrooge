namespace Scrooge.Api.Models;

public class AppCredentials
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public string? SessionToken { get; set; }
    public string CurrencyCode { get; set; } = "ISK";
    public int CurrencyDecimals { get; set; }
}
