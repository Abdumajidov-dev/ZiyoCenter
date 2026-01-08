using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Sms;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// SMS xizmati interface
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// SMS yuborish
    /// </summary>
    Task<Result<SmsResultDto>> SendSmsAsync(SendSmsDto request);

    /// <summary>
    /// Verification code yuborish (6 raqamli)
    /// </summary>
    Task<Result<VerificationResultDto>> SendVerificationCodeAsync(SendVerificationCodeDto request);

    /// <summary>
    /// Verification code tekshirish
    /// </summary>
    Task<Result<bool>> VerifyCodeAsync(VerifySmsCodeDto request);

    /// <summary>
    /// Ko'plab SMS yuborish
    /// </summary>
    Task<Result<List<SmsResultDto>>> SendBulkSmsAsync(List<SendSmsDto> requests);

    /// <summary>
    /// SMS loglarni olish (Admin)
    /// </summary>
    Task<Result<List<SmsLogDto>>> GetSmsLogsAsync(int pageNumber = 1, int pageSize = 50, SmsPurpose? purpose = null, SmsStatus? status = null);

    /// <summary>
    /// Foydalanuvchi SMS loglarini olish
    /// </summary>
    Task<Result<List<SmsLogDto>>> GetUserSmsLogsAsync(int userId, string userType, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// SMS statistikasini olish (Admin)
    /// </summary>
    Task<Result<SmsStatisticsDto>> GetSmsStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// SMS statistikasi
/// </summary>
public class SmsStatisticsDto
{
    public int TotalSent { get; set; }
    public int Delivered { get; set; }
    public int Failed { get; set; }
    public int Pending { get; set; }
    public Dictionary<string, int> ByPurpose { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
}
