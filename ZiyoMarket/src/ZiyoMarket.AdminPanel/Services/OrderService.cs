using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public class OrderService : IOrderService
{
    private readonly IApiService _apiService;

    public OrderService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<PaginatedResult<Order>>> GetOrdersAsync(int pageNumber = 1, int pageSize = 20, string? status = null, string? search = null)
    {
        var endpoint = $"order?pageNumber={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(status))
        {
            endpoint += $"&status={Uri.EscapeDataString(status)}";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            endpoint += $"&search={Uri.EscapeDataString(search)}";
        }

        return await _apiService.GetAsync<PaginatedResult<Order>>(endpoint);
    }

    public async Task<ApiResponse<Order>> GetOrderByIdAsync(int id)
    {
        return await _apiService.GetAsync<Order>($"order/{id}");
    }

    public async Task<ApiResponse<Order>> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
    {
        return await _apiService.PutAsync<Order>($"order/{dto.OrderId}/status", dto);
    }
}
