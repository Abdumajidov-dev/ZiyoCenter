using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public interface INotificationsAdminService
{
    Task<NotificationListResponse> GetNotificationsAsync(int page = 1, int pageSize = 30);
    Task<AdminOrderDetailResponse> GetOrderDetailAsync(int orderId);
    Task<ApiResponse<object>> ApprovePaymentAsync(int orderId);
    Task<ApiResponse<object>> RejectPaymentAsync(int orderId, string reason);
    Task<ApiResponse<object>> MarkAsReadAsync(int notificationId);
}
