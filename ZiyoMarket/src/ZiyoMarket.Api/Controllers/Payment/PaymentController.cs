using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ZiyoMarket.Api.Settings;
using ZiyoMarket.Service.DTOs.Orders;

namespace ZiyoMarket.Api.Controllers.Payment;

/// <summary>
/// To'lov ma'lumotlari — karta raqami appsettings dan qaytariladi
/// </summary>
[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PaymentSettings _paymentSettings;

    public PaymentController(IOptions<PaymentSettings> paymentSettings)
    {
        _paymentSettings = paymentSettings.Value;
    }

    /// <summary>
    /// To'lov karta ma'lumotlarini olish (statik — appsettings dan)
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetPaymentInfo()
    {
        var info = new PaymentInfoDto
        {
            CardNumber    = _paymentSettings.CardNumber,
            CardHolder    = _paymentSettings.CardHolder,
            BankName      = _paymentSettings.BankName,
            Note          = _paymentSettings.Note
        };

        return Ok(new { success = true, data = info });
    }
}
