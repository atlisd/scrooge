using Microsoft.AspNetCore.Mvc;
using Scrooge.Api.Services;
using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalanceController : ControllerBase
{
    private readonly IBalanceService _balanceService;

    public BalanceController(IBalanceService balanceService) => _balanceService = balanceService;

    [HttpGet]
    public async Task<ActionResult<BalanceDto>> Get()
    {
        var balance = await _balanceService.GetBalanceAsync();
        return balance is null ? NotFound("Setup not complete") : Ok(balance);
    }
}
