namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Hisobot turlari
/// </summary>
public enum ReportType
{
    /// <summary>
    /// Kunlik sotuvlar hisoboti
    /// </summary>
    DailySales = 1,

    /// <summary>
    /// Haftalik sotuvlar hisoboti
    /// </summary>
    WeeklySales = 2,

    /// <summary>
    /// Oylik sotuvlar hisoboti
    /// </summary>
    MonthlySales = 3,

    /// <summary>
    /// Eng ko'p sotilgan mahsulotlar
    /// </summary>
    TopSellingProducts = 4,

    /// <summary>
    /// Faol mijozlar hisoboti
    /// </summary>
    ActiveCustomers = 5,

    /// <summary>
    /// Chegirma hisoboti
    /// </summary>
    DiscountUsage = 6,

    /// <summary>
    /// Cashback hisoboti
    /// </summary>
    CashbackReport = 7,

    /// <summary>
    /// Zaxira hisoboti (stock)
    /// </summary>
    InventoryReport = 8,

    /// <summary>
    /// Sotuvchi samaradorligi
    /// </summary>
    SellerPerformance = 9,

    /// <summary>
    /// Kategoriya bo'yicha sotuvlar
    /// </summary>
    CategorySales = 10,

    /// <summary>
    /// Yetkazib berish hisoboti
    /// </summary>
    DeliveryReport = 11,

    /// <summary>
    /// Daromad hisoboti
    /// </summary>
    RevenueReport = 12
}
