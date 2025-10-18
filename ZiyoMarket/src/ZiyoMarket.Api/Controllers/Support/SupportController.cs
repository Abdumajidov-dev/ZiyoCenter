using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Notifications;

namespace ZiyoMarket.Api.Controllers.Support;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupportController : BaseController
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetChatById(int id)
    {
        var result = await _supportService.GetChatByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetCustomerChats()
    {
        var customerId = GetCurrentUserId();
        var result = await _supportService.GetCustomerChatsAsync(customerId);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminChats()
    {
        var adminId = GetCurrentUserId();
        var result = await _supportService.GetAdminChatsAsync(adminId);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateChat([FromBody] CreateSupportChatDto request)
    {
        var customerId = GetCurrentUserId();
        var result = await _supportService.CreateChatAsync(request, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignChatToAdmin(int id, [FromBody] int adminId)
    {
        var result = await _supportService.AssignChatToAdminAsync(id, adminId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("{id}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CloseChat(int id, [FromBody] string resolution)
    {
        var closedBy = GetCurrentUserId();
        var result = await _supportService.CloseChatAsync(id, resolution, closedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetChatMessages(int chatId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _supportService.GetChatMessagesAsync(chatId, pageNumber, pageSize);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] CreateSupportMessageDto request)
    {
        var senderId = GetCurrentUserId();
        var senderType = GetCurrentUserType();
        var result = await _supportService.SendMessageAsync(request, senderId, senderType);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("{chatId}/messages/read")]
    public async Task<IActionResult> MarkMessagesAsRead(int chatId)
    {
        var userId = GetCurrentUserId();
        var result = await _supportService.MarkMessagesAsReadAsync(chatId, userId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllChats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _supportService.DeleteAllChatsAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("seed")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> SeedMockChats([FromQuery] int count = 10)
    {
        var customerId = GetCurrentUserId();
        var result = await _supportService.SeedMockChatsAsync(customerId, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}