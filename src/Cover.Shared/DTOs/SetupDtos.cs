namespace Cover.Shared.DTOs;

public record SetupRequest(string Name1, string Name2, string Username, string Password);

public record SetupStatusDto(bool IsComplete);
