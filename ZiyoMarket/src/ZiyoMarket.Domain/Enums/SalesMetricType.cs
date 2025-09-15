namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Sotuv metrikalari turlari
/// </summary>
public enum SalesMetricType
{
    /// <summary>
    /// Jami sotuvlar soni
    /// </summary>
    TotalSales = 1,

    /// <summary>
    /// Jami daromad
    /// </summary>
    TotalRevenue = 2,

    /// <summary>
    /// O'rtacha buyurtma qiymati
    /// </summary>
    AverageOrderValue = 3,

    /// <summary>
    /// Buyurtmalar soni
    /// </summary>
    OrderCount = 4,

    /// <summary>
    /// Sotilgan mahsulotlar soni
    /// </summary>
    ItemsSold = 5,

    /// <summary>
    /// Bekor qilingan buyurtmalar
    /// </summary>
    CancelledOrders = 6,

    /// <summary>
    /// Cashback berilgan
    /// </summary>
    CashbackGiven = 7,

    /// <summary>
    /// Chegirma berilgan
    /// </summary>
    DiscountGiven = 8,

    /// <summary>
    /// Yangi mijozlar
    /// </summary>
    NewCustomers = 9,

    /// <summary>
    /// Qaytgan mijozlar
    /// </summary>
    ReturningCustomers = 10
}