using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cover.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Cover.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly NavigationManager _nav;

    private const string TokenKey = "auth_token";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(HttpClient http, IJSRuntime js, NavigationManager nav)
    {
        _http = http;
        _js = js;
        _nav = nav;
    }

    public async Task InitializeAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions))!;
        await SetTokenAsync(result.Token);
        return result;
    }

    public async Task LogoutAsync()
    {
        await _http.PostAsync("api/auth/logout", null);
        await ClearTokenAsync();
        _nav.NavigateTo("/login");
    }

    public async Task<bool> GetAuthStatusAsync()
    {
        var response = await _http.GetAsync("api/auth/status");
        return response.IsSuccessStatusCode;
    }

    public async Task<SetupStatusDto> GetSetupStatusAsync()
        => await _http.GetFromJsonAsync<SetupStatusDto>("api/setup/status", JsonOptions)
           ?? new SetupStatusDto(false);

    public async Task<List<UserDto>> SetupAsync(SetupRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/setup", request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
        }
        return await response.Content.ReadFromJsonAsync<List<UserDto>>(JsonOptions) ?? [];
    }

    public async Task<List<UserDto>> GetUsersAsync()
        => await _http.GetFromJsonAsync<List<UserDto>>("api/users", JsonOptions) ?? [];

    public async Task<BalanceDto?> GetBalanceAsync()
    {
        var response = await _http.GetAsync("api/balance");
        if (response.StatusCode == HttpStatusCode.Unauthorized) { await HandleUnauthorized(); return null; }
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<BalanceDto>(JsonOptions);
    }

    public async Task<PagedResult<ExpenseDto>> GetExpensesAsync(int page = 1, int pageSize = 20, int? paidById = null)
    {
        var url = $"api/expenses?page={page}&pageSize={pageSize}";
        if (paidById.HasValue) url += $"&paidById={paidById}";
        var response = await _http.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.Unauthorized) { await HandleUnauthorized(); return new PagedResult<ExpenseDto>([], 0, page, pageSize); }
        return await response.Content.ReadFromJsonAsync<PagedResult<ExpenseDto>>(JsonOptions)
               ?? new PagedResult<ExpenseDto>([], 0, page, pageSize);
    }

    public async Task<ExpenseDto?> GetExpenseAsync(int id)
    {
        var response = await _http.GetAsync($"api/expenses/{id}");
        if (response.StatusCode == HttpStatusCode.Unauthorized) { await HandleUnauthorized(); return null; }
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ExpenseDto>(JsonOptions);
    }

    public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/expenses", request, JsonOptions);
        if (response.StatusCode == HttpStatusCode.Unauthorized) { await HandleUnauthorized(); throw new UnauthorizedAccessException(); }
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>(JsonOptions))!;
    }

    public async Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/expenses/{id}", request, JsonOptions);
        if (response.StatusCode == HttpStatusCode.Unauthorized) { await HandleUnauthorized(); throw new UnauthorizedAccessException(); }
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>(JsonOptions))!;
    }

    public async Task<List<string>> GetMerchantsAsync(string query)
    {
        var response = await _http.GetAsync($"api/expenses/merchants?q={Uri.EscapeDataString(query)}");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<string>>(JsonOptions) ?? [];
    }

    public async Task DeleteExpenseAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/expenses/{id}");
        if (response.StatusCode == HttpStatusCode.Unauthorized) { await HandleUnauthorized(); throw new UnauthorizedAccessException(); }
        response.EnsureSuccessStatusCode();
    }

    private async Task SetTokenAsync(string token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task ClearTokenAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        _http.DefaultRequestHeaders.Authorization = null;
    }

    private async Task HandleUnauthorized()
    {
        await ClearTokenAsync();
        _nav.NavigateTo("/login");
    }
}
