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
    /// Universal register for all user types (Customer, Seller, Admin)
    /// </summary>
    /// <param name="request">User registration data with UserType</param>
    /// <returns>Access token and user profile</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var result = await _authService.RegisterUserAsync(request);

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
    /// Change user role (Admin only) - Move user from one role to another
    /// </summary>
    /// <param name="request">User ID, current type, and new type</param>
    /// <returns>New access token and updated user profile</returns>
    [HttpPost("change-role")]
    [Authorize]
    public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleDto request)
    {
        // Only admins can change roles
        var userType = GetCurrentUserType();
        if (userType != "Admin")
            return StatusCode(403, new { message = "Only admins can change user roles" });

        var adminUserId = GetCurrentUserId();
        var result = await _authService.ChangeUserRoleAsync(request, adminUserId);

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
    /// Create development admin (No authentication required - For development/testing only)
    /// </summary>
    /// <param name="request">Admin registration data</param>
    /// <returns>Access token and admin profile</returns>
    [HttpPost("dev/create-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateDevAdmin([FromBody] CreateDevAdminDto request)
    {
        var result = await _authService.CreateDevAdminAsync(request);

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
