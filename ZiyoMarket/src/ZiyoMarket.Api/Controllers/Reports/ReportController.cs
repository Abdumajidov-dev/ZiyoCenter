using ZiyoMarket.Domain.Common;
using ZiyoMarket.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Reports;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportController : BaseController
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetSalesReportAsync(startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("sales/daily")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime date)
    {
        var result = await _reportService.GetDailySalesAsync(date);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("sales/chart")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetSalesChartData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetSalesChartDataAsync(startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("products/top")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int count = 10)
    {
        var result = await _reportService.GetTopSellingProductsAsync(startDate, endDate, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("inventory")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetInventoryReport()
    {
        var result = await _reportService.GetInventoryReportAsync();

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("inventory/low-stock")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetLowStockReport()
    {
        var result = await _reportService.GetLowStockReportAsync();

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("customers/analytics")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetCustomerAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetCustomerAnalyticsAsync(startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("customers/top")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
    {
        var result = await _reportService.GetTopCustomersReportAsync(count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        var result = await _reportService.GetDashboardStatisticsAsync();

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("dashboard/today")]
    public async Task<IActionResult> GetTodayStatistics()
    {
        var result = await _reportService.GetTodayStatisticsAsync();

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("sellers/performance")]
    [RequirePermission(AdminPermissions.ReportsView)]
    public async Task<IActionResult> GetSellerPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetSellerPerformanceReportAsync(startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }
}