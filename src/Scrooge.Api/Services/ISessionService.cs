namespace Scrooge.Api.Services;

public enum SessionValidation
{
    Invalid,
    Valid,
    /// <summary>Valid, and LastUsedAt was just refreshed — callers may re-issue the cookie.</summary>
    Renewed
}

public interface ISessionService
{
    Task<string> CreateAsync();
    Task<SessionValidation> ValidateAsync(string? token);
    Task DeleteAsync(string? token);
}
