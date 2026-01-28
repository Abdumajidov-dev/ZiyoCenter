using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.DTOs.Sms;
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
    private readonly ISmsService _smsService;
    private readonly IMemoryCache _memoryCache;

    public AuthService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings,
        ISmsService smsService,
        IMemoryCache memoryCache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
        _smsService = smsService;
        _memoryCache = memoryCache;
    }

    // ============ Login ============

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {


        // Normalize Phone Number
        request.Phone = NormalizePhoneNumber(request.Phone);

        try
        {
            // DEBUG: Log incoming UserType
            Console.WriteLine($"🔍 LOGIN REQUEST - Phone: {request.Phone}");

            // Auto-detect UserType
            object? user = null;
            string userType = "";

            // Try Customer
            user = await FindCustomerByPhoneAsync(request.Phone);
            if (user != null) userType = "Customer";
            else
            {
                // Try Seller
                user = await FindSellerByPhoneAsync(request.Phone);
                if (user != null) userType = "Seller";
                else
                {
                    // Try Admin
                    user = await FindAdminByUsernameOrPhoneAsync(request.Phone);
                    if (user != null) userType = "Admin";
                }
            }

            if (user == null)
                return Result<LoginResponseDto>.Unauthorized("Invalid credentials");

            // Verify password
            // Verify password
            string passwordHash = GetPasswordHash(user, userType);
            if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
                return Result<LoginResponseDto>.Unauthorized("Invalid credentials");

            // Check if user is active
            if (!IsUserActive(user, userType))
                return Result<LoginResponseDto>.Forbidden("Account is inactive");

            // Generate token
            var userId = GetUserId(user, userType);
            var accessToken = await GenerateAccessTokenAsync(userId, userType);

            // Map to response
            var userProfile = MapUserToProfileDto(user, userType);

            var response = new LoginResponseDto
            {
                AccessToken = accessToken.Data!,
                ExpiresAt = TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = userProfile
            };

            // Update last login for admin
            // Update last login for admin
            if (userType == "Admin" && user is Admin admin)
            {
                admin.LastLoginAt = TimeHelper.GetCurrentServerTime();
                await _unitOfWork.Admins.Update(admin, admin.Id);
                await _unitOfWork.SaveChangesAsync();
            }

            return Result<LoginResponseDto>.Success(response, "Login successful");
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Login failed: {ex.Message}");
        }
    }

    // ============ Universal Register ============

    public async Task<Result<LoginResponseDto>> RegisterUserAsync(RegisterUserDto request)
    {
        // Always default to Customer
        string userType = "Customer";

        // Normalize Phone Number
        request.Phone = NormalizePhoneNumber(request.Phone);

        try
        {
            // Check if phone already exists in any user type
            var existingCustomer = await _unitOfWork.Customers.SelectAsync(c => c.Phone == request.Phone);
            var existingSeller = await _unitOfWork.Sellers.SelectAsync(s => s.Phone == request.Phone);
            var existingAdmin = await _unitOfWork.Admins.SelectAsync(a => a.Phone == request.Phone);

            if (existingCustomer != null || existingSeller != null || existingAdmin != null)
                return Result<LoginResponseDto>.Conflict("Phone number already registered");

            // Create Customer
            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Address = request.Address,
                CashbackBalance = 0,
                IsActive = true
            };
            
            await _unitOfWork.Customers.InsertAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            
            // Generate token
            var accessToken = await GenerateAccessTokenAsync(customer.Id, userType);

            var response = new LoginResponseDto
            {
                AccessToken = accessToken.Data!,
                ExpiresAt = TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = MapUserToProfileDto(customer, userType)
            };

            return Result<LoginResponseDto>.Success(response, "Registration successful", 201);
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Registration failed: {ex.Message}");
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


        // Normalize Phone Number
        request.Phone = NormalizePhoneNumber(request.Phone);

        try
        {
            // Find user (Auto-detect from phone)
            object? user = null;
            string userType = "";

            // Check Customer
            user = await FindCustomerByPhoneAsync(request.Phone);
            if (user != null) userType = "Customer";
            else
            {
                // Check Seller
                user = await FindSellerByPhoneAsync(request.Phone);
                if (user != null) userType = "Seller";
                else
                {
                    // Check Admin
                    user = await FindAdminByUsernameOrPhoneAsync(request.Phone);
                    if (user != null) userType = "Admin";
                }
            }

            if (user == null)
            {
                // Don't reveal if user exists or not
                return Result.Success("If account exists, verification code has been sent");
            }

            // Generate and send verification code
            var code = GenerateVerificationCode();

            // TODO: Send code via SMS/Email
            // For now, just log it (in production, integrate with SMS/Email service)
            Console.WriteLine($"Verification code for {request.Phone}: {code}");

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
            // For now, accept any 4-digit code for demo

            // Find user
            object? user = null;
            string userType = "";

            // Try to find in all user types
            var customer = await FindCustomerByPhoneAsync(request.Phone);
            if (customer != null)
            {
                user = customer;
                userType = "Customer";
            }
            else
            {
                var seller = await FindSellerByPhoneAsync(request.Phone);
                if (seller != null)
                {
                    user = seller;
                    userType = "Seller";
                }
                else
                {
                    var admin = await FindAdminByUsernameOrPhoneAsync(request.Phone);
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

    public async Task<Result> SendVerificationCodeAsync(string phone)
    {
        try
        {
            var code = GenerateVerificationCode();

            // TODO: Send via SMS/Email
            Console.WriteLine($"Verification code for {phone}: {code}");

            await Task.CompletedTask;
            return Result.Success("Verification code sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Failed to send verification code: {ex.Message}");
        }
    }

    public async Task<Result> VerifyCodeAsync(string phone, string code)
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
                new Claim(ClaimTypes.Role, userType), // Required for [Authorize(Roles = "...")] to work
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
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

    private async Task<Customer?> FindCustomerByPhoneAsync(string phone)
    {
        return await _unitOfWork.Customers
            .SelectAsync(c => c.Phone == phone);
    }

    private async Task<Seller?> FindSellerByPhoneAsync(string phone)
    {
        return await _unitOfWork.Sellers
            .SelectAsync(s => s.Phone == phone);
    }

    private async Task<Admin?> FindAdminByUsernameOrPhoneAsync(string usernameOrPhone)
    {
        return await _unitOfWork.Admins
            .SelectAsync(a => (a.Username == usernameOrPhone || a.Phone == usernameOrPhone));
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
        return random.Next(1000, 9999).ToString();
    }

    private string NormalizePhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        // Remove all non-digit characters except +
        string cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // If starts with 998... but no +, add it
        if (cleaned.StartsWith("998") && cleaned.Length == 12)
        {
            cleaned = "+" + cleaned;
        }
        // If starts with 9... (9 digits) and no prefix, assume 998
        else if (!cleaned.StartsWith("+") && cleaned.Length == 9)
        {
            cleaned = "+998" + cleaned;
        }

        return cleaned;
    }

    // ============ Change User Role (Admin Only) ============

    public async Task<Result<LoginResponseDto>> ChangeUserRoleAsync(ChangeUserRoleDto request, int adminUserId)
    {
        // Normalize UserTypes to Title Case
        if (!string.IsNullOrEmpty(request.CurrentUserType) && request.CurrentUserType.Length > 1)
        {
            request.CurrentUserType = char.ToUpper(request.CurrentUserType[0]) + request.CurrentUserType.Substring(1).ToLower();
        }
        if (!string.IsNullOrEmpty(request.NewUserType) && request.NewUserType.Length > 1)
        {
            request.NewUserType = char.ToUpper(request.NewUserType[0]) + request.NewUserType.Substring(1).ToLower();
        }

        try
        {
            // Verify admin exists and is active
            var admin = await _unitOfWork.Admins.GetByIdAsync(adminUserId);
            if (admin == null || !admin.IsActive)
                return Result<LoginResponseDto>.Forbidden("Admin not found or inactive");

            // Validate user types
            if (request.CurrentUserType != "Customer" && request.CurrentUserType != "Seller" && request.CurrentUserType != "Admin")
                return Result<LoginResponseDto>.BadRequest("Invalid current user type");

            if (request.NewUserType != "Customer" && request.NewUserType != "Seller" && request.NewUserType != "Admin")
                return Result<LoginResponseDto>.BadRequest("Invalid new user type");

            if (request.CurrentUserType == request.NewUserType)
                return Result<LoginResponseDto>.BadRequest("Current and new user types are the same");

            // Find existing user
            object? existingUser = null;
            switch (request.CurrentUserType)
            {
                case "Customer":
                    existingUser = await _unitOfWork.Customers.GetByIdAsync(request.UserId);
                    break;
                case "Seller":
                    existingUser = await _unitOfWork.Sellers.GetByIdAsync(request.UserId);
                    break;
                case "Admin":
                    existingUser = await _unitOfWork.Admins.GetByIdAsync(request.UserId);
                    break;
            }

            if (existingUser == null)
                return Result<LoginResponseDto>.NotFound($"{request.CurrentUserType} user not found");

            // Check if phone already exists in target user type
            string phone = GetUserPhone(existingUser, request.CurrentUserType);
            bool phoneExists = await CheckPhoneExistsInUserType(phone, request.NewUserType);

            if (phoneExists)
                return Result<LoginResponseDto>.Conflict($"Phone number already exists in {request.NewUserType} table");

            // Get user data
            string firstName = GetUserFirstName(existingUser, request.CurrentUserType);
            string lastName = GetUserLastName(existingUser, request.CurrentUserType);
            string passwordHash = GetPasswordHash(existingUser, request.CurrentUserType);
            string? address = GetUserAddress(existingUser, request.CurrentUserType);

            // Create new user in target table
            object newUser;
            int newUserId;

            switch (request.NewUserType)
            {
                case "Customer":
                    var customer = new Customer
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Phone = phone,
                        PasswordHash = passwordHash,
                        Address = address,
                        CashbackBalance = 0,
                        IsActive = true
                    };
                    await _unitOfWork.Customers.InsertAsync(customer);
                    await _unitOfWork.SaveChangesAsync();
                    newUser = customer;
                    newUserId = customer.Id;
                    break;

                case "Seller":
                    var seller = new Seller
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Phone = phone,
                        PasswordHash = passwordHash,
                        Role = "Seller",
                        IsActive = true
                    };
                    await _unitOfWork.Sellers.InsertAsync(seller);
                    await _unitOfWork.SaveChangesAsync();
                    newUser = seller;
                    newUserId = seller.Id;
                    break;

                case "Admin":
                    // Generate username for new admin
                    string username = $"{firstName.ToLower()}.{lastName.ToLower()}{new Random().Next(100, 999)}";

                    var newAdmin = new Admin
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Username = username,
                        Phone = phone,
                        PasswordHash = passwordHash,
                        Role = "Admin",
                        IsActive = true
                    };
                    await _unitOfWork.Admins.InsertAsync(newAdmin);
                    await _unitOfWork.SaveChangesAsync();
                    newUser = newAdmin;
                    newUserId = newAdmin.Id;
                    break;

                default:
                    return Result<LoginResponseDto>.BadRequest("Invalid new user type");
            }

            // Soft delete old user
            switch (request.CurrentUserType)
            {
                case "Customer":
                    ((Customer)existingUser).Delete();
                    await _unitOfWork.Customers.UpdateAsync((Customer)existingUser);
                    break;
                case "Seller":
                    ((Seller)existingUser).Delete();
                    await _unitOfWork.Sellers.UpdateAsync((Seller)existingUser);
                    break;
                case "Admin":
                    ((Admin)existingUser).Delete();
                    await _unitOfWork.Admins.UpdateAsync((Admin)existingUser);
                    break;
            }

            await _unitOfWork.SaveChangesAsync();

            // Generate new token
            var accessToken = await GenerateAccessTokenAsync(newUserId, request.NewUserType);

            var response = new LoginResponseDto
            {
                AccessToken = accessToken.Data!,
                ExpiresAt = TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = MapUserToProfileDto(newUser, request.NewUserType)
            };

            return Result<LoginResponseDto>.Success(response, $"User role changed from {request.CurrentUserType} to {request.NewUserType}");
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Failed to change user role: {ex.Message}");
        }
    }

    // ============ Create Dev Admin (No Auth Required) ============

    public async Task<Result<LoginResponseDto>> CreateDevAdminAsync(CreateDevAdminDto request)
    {
        try
        {
            // Check if username already exists
            var existingUsername = await _unitOfWork.Admins.SelectAsync(a => a.Username == request.Username);
            if (existingUsername != null)
                return Result<LoginResponseDto>.Conflict("Username already registered");

            // Check if phone already exists
            var existingPhone = await _unitOfWork.Admins.SelectAsync(a => a.Phone == request.Phone);
            if (existingPhone != null)
                return Result<LoginResponseDto>.Conflict("Phone number already registered");

            // Create admin
            var admin = new Admin
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Admin",
                IsActive = true
            };

            await _unitOfWork.Admins.InsertAsync(admin);
            await _unitOfWork.SaveChangesAsync();

            // Generate token
            var accessToken = await GenerateAccessTokenAsync(admin.Id, "Admin");

            var response = new LoginResponseDto
            {
                AccessToken = accessToken.Data!,
                ExpiresAt = TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = MapUserToProfileDto(admin, "Admin")
            };

            return Result<LoginResponseDto>.Success(response, "Development admin created successfully", 201);
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Failed to create dev admin: {ex.Message}");
        }
    }

    // ============ Helper Methods for Role Change ============

    private string GetUserPhone(object user, string userType)
    {
        return userType switch
        {
            "Customer" => ((Customer)user).Phone,
            "Seller" => ((Seller)user).Phone,
            "Admin" => ((Admin)user).Phone,
            _ => ""
        };
    }

    private string GetUserFirstName(object user, string userType)
    {
        return userType switch
        {
            "Customer" => ((Customer)user).FirstName,
            "Seller" => ((Seller)user).FirstName,
            "Admin" => ((Admin)user).FirstName,
            _ => ""
        };
    }

    private string GetUserLastName(object user, string userType)
    {
        return userType switch
        {
            "Customer" => ((Customer)user).LastName,
            "Seller" => ((Seller)user).LastName,
            "Admin" => ((Admin)user).LastName,
            _ => ""
        };
    }

    private string? GetUserAddress(object user, string userType)
    {
        return userType switch
        {
            "Customer" => ((Customer)user).Address,
            _ => null
        };
    }

    private async Task<bool> CheckPhoneExistsInUserType(string phone, string userType)
    {
        return userType switch
        {
            "Customer" => await _unitOfWork.Customers.SelectAsync(c => c.Phone == phone) != null,
            "Seller" => await _unitOfWork.Sellers.SelectAsync(s => s.Phone == phone) != null,
            "Admin" => await _unitOfWork.Admins.SelectAsync(a => a.Phone == phone) != null,
            _ => false
        };
    }



    // ============ OTP Methods ============

    /// <summary>
    /// Send OTP code to phone number (for registration/login)
    /// </summary>
    public async Task<Result<VerificationResultDto>> SendOtpAsync(SendVerificationCodeDto request)
    {
        try
        {
            // Validate phone number format
            if (!request.PhoneNumber.StartsWith("+998") || request.PhoneNumber.Length != 13)
            {
                return Result<VerificationResultDto>.BadRequest("Phone number must be in format +998XXXXXXXXX");
            }

            // Send verification code via SMS service
            var smsResult = await _smsService.SendVerificationCodeAsync(request);

            if (!smsResult.IsSuccess || smsResult.Data == null)
            {
                return Result<VerificationResultDto>.InternalError(smsResult.Message ?? "Failed to send OTP");
            }

            return Result<VerificationResultDto>.Success(smsResult.Data, "OTP code sent successfully");
        }
        catch (Exception ex)
        {
            return Result<VerificationResultDto>.InternalError($"Failed to send OTP: {ex.Message}");
        }
    }

    /// <summary>
    /// Verify OTP code
    /// </summary>
    public async Task<Result<string>> VerifyOtpAsync(VerifySmsCodeDto request)
    {
        try
        {
            // Normalize phone number
            request.PhoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            // Validate phone number format
            if (!request.PhoneNumber.StartsWith("+998") || request.PhoneNumber.Length != 13)
            {
                return Result<string>.BadRequest("Phone number must be in format +998XXXXXXXXX");
            }

            // Validate code format (4 digits)
            if (request.Code.Length != 4 || !request.Code.All(char.IsDigit))
            {
                return Result<string>.BadRequest("Code must be 4 digits");
            }

            // Verify code via SMS service
            var verifyResult = await _smsService.VerifyCodeAsync(request);

            if (!verifyResult.IsSuccess)
            {
                return Result<string>.BadRequest(verifyResult.Message ?? "Invalid or expired OTP code");
            }

            // Find user
            object? user = null;
            string userType = "";

            // Try Customer
            var customer = await FindCustomerByPhoneAsync(request.PhoneNumber);
            if (customer != null) { user = customer; userType = "Customer"; }
            else
            {
                var seller = await FindSellerByPhoneAsync(request.PhoneNumber);
                if (seller != null) { user = seller; userType = "Seller"; }
                else
                {
                    var admin = await FindAdminByUsernameOrPhoneAsync(request.PhoneNumber);
                    if (admin != null) { user = admin; userType = "Admin"; }
                }
            }

            if (user == null)
            {
                // Phone is verified, but user is not registered yet
                return Result<string>.Success(null, "Phone verified, but user not found. Please register.");
            }

            // Check if user is active
            if (!IsUserActive(user, userType))
                return Result<string>.Forbidden("Account is inactive");

            // Generate token
            var userId = GetUserId(user, userType);
            var accessToken = await GenerateAccessTokenAsync(userId, userType);

            return Result<string>.Success(accessToken.Data, "OTP code verified successfully");
        }
        catch (Exception ex)
        {
            return Result<string>.InternalError($"Failed to verify OTP: {ex.Message}");
        }
    }
}
