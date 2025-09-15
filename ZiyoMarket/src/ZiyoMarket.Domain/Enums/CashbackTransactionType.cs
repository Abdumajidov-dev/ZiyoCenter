namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Cashback tranzaksiya turlari
/// </summary>
public enum CashbackTransactionType
{
    /// <summary>
    /// Yig'ildi - xarid qilganda cashback olindi
    /// </summary>
    Earned = 1,

    /// <summary>
    /// Ishlatildi - cashback to'lovda ishlatildi
    /// </summary>
    Used = 2,

    /// <summary>
    /// Muddati tugadi - 30 kun o'tgach avtomatik
    /// </summary>
    Expired = 3
}