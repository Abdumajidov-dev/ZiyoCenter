using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Notifications;

/// <summary>
/// Device tokens for push notifications (FCM)
/// Supports multiple devices per user
/// </summary>
public class DeviceToken : BaseEntity
{
    public int UserId { get; set; }
    public UserType UserType { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty; // "iPhone 14", "Samsung Galaxy S21"
    public string DeviceOs { get; set; } = string.Empty; // "iOS", "Android"
    public string AppVersion { get; set; } = string.Empty; // "1.0.5"
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }

    // Computed property
    public bool IsExpired => LastUsedAt.HasValue && DateTime.UtcNow.Subtract(LastUsedAt.Value).TotalDays > 60;
}
