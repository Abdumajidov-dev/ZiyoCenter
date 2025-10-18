using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Cashback;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CashbackController : BaseController
{
    private readonly ICashbackService _cashbackService;

    public CashbackController(ICashbackService cashbackService)
    {
        _cashbackService = cashbackService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetCashbackSummary()
    {
        var customerId = GetCurrentUserId();
        var result = await _cashbackService.GetCashbackSummaryAsync(customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetCashbackHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var customerId = GetCurrentUserId();
        var result = await _cashbackService.GetCashbackHistoryAsync(customerId, pageNumber, pageSize);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableCashback()
    {
        var customerId = GetCurrentUserId();
        var result = await _cashbackService.GetAvailableCashbackAsync(customerId);
        return Ok(new { success = true, amount = result.Data });
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringCashback([FromQuery] int daysThreshold = 7)
    {
        var customerId = GetCurrentUserId();
        var result = await _cashbackService.GetExpiringCashbackAsync(customerId, daysThreshold);
        return Ok(new { success = true, amount = result.Data });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedMockCashback([FromQuery] int count = 10)
    {
        var customerId = GetCurrentUserId();
        var result = await _cashbackService.SeedMockCashbackAsync(customerId, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}