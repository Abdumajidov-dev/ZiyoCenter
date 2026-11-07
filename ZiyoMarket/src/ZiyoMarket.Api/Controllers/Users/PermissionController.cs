using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Domain.Common;
using ZiyoMarket.Service.DTOs.Admins;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Users;

/// <summary>
/// Ruxsatlar katalogi. Ruxsatlar kodda belgilanadi (AdminPermissions), shuning uchun bu yerda
/// faqat o'qish bor — biriktirish AdminController'da (SuperAdmin).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PermissionController : BaseController
{
    private readonly IAdminService _adminService;

    public PermissionController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new { success = true, data = AdminPermissions.All.Select(PermissionDto.From).ToList() });
    }

    [HttpGet("module/{module}")]
    public IActionResult GetByModule(string module)
    {
        var permissions = AdminPermissions.All
            .Where(p => p.Module.Equals(module, StringComparison.OrdinalIgnoreCase))
            .Select(PermissionDto.From)
            .ToList();

        return Ok(new { success = true, data = permissions });
    }

    /// <summary>
    /// Joriy adminning ruxsatlari — admin panel menyuni shunga qarab ko'rsatishi mumkin.
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
    {
        var result = await _adminService.GetAdminPermissionsAsync(GetCurrentUserId());

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }
}
