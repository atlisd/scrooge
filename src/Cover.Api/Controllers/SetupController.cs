using Microsoft.AspNetCore.Mvc;
using Cover.Api.Data;
using Cover.Api.Models;
using Cover.Api.Services;
using Cover.Shared.DTOs;

namespace Cover.Api.Controllers;

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
        return new SetupStatusDto(isComplete);
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

        var users = await _userService.CreateUsersAsync(request.Name1.Trim(), request.Name2.Trim());

        var creds = new AppCredentials
        {
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _db.AppCredentials.Add(creds);
        await _db.SaveChangesAsync();

        return Ok(users);
    }
}
