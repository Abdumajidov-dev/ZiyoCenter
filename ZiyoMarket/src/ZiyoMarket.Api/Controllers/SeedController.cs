using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.Services;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Database seed controller - faqat development uchun
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly RolePermissionSeedService _seedService;
    private readonly IConfiguration _configuration;

    public SeedController(
        RolePermissionSeedService seedService,
        IConfiguration configuration)
    {
        _seedService = seedService;
        _configuration = configuration;
    }

    /// <summary>
    /// Barcha role va permissionlarni seed qilish
    /// </summary>
    [HttpPost("roles-and-permissions")]
    public async Task<IActionResult> SeedRolesAndPermissions()
    {
        try
        {
            await _seedService.SeedRolesAndPermissionsAsync();
            return Ok(new
            {
                success = true,
                message = "Roles and permissions seeded successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = $"Failed to seed: {ex.Message}"
            });
        }
    }
}
