using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Admin seed controller - test admin yaratish uchun
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminSeedController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminSeedController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Test admin yaratish yoki mavjudligini tekshirish
    /// POST /api/adminseed/create-test-admin
    /// </summary>
    [HttpPost("create-test-admin")]
    public async Task<IActionResult> CreateTestAdmin()
    {
        try
        {
            // Username bo'yicha tekshirish
            var existingAdmin = await _unitOfWork.Admins.SelectAsync(a => a.Username == "admin");

            if (existingAdmin != null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Test admin allaqachon mavjud",
                    data = new
                    {
                        username = existingAdmin.Username,
                        email = existingAdmin.Phone,
                        role = existingAdmin.Role,
                        password = "Admin@123 (o'zgartirilmagan bo'lsa)"
                    }
                });
            }

            // Yangi admin yaratish
            var admin = new Admin
            {
                FirstName = "Super",
                LastName = "Admin",
                Username = "admin",
                Phone = "+998901234567",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "SuperAdmin",
                Permissions = null, // SuperAdmin'ga permissions kerak emas
                IsActive = true,
                CreatedBy = null // Seed data
            };

            await _unitOfWork.Admins.InsertAsync(admin);
            await _unitOfWork.SaveChangesAsync();

            return StatusCode(201, new
            {
                success = true,
                message = "Test admin muvaffaqiyatli yaratildi",
                data = new
                {
                    id = admin.Id,
                    username = admin.Username,
                    password = "Admin@123",
                    role = admin.Role,
                    loginUrl = "/api/auth/login",
                    credentials = new
                    {
                        username = "admin",
                        password = "Admin@123"
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Xatolik: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Barcha adminlarni ko'rish
    /// GET /api/adminseed/list-admins
    /// </summary>
    [HttpGet("list-admins")]
    public async Task<IActionResult> ListAdmins()
    {
        try
        {
            var admins = await _unitOfWork.Admins.FindAsync(a => a.IsActive);

            var adminList = admins.Select(a => new
            {
                id = a.Id,
                username = a.Username,
                fullName = a.FullName,
                phone = a.Phone,
                role = a.Role,
                isActive = a.IsActive,
                createdAt = a.CreatedAt
            }).ToList();

            return Ok(new
            {
                success = true,
                data = adminList,
                count = adminList.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Xatolik: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Admin parolini reset qilish
    /// POST /api/adminseed/reset-password/{username}
    /// </summary>
    [HttpPost("reset-password/{username}")]
    public async Task<IActionResult> ResetPassword(string username, [FromQuery] string newPassword = "Admin@123")
    {
        try
        {
            var admin = await _unitOfWork.Admins.SelectAsync(a => a.Username == username);

            if (admin == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Admin '{username}' topilmadi"
                });
            }

            // Parolni yangilash
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            admin.MarkAsUpdated();

            await _unitOfWork.Admins.Update(admin, admin.Id);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Admin '{username}' paroli yangilandi",
                data = new
                {
                    username = admin.Username,
                    newPassword = newPassword
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Xatolik: {ex.Message}"
            });
        }
    }
}
