using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public interface IOrderService
{
    Task<ApiResponse<PaginatedResult<Order>>> GetOrdersAsync(int pageNumber = 1, int pageSize = 20, string? status = null, string? search = null);
    Task<ApiResponse<Order>> GetOrderByIdAsync(int id);
    Task<ApiResponse<Order>> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
}
