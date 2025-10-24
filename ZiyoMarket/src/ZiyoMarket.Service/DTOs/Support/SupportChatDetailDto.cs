// ZiyoMarket.Service/DTOs/Support/SupportDtos.cs

using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Common;

namespace ZiyoMarket.Service.DTOs.Support;

// Chat DTOs
public class SupportChatDetailDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int? AdminId { get; set; }
    public string? AdminName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int? OrderId { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public int? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
    public List<SupportMessageDto> Messages { get; set; } = new();
}

public class SupportChatListDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? AdminName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime StartedAt { get; set; }
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
}

public class CreateChatDto
{
    public string Subject { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public int? OrderId { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public string? InitialMessage { get; set; }
    public string? AttachmentUrl { get; set; }
}

// Message DTOs
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

public class SendMessageDto
{
    public int ChatId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

// Filter DTOs
public class SupportChatFilterRequest : PaginationRequest
{
    public SupportChatStatus? Status { get; set; }
    public string? Priority { get; set; }
    public int? CustomerId { get; set; }
    public int? AdminId { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

// Feedback DTOs
public class SubmitFeedbackDto
{
    public int Rating { get; set; } // 1-5
    public string? Feedback { get; set; }
}

public class ChatFeedbackDto
{
    public int ChatId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? AdminName { get; set; }
    public int Rating { get; set; }
    public string? Feedback { get; set; }
    public DateTime ClosedAt { get; set; }
}

public class FeedbackStatsDto
{
    public int TotalRatings { get; set; }
    public double AverageRating { get; set; }
    public int Rating1Count { get; set; }
    public int Rating2Count { get; set; }
    public int Rating3Count { get; set; }
    public int Rating4Count { get; set; }
    public int Rating5Count { get; set; }
}

// Statistics DTOs
public class CustomerSupportStatsDto
{
    public int TotalChats { get; set; }
    public int OpenChats { get; set; }
    public int InProgressChats { get; set; }
    public int ClosedChats { get; set; }
    public double AverageResponseTime { get; set; } // minutes
    public double AverageResolutionTime { get; set; } // hours
}

public class AdminSupportStatsDto
{
    public int AssignedChats { get; set; }
    public int OpenChats { get; set; }
    public int InProgressChats { get; set; }
    public int ClosedChats { get; set; }
    public double AverageResponseTime { get; set; }
    public double AverageResolutionTime { get; set; }
}

public class SupportStatsDto
{
    public int TotalChats { get; set; }
    public int OpenChats { get; set; }
    public int InProgressChats { get; set; }
    public int ClosedChats { get; set; }
    public int EscalatedChats { get; set; }
    public double AverageResponseTime { get; set; }
    public double AverageResolutionTime { get; set; }
    public double CustomerSatisfactionScore { get; set; }
}

public class ChatResponseTimeDto
{
    public int ChatId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public double ResponseTimeMinutes { get; set; }
}

public class ChatResolutionTimeDto
{
    public int ChatId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public double ResolutionTimeHours { get; set; }
    public int? CustomerRating { get; set; }
}