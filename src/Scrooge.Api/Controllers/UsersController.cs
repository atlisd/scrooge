using Microsoft.AspNetCore.Mvc;
using Scrooge.Api.Services;
using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<List<UserDto>> GetAll() => await _userService.GetAllAsync();

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required");

        return await _userService.UpdateNameAsync(id, dto.Name.Trim());
    }
}
