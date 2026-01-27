using System.ComponentModel.DataAnnotations;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Service.DTOs.Sms;

/// <summary>
/// SMS yuborish uchun DTO
/// </summary>
public class SendSmsDto
{
    /// <summary>
    /// Telefon raqami (+998XXXXXXXXX)
    /// </summary>
    [Required(ErrorMessage = "Telefon raqami kiritilishi shart")]
    [Phone(ErrorMessage = "Telefon raqami formati noto'g'ri")]

    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// SMS matni
    /// </summary>
    [Required(ErrorMessage = "SMS matni kiritilishi shart")]
    [MaxLength(1000, ErrorMessage = "SMS matni 1000 belgidan oshmasligi kerak")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// SMS maqsadi
    /// </summary>
    public SmsPurpose Purpose { get; set; } = SmsPurpose.Other;

    /// <summary>
    /// Foydalanuvchi ID (ixtiyoriy)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Foydalanuvchi turi (ixtiyoriy)
    /// </summary>
    public string? UserType { get; set; }
}

/// <summary>
/// SMS verification code yuborish uchun
/// </summary>
public class SendVerificationCodeDto
{
    /// <summary>
    /// Telefon raqami (+998XXXXXXXXX)
    /// </summary>
    [Required(ErrorMessage = "Telefon raqami kiritilishi shart")]
    [Phone(ErrorMessage = "Telefon raqami formati noto'g'ri")]

    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Tasdiqlash kodi maqsadi (Registration, PasswordReset, LoginVerification)
    /// </summary>
    [Required]
    public SmsPurpose Purpose { get; set; }
}

/// <summary>
/// SMS tasdiqlash kodini tekshirish uchun
/// </summary>
public class VerifySmsCodeDto
{
    /// <summary>
    /// Telefon raqami
    /// </summary>
    [Required(ErrorMessage = "Telefon raqami kiritilishi shart")]
    [Phone(ErrorMessage = "Telefon raqami formati noto'g'ri")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Tasdiqlash kodi (6 raqam)
    /// </summary>
    [Required(ErrorMessage = "Tasdiqlash kodi kiritilishi shart")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Tasdiqlash kodi 6 ta raqamdan iborat bo'lishi kerak")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tasdiqlash kodi maqsadi
    /// </summary>
    [Required]
    public SmsPurpose Purpose { get; set; }
}

/// <summary>
/// SMS log natijasi
/// </summary>
public class SmsLogDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SentAt { get; set; }
    public int? UserId { get; set; }
    public string? UserType { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

/// <summary>
/// SMS yuborish natijasi
/// </summary>
public class SmsResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageId { get; set; }
    public int? SmsLogId { get; set; }
}

/// <summary>
/// Verification code natijasi
/// </summary>
public class VerificationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; } // Development muhitida kod qaytariladi
    public DateTime ExpiresAt { get; set; }
}
