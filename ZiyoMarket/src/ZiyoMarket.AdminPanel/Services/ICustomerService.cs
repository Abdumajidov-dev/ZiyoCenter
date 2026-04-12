using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public interface ICustomerService
{
    Task<CustomerListApiResponse> GetCustomersAsync(int page = 1, int pageSize = 20, string? search = null, bool? isActive = null);
    Task<CustomerDetailApiResponse> GetCustomerByIdAsync(int id);
    Task<ApiResponse<object>> ToggleStatusAsync(int id);
    Task<ApiResponse<object>> PromoteToAdminAsync(int customerId, string role = "Admin");
}
