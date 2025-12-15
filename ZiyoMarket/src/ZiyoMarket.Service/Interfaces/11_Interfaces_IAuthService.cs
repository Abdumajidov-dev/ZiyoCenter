using System.Threading.Tasks;
using ZiyoMarket.Service.DTOs.Auth;
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
    /// Register new customer (DEPRECATED - use RegisterUserAsync instead)
    /// </summary>
    [Obsolete("Use RegisterUserAsync instead")]
    Task<Result<LoginResponseDto>> RegisterCustomerAsync(RegisterCustomerDto request);

    /// <summary>
    /// Register new seller (DEPRECATED - use RegisterUserAsync instead)
    /// </summary>
    [Obsolete("Use RegisterUserAsync instead")]
    Task<Result<LoginResponseDto>> RegisterSellerAsync(RegisterSellerDto request);

    /// <summary>
    /// Register new admin (DEPRECATED - use RegisterUserAsync instead)
    /// </summary>
    [Obsolete("Use RegisterUserAsync instead")]
    Task<Result<LoginResponseDto>> RegisterAdminAsync(RegisterAdminDto request);

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
    /// Change user password
    /// </summary>
    Task<Result> ChangePasswordAsync(int userId, string userType, ChangePasswordDto request);
    
    /// <summary>
    /// Request password reset (send verification code)
    /// </summary>
    Task<Result> RequestPasswordResetAsync(ResetPasswordRequestDto request);
    
    /// <summary>
    /// Confirm password reset with verification code
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordConfirmDto request);
    
    /// <summary>
    /// Send verification code to phone or email
    /// </summary>
    Task<Result> SendVerificationCodeAsync(string phoneOrEmail);
    
    /// <summary>
    /// Verify code
    /// </summary>
    Task<Result> VerifyCodeAsync(string phoneOrEmail, string code);
    
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
}
