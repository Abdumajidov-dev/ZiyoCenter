using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ZiyoMarket.Api.Controllers.Delivery;


[ApiController]
[Route("api/[controller]")]
public class DeliveryController : BaseController
{
    private readonly IDeliveryService _deliveryService;

    public DeliveryController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet("partners")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllDeliveryPartners()
    {
        var result = await _deliveryService.GetAllDeliveryPartnersAsync();
        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("partners/active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveDeliveryPartners()
    {
        var result = await _deliveryService.GetActiveDeliveryPartnersAsync();
        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("partners/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDeliveryPartnerById(int id)
    {
        var result = await _deliveryService.GetDeliveryPartnerByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost("partners")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDeliveryPartner([FromBody] SaveDeliveryPartnerDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _deliveryService.CreateDeliveryPartnerAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("partners/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDeliveryPartner(int id, [FromBody] SaveDeliveryPartnerDto request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _deliveryService.UpdateDeliveryPartnerAsync(id, request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpDelete("partners/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDeliveryPartner(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _deliveryService.DeleteDeliveryPartnerAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("orders/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderDelivery(int orderId)
    {
        var result = await _deliveryService.GetOrderDeliveryAsync(orderId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost("orders")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> CreateOrderDelivery([FromBody] CreateOrderDeliveryDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _deliveryService.CreateOrderDeliveryAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDeliveryStatus(int id, [FromBody] string status)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _deliveryService.UpdateDeliveryStatusAsync(id, status, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("track/{trackingCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackDelivery(string trackingCode)
    {
        var result = await _deliveryService.GetDeliveryByTrackingCodeAsync(trackingCode);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpDelete("partners/bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllDeliveryPartners([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _deliveryService.DeleteAllDeliveryPartnersAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("partners/seed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedMockDeliveryPartners([FromQuery] int count = 3)
    {
        var createdBy = GetCurrentUserId();
        var result = await _deliveryService.SeedMockDeliveryPartnersAsync(createdBy, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}
