namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Foydalanuvchi turlari
/// </summary>
public enum UserType
{
    /// <summary>
    /// Mijoz - mahsulot sotib oluvchi
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Sotuvchi - kutubxonada ishlaydi
    /// </summary>
    Seller = 2,

    /// <summary>
    /// Administrator - tizimni boshqaradi
    /// </summary>
    Admin = 3
}