namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// To'lov holati (Manual Payment Verification uchun)
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Kutilmoqda - Mijoz hali to'lov qilmagan
    /// </summary>
    Pending = 1,

    /// <summary>
    /// To'lov isboti kutilmoqda - Mijoz to'lov qildi, isbotni yuklash kerak
    /// </summary>
    AwaitingProof = 2,

    /// <summary>
    /// To'lov isboti yuklandi, admin tasdiqlashi kutilmoqda
    /// </summary>
    UnderReview = 3,

    /// <summary>
    /// To'lov tasdiqlandi - Admin to'lovni tasdiqladi
    /// </summary>
    Verified = 4,

    /// <summary>
    /// To'lov rad etildi - Admin to'lovni qabul qilmadi
    /// </summary>
    Rejected = 5,

    /// <summary>
    /// To'lov qaytarildi (Refund)
    /// </summary>
    Refunded = 6
}
