namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// SMS yuborish maqsadi
/// </summary>
public enum SmsPurpose
{
    /// <summary>
    /// Ro'yxatdan o'tish tasdiqlash kodi
    /// </summary>
    Registration = 1,

    /// <summary>
    /// Parolni tiklash tasdiqlash kodi
    /// </summary>
    PasswordReset = 2,

    /// <summary>
    /// Login tasdiqlash (2FA)
    /// </summary>
    LoginVerification = 3,

    /// <summary>
    /// Buyurtma tasdiqlash
    /// </summary>
    OrderConfirmation = 4,

    /// <summary>
    /// Buyurtma holati o'zgarishi
    /// </summary>
    OrderStatusChange = 5,

    /// <summary>
    /// Yetkazib berish xabari
    /// </summary>
    DeliveryNotification = 6,

    /// <summary>
    /// Marketing xabari
    /// </summary>
    Marketing = 7,

    /// <summary>
    /// Boshqa maqsad
    /// </summary>
    Other = 99
}
