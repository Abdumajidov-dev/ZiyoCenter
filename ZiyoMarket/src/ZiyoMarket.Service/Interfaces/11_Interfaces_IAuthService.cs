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
    /// Register new customer
    /// </summary>
    Task<Result<LoginResponseDto>> RegisterCustomerAsync(RegisterCustomerDto request);

    /// <summary>
    /// Register new seller
    /// </summary>
    Task<Result<LoginResponseDto>> RegisterSellerAsync(RegisterSellerDto request);

    /// <summary>
    /// Register new admin
    /// </summary>
    Task<Result<LoginResponseDto>> RegisterAdminAsync(RegisterAdminDto request);
    
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
