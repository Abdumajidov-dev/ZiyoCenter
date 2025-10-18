using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Orders;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();
        var result = await _orderService.GetOrderByIdAsync(id, userId, userType);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] OrderFilterRequest request)
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();
        var result = await _orderService.GetOrdersAsync(request, userId, userType);

        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
    {
        var customerId = GetCurrentUserId();
        var result = await _orderService.CreateOrderAsync(request, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPost("seller")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateOrderBySeller([FromBody] CreateOrderDto request)
    {
        var sellerId = GetCurrentUserId();
        var result = await _orderService.CreateOrderBySellerAsync(request, sellerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromBody] string? reason)
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();
        var result = await _orderService.CancelOrderAsync(id, userId, userType, reason);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _orderService.UpdateOrderStatusAsync(id, status, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> ConfirmOrder(int id)
    {
        var sellerId = GetCurrentUserId();
        var result = await _orderService.ConfirmOrderAsync(id, sellerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("discount")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountDto request)
    {
        var sellerId = GetCurrentUserId();
        var result = await _orderService.ApplyDiscountAsync(request, sellerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOrderSummary([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        var result = await _orderService.GetOrderSummaryAsync(dateFrom, dateTo);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllOrders([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _orderService.DeleteAllOrdersAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedMockOrders([FromQuery] int count = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var createdBy = GetCurrentUserId();
        var result = await _orderService.SeedMockOrdersAsync(createdBy, count, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}

