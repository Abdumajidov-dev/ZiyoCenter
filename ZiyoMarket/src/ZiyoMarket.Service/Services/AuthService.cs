using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.Extensions;
using ZiyoMarket.Service.Helpers;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Authentication and authorization service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }

    // ============ Login ============

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            // Find user based on UserType
            object? user = request.UserType switch
            {
                "Customer" => await FindCustomerByPhoneOrEmailAsync(request.PhoneOrEmail),
                "Seller" => await FindSellerByPhoneOrEmailAsync(request.PhoneOrEmail),
                "Admin" => await FindAdminByUsernameOrEmailAsync(request.PhoneOrEmail),
                _ => null
            };

            if (user == null)
                return Result<LoginResponseDto>.Unauthorized("Invalid credentials");

            // Verify password
            string passwordHash = GetPasswordHash(user, request.UserType);
            if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
                return Result<LoginResponseDto>.Unauthorized("Invalid credentials");

            // Check if user is active
            if (!IsUserActive(user, request.UserType))
                return Result<LoginResponseDto>.Forbidden("Account is inactive");

            // Generate tokens
            var userId = GetUserId(user, request.UserType);
            var accessToken = await GenerateAccessTokenAsync(userId, request.UserType);
            var refreshToken = await GenerateRefreshTokenAsync(userId, request.UserType);

            // Map to response
            var userProfile = MapUserToProfileDto(user, request.UserType);

            var response = new LoginResponseDto
            {
                AccessToken = accessToken.Data!,
                RefreshToken = refreshToken.Data!,
                ExpiresAt = TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = userProfile
            };

            // Update last login for admin
            if (request.UserType == "Admin" && user is Admin admin)
            {
                admin.LastLoginAt = TimeHelper.GetCurrentServerTime();
                await _unitOfWork.Admins.Update(admin,admin.Id);
                await _unitOfWork.SaveChangesAsync();
            }

            return Result<LoginResponseDto>.Success(response, "Login successful");
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Login failed: {ex.Message}");
        }
    }

    // ============ Register Customer ============

    public async Task<Result<LoginResponseDto>> RegisterCustomerAsync(RegisterCustomerDto request)
    {
        try
        {
            // Check if phone already exists
            var existingCustomer = await _unitOfWork.Customers
                .SelectAsync(c => c.Phone == request.Phone && !c.IsDeleted);

            if (existingCustomer != null)
                return Result<LoginResponseDto>.Conflict("Phone number already registered");

            // Check if email exists (if provided)
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingEmail = await _unitOfWork.Customers
                    .SelectAsync(c => c.Email == request.Email && !c.IsDeleted);

                if (existingEmail != null)
                    return Result<LoginResponseDto>.Conflict("Email already registered");
            }

            // Create customer
            var customer = _mapper.Map<Customer>(request);
            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            customer.CashbackBalance = 0;
            customer.IsActive = true;

            await _unitOfWork.Customers.InsertAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            // Generate tokens
            var accessToken = await GenerateAccessTokenAsync(customer.Id, "Customer");
            var refreshToken = await GenerateRefreshTokenAsync(customer.Id, "Customer");

            var response = new LoginResponseDto
            {
                AccessToken = accessToken.Data!,
                RefreshToken = refreshToken.Data!,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = _mapper.Map<UserProfileDto>(customer)
            };

            return Result<LoginResponseDto>.Success(response, "Registration successful", 201);
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Registration failed: {ex.Message}");
        }
    }

    // ============ Refresh Token ============

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            // Validate refresh token
            var principal = GetPrincipalFromToken(request.RefreshToken);
            if (principal == null)
                return Result<LoginResponseDto>.Unauthorized("Invalid refresh token");

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userType = principal.FindFirst("UserType")?.Value ?? "";

            if (userId == 0 || string.IsNullOrEmpty(userType))
                return Result<LoginResponseDto>.Unauthorized("Invalid token claims");

            // Verify user still exists and is active
            var isActive = await VerifyUserExistsAndActive(userId, userType);
            if (!isActive)
                return Result<LoginResponseDto>.Forbidden("User account is inactive or deleted");

            // Generate new tokens
            var accessToken = await GenerateAccessTokenAsync(userId, userType);
            var refreshToken = await GenerateRefreshTokenAsync(userId, userType);

            // Get user profile
            var userProfile = await GetUserProfileAsync(userId, userType);

            var response = new LoginResponseDto
            {
                AccessToken = accessToken.Data!,
                RefreshToken = refreshToken.Data!,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = userProfile!
            };

            return Result<LoginResponseDto>.Success(response, "Token refreshed");
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Token refresh failed: {ex.Message}");
        }
    }

    // ============ Logout ============

    public async Task<Result> LogoutAsync(int userId, string userType)
    {
        try
        {
            // In stateless JWT, logout is handled client-side by removing token
            // Here we can log the logout action if needed

            await Task.CompletedTask;
            return Result.Success("Logged out successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Logout failed: {ex.Message}");
        }
    }

    // ============ Change Password ============

    public async Task<Result> ChangePasswordAsync(int userId, string userType, ChangePasswordDto request)
    {
        try
        {
            object? user = userType switch
            {
                "Customer" => await _unitOfWork.Customers.GetByIdAsync(userId),
                "Seller" => await _unitOfWork.Sellers.GetByIdAsync(userId),
                "Admin" => await _unitOfWork.Admins.GetByIdAsync(userId),
                _ => null
            };

            if (user == null)
                return Result.NotFound("User not found");

            // Verify current password
            var currentPasswordHash = GetPasswordHash(user, userType);
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, currentPasswordHash))
                return Result.BadRequest("Current password is incorrect");

            // Update password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            SetPasswordHash(user, userType, newPasswordHash);

            // Save changes
            switch (userType)
            {
                case "Customer":
                    _unitOfWork.Customers.Update((Customer)user, userId);
                    break;
                case "Seller":
                    _unitOfWork.Sellers.Update((Seller)user, userId);
                    break;
                case "Admin":
                    _unitOfWork.Admins.Update((Admin)user, userId);
                    break;
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Password changed successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Password change failed: {ex.Message}");
        }
    }

    // ============ Password Reset ============

    public async Task<Result> RequestPasswordResetAsync(ResetPasswordRequestDto request)
    {
        try
        {
            // Find user
            object? user = request.UserType switch
            {
                "Customer" => await FindCustomerByPhoneOrEmailAsync(request.PhoneOrEmail),
                "Seller" => await FindSellerByPhoneOrEmailAsync(request.PhoneOrEmail),
                "Admin" => await FindAdminByUsernameOrEmailAsync(request.PhoneOrEmail),
                _ => null
            };

            if (user == null)
            {
                // Don't reveal if user exists or not
                return Result.Success("If account exists, verification code has been sent");
            }

            // Generate and send verification code
            var code = GenerateVerificationCode();

            // TODO: Send code via SMS/Email
            // For now, just log it (in production, integrate with SMS/Email service)
            Console.WriteLine($"Verification code for {request.PhoneOrEmail}: {code}");

            // In production: Store code in cache/database with expiration

            return Result.Success("Verification code sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Password reset request failed: {ex.Message}");
        }
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordConfirmDto request)
    {
        try
        {
            // TODO: Verify code from cache/database
            // For now, accept any 6-digit code for demo

            // Find user
            object? user = null;
            string userType = "";

            // Try to find in all user types
            var customer = await FindCustomerByPhoneOrEmailAsync(request.PhoneOrEmail);
            if (customer != null)
            {
                user = customer;
                userType = "Customer";
            }
            else
            {
                var seller = await FindSellerByPhoneOrEmailAsync(request.PhoneOrEmail);
                if (seller != null)
                {
                    user = seller;
                    userType = "Seller";
                }
                else
                {
                    var admin = await FindAdminByUsernameOrEmailAsync(request.PhoneOrEmail);
                    if (admin != null)
                    {
                        user = admin;
                        userType = "Admin";
                    }
                }
            }

            if (user == null)
                return Result.NotFound("User not found");

            // Update password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            SetPasswordHash(user, userType, newPasswordHash);

            // Save changes
            switch (userType)
            {
                case "Customer":
                    await _unitOfWork.Customers.UpdateAsync((Customer)user);
                    break;
                case "Seller":
                    await _unitOfWork.Sellers.UpdateAsync((Seller)user);
                    break;
                case "Admin":
                    await _unitOfWork.Admins.UpdateAsync((Admin)user);
                    break;
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Password reset successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Password reset failed: {ex.Message}");
        }
    }

    // ============ Verification ============

    public async Task<Result> SendVerificationCodeAsync(string phoneOrEmail)
    {
        try
        {
            var code = GenerateVerificationCode();

            // TODO: Send via SMS/Email
            Console.WriteLine($"Verification code for {phoneOrEmail}: {code}");

            await Task.CompletedTask;
            return Result.Success("Verification code sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Failed to send verification code: {ex.Message}");
        }
    }

    public async Task<Result> VerifyCodeAsync(string phoneOrEmail, string code)
    {
        try
        {
            // TODO: Verify code from cache/database

            await Task.CompletedTask;

            if (code.Length != 6)
                return Result.BadRequest("Invalid verification code");

            return Result.Success("Code verified");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Verification failed: {ex.Message}");
        }
    }

    // ============ Token Generation ============

    public async Task<Result<string>> GenerateAccessTokenAsync(int userId, string userType)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("UserType", userType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            await Task.CompletedTask;
            return Result<string>.Success(tokenString);
        }
        catch (Exception ex)
        {
            return Result<string>.InternalError($"Token generation failed: {ex.Message}");
        }
    }

    public async Task<Result<string>> GenerateRefreshTokenAsync(int userId, string userType)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("UserType", userType),
                new Claim("TokenType", "Refresh")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            await Task.CompletedTask;
            return Result<string>.Success(tokenString);
        }
        catch (Exception ex)
        {
            return Result<string>.InternalError($"Refresh token generation failed: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = GetPrincipalFromToken(token);
            await Task.CompletedTask;
            return Result<bool>.Success(principal != null);
        }
        catch
        {
            return Result<bool>.Success(false);
        }
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(int userId, string userType)
    {
        try
        {
            var profile = await GetUserProfileAsync(userId, userType);

            if (profile == null)
                return Result<UserProfileDto>.NotFound("User not found");

            return Result<UserProfileDto>.Success(profile);
        }
        catch (Exception ex)
        {
            return Result<UserProfileDto>.InternalError($"Failed to get user profile: {ex.Message}");
        }
    }

    // ============ Helper Methods ============

    private async Task<Customer?> FindCustomerByPhoneOrEmailAsync(string phoneOrEmail)
    {
        return await _unitOfWork.Customers
            .SelectAsync(c =>
                (c.Phone == phoneOrEmail || c.Email == phoneOrEmail) && !c.IsDeleted);
    }

    private async Task<Seller?> FindSellerByPhoneOrEmailAsync(string phoneOrEmail)
    {
        return await _unitOfWork.Sellers
            .SelectAsync(s =>
                (s.Phone == phoneOrEmail) && !s.IsDeleted);
    }

    private async Task<Admin?> FindAdminByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _unitOfWork.Admins
            .SelectAsync(a =>
                (a.Username == usernameOrEmail) && !a.IsDeleted);
    }

    private string GetPasswordHash(object user, string userType)
    {
        return userType switch
        {
            "Customer" => ((Customer)user).PasswordHash,
            "Seller" => ((Seller)user).PasswordHash,
            "Admin" => ((Admin)user).PasswordHash,
            _ => ""
        };
    }

    private void SetPasswordHash(object user, string userType, string passwordHash)
    {
        switch (userType)
        {
            case "Customer":
                ((Customer)user).PasswordHash = passwordHash;
                break;
            case "Seller":
                ((Seller)user).PasswordHash = passwordHash;
                break;
            case "Admin":
                ((Admin)user).PasswordHash = passwordHash;
                break;
        }
    }

    private int GetUserId(object user, string userType)
    {
        return userType switch
        {
            "Customer" => ((Customer)user).Id,
            "Seller" => ((Seller)user).Id,
            "Admin" => ((Admin)user).Id,
            _ => 0
        };
    }

    private bool IsUserActive(object user, string userType)
    {
        return userType switch
        {
            "Customer" => ((Customer)user).IsActive,
            "Seller" => ((Seller)user).IsActive,
            "Admin" => ((Admin)user).IsActive,
            _ => false
        };
    }

    private UserProfileDto MapUserToProfileDto(object user, string userType)
    {
        var dto = userType switch
        {
            "Customer" => _mapper.Map<UserProfileDto>((Customer)user),
            "Seller" => _mapper.Map<UserProfileDto>((Seller)user),
            "Admin" => _mapper.Map<UserProfileDto>((Admin)user),
            _ => new UserProfileDto()
        };

        dto.UserType = userType;
        return dto;
    }

    private async Task<UserProfileDto?> GetUserProfileAsync(int userId, string userType)
    {
        object? user = userType switch
        {
            "Customer" => await _unitOfWork.Customers.GetByIdAsync(userId),
            "Seller" => await _unitOfWork.Sellers.GetByIdAsync(userId),
            "Admin" => await _unitOfWork.Admins.GetByIdAsync(userId),
            _ => null
        };

        if (user == null)
            return null;

        return MapUserToProfileDto(user, userType);
    }

    private async Task<bool> VerifyUserExistsAndActive(int userId, string userType)
    {
        object? user = userType switch
        {
            "Customer" => await _unitOfWork.Customers.GetByIdAsync(userId),
            "Seller" => await _unitOfWork.Sellers.GetByIdAsync(userId),
            "Admin" => await _unitOfWork.Admins.GetByIdAsync(userId),
            _ => null
        };

        if (user == null)
            return false;

        return IsUserActive(user, userType);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false // Don't validate expiration for refresh
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
