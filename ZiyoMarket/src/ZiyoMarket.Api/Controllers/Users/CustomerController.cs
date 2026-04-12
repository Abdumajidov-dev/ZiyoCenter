using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Customers;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CustomerController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Mijozlar ro'yxatini olish (filtr va pagination bilan)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerFilterRequest request)
    {
        var result = await _customerService.GetCustomersAsync(request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Bitta mijozni ID bo'yicha olish
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Mijoz buyurtmalarini olish
    /// </summary>
    [HttpGet("{id}/orders")]
    public async Task<IActionResult> GetCustomerOrders(int id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Yangi mijoz yaratish
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _customerService.CreateCustomerAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    /// <summary>
    /// Mijozni yangilash
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _customerService.UpdateCustomerAsync(id, request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    /// <summary>
    /// Mijozni o'chirish (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _customerService.DeleteCustomerAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Mijoz faolligini o'zgartirish
    /// </summary>
    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _customerService.ToggleCustomerStatusAsync(id, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Mijozlarni qidirish
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest(new { message = "Qidiruv matni bo'sh bo'lishi mumkin emas" });

        var result = await _customerService.SearchCustomersAsync(searchTerm);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }
}
