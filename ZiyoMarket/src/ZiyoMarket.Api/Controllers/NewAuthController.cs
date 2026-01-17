using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.Services;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Yangi unified authentication controller
/// </summary>
[ApiController]
[Route("api/v2/auth")]
public class NewAuthController : ControllerBase
{
    private readonly NewAuthService _authService;

    public NewAuthController(NewAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Yangi foydalanuvchi ro'yxatdan o'tish - HAMMA CUSTOMER BO'LIB BOSHLANADI
    /// </summary>
    /// <remarks>
    /// Barcha yangi foydalanuvchilar "Customer" role bilan ro'yxatdan o'tadilar.
    /// Keyinchalik admin panel orqali boshqa rolelar (Seller, Manager, Admin) beriladi.
    /// </remarks>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { success = false, message = "Barcha maydonlarni to'ldiring" });
        }

        var result = await _authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Password
        );

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                409 => Conflict(new { success = false, message = result.Message }),
                _ => BadRequest(new { success = false, message = result.Message })
            };
        }

        return CreatedAtAction(nameof(Register), new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Login - telefon raqam va parol bilan
    /// </summary>
    /// <remarks>
    /// Login qilganda JWT token'da barcha role va permissionlar keladi.
    /// Token'ni decode qilib, foydalanuvchining huquqlarini tekshirish mumkin.
    /// </remarks>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { success = false, message = "Telefon raqam va parolni kiriting" });
        }

        var result = await _authService.LoginAsync(request.Phone, request.Password);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                401 => Unauthorized(new { success = false, message = result.Message }),
                403 => StatusCode(403, new { success = false, message = result.Message }),
                _ => BadRequest(new { success = false, message = result.Message })
            };
        }

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }
}

// Request DTOs
public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
