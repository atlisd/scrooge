using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scrooge.Api.Data;
using Scrooge.Api.Middleware;
using Scrooge.Api.Services;
using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISessionService _sessions;

    public AuthController(AppDbContext db, ISessionService sessions)
    {
        _db = db;
        _sessions = sessions;
    }

    internal static bool IsValidPassword(string p) =>
        p.Length >= 12 && p.Any(char.IsUpper) && p.Any(char.IsLower) &&
        p.Any(char.IsDigit) && p.Any(c => !char.IsLetterOrDigit(c));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var creds = await _db.AppCredentials.FirstOrDefaultAsync();
        if (creds is null)
            return Unauthorized("App not set up.");

        if (creds.LockoutEnd.HasValue && creds.LockoutEnd.Value > DateTime.UtcNow)
            return StatusCode(429, $"Too many attempts. Try again at {creds.LockoutEnd.Value:HH:mm} UTC.");

        if (!string.Equals(creds.Username, request.Username, StringComparison.OrdinalIgnoreCase) ||
            !BCrypt.Net.BCrypt.Verify(request.Password, creds.PasswordHash))
        {
            creds.FailedAttempts++;
            if (creds.FailedAttempts >= 10)
                creds.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
            await _db.SaveChangesAsync();
            return Unauthorized("Invalid username or password.");
        }

        creds.FailedAttempts = 0;
        creds.LockoutEnd = null;
        await _db.SaveChangesAsync();

        var token = await _sessions.CreateAsync();
        SessionToken.SetCookie(HttpContext, token);
        return Ok(new LoginResponse(token));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _sessions.DeleteAsync(SessionToken.Get(Request));
        SessionToken.DeleteCookie(HttpContext);
        return Ok();
    }

    [HttpGet("status")]
    public IActionResult Status() =>
        // The auth middleware has already validated the session for every non-public /api route.
        Ok();
}
