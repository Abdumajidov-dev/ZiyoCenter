using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.Services;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Role va Permission management - faqat Admin uchun
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class RoleManagementController : ControllerBase
{
    private readonly RoleManagementService _roleService;

    public RoleManagementController(RoleManagementService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Foydalanuvchiga role biriktirish
    /// </summary>
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var result = await _roleService.AssignRoleToUserAsync(
            request.UserId,
            request.RoleName,
            adminId
        );

        if (!result.IsSuccess)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Foydalanuvchidan roleni olib tashlash
    /// </summary>
    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleRequest request)
    {
        var result = await _roleService.RemoveRoleFromUserAsync(request.UserId, request.RoleName);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Foydalanuvchining rollarini ko'rish
    /// </summary>
    [HttpGet("user-roles/{userId}")]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        var result = await _roleService.GetUserRolesAsync(userId);

        if (!result.IsSuccess)
            return NotFound(new { success = false, message = result.Message });

        return Ok(new { success = true, roles = result.Data });
    }

    /// <summary>
    /// Foydalanuvchining permissionlarini ko'rish
    /// </summary>
    [HttpGet("user-permissions/{userId}")]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var result = await _roleService.GetUserPermissionsAsync(userId);

        if (!result.IsSuccess)
            return NotFound(new { success = false, message = result.Message });

        return Ok(new { success = true, permissions = result.Data });
    }

    /// <summary>
    /// Barcha rollarni ko'rish
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _roleService.GetAllRolesAsync();

        if (!result.IsSuccess)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, roles = result.Data });
    }

    /// <summary>
    /// Barcha permissionlarni ko'rish
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _roleService.GetAllPermissionsAsync();

        if (!result.IsSuccess)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, permissions = result.Data });
    }
}

// Request DTOs
public class AssignRoleRequest
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public class RemoveRoleRequest
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
