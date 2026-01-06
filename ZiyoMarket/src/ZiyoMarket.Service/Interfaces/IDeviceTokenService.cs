using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Notifications;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IDeviceTokenService
{
    /// <summary>
    /// Register or update device FCM token for a user
    /// </summary>
    Task<Result> RegisterDeviceTokenAsync(int userId, UserType userType, RegisterDeviceTokenDto dto);

    /// <summary>
    /// Get all active device tokens for a user
    /// </summary>
    Task<Result<List<DeviceTokenResultDto>>> GetUserDeviceTokensAsync(int userId, UserType userType);

    /// <summary>
    /// Deactivate a specific device token
    /// </summary>
    Task<Result> DeactivateDeviceTokenAsync(int userId, string token);

    /// <summary>
    /// Deactivate all tokens for a user (logout from all devices)
    /// </summary>
    Task<Result> DeactivateAllUserTokensAsync(int userId, UserType userType);

    /// <summary>
    /// Clean up expired tokens (last used > 60 days ago)
    /// </summary>
    Task<Result> CleanupExpiredTokensAsync();

    /// <summary>
    /// Send push notification to a specific user (all their devices)
    /// </summary>
    Task<Result> SendPushNotificationAsync(SendPushNotificationDto dto);

    /// <summary>
    /// Send push notification to multiple users
    /// </summary>
    Task<Result> SendBatchPushNotificationAsync(SendBatchPushNotificationDto dto);

    /// <summary>
    /// Send push notification to a topic
    /// </summary>
    Task<Result> SendTopicNotificationAsync(SendTopicNotificationDto dto);
}
