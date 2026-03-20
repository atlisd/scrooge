using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scrooge.Api.Data;
using Scrooge.Api.Models;
using Scrooge.Api.Services;
using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AppDbContext _db;

    public SetupController(IUserService userService, AppDbContext db)
    {
        _userService = userService;
        _db = db;
    }

    [HttpGet("status")]
    public async Task<SetupStatusDto> GetStatus()
    {
        var isComplete = await _userService.IsSetupCompleteAsync();
        var creds = await _db.AppCredentials.FirstOrDefaultAsync();
        return new SetupStatusDto(
            isComplete,
            creds?.CurrencyCode ?? "ISK",
            creds?.CurrencyDecimals ?? 0);
    }

    [HttpPost]
    public async Task<ActionResult<List<UserDto>>> Setup(SetupRequest request)
    {
        if (await _userService.IsSetupCompleteAsync())
            return BadRequest("Setup already completed");

        if (string.IsNullOrWhiteSpace(request.Name1) || string.IsNullOrWhiteSpace(request.Name2))
            return BadRequest("Both names are required");

        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username is required");

        if (!AuthController.IsValidPassword(request.Password))
            return BadRequest("Password must be at least 12 characters and contain uppercase, lowercase, digit, and symbol.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            return BadRequest("Currency is required");

        if (request.CurrencyDecimals < 0 || request.CurrencyDecimals > 3)
            return BadRequest("Decimal places must be between 0 and 3");

        var users = await _userService.CreateUsersAsync(request.Name1.Trim(), request.Name2.Trim());

        var creds = new AppCredentials
        {
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CurrencyCode = request.Currency.Trim(),
            CurrencyDecimals = request.CurrencyDecimals
        };
        _db.AppCredentials.Add(creds);
        await _db.SaveChangesAsync();

        return Ok(users);
    }
}
