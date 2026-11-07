using ZiyoMarket.Domain.Common;
using ZiyoMarket.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Api.Controllers;
using ZiyoMarket.Service.DTOs.Payment;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Payment;

[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController : BaseController
{
    private readonly IPaymentCardService _paymentCardService;

    public PaymentController(IPaymentCardService paymentCardService)
    {
        _paymentCardService = paymentCardService;
    }

    /// <summary>
    /// Barcha to'lov kartalarini olish (Admin)
    /// </summary>
    // Ilgari faqat [Authorize] edi: istalgan mijoz o'z kartasini qo'shib, uni do'konning
    // faol to'lov kartasi qilib qo'ya olardi — barcha mijozlar to'lovi o'sha kartaga ketardi
    [Authorize(Roles = "Admin")]
    [HttpGet("cards")]
    [RequirePermission(AdminPermissions.FinancePaymentCards)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _paymentCardService.GetAllAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Aktiv kartani olish — /info endpointi bilan bir xil (barcha foydalanuvchilar)
    /// </summary>
    [HttpGet("info")]
    [HttpGet("cards/active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _paymentCardService.GetActiveAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Yangi karta qo'shish (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("cards")]
    [RequirePermission(AdminPermissions.FinancePaymentCards)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCardDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _paymentCardService.CreateAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return StatusCode(201, new { success = true, data = result.Data });
    }

    /// <summary>
    /// Kartani aktiv qilish (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("cards/{id}/set-active")]
    [RequirePermission(AdminPermissions.FinancePaymentCards)]
    public async Task<IActionResult> SetActive(int id)
    {
        var result = await _paymentCardService.SetActiveAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Kartani o'chirish (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("cards/{id}")]
    [RequirePermission(AdminPermissions.FinancePaymentCards)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _paymentCardService.DeleteAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }
}
