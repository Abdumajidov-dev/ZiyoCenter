namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Buyurtma holatlari
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Kutilmoqda - yangi yaratilgan buyurtma
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Tasdiqlandi - to'lov qabul qilindi
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// Tayyorlanmoqda - mahsulotlar yig'ilmoqda
    /// </summary>
    Preparing = 3,

    /// <summary>
    /// Olib ketishga tayyor - kutubxonada tayyor
    /// </summary>
    ReadyForPickup = 4,

    /// <summary>
    /// Jo'natildi - yetkazib berish xizmatiga berildi
    /// </summary>
    Shipped = 5,

    /// <summary>
    /// Yetkazildi - mijozga yetkazildi
    /// </summary>
    Delivered = 6,

    /// <summary>
    /// Bekor qilindi
    /// </summary>
    Cancelled = 7
}