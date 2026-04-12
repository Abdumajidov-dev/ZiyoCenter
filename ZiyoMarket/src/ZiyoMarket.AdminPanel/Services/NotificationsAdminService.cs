using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public class NotificationsAdminService : INotificationsAdminService
{
    private readonly IApiService _api;

    public NotificationsAdminService(IApiService api) => _api = api;

    public async Task<NotificationListResponse> GetNotificationsAsync(int page = 1, int pageSize = 30)
    {
        var result = await _api.GetAsync<NotificationListResponse>(
            $"notifications?page_number={page}&page_size={pageSize}");
        return result.Data ?? new NotificationListResponse { Success = false };
    }

    public async Task<AdminOrderDetailResponse> GetOrderDetailAsync(int orderId)
    {
        var result = await _api.GetAsync<AdminOrderDetailResponse>($"order/{orderId}");
        return result.Data ?? new AdminOrderDetailResponse { Success = false, Message = result.Message };
    }

    public async Task<ApiResponse<object>> ApprovePaymentAsync(int orderId)
        => await _api.PostAsync<object>($"order/{orderId}/approve-payment", new { });

    public async Task<ApiResponse<object>> RejectPaymentAsync(int orderId, string reason)
        => await _api.PostAsync<object>($"order/{orderId}/reject-payment",
            new { reason });

    public async Task<ApiResponse<object>> MarkAsReadAsync(int notificationId)
        => await _api.PostAsync<object>($"notifications/{notificationId}/read", new { });
}
