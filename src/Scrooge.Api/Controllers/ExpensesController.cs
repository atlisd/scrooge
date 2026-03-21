using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Scrooge.Api.Hubs;
using Scrooge.Api.Services;
using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly IHubContext<ExpenseHub> _hub;

    public ExpensesController(IExpenseService expenseService, IHubContext<ExpenseHub> hub)
    {
        _expenseService = expenseService;
        _hub = hub;
    }

    [HttpGet]
    public async Task<PagedResult<ExpenseDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? paidById = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return await _expenseService.GetAllAsync(page, pageSize, paidById);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> GetById(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpGet("merchants")]
    public async Task<List<string>> GetMerchants([FromQuery] string q = "")
        => await _expenseService.GetMerchantsAsync(q);

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(CreateExpenseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Merchant))
            return BadRequest("Merchant is required");
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        var expense = await _expenseService.CreateAsync(request);
        await _hub.Clients.All.SendAsync("ExpenseChanged");
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> Update(int id, UpdateExpenseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Merchant))
            return BadRequest("Merchant is required");
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        var result = await _expenseService.UpdateAsync(id, request);
        await _hub.Clients.All.SendAsync("ExpenseChanged");
        return result;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _expenseService.DeleteAsync(id);
        await _hub.Clients.All.SendAsync("ExpenseChanged");
        return NoContent();
    }
}
