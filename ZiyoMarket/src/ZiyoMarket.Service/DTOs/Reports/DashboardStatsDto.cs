// ZiyoMarket.Service/DTOs/Reports/ReportDtos.cs

namespace ZiyoMarket.Service.DTOs.Reports;

// ============ DASHBOARD STATISTICS ============

/// <summary>
/// Dashboard asosiy statistika
/// </summary>
public class DashboardStatsDto
{
    // Revenue metrics
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal ThisWeekRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public decimal LastMonthRevenue { get; set; }
    public decimal RevenueGrowth { get; set; } // Percentage
    public string RevenueGrowthTrend { get; set; } = "Up"; // Up, Down, Stable

    // Order metrics
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public int ThisWeekOrders { get; set; }
    public int ThisMonthOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal OrderGrowth { get; set; } // Percentage

    // Customer metrics
    public int TotalCustomers { get; set; }
    public int NewCustomersToday { get; set; }
    public int NewCustomersThisWeek { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal CustomerGrowth { get; set; } // Percentage
    public decimal CustomerRetentionRate { get; set; }

    // Product metrics
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int ProductsSoldToday { get; set; }
    public int ProductsSoldThisMonth { get; set; }

    // Payment methods
    public decimal CashPayments { get; set; }
    public decimal CardPayments { get; set; }
    public decimal CashbackPayments { get; set; }
    public decimal MixedPayments { get; set; }

    // Delivery metrics
    public int PickupOrders { get; set; }
    public int DeliveryOrders { get; set; }
    public decimal TotalDeliveryFees { get; set; }

    // Cashback & Discount
    public decimal TotalCashbackEarned { get; set; }
    public decimal TotalCashbackUsed { get; set; }
    public decimal TotalDiscountGiven { get; set; }

    // Top performers
    public string? TopSellingProduct { get; set; }
    public int TopSellingProductCount { get; set; }
    public string? TopCategory { get; set; }
    public string? TopCustomer { get; set; }
    public string? TopSeller { get; set; }

    // Performance indicators
    public decimal ConversionRate { get; set; } // Orders / Customers
    public decimal AverageItemsPerOrder { get; set; }
    public string PerformanceStatus { get; set; } = "Good"; // Excellent, Good, Fair, Poor
}

/// <summary>
/// Chart data DTO - grafiklar uchun
/// </summary>
public class ChartDataDto
{
    public string Label { get; set; } = string.Empty; // Date, Category name, etc.
    public decimal Value { get; set; }
    public int Count { get; set; }
    public string? AdditionalInfo { get; set; }
}

// ============ SALES REPORTS ============

/// <summary>
/// Umumiy savdo hisoboti
/// </summary>
public class SalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }

    // Revenue breakdown
    public decimal TotalRevenue { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalCashbackUsed { get; set; }
    public decimal TotalDeliveryFees { get; set; }

    // Order statistics
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal OrderCompletionRate { get; set; } // Percentage
    public decimal OrderCancellationRate { get; set; }

    // Average metrics
    public decimal AverageOrderValue { get; set; }
    public decimal AverageDailyRevenue { get; set; }
    public decimal AverageDailyOrders { get; set; }
    public decimal AverageItemsPerOrder { get; set; }
    public decimal AverageDiscountPerOrder { get; set; }

    // Sales channels
    public int OnlineOrders { get; set; }
    public int OfflineOrders { get; set; }
    public decimal OnlineRevenue { get; set; }
    public decimal OfflineRevenue { get; set; }
    public decimal OnlinePercentage { get; set; }
    public decimal OfflinePercentage { get; set; }

    // Payment methods breakdown
    public decimal CashPayments { get; set; }
    public decimal CardPayments { get; set; }
    public decimal CashbackPayments { get; set; }
    public decimal MixedPayments { get; set; }

    // Delivery breakdown
    public int PickupOrders { get; set; }
    public int PostalOrders { get; set; }
    public int CourierOrders { get; set; }
    public decimal DeliveryRevenue { get; set; }

    // Growth metrics
    public decimal RevenueGrowth { get; set; } // Compared to previous period
    public decimal OrderGrowth { get; set; }
    public string PerformanceTrend { get; set; } = "Stable"; // Growing, Declining, Stable

    // Detailed breakdown
    public List<DailySalesDto> DailySales { get; set; } = new();
}

/// <summary>
/// Kunlik savdo statistikasi
/// </summary>
public class DailySalesDto
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public int CustomerCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ProductsSold { get; set; }
}

/// <summary>
/// Mahsulot savdo hisoboti
/// </summary>
public class ProductSalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int TotalProductsSold { get; set; }
    public int UniqueProductsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageProductPrice { get; set; }
    public decimal AverageQuantityPerProduct { get; set; }

    public List<ProductSalesDetailDto> ProductSales { get; set; } = new();
    public List<ChartDataDto> TopProducts { get; set; } = new();
}

/// <summary>
/// Mahsulot savdo tafsiloti
/// </summary>
public class ProductSalesDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalDiscount { get; set; }
    public int OrderCount { get; set; }
    public int StockRemaining { get; set; }
    public bool IsLowStock { get; set; }
}

/// <summary>
/// Kategoriya savdo hisoboti
/// </summary>
public class CategorySalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int TotalCategoriesWithSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public string? TopCategory { get; set; }
    public decimal TopCategoryRevenue { get; set; }

    public List<CategorySalesDetailDto> CategorySales { get; set; } = new();
    public List<ChartDataDto> CategoryChart { get; set; } = new();
}

/// <summary>
/// Kategoriya savdo tafsiloti
/// </summary>
public class CategorySalesDetailDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
    public int OrderCount { get; set; }
    public decimal RevenuePercentage { get; set; }
}

// ============ TOP REPORTS ============

/// <summary>
/// Top mahsulotlar
/// </summary>
public class TopProductDto
{
    public int Rank { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
    public int OrderCount { get; set; }
    public int StockRemaining { get; set; }
    public decimal GrowthRate { get; set; } // Compared to previous period
}

/// <summary>
/// Top kategoriyalar
/// </summary>
public class TopCategoryDto
{
    public int Rank { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal RevenuePercentage { get; set; }
    public decimal GrowthRate { get; set; }
}

/// <summary>
/// Top mijozlar
/// </summary>
public class TopCustomerDto
{
    public int Rank { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ProductsPurchased { get; set; }
    public decimal CashbackBalance { get; set; }
    public DateTime LastOrderDate { get; set; }
    public string CustomerType { get; set; } = "Regular"; // VIP, Regular, New
}

/// <summary>
/// Top sotuvchilar
/// </summary>
public class TopSellerDto
{
    public int Rank { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ProductsSold { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string PerformanceRating { get; set; } = "Good"; // Excellent, Good, Average, Poor
}

// ============ INVENTORY REPORTS ============

/// <summary>
/// Inventar hisoboti
/// </summary>
public class InventoryReportDto
{
    public DateTime ReportDate { get; set; }

    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }

    public int TotalStockQuantity { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public decimal AverageProductValue { get; set; }

    public List<CategoryInventoryDto> CategoryInventory { get; set; } = new();
    public List<LowStockProductDto> LowStockItems { get; set; } = new();
    public List<ProductStockDto> ProductStock { get; set; } = new();
}

/// <summary>
/// Kategoriya inventari
/// </summary>
public class CategoryInventoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}

/// <summary>
/// Kam qolgan mahsulotlar
/// </summary>
public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public int StockDeficit { get; set; }
    public decimal Price { get; set; }
    public int LastMonthSales { get; set; }
    public int EstimatedDaysRemaining { get; set; }
    public string UrgencyLevel { get; set; } = "Medium"; // Critical, High, Medium, Low
}

/// <summary>
/// Mahsulot zaxirasi
/// </summary>
public class ProductStockDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public decimal Price { get; set; }
    public decimal StockValue { get; set; }
    public string Status { get; set; } = "InStock"; // InStock, LowStock, OutOfStock
}

/// <summary>
/// Mahsulot harakati
/// </summary>
public class ProductMovementDto
{
    public DateTime Date { get; set; }
    public string MovementType { get; set; } = string.Empty; // Sale, Restock, Adjustment, Return
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public string? Reference { get; set; } // Order ID, Supplier, etc.
    public string? Reason { get; set; }
    public int? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }
}

// ============ CUSTOMER ANALYTICS ============

/// <summary>
/// Mijozlar tahlili
/// </summary>
public class CustomerAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Customer counts
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }

    // Behavior metrics
    public decimal AverageOrderValue { get; set; }
    public decimal AverageOrdersPerCustomer { get; set; }
    public decimal AverageItemsPerOrder { get; set; }
    public decimal CustomerLifetimeValue { get; set; }

    // Retention metrics
    public decimal RetentionRate { get; set; }
    public decimal ChurnRate { get; set; }
    public decimal RepeatCustomerRate { get; set; }

    // Cashback metrics
    public decimal TotalCashbackBalance { get; set; }
    public decimal AverageCashbackBalance { get; set; }
    public decimal CashbackRedemptionRate { get; set; }

    // Segmentation
    public int VIPCustomers { get; set; } // Top 10% spenders
    public int RegularCustomers { get; set; }
    public int OccasionalCustomers { get; set; }

    // Growth
    public decimal CustomerGrowthRate { get; set; }
    public int CustomersLost { get; set; }

    public List<CustomerSegmentDto> CustomerSegments { get; set; } = new();
    public List<ChartDataDto> CustomerGrowthChart { get; set; } = new();
}

/// <summary>
/// Mijozlar segmenti
/// </summary>
public class CustomerSegmentDto
{
    public string SegmentName { get; set; } = string.Empty; // VIP, Regular, Occasional, New
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal RevenuePercentage { get; set; }
}

// ============ SELLER ANALYTICS ============

/// <summary>
/// Sotuvchilar tahlili
/// </summary>
public class SellerAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int TotalSellers { get; set; }
    public int ActiveSellers { get; set; }

    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageRevenuePerSeller { get; set; }
    public decimal AverageOrdersPerSeller { get; set; }

    public decimal TotalDiscountGiven { get; set; }
    public decimal AverageDiscountPerOrder { get; set; }
    public decimal DiscountToRevenueRatio { get; set; }

    public string? TopSeller { get; set; }
    public decimal TopSellerRevenue { get; set; }

    public List<SellerPerformanceDto> SellerPerformance { get; set; } = new();
}

/// <summary>
/// Sotuvchi ishlashi
/// </summary>
public class SellerPerformanceDto
{
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ProductsSold { get; set; }

    public decimal TotalDiscountGiven { get; set; }
    public decimal AverageDiscount { get; set; }
    public int DiscountCount { get; set; }

    public decimal ConversionRate { get; set; }
    public string PerformanceRating { get; set; } = "Average";
    public int Rank { get; set; }
}

// ============ CASHBACK ANALYTICS ============

/// <summary>
/// Cashback tahlili
/// </summary>
public class CashbackAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Earned cashback
    public decimal TotalCashbackEarned { get; set; }
    public int TransactionsEarned { get; set; }
    public decimal AverageCashbackPerTransaction { get; set; }

    // Used cashback
    public decimal TotalCashbackUsed { get; set; }
    public int TransactionsUsed { get; set; }
    public decimal AverageCashbackUsedPerTransaction { get; set; }

    // Expired cashback
    public decimal TotalCashbackExpired { get; set; }
    public int TransactionsExpired { get; set; }

    // Current state
    public decimal TotalActiveCashback { get; set; }
    public decimal TotalExpiringSoon { get; set; } // Within 7 days

    // Metrics
    public decimal CashbackRedemptionRate { get; set; } // Used / Earned
    public decimal CashbackExpirationRate { get; set; } // Expired / Earned
    public decimal AverageCashbackBalance { get; set; }

    // Customer engagement
    public int CustomersWithCashback { get; set; }
    public int CustomersUsingCashback { get; set; }
    public decimal CustomerEngagementRate { get; set; }

    public List<ChartDataDto> CashbackTrend { get; set; } = new();
    public List<TopCashbackUserDto> TopUsers { get; set; } = new();
}

/// <summary>
/// Top cashback foydalanuvchilari
/// </summary>
public class TopCashbackUserDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalEarned { get; set; }
    public decimal TotalUsed { get; set; }
    public decimal CurrentBalance { get; set; }
    public int TransactionCount { get; set; }
}

// ============ DISCOUNT ANALYTICS ============

/// <summary>
/// Chegirma tahlili
/// </summary>
public class DiscountAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalDiscountGiven { get; set; }
    public int OrdersWithDiscount { get; set; }
    public int TotalOrders { get; set; }
    public decimal DiscountPercentage { get; set; } // Of total revenue

    public decimal AverageDiscountPerOrder { get; set; }
    public decimal AverageDiscountAmount { get; set; }
    public decimal MaxDiscountGiven { get; set; }
    public decimal MinDiscountGiven { get; set; }

    public int DiscountsByManagers { get; set; }
    public int DiscountsBySellers { get; set; }
    public decimal ManagerDiscountTotal { get; set; }
    public decimal SellerDiscountTotal { get; set; }

    public List<DiscountReasonBreakdownDto> ReasonBreakdown { get; set; } = new();
    public List<DiscountBySellerDto> SellerDiscounts { get; set; } = new();
    public List<ChartDataDto> DiscountTrend { get; set; } = new();
}

/// <summary>
/// Chegirma sababi bo'yicha
/// </summary>
public class DiscountReasonBreakdownDto
{
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Sotuvchi bo'yicha chegirmalar
/// </summary>
public class DiscountBySellerDto
{
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int DiscountCount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal AverageDiscount { get; set; }
    public decimal MaxAllowedPercentage { get; set; }
}

// ============ MONTHLY/YEARLY REPORTS ============

/// <summary>
/// Oylik hisobot
/// </summary>
public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;

    // Revenue
    public decimal TotalRevenue { get; set; }
    public decimal GrowthFromLastMonth { get; set; }
    public decimal AverageDailyRevenue { get; set; }

    // Orders
    public int TotalOrders { get; set; }
    public int OrderGrowth { get; set; }
    public decimal AverageOrderValue { get; set; }

    // Customers
    public int NewCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal CustomerGrowth { get; set; }

    // Products
    public int ProductsSold { get; set; }
    public int UniqueProductsSold { get; set; }
    public string? TopProduct { get; set; }

    // Performance
    public string PerformanceRating { get; set; } = "Average";
    public List<string> KeyHighlights { get; set; } = new();
    public List<string> AreasForImprovement { get; set; } = new();

    public List<DailySalesDto> DailyBreakdown { get; set; } = new();
    public List<WeeklySummaryDto> WeeklyBreakdown { get; set; } = new();
}

/// <summary>
/// Haftalik xulosa
/// </summary>
public class WeeklySummaryDto
{
    public int WeekNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int Customers { get; set; }
}

/// <summary>
/// Yillik hisobot
/// </summary>
public class YearlyReportDto
{
    public int Year { get; set; }

    // Revenue
    public decimal TotalRevenue { get; set; }
    public decimal GrowthFromLastYear { get; set; }
    public decimal AverageMonthlyRevenue { get; set; }
    public string BestMonth { get; set; } = string.Empty;
    public decimal BestMonthRevenue { get; set; }

    // Orders
    public int TotalOrders { get; set; }
    public decimal AverageMonthlyOrders { get; set; }
    public decimal AverageOrderValue { get; set; }

    // Customers
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public decimal CustomerRetentionRate { get; set; }

    // Products
    public int TotalProductsSold { get; set; }
    public string? TopSellingProduct { get; set; }
    public string? TopCategory { get; set; }

    // Financial
    public decimal TotalCashbackGiven { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal TotalDeliveryRevenue { get; set; }

    // Performance
    public string OverallPerformance { get; set; } = "Good";
    public List<string> YearHighlights { get; set; } = new();
    public List<string> Goals { get; set; } = new();

    public List<MonthlyPerformanceDto> MonthlyBreakdown { get; set; } = new();
    public List<QuarterlySummaryDto> QuarterlyBreakdown { get; set; } = new();
}

/// <summary>
/// Oylik ishlash
/// </summary>
public class MonthlyPerformanceDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int Customers { get; set; }
    public decimal AverageOrderValue { get; set; }
}

/// <summary>
/// Choraklik xulosa
/// </summary>
public class QuarterlySummaryDto
{
    public int Quarter { get; set; }
    public string QuarterName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int Customers { get; set; }
    public decimal GrowthRate { get; set; }
}

// ============ CUSTOM REPORTS ============

/// <summary>
/// Custom hisobot
/// </summary>
public class CustomReportDto
{
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Hisobot filtri
/// </summary>
public class ReportFilterDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ReportType { get; set; } // Sales, Products, Customers, etc.
    public string? GroupBy { get; set; } // Day, Week, Month, Category, etc.
    public int? CategoryId { get; set; }
    public int? ProductId { get; set; }
    public int? CustomerId { get; set; }
    public int? SellerId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? DeliveryType { get; set; }
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeComparison { get; set; } = true;
    public string? ExportFormat { get; set; } // PDF, Excel, CSV
}

// ============ ADDITIONAL HELPER DTOS ============

/// <summary>
/// Taqqoslash ma'lumoti
/// </summary>
public class ComparisonDto
{
    public string Period1Label { get; set; } = string.Empty;
    public string Period2Label { get; set; } = string.Empty;
    public decimal Period1Value { get; set; }
    public decimal Period2Value { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercentage { get; set; }
    public string Trend { get; set; } = "Stable"; // Up, Down, Stable
}

/// <summary>
/// Performance metrikasi
/// </summary>
public class PerformanceMetricDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal TargetValue { get; set; }
    public decimal Achievement { get; set; } // Percentage
    public string Status { get; set; } = "OnTrack"; // Exceeded, OnTrack, BehindTarget
}

/// <summary>
/// Trend ma'lumoti
/// </summary>
public class TrendDataDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal Change { get; set; }
    public string Direction { get; set; } = "Stable"; // Up, Down, Stable
}