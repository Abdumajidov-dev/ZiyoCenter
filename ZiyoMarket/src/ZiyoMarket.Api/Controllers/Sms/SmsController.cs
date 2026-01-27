using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Sms;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Sms;

/// <summary>
/// SMS controller - Eskiz.uz orqali SMS yuborish
/// </summary>
[ApiController]
[Route("api/sms")]
public class SmsController : BaseController
{
    private readonly ISmsService _smsService;

    public SmsController(ISmsService smsService)
    {
        _smsService = smsService;
    }

    /// <summary>
    /// SMS yuborish (Admin)
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendSms([FromBody] SendSmsDto dto)
    {
        var result = await _smsService.SendSmsAsync(dto);
        return HandleResult(result);
    }

    /// <summary>
    /// Verification code yuborish (Public - ro'yxatdan o'tish uchun)
    /// </summary>
    [HttpPost("send-verification-code")]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeDto dto)
    {
        var result = await _smsService.SendVerificationCodeAsync(dto);
        return HandleResult(result);
    }

    /// <summary>
    /// Verification code tekshirish (Public)
    /// </summary>
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifySmsCodeDto dto)
    {
        var result = await _smsService.VerifyCodeAsync(dto);
        return HandleResult(result);
    }

    /// <summary>
    /// Ko'plab SMS yuborish (Admin)
    /// </summary>
    [HttpPost("send-bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBulkSms([FromBody] List<SendSmsDto> requests)
    {
        var result = await _smsService.SendBulkSmsAsync(requests);
        return HandleResult(result);
    }

    /// <summary>
    /// SMS loglarni olish (Admin)
    /// </summary>
    [HttpGet("logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSmsLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] SmsPurpose? purpose = null,
        [FromQuery] SmsStatus? status = null)
    {
        var result = await _smsService.GetSmsLogsAsync(pageNumber, pageSize, purpose, status);
        return HandleResult(result);
    }

    /// <summary>
    /// Foydalanuvchi SMS loglarini olish (Authenticated users)
    /// </summary>
    [HttpGet("my-logs")]
    [Authorize]
    public async Task<IActionResult> GetMyLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();

        var result = await _smsService.GetUserSmsLogsAsync(userId, userType, pageNumber, pageSize);
        return HandleResult(result);
    }

    /// <summary>
    /// SMS statistikasini olish (Admin)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _smsService.GetSmsStatisticsAsync(startDate, endDate);
        return HandleResult(result);
    }
}
