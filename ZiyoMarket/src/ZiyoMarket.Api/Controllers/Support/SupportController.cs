using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Api.Controllers;
using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.DTOs.Support;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Support;

/// <summary>
/// Support chat and messaging endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SupportController : BaseController
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    // ============ CHAT OPERATIONS ============

    /// <summary>
    /// Get chat by ID
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <response code="200">Chat retrieved successfully</response>
    /// <response code="404">Chat not found</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(SupportChatDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatById(int id)
    {
        var result = await _supportService.GetChatByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get paginated list of chats with filters
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <response code="200">Chats retrieved successfully</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginationResponse<SupportChatListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChats([FromQuery] SupportChatFilterRequest request)
    {
        var result = await _supportService.GetChatsAsync(request);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Create new support chat (Customer only)
    /// </summary>
    /// <param name="request">Chat details</param>
    /// <response code="201">Chat created successfully</response>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(SupportChatDetailDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatDto request)
    {
        var customerId = GetCurrentUserId();
        var result = await _supportService.CreateChatAsync(request, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Close chat (Admin only)
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <param name="closeReason">Reason for closing</param>
    /// <response code="200">Chat closed successfully</response>
    [HttpPost("{id}/close")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseChat(int id, [FromBody] string closeReason)
    {
        var closedBy = GetCurrentUserId();
        var result = await _supportService.CloseChatAsync(id, closeReason, closedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Reopen closed chat
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <response code="200">Chat reopened successfully</response>
    [HttpPost("{id}/reopen")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReopenChat(int id)
    {
        var reopenedBy = GetCurrentUserId();
        var result = await _supportService.ReopenChatAsync(id, reopenedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Assign chat to admin (Admin only)
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <param name="adminId">Admin ID to assign</param>
    /// <response code="200">Chat assigned successfully</response>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignChat(int id, [FromBody] int adminId)
    {
        var assignedBy = GetCurrentUserId();
        var result = await _supportService.AssignChatAsync(id, adminId, assignedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Unassign chat from admin (Admin only)
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <response code="200">Chat unassigned successfully</response>
    [HttpPost("{id}/unassign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UnassignChat(int id)
    {
        var unassignedBy = GetCurrentUserId();
        var result = await _supportService.UnassignChatAsync(id, unassignedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    // ============ MESSAGE OPERATIONS ============

    /// <summary>
    /// Get chat messages
    /// </summary>
    /// <param name="chatId">Chat ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <response code="200">Messages retrieved successfully</response>
    [HttpGet("{chatId}/messages")]
    [Authorize]
    [ProducesResponseType(typeof(List<SupportMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatMessages(
        int chatId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _supportService.GetChatMessagesAsync(chatId, pageNumber, pageSize);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Send message in chat
    /// </summary>
    /// <param name="request">Message details</param>
    /// <response code="201">Message sent successfully</response>
    [HttpPost("messages")]
    [Authorize]
    [ProducesResponseType(typeof(SupportMessageDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
    {
        var senderId = GetCurrentUserId();
        var senderType = GetCurrentUserType();
        var result = await _supportService.SendMessageAsync(request, senderId, senderType);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Delete message
    /// </summary>
    /// <param name="messageId">Message ID</param>
    /// <response code="200">Message deleted successfully</response>
    [HttpDelete("messages/{messageId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _supportService.DeleteMessageAsync(messageId, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Edit message
    /// </summary>
    /// <param name="messageId">Message ID</param>
    /// <param name="newMessage">New message text</param>
    /// <response code="200">Message updated successfully</response>
    [HttpPut("messages/{messageId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EditMessage(int messageId, [FromBody] string newMessage)
    {
        var editedBy = GetCurrentUserId();
        var result = await _supportService.EditMessageAsync(messageId, newMessage, editedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Mark message as read
    /// </summary>
    /// <param name="messageId">Message ID</param>
    /// <response code="200">Message marked as read</response>
    [HttpPost("messages/{messageId}/read")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkMessageAsRead(int messageId)
    {
        var userId = GetCurrentUserId();
        var result = await _supportService.MarkMessageAsReadAsync(messageId, userId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    // ============ CUSTOMER SUPPORT ============

    /// <summary>
    /// Get customer's chats
    /// </summary>
    /// <response code="200">Chats retrieved successfully</response>
    [HttpGet("my-chats")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(List<SupportChatListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyChats()
    {
        var customerId = GetCurrentUserId();
        var result = await _supportService.GetCustomerChatsAsync(customerId);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get customer support statistics
    /// </summary>
    /// <response code="200">Stats retrieved successfully</response>
    [HttpGet("my-stats")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(CustomerSupportStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyStats()
    {
        var customerId = GetCurrentUserId();
        var result = await _supportService.GetCustomerSupportStatsAsync(customerId);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get customer's latest chat
    /// </summary>
    /// <response code="200">Chat retrieved successfully</response>
    [HttpGet("my-latest-chat")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(SupportChatDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLatestChat()
    {
        var customerId = GetCurrentUserId();
        var result = await _supportService.GetLatestCustomerChatAsync(customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    // ============ ADMIN SUPPORT ============

    /// <summary>
    /// Get admin's assigned chats (Admin only)
    /// </summary>
    /// <response code="200">Chats retrieved successfully</response>
    [HttpGet("admin/my-chats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SupportChatListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminChats()
    {
        var adminId = GetCurrentUserId();
        var result = await _supportService.GetAdminChatsAsync(adminId);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get admin support statistics (Admin only)
    /// </summary>
    /// <response code="200">Stats retrieved successfully</response>
    [HttpGet("admin/my-stats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AdminSupportStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminStats()
    {
        var adminId = GetCurrentUserId();
        var result = await _supportService.GetAdminSupportStatsAsync(adminId);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get unassigned chats (Admin only)
    /// </summary>
    /// <response code="200">Chats retrieved successfully</response>
    [HttpGet("admin/unassigned")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SupportChatListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnassignedChats()
    {
        var result = await _supportService.GetUnassignedChatsAsync();
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get overdue chats (Admin only)
    /// </summary>
    /// <response code="200">Chats retrieved successfully</response>
    [HttpGet("admin/overdue")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SupportChatListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdueChats()
    {
        var result = await _supportService.GetOverdueChatsAsync();
        return Ok(new { success = true, data = result.Data });
    }

    // ============ FEEDBACK ============

    /// <summary>
    /// Submit chat feedback (Customer only)
    /// </summary>
    /// <param name="chatId">Chat ID</param>
    /// <param name="request">Feedback details</param>
    /// <response code="200">Feedback submitted successfully</response>
    [HttpPost("{chatId}/feedback")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitFeedback(int chatId, [FromBody] SubmitFeedbackDto request)
    {
        var result = await _supportService.SubmitChatFeedbackAsync(chatId, request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Get chat feedback (Admin only)
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <response code="200">Feedback retrieved successfully</response>
    [HttpGet("feedback")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<ChatFeedbackDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatFeedback(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _supportService.GetChatFeedbackAsync(startDate, endDate);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get feedback statistics (Admin only)
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <response code="200">Stats retrieved successfully</response>
    [HttpGet("feedback/statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FeedbackStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeedbackStatistics(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _supportService.GetFeedbackStatisticsAsync(startDate, endDate);
        return Ok(new { success = true, data = result.Data });
    }

    // ============ STATISTICS ============

    /// <summary>
    /// Get support statistics (Admin only)
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <response code="200">Stats retrieved successfully</response>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SupportStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportStatistics(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _supportService.GetSupportStatisticsAsync(startDate, endDate);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get response time statistics (Admin only)
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <response code="200">Stats retrieved successfully</response>
    [HttpGet("statistics/response-time")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<ChatResponseTimeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResponseTimeStats(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _supportService.GetResponseTimeStatsAsync(startDate, endDate);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get resolution time statistics (Admin only)
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <response code="200">Stats retrieved successfully</response>
    [HttpGet("statistics/resolution-time")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<ChatResolutionTimeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResolutionTimeStats(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _supportService.GetResolutionTimeStatsAsync(startDate, endDate);
        return Ok(new { success = true, data = result.Data });
    }

    // ============ TAGS AND CATEGORIES ============

    /// <summary>
    /// Get all chat tags
    /// </summary>
    /// <response code="200">Tags retrieved successfully</response>
    [HttpGet("tags")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTags()
    {
        var result = await _supportService.GetAllChatTagsAsync();
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get all chat categories
    /// </summary>
    /// <response code="200">Categories retrieved successfully</response>
    [HttpGet("categories")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _supportService.GetAllChatCategoriesAsync();
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get chats by tag (Admin only)
    /// </summary>
    /// <param name="tag">Tag name</param>
    /// <response code="200">Chats retrieved successfully</response>
    [HttpGet("tags/{tag}/chats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SupportChatListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatsByTag(string tag)
    {
        var result = await _supportService.GetChatsByTagAsync(tag);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Get chats by category (Admin only)
    /// </summary>
    /// <param name="category">Category name</param>
    /// <response code="200">Chats retrieved successfully</response>
    [HttpGet("categories/{category}/chats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SupportChatListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatsByCategory(string category)
    {
        var result = await _supportService.GetChatsByCategoryAsync(category);
        return Ok(new { success = true, data = result.Data });
    }

    // ============ BULK OPERATIONS ============

    /// <summary>
    /// Delete all chats (Admin only)
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <response code="200">Chats deleted successfully</response>
    [HttpDelete("bulk/delete")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllChats(
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _supportService.DeleteAllChatsAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Seed mock chats for testing (Admin only)
    /// </summary>
    /// <param name="count">Number of chats to create</param>
    /// <response code="201">Mock chats created successfully</response>
    [HttpPost("seed/chats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SupportChatDetailDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> SeedMockChats([FromQuery] int count = 10)
    {
        var createdBy = GetCurrentUserId();
        var result = await _supportService.SeedMockChatsAsync(createdBy, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Seed mock messages for testing (Admin only)
    /// </summary>
    /// <param name="chatId">Chat ID</param>
    /// <param name="count">Number of messages to create</param>
    /// <response code="201">Mock messages created successfully</response>
    [HttpPost("seed/messages/{chatId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SupportMessageDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> SeedMockMessages(int chatId, [FromQuery] int count = 10)
    {
        var createdBy = GetCurrentUserId();
        var result = await _supportService.SeedMockMessagesAsync(chatId, createdBy, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }
}