using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Notifications;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Notifications;

[ApiController]
[Route("api/push-notification")]
public class PushNotificationController : BaseController
{
    private readonly IDeviceTokenService _deviceTokenService;

    public PushNotificationController(IDeviceTokenService deviceTokenService)
    {
        _deviceTokenService = deviceTokenService;
    }

    /// <summary>
    /// Register or update device FCM token (Called from mobile app on login/launch)
    /// </summary>
    [HttpPost("register-token")]
    [Authorize]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenDto dto)
    {
        var userId = GetCurrentUserId();
        var userType = Enum.Parse<UserType>(GetCurrentUserType());

        var result = await _deviceTokenService.RegisterDeviceTokenAsync(userId, userType, dto);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Get all active device tokens for current user
    /// </summary>
    [HttpGet("my-devices")]
    [Authorize]
    public async Task<IActionResult> GetMyDevices()
    {
        var userId = GetCurrentUserId();
        var userType = Enum.Parse<UserType>(GetCurrentUserType());

        var result = await _deviceTokenService.GetUserDeviceTokensAsync(userId, userType);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Deactivate a specific device token (logout from specific device)
    /// </summary>
    [HttpPost("deactivate-token")]
    [Authorize]
    public async Task<IActionResult> DeactivateToken([FromBody] string token)
    {
        var userId = GetCurrentUserId();
        var result = await _deviceTokenService.DeactivateDeviceTokenAsync(userId, token);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Deactivate all tokens for current user (logout from all devices)
    /// </summary>
    [HttpPost("logout-all-devices")]
    [Authorize]
    public async Task<IActionResult> LogoutAllDevices()
    {
        var userId = GetCurrentUserId();
        var userType = Enum.Parse<UserType>(GetCurrentUserType());

        var result = await _deviceTokenService.DeactivateAllUserTokensAsync(userId, userType);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Send push notification to a specific user (Admin only)
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendPushNotification([FromBody] SendPushNotificationDto dto)
    {
        var result = await _deviceTokenService.SendPushNotificationAsync(dto);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Send push notification to multiple users (Admin only)
    /// </summary>
    [HttpPost("send-batch")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBatchPushNotification([FromBody] SendBatchPushNotificationDto dto)
    {
        var result = await _deviceTokenService.SendBatchPushNotificationAsync(dto);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Send push notification to a topic (Admin only)
    /// </summary>
    [HttpPost("send-topic")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendTopicNotification([FromBody] SendTopicNotificationDto dto)
    {
        var result = await _deviceTokenService.SendTopicNotificationAsync(dto);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Cleanup expired tokens (older than 60 days) - Admin only
    /// </summary>
    [HttpPost("cleanup-expired")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupExpiredTokens()
    {
        var result = await _deviceTokenService.CleanupExpiredTokensAsync();

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
