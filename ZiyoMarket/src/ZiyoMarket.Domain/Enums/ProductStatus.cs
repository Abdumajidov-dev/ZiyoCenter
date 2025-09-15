namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Mahsulot holatlari
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// Faol - sotuvda mavjud
    /// </summary>
    Active = 1,

    /// <summary>
    /// Nofaol - vaqtincha to'xtatilgan
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Tugab qolgan - zaxirada yo'q
    /// </summary>
    OutOfStock = 3,

    /// <summary>
    /// O'chirilgan
    /// </summary>
    Deleted = 4
}