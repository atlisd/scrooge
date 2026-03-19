namespace Cover.Shared.DTOs;

public record SetupRequest(string Name1, string Name2, string Username, string Password, string Currency, int CurrencyDecimals);

public record SetupStatusDto(bool IsComplete, string CurrencyCode = "ISK", int CurrencyDecimals = 0);
