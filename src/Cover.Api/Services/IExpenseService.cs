using Cover.Shared.DTOs;

namespace Cover.Api.Services;

public interface IExpenseService
{
    Task<PagedResult<ExpenseDto>> GetAllAsync(int page, int pageSize, int? paidById = null);
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request);
    Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseRequest request);
    Task DeleteAsync(int id);
    Task<List<string>> GetMerchantsAsync(string query);
}
