using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Bootstrap controller - birinchi admin yaratish uchun
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BootstrapController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public BootstrapController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Birinchi SuperAdmin yaratish (faqat bir marta)
    /// </summary>
    [HttpPost("create-first-admin")]
    public async Task<IActionResult> CreateFirstAdmin([FromQuery] int userId)
    {
        try
        {
            // Allaqachon SuperAdmin borligini tekshirish
            var existingSuperAdmin = await _unitOfWork.UserRoles.SelectAsync(ur =>
                ur.Role.Name == "SuperAdmin",
                includes: new[] { "Role" }
            );

            if (existingSuperAdmin != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "SuperAdmin allaqachon mavjud"
                });
            }

            // User'ni topish
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Foydalanuvchi topilmadi"
                });
            }

            // SuperAdmin roleni topish
            var superAdminRole = await _unitOfWork.Roles.SelectAsync(r => r.Name == "SuperAdmin");
            if (superAdminRole == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "SuperAdmin role topilmadi. Seed data bajaring."
                });
            }

            // SuperAdmin roleni biriktirish
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = superAdminRole.Id,
                AssignedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRoles.InsertAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"User #{userId} SuperAdmin role bilan tayinlandi. Qaytadan login qiling."
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
