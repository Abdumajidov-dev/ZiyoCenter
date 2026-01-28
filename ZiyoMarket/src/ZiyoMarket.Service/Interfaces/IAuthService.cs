using System.Threading.Tasks;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.DTOs.Sms;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Authentication and authorization service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login user (Customer, Seller, or Admin)
    /// </summary>
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    
    /// <summary>
    /// Universal register for all user types (Customer, Seller, Admin)
    /// </summary>
    Task<Result<LoginResponseDto>> RegisterUserAsync(RegisterUserDto request);



    /// <summary>
    /// Change user role (Admin only)
    /// </summary>
    Task<Result<LoginResponseDto>> ChangeUserRoleAsync(ChangeUserRoleDto request, int adminUserId);

    /// <summary>
    /// Create development admin without authentication (Development only)
    /// </summary>
    Task<Result<LoginResponseDto>> CreateDevAdminAsync(CreateDevAdminDto request);
    
    /// <summary>
    /// Logout user
    /// </summary>
    Task<Result> LogoutAsync(int userId, string userType);
    

    
    /// <summary>
    /// Request password reset (send verification code)
    /// </summary>
    Task<Result> RequestPasswordResetAsync(ResetPasswordRequestDto request);
    
    /// <summary>
    /// Confirm password reset with verification code
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordConfirmDto request);
    

    
    /// <summary>
    /// Generate JWT access token
    /// </summary>
    Task<Result<string>> GenerateAccessTokenAsync(int userId, string userType);

    /// <summary>
    /// Validate token
    /// </summary>
    Task<Result<bool>> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Get current user profile
    /// </summary>
    Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(int userId, string userType);

    /// <summary>
    /// Send OTP code to phone number (for registration/login verification)
    /// </summary>
    Task<Result<VerificationResultDto>> SendOtpAsync(SendVerificationCodeDto request);

    /// <summary>
    /// Verify OTP code
    /// </summary>
    Task<Result<string>> VerifyOtpAsync(VerifySmsCodeDto request);
}
