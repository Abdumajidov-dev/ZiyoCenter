namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// To'lov usullari
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Naqd pul - kutubxonada to'lov
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Bank kartasi - online to'lov
    /// </summary>
    Card = 2,

    /// <summary>
    /// Cashback - yig'ilgan cashback'dan to'lov
    /// </summary>
    Cashback = 3,

    /// <summary>
    /// Aralash - bir necha usul bilan
    /// </summary>
    Mixed = 4
}