namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Xabar turlari
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Yangi buyurtma yaratildi
    /// </summary>
    OrderCreated = 1,

    /// <summary>
    /// Buyurtma holati o'zgartirildi
    /// </summary>
    OrderStatusChanged = 2,

    /// <summary>
    /// Cashback yig'ildi
    /// </summary>
    CashbackEarned = 3,

    /// <summary>
    /// Mahsulot tugab qoldi (admin uchun)
    /// </summary>
    LowStock = 4,

    /// <summary>
    /// Yangi mijoz ro'yxatdan o'tdi
    /// </summary>
    NewCustomer = 5,

    /// <summary>
    /// Tizim xabari
    /// </summary>
    SystemMessage = 6,

    /// <summary>
    /// Aksiya/chegirma haqida
    /// </summary>
    Promotion = 7,
    SupportMessage = 8,
    NewOrder = 9
}