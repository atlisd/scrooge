namespace Scrooge.Shared.DTOs;

public record LoginRequest(string Username, string Password);

/// <summary>Returned on login. Web clients can ignore it (they use the cookie); native clients send it as a Bearer token.</summary>
public record LoginResponse(string Token);
