using System;
using System.ComponentModel.DataAnnotations;
using ZiyoMarket.Service.DTOs.Common;

namespace ZiyoMarket.Service.DTOs.Notifications;

/// <summary>
/// Notification DTO
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string? Data { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool PushSent { get; set; }
    public bool EmailSent { get; set; }
    public bool SMSSent { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create notification DTO
/// </summary>
public class CreateNotificationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    public string UserType { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Priority { get; set; } = "Normal";

    public string? Data { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public string? ImageUrl { get; set; }

    public bool SendPush { get; set; } = true;
    public bool SendEmail { get; set; } = false;
    public bool SendSMS { get; set; } = false;
}

/// <summary>
/// Support chat DTO
/// </summary>
public class SupportChatDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? AdminId { get; set; }
    public string? AdminName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string PriorityLabel => Priority switch
    {
        1 => "High",
        2 => "Medium",
        3 => "Low",
        _ => "Unknown"
    };
    public int? OrderId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public int UnreadMessagesCount { get; set; }
}

/// <summary>
/// Create support chat DTO
/// </summary>
public class CreateSupportChatDto
{
    [Required(ErrorMessage = "Subject is required")]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Initial message is required")]
    [MaxLength(2000)]
    public string InitialMessage { get; set; } = string.Empty;
    
    [Range(1, 3)]
    public int Priority { get; set; } = 3;
    
    public int? OrderId { get; set; }
}

/// <summary>
/// Support message DTO
/// </summary>
public class SupportMessageDto
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Send support message DTO
/// </summary>
public class SendSupportMessageDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ChatId { get; set; }
    
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
}

/// <summary>
/// Close support chat DTO
/// </summary>
public class CloseSupportChatDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ChatId { get; set; }
    
    [Required(ErrorMessage = "Resolution is required")]
    [MaxLength(1000)]
    public string Resolution { get; set; } = string.Empty;
}
