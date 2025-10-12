using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.AuthControllers
{

    /// <summary>
    /// Authentication and Authorization endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login for Customer, Seller, or Admin
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT tokens and user profile</returns>
        /// <response code="200">Login successful</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="403">Account is inactive</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Message, errors = result.Errors });

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Register new customer account
        /// </summary>
        /// <param name="request">Customer registration details</param>
        /// <returns>JWT tokens and user profile</returns>
        /// <response code="201">Registration successful</response>
        /// <response code="409">Phone or email already exists</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterCustomerDto request)
        {
            var result = await _authService.RegisterCustomerAsync(request);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Message, errors = result.Errors });

            return StatusCode(StatusCodes.Status201Created, new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New JWT tokens</returns>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="401">Invalid refresh token</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

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
        /// Logout current user
        /// </summary>
        /// <response code="200">Logged out successfully</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            var userType = GetCurrentUserType();

            var result = await _authService.LogoutAsync(userId, userType);

            return Ok(new
            {
                success = result.IsSuccess,
                message = result.Message
            });
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        /// <param name="request">Current and new password</param>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Current password is incorrect</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// Request password reset (sends verification code)
        /// </summary>
        /// <param name="request">Phone or email</param>
        /// <response code="200">Verification code sent</response>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await _authService.RequestPasswordResetAsync(request);

            return Ok(new
            {
                success = result.IsSuccess,
                message = result.Message
            });
        }

        /// <summary>
        /// Reset password with verification code
        /// </summary>
        /// <param name="request">Verification code and new password</param>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid verification code</response>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// Send verification code to phone or email
        /// </summary>
        /// <param name="phoneOrEmail">Phone number or email address</param>
        /// <response code="200">Verification code sent</response>
        [HttpPost("send-verification-code")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SendVerificationCode([FromBody] string phoneOrEmail)
        {
            var result = await _authService.SendVerificationCodeAsync(phoneOrEmail);

            return Ok(new
            {
                success = result.IsSuccess,
                message = result.Message
            });
        }

        /// <summary>
        /// Verify code sent to phone or email
        /// </summary>
        /// <param name="phoneOrEmail">Phone number or email</param>
        /// <param name="code">6-digit verification code</param>
        /// <response code="200">Code verified successfully</response>
        /// <response code="400">Invalid verification code</response>
        [HttpPost("verify-code")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyCode([FromQuery] string phoneOrEmail, [FromQuery] string code)
        {
            var result = await _authService.VerifyCodeAsync(phoneOrEmail, code);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        /// <summary>
        /// Get current authenticated user profile
        /// </summary>
        /// <response code="200">User profile retrieved</response>
        /// <response code="404">User not found</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUserProfile()
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
        /// <param name="token">JWT token to validate</param>
        /// <response code="200">Token validation result</response>
        [HttpPost("validate-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
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
}
