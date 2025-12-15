using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Authentication and Authorization Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login for all user types (Customer, Seller, Admin)
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Access token and user profile</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Register a new customer
    /// </summary>
    /// <param name="request">Customer registration data</param>
    /// <returns>Access token and user profile</returns>
    [HttpPost("register/customer")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto request)
    {
        var result = await _authService.RegisterCustomerAsync(request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Register a new seller
    /// </summary>
    /// <param name="request">Seller registration data</param>
    /// <returns>Access token and user profile</returns>
    [HttpPost("register/seller")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterSeller([FromBody] RegisterSellerDto request)
    {
        var result = await _authService.RegisterSellerAsync(request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Register a new admin
    /// </summary>
    /// <param name="request">Admin registration data</param>
    /// <returns>Access token and user profile</returns>
    [HttpPost("register/admin")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto request)
    {
        var result = await _authService.RegisterAdminAsync(request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();

        var result = await _authService.LogoutAsync(userId, userType);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Change current user password
    /// </summary>
    /// <param name="request">Password change data</param>
    /// <returns>Success message</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();

        var result = await _authService.ChangePasswordAsync(userId, userType, request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Request password reset (send verification code)
    /// </summary>
    /// <param name="request">Phone/Email and UserType</param>
    /// <returns>Success message</returns>
    [HttpPost("password-reset/request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.RequestPasswordResetAsync(request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Reset password with verification code
    /// </summary>
    /// <param name="request">Verification code and new password</param>
    /// <returns>Success message</returns>
    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Send verification code to phone/email
    /// </summary>
    /// <param name="request">Phone or email</param>
    /// <returns>Success message</returns>
    [HttpPost("verification/send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendVerificationCode([FromBody] VerificationCodeRequestDto request)
    {
        var result = await _authService.SendVerificationCodeAsync(request.PhoneOrEmail);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Verify code
    /// </summary>
    /// <param name="request">Phone/Email and verification code</param>
    /// <returns>Success message</returns>
    [HttpPost("verification/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto request)
    {
        var result = await _authService.VerifyCodeAsync(request.PhoneOrEmail, request.Code);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile</returns>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetCurrentProfile()
    {
        var userId = GetCurrentUserId();
        var userType = GetCurrentUserType();

        var result = await _authService.GetCurrentUserProfileAsync(userId, userType);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Validate token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        var result = await _authService.ValidateTokenAsync(token);

        return Ok(new
        {
            success = true,
            isValid = result.Data
        });
    }
}
