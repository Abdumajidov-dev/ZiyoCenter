namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Yetkazib berish turlari
/// </summary>
public enum DeliveryType
{
    /// <summary>
    /// Olib ketish - kutubxonadan olish
    /// </summary>
    Pickup = 1,

    /// <summary>
    /// Pochta orqali yetkazish
    /// </summary>
    Postal = 2,

    /// <summary>
    /// Kuryer orqali yetkazish
    /// </summary>
    Courier = 3
}