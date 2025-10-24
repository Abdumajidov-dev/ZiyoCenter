using ZiyoMarket.Service.DTOs.Reports;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IReportService
{
    // Umumiy statistika
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(DateTime startDate, DateTime endDate);
    Task<Result<List<ChartDataDto>>> GetSalesChartDataAsync(DateTime startDate, DateTime endDate, string groupBy = "day");
    
    // Savdo hisobotlari
    Task<Result<SalesReportDto>> GetSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<Result<ProductSalesReportDto>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<Result<CategorySalesReportDto>> GetCategorySalesReportAsync(DateTime startDate, DateTime endDate);
    
    // Top hisobotlar
    Task<Result<List<TopProductDto>>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<Result<List<TopCategoryDto>>> GetTopCategoriesAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<Result<List<TopCustomerDto>>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<Result<List<TopSellerDto>>> GetTopSellersAsync(DateTime startDate, DateTime endDate, int count = 10);
    
    // Zaxira va inventar
    Task<Result<InventoryReportDto>> GetInventoryReportAsync();
    Task<Result<List<LowStockProductDto>>> GetLowStockProductsAsync(int threshold = 5);
    Task<Result<List<ProductMovementDto>>> GetProductMovementsAsync(int productId, DateTime startDate, DateTime endDate);
    
    // Mijozlar va sotuvchilar
    Task<Result<CustomerAnalyticsDto>> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate);
 Task<Result<SellerAnalyticsDto>> GetSellerAnalyticsAsync(DateTime startDate, DateTime endDate);
    
    // Cashback va chegirma
    Task<Result<CashbackAnalyticsDto>> GetCashbackAnalyticsAsync(DateTime startDate, DateTime endDate);
  Task<Result<DiscountAnalyticsDto>> GetDiscountAnalyticsAsync(DateTime startDate, DateTime endDate);
    
    // Oylik/Yillik hisobotlar
    Task<Result<MonthlyReportDto>> GetMonthlyReportAsync(int year, int month);
    Task<Result<YearlyReportDto>> GetYearlyReportAsync(int year);
    
    // Custom report
    Task<Result<List<CustomReportDto>>> GenerateCustomReportAsync(ReportFilterDto filter);
}