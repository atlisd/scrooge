using Scrooge.Shared.DTOs;

namespace Scrooge.Api.Services;

public interface IBalanceService
{
    Task<BalanceDto?> GetBalanceAsync();
}
