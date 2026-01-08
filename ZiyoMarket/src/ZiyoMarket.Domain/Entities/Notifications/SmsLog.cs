using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Notifications;

/// <summary>
/// SMS yuborilgan loglar
/// </summary>
public class SmsLog : BaseEntity
{
    /// <summary>
    /// Telefon raqami (+998XXXXXXXXX)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// SMS matni
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// SMS maqsadi
    /// </summary>
    public SmsPurpose Purpose { get; set; }

    /// <summary>
    /// SMS yuborilish holati
    /// </summary>
    public SmsStatus Status { get; set; }

    /// <summary>
    /// Provider (Eskiz.uz)
    /// </summary>
    public string Provider { get; set; } = "Eskiz.uz";

    /// <summary>
    /// Provider message ID
    /// </summary>
    public string? ProviderMessageId { get; set; }

    /// <summary>
    /// Xatolik xabari (agar bo'lsa)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Yuborilgan vaqt
    /// </summary>
    public string? SentAt { get; set; }

    /// <summary>
    /// Foydalanuvchi ID (agar ma'lum bo'lsa)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Foydalanuvchi turi (Customer, Seller, Admin)
    /// </summary>
    public string? UserType { get; set; }
}
