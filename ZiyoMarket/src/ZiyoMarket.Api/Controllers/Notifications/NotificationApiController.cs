using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Notifications;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Notifications;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationApiController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationApiController(INotificationService notificationService)
    {
     _notificationService = notificationService;
    }

  [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();
        var result = await _notificationService.GetNotificationsAsync(userId, userType, pageNumber, pageSize);

  if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();
        var result = await _notificationService.GetUnreadCountAsync(userId, userType);

if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

      return Ok(new { success = true, count = result.Data });
    }

    [HttpPost("{id}/read")]
 public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.MarkAsReadAsync(id, userId);

        if (!result.IsSuccess)
    return StatusCode(result.StatusCode, new { message = result.Message });

  return Ok(new { success = true, message = result.Message });
    }

  [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
   var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();
        var result = await _notificationService.MarkAllAsReadAsync(userId, userType);

        if (!result.IsSuccess)
 return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
    var userId = GetCurrentUserId();
        var result = await _notificationService.DeleteNotificationAsync(id, userId);

  if (!result.IsSuccess)
         return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDto request)
    {
   var result = await _notificationService.SendNotificationAsync(request);

        if (!result.IsSuccess)
          return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("send-bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBulkNotification([FromBody] List<CreateNotificationDto> requests)
    {
        var result = await _notificationService.SendBulkNotificationAsync(requests);

        if (!result.IsSuccess)
    return StatusCode(result.StatusCode, new { message = result.Message });

  return Ok(new { success = true, message = result.Message });
    }

    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllNotifications(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _notificationService.DeleteAllNotificationsAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
       return StatusCode(result.StatusCode, new { message = result.Message });

  return Ok(new { success = true, message = result.Message });
    }

[HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedMockNotifications([FromQuery] int count = 10)
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();
        var result = await _notificationService.SeedMockNotificationsAsync(userId, userType, count);

        if (!result.IsSuccess)
    return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}