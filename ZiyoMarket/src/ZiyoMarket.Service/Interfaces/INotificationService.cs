using ZiyoMarket.Service.DTOs.Notifications;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface INotificationService
{
    Task<Result> SendNotificationAsync(CreateNotificationDto request);
    Task<Result> SendBulkNotificationAsync(List<CreateNotificationDto> requests);
    Task<Result<List<NotificationDto>>> GetNotificationsAsync(int userId, string userType, int pageNumber = 1, int pageSize = 20);
    Task<Result<int>> GetUnreadCountAsync(int userId, string userType);
    Task<Result> MarkAsReadAsync(int notificationId, int userId);
    Task<Result> MarkAllAsReadAsync(int userId, string userType);
    Task<Result> DeleteNotificationAsync(int notificationId, int userId);

    // Helper methods
    Task<Result> NotifyOrderCreatedAsync(int customerId, int orderId);
    Task<Result> NotifyOrderStatusChangedAsync(int orderId, string newStatus);
    Task<Result> NotifyCashbackEarnedAsync(int customerId, decimal amount, int orderId);
    Task<Result> NotifyAdminAboutNewOrderAsync(int orderId);

    // Bulk operations
    Task<Result> DeleteAllNotificationsAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<NotificationDto>>> SeedMockNotificationsAsync(int userId, string userType, int count = 10);
}