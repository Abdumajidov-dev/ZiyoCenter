using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Payments.Click;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Payment;

[ApiController]
[Route("api/click")]
public class ClickController : ControllerBase
{
    private readonly IClickService _clickService;

    public ClickController(IClickService clickService)
    {
        _clickService = clickService;
    }

    /// <summary>
    ///  Click Merchant API callback handler.
    ///  Click sends form-url-encoded data usually.
    /// </summary>
    [HttpPost("transaction")]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")] 
    public async Task<IActionResult> HandleTransaction([FromForm] ClickRequestDto request)
    {
        // Even if FromForm is used, sometimes Click might send JSON if configured so. 
        // But standard Shop API uses POST params. FromForm covers that.
        
        var response = await _clickService.HandleRequestAsync(request);
        return Ok(response);
    }
}
