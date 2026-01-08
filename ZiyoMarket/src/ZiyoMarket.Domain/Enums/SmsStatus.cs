namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// SMS yuborilish holati
/// </summary>
public enum SmsStatus
{
    /// <summary>
    /// Navbatda (hali yuborilmagan)
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Muvaffaqiyatli yuborildi
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Yetib bordi
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Yuborishda xatolik
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Yetib bormadi
    /// </summary>
    NotDelivered = 5
}
