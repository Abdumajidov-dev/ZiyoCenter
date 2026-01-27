using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Sms;
using ZiyoMarket.Service.Helpers;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// SMS xizmati - Eskiz.uz orqali SMS yuborish
/// </summary>
public class SmsService : ISmsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly EskizSmsClient _eskizClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;

    public SmsService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        EskizSmsClient eskizClient,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<SmsService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eskizClient = eskizClient;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// SMS yuborish
    /// </summary>
    public async Task<Result<SmsResultDto>> SendSmsAsync(SendSmsDto request)
    {
        try
        {
            request.PhoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            // SMS log yaratish
            var smsLog = new SmsLog
            {
                PhoneNumber = request.PhoneNumber,
                Message = request.Message,
                Purpose = request.Purpose,
                Status = SmsStatus.Pending,
                Provider = "Eskiz.uz",
                UserId = request.UserId,
                UserType = request.UserType
            };

            await _unitOfWork.SmsLogs.InsertAsync(smsLog);
            await _unitOfWork.SaveChangesAsync();

            // Eskiz.uz orqali SMS yuborish
            var response = await _eskizClient.SendSmsAsync(request.PhoneNumber, request.Message);

            // Log holatini yangilash
            smsLog.Status = response.Success ? SmsStatus.Sent : SmsStatus.Failed;
            smsLog.ProviderMessageId = response.MessageId;
            smsLog.ErrorMessage = response.Success ? null : response.Message;
            smsLog.SentAt = response.Success ? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") : null;

            _unitOfWork.SmsLogs.Update(smsLog, smsLog.Id);
            await _unitOfWork.SaveChangesAsync();

            var result = new SmsResultDto
            {
                Success = response.Success,
                Message = response.Message,
                MessageId = response.MessageId,
                SmsLogId = smsLog.Id
            };

            if (response.Success)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}, MessageId: {MessageId}",
                    request.PhoneNumber, response.MessageId);
                return Result<SmsResultDto>.Success(result);
            }
            else
            {
                _logger.LogWarning("SMS send failed to {PhoneNumber}: {Error}",
                    request.PhoneNumber, response.Message);
                return Result<SmsResultDto>.Failure(response.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", request.PhoneNumber);
            return Result<SmsResultDto>.Failure($"SMS yuborishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Verification code yuborish (6 raqamli)
    /// </summary>
    public async Task<Result<VerificationResultDto>> SendVerificationCodeAsync(SendVerificationCodeDto request)
    {
        try
        {
            request.PhoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            // Check if phone is privileged (no SMS sent, always code "1111")
            var privilegedPhones = _configuration.GetSection("EskizSms:PrivilegedPhones").GetChildren().Select(x => x.Value).ToList() ?? new List<string>();
            var isPrivileged = privilegedPhones.Any(p => request.PhoneNumber.EndsWith(p) || request.PhoneNumber.Contains(p));

            string code;
            if (isPrivileged)
            {
                // Privileged raqamlar uchun har doim 1111
                code = "1111";
                _logger.LogInformation("Privileged phone {PhoneNumber} detected, using code 1111", request.PhoneNumber);
            }
            else
            {
                // 4 raqamli tasdiqlash kodi yaratish
                code = new Random().Next(1000, 9999).ToString();
            }

            // Purpose uchun default qiymat - Registration
            var purpose = SmsPurpose.Registration;

            // Kodni cache'ga saqlash (5 daqiqa)
            var cacheKey = $"sms_verification_{request.PhoneNumber}_{purpose}";
            var expiryTime = DateTime.UtcNow.AddMinutes(5);

            _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));

            // SMS matni - default Registration uchun
            string smsMessage = $"ZiyoMarket ro'yxatdan o'tish kodi: {code}. Kod 5 daqiqa amal qiladi.";

            // Privileged raqamlar uchun SMS yuborilmaydi
            if (!isPrivileged)
            {
                // SMS yuborish
                var smsRequest = new SendSmsDto
                {
                    PhoneNumber = request.PhoneNumber,
                    Message = smsMessage,
                    Purpose = purpose
                };

            var smsResult = await SendSmsAsync(smsRequest);

                if (!smsResult.IsSuccess)
            {
                return Result<VerificationResultDto>.Failure(smsResult.Message);
            }
            }

            var result = new VerificationResultDto
            {
                Code = code, // Always return code as requested
                ExpiresAt = expiryTime
            };

            var responseMessage = isPrivileged ? "Imtiyozli raqam, kod: 1111" : "OTP code sent successfully";

            _logger.LogInformation("Verification code sent to {PhoneNumber} for {Purpose}",
                request.PhoneNumber, purpose);

            return Result<VerificationResultDto>.Success(result, responseMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code to {PhoneNumber}", request.PhoneNumber);
            return Result<VerificationResultDto>.Failure($"Error sending OTP code: {ex.Message}");
        }
    }

    /// <summary>
    /// Verification code tekshirish
    /// </summary>
    public async Task<Result<bool>> VerifyCodeAsync(VerifySmsCodeDto request)
    {
        try
        {
            request.PhoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            // Purpose uchun default qiymat - Registration (SendVerificationCode bilan bir xil)
            var purpose = SmsPurpose.Registration;
            var cacheKey = $"sms_verification_{request.PhoneNumber}_{purpose}";

            if (!_cache.TryGetValue<string>(cacheKey, out var cachedCode))
            {
                _logger.LogWarning("Verification code expired or not found for {PhoneNumber}", request.PhoneNumber);
                return Result<bool>.Failure("Verification code expired or not found");
            }

            if (cachedCode != request.Code)
            {
                _logger.LogWarning("Invalid verification code for {PhoneNumber}", request.PhoneNumber);
                return Result<bool>.Failure("Invalid verification code");
            }

            // Kod to'g'ri bo'lsa, cache'dan o'chirish
            _cache.Remove(cacheKey);

            _logger.LogInformation("Verification code verified successfully for {PhoneNumber}", request.PhoneNumber);
            return Result<bool>.Success(true, "Verification code verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code for {PhoneNumber}", request.PhoneNumber);
            return Result<bool>.Failure($"Error verifying code: {ex.Message}");
        }
    }

    /// <summary>
    /// Ko'plab SMS yuborish
    /// </summary>
    public async Task<Result<List<SmsResultDto>>> SendBulkSmsAsync(List<SendSmsDto> requests)
    {
        try
        {
            var results = new List<SmsResultDto>();

            foreach (var request in requests)
            {
                var result = await SendSmsAsync(request);
                results.Add(result.Data ?? new SmsResultDto
                {
                    Success = false,
                    Message = result.Message
                });

                // Rate limiting - har bir SMS o'rtasida 100ms kutish
                await Task.Delay(100);
            }

            var successCount = results.Count(r => r.Success);
            var message = $"{successCount}/{requests.Count} ta SMS muvaffaqiyatli yuborildi";

            _logger.LogInformation("Bulk SMS sent: {SuccessCount}/{TotalCount}", successCount, requests.Count);

            return Result<List<SmsResultDto>>.Success(results, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk SMS");
            return Result<List<SmsResultDto>>.Failure($"Ko'plab SMS yuborishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// SMS loglarni olish (Admin)
    /// </summary>
    public async Task<Result<List<SmsLogDto>>> GetSmsLogsAsync(
        int pageNumber = 1,
        int pageSize = 50,
        SmsPurpose? purpose = null,
        SmsStatus? status = null)
    {
        try
        {
            var query = await _unitOfWork.SmsLogs.FindAsync(log => log.DeletedAt == null);

            if (purpose.HasValue)
            {
                query = query.Where(log => log.Purpose == purpose.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(log => log.Status == status.Value);
            }

            var totalCount = query.Count();
            var logs = query
                .OrderByDescending(log => log.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = _mapper.Map<List<SmsLogDto>>(logs);

            _logger.LogInformation("Retrieved {Count} SMS logs (page {PageNumber})", logs.Count, pageNumber);

            return Result<List<SmsLogDto>>.Success(result, $"Jami {totalCount} ta SMS log topildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS logs");
            return Result<List<SmsLogDto>>.Failure($"SMS loglarni olishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Foydalanuvchi SMS loglarini olish
    /// </summary>
    public async Task<Result<List<SmsLogDto>>> GetUserSmsLogsAsync(
        int userId,
        string userType,
        int pageNumber = 1,
        int pageSize = 20)
    {
        try
        {
            var logs = await _unitOfWork.SmsLogs.FindAsync(log =>
                log.DeletedAt == null &&
                log.UserId == userId &&
                log.UserType == userType);

            var totalCount = logs.Count();
            var pagedLogs = logs
                .OrderByDescending(log => log.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = _mapper.Map<List<SmsLogDto>>(pagedLogs);

            return Result<List<SmsLogDto>>.Success(result, $"Jami {totalCount} ta SMS log topildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user SMS logs for userId {UserId}", userId);
            return Result<List<SmsLogDto>>.Failure($"SMS loglarni olishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// SMS statistikasini olish (Admin)
    /// </summary>
    public async Task<Result<SmsStatisticsDto>> GetSmsStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = await _unitOfWork.SmsLogs.FindAsync(log => log.DeletedAt == null);

            if (startDate.HasValue)
            {
                var startDateStr = startDate.Value.ToString("yyyy-MM-dd");
                query = query.Where(log => string.Compare(log.CreatedAt, startDateStr) >= 0);
            }

            if (endDate.HasValue)
            {
                var endDateStr = endDate.Value.ToString("yyyy-MM-dd");
                query = query.Where(log => string.Compare(log.CreatedAt, endDateStr) <= 0);
            }

            var logs = query.ToList();

            var statistics = new SmsStatisticsDto
            {
                TotalSent = logs.Count,
                Delivered = logs.Count(l => l.Status == SmsStatus.Delivered),
                Failed = logs.Count(l => l.Status == SmsStatus.Failed),
                Pending = logs.Count(l => l.Status == SmsStatus.Pending),
                ByPurpose = logs.GroupBy(l => l.Purpose.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                ByStatus = logs.GroupBy(l => l.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            _logger.LogInformation("SMS statistics retrieved: {TotalSent} total", statistics.TotalSent);

            return Result<SmsStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS statistics");
            return Result<SmsStatisticsDto>.Failure($"SMS statistikasini olishda xatolik: {ex.Message}");
        }
    }


    private string NormalizePhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        // Remove all non-digit characters except +
        string cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // If starts with 998... but no +, add it
        if (cleaned.StartsWith("998") && cleaned.Length == 12)
        {
            cleaned = "+" + cleaned;
        }
        // If starts with 9... (9 digits) and no prefix, assume 998
        else if (!cleaned.StartsWith("+") && cleaned.Length == 9)
        {
            cleaned = "+998" + cleaned;
        }

        return cleaned;
    }
}
