using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public class CustomerService : ICustomerService
{
    private readonly IApiService _apiService;

    public CustomerService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<CustomerListApiResponse> GetCustomersAsync(int page = 1, int pageSize = 20, string? search = null, bool? isActive = null)
    {
        var url = $"customer?page={page}&page_size={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (isActive.HasValue)
            url += $"&is_active={isActive.Value.ToString().ToLower()}";

        var result = await _apiService.GetAsync<CustomerListApiResponse>(url);
        return result.Data ?? new CustomerListApiResponse { Success = false, Message = result.Message };
    }

    public async Task<CustomerDetailApiResponse> GetCustomerByIdAsync(int id)
    {
        var result = await _apiService.GetAsync<CustomerDetailApiResponse>($"customer/{id}");
        return result.Data ?? new CustomerDetailApiResponse { Success = false, Message = result.Message };
    }

    public async Task<ApiResponse<object>> ToggleStatusAsync(int id)
    {
        return await _apiService.PostAsync<object>($"customer/{id}/toggle-status", new { });
    }

    public async Task<ApiResponse<object>> PromoteToAdminAsync(int customerId, string role = "Admin")
    {
        return await _apiService.PostAsync<object>($"admin/promote-customer/{customerId}?role={role}", new { });
    }
}
