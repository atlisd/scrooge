namespace Scrooge.Api.Middleware;

/// <summary>
/// Reads/writes the session token. Browsers use the HttpOnly <c>session_token</c> cookie;
/// native clients (iOS) send <c>Authorization: Bearer &lt;token&gt;</c>.
/// </summary>
public static class SessionToken
{
    public const string CookieName = "session_token";

    public static string? FromCookie(HttpRequest request) =>
        request.Cookies[CookieName];

    public static string? FromBearer(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            var token = header[prefix.Length..].Trim();
            return token.Length > 0 ? token : null;
        }
        return null;
    }

    /// <summary>Cookie takes precedence; falls back to the Bearer header.</summary>
    public static string? Get(HttpRequest request) =>
        FromCookie(request) ?? FromBearer(request);

    public static void SetCookie(HttpContext context, string token) =>
        context.Response.Cookies.Append(CookieName, token, CookieOptions(context.Request));

    public static void DeleteCookie(HttpContext context) =>
        context.Response.Cookies.Delete(CookieName, CookieOptions(context.Request));

    private static CookieOptions CookieOptions(HttpRequest request) => new()
    {
        HttpOnly = true,
        Secure = request.IsHttps,
        SameSite = SameSiteMode.Strict,
        Path = "/api",
        Expires = DateTimeOffset.UtcNow.AddDays(365)
    };
}
