namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Yetkazib berish holatlari
/// </summary>
public enum DeliveryStatus
{
    /// <summary>
    /// Tayinlangan - yetkazib berish xizmatiga berildi
    /// </summary>
    Assigned = 1,

    /// <summary>
    /// Olib ketildi - pochta tomonidan olib ketildi
    /// </summary>
    PickedUp = 2,

    /// <summary>
    /// Yo'lda - yetkazilmoqda
    /// </summary>
    InTransit = 3,

    /// <summary>
    /// Yetkazildi - mijozga yetkazildi
    /// </summary>
    Delivered = 4,

    /// <summary>
    /// Muvaffaqiyatsiz - yetkazib bo'lmadi
    /// </summary>
    Failed = 5
}