using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.Helpers;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Yangi unified authentication service - faqat User table ishlatadi
/// </summary>
public class NewAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public NewAuthService(
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Yangi foydalanuvchi registratsiyasi - HAMMA CUSTOMER ROLE BILAN BOSHLANADI
    /// </summary>
    public async Task<Result<LoginResponseDto>> RegisterAsync(string firstName, string lastName, string phone, string password)
    {
        try
        {
            // Telefon raqami unique ekanligini tekshirish
            var existingUser = await _unitOfWork.Users.SelectAsync(u => u.Phone == phone);
            if (existingUser != null)
                return Result<LoginResponseDto>.Conflict("Bu telefon raqami allaqachon ro'yxatdan o'tgan");

            // Yangi user yaratish
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                IsPhoneVerified = false,
                CashbackBalance = 0
            };

            await _unitOfWork.Users.InsertAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Default "Customer" roleni topish
            var customerRole = await _unitOfWork.Roles.SelectAsync(r => r.Name == "Customer");
            if (customerRole == null)
            {
                return Result<LoginResponseDto>.InternalError("Customer role topilmadi. Iltimos, avval seed data bajaring.");
            }

            // Foydalanuvchiga Customer roleni biriktirish
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = customerRole.Id,
                AssignedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRoles.InsertAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            // JWT token yaratish
            var token = await GenerateTokenAsync(user);

            var response = new LoginResponseDto
            {
                AccessToken = token,
                ExpiresAt = TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    UserType = "Customer", // Hamma customer bo'lib boshlanadi
                    IsActive = user.IsActive
                }
            };

            return Result<LoginResponseDto>.Success(response, "Ro'yxatdan o'tish muvaffaqiyatli", 201);
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Ro'yxatdan o'tishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Login - yangi User table bilan
    /// </summary>
    public async Task<Result<LoginResponseDto>> LoginAsync(string phone, string password)
    {
        try
        {
            // Foydalanuvchini topish (roles bilan)
            var user = await _unitOfWork.Users.SelectAsync(
                u => u.Phone == phone,
                includes: new[] { "UserRoles.Role.RolePermissions.Permission" }
            );

            if (user == null)
                return Result<LoginResponseDto>.Unauthorized("Telefon raqam yoki parol noto'g'ri");

            // Parolni tekshirish
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return Result<LoginResponseDto>.Unauthorized("Telefon raqam yoki parol noto'g'ri");

            // Faol ekanligini tekshirish
            if (!user.IsActive)
                return Result<LoginResponseDto>.Forbidden("Akkaunt faol emas");

            // Last login yangilash
            user.UpdateLastLogin();
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // JWT token yaratish
            var token = await GenerateTokenAsync(user);

            // Role nomlarini olish
            var roleNames = user.GetRoleNames().ToList();
            var primaryRole = roleNames.FirstOrDefault() ?? "Customer";

            var response = new LoginResponseDto
            {
                AccessToken = token,
                ExpiresAt = TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    UserType = primaryRole,
                    IsActive = user.IsActive
                }
            };

            return Result<LoginResponseDto>.Success(response, "Login muvaffaqiyatli");
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.InternalError($"Login xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// JWT token yaratish - barcha role va permissionlar bilan
    /// </summary>
    private async Task<string> GenerateTokenAsync(User user)
    {
        // Foydalanuvchi rollarini olish (agar yuklanmagan bo'lsa)
        if (!user.UserRoles.Any())
        {
            var userWithRoles = await _unitOfWork.Users.SelectAsync(
                u => u.Id == user.Id,
                includes: new[] { "UserRoles.Role.RolePermissions.Permission" }
            );
            if (userWithRoles != null)
                user = userWithRoles;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("Phone", user.Phone),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Email qo'shish (agar bor bo'lsa)
        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

        // Barcha rollarni qo'shish
        var roles = user.GetRoleNames();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Barcha permissionlarni qo'shish
        var permissions = user.GetPermissions();
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: TimeHelper.GetCurrentServerTime().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
