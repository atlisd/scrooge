using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cover.Api.Data;
using Cover.Shared.DTOs;

namespace Cover.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

    internal static bool IsValidPassword(string p) =>
        p.Length >= 12 && p.Any(char.IsUpper) && p.Any(char.IsLower) &&
        p.Any(char.IsDigit) && p.Any(c => !char.IsLetterOrDigit(c));

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
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
        creds.SessionToken = Guid.NewGuid().ToString();
        await _db.SaveChangesAsync();

        return Ok(new LoginResponse(creds.SessionToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        var creds = await _db.AppCredentials.FirstOrDefaultAsync();
        if (creds is not null && creds.SessionToken == token)
        {
            creds.SessionToken = null;
            await _db.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        var creds = await _db.AppCredentials.FirstOrDefaultAsync();
        if (token is null || creds?.SessionToken != token)
            return Unauthorized();
        return Ok();
    }
}
