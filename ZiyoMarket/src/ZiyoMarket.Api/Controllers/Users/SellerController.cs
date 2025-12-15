using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Sellers;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SellerController : BaseController
{
    private readonly ISellerService _sellerService;

    public SellerController(ISellerService sellerService)
    {
        _sellerService = sellerService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSellerById(int id)
    {
        var result = await _sellerService.GetSellerByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> GetSellers([FromQuery] SellerFilterRequest request)
    {
        var result = await _sellerService.GetSellersAsync(request);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSeller([FromBody] CreateSellerDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _sellerService.CreateSellerAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSeller(int id, [FromBody] UpdateSellerDto request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _sellerService.UpdateSellerAsync(id, request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSeller(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _sellerService.DeleteSellerAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("{id}/performance")]
    public async Task<IActionResult> GetSellerPerformance(int id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _sellerService.GetSellerPerformanceAsync(id, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("top")]
    public async Task<IActionResult> GetTopSellers([FromQuery] int count = 10)
    {
        var result = await _sellerService.GetTopSellersAsync(count);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _sellerService.ToggleSellerStatusAsync(id, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPut("{id}/change-role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _sellerService.ChangeSellerRoleAsync(id, request.NewRole, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpDelete("bulk")]
    public async Task<IActionResult> DeleteAllSellers([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _sellerService.DeleteAllSellersAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedMockSellers([FromQuery] int count = 10)
    {
        var createdBy = GetCurrentUserId();
        var result = await _sellerService.SeedMockSellersAsync(createdBy, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}
