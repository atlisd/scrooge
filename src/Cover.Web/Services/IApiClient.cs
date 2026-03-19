using Cover.Shared.DTOs;

namespace Cover.Web.Services;

public interface IApiClient
{
    Task InitializeAsync();
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<bool> GetAuthStatusAsync();
    Task<SetupStatusDto> GetSetupStatusAsync();
    Task<List<UserDto>> SetupAsync(SetupRequest request);
    Task<List<UserDto>> GetUsersAsync();
    Task<BalanceDto?> GetBalanceAsync();
    Task<PagedResult<ExpenseDto>> GetExpensesAsync(int page = 1, int pageSize = 20, int? paidById = null);
    Task<ExpenseDto?> GetExpenseAsync(int id);
    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest request);
    Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseRequest request);
    Task DeleteExpenseAsync(int id);
    Task<List<string>> GetMerchantsAsync(string query);
}
