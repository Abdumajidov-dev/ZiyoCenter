using System.ComponentModel.DataAnnotations;

namespace ZiyoMarket.Service.DTOs.Notifications;

/// <summary>
/// DTO for registering/updating device FCM token
/// </summary>
public class RegisterDeviceTokenDto
{
    [Required(ErrorMessage = "FCM token is required")]
    public string Token { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DeviceName { get; set; } = string.Empty; // "iPhone 14 Pro", "Samsung Galaxy S21"

    [Required]
    [MaxLength(50)]
    public string DeviceOs { get; set; } = string.Empty; // "iOS", "Android"

    [MaxLength(20)]
    public string AppVersion { get; set; } = string.Empty; // "1.0.5"
}

/// <summary>
/// DTO for push notification request
/// </summary>
public class SendPushNotificationDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional data to send with notification (optional)
    /// </summary>
    public Dictionary<string, string>? Data { get; set; }

    /// <summary>
    /// Image URL to display in notification (optional)
    /// </summary>
    [Url]
    public string? ImageUrl { get; set; }
}

/// <summary>
/// DTO for batch push notification request
/// </summary>
public class SendBatchPushNotificationDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one user ID is required")]
    public List<int> UserIds { get; set; } = new();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public Dictionary<string, string>? Data { get; set; }

    [Url]
    public string? ImageUrl { get; set; }
}

/// <summary>
/// DTO for topic notification request
/// </summary>
public class SendTopicNotificationDto
{
    [Required]
    [MaxLength(100)]
    public string Topic { get; set; } = string.Empty; // "all_customers", "new_products", "promotions"

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public Dictionary<string, string>? Data { get; set; }

    [Url]
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Device token response DTO
/// </summary>
public class DeviceTokenResultDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceOs { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
