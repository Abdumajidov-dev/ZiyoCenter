using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Api.Attributes;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Test controller for permission-based authorization demo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestPermissionController : ControllerBase
{
    /// <summary>
    /// Test endpoint - any authenticated user can access
    /// </summary>
    [HttpGet("public")]
    [Authorize]
    public IActionResult PublicEndpoint()
    {
        var userName = User.Identity?.Name;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();

        return Ok(new
        {
            success = true,
            message = "Public endpoint - har qanday authenticated user kirishi mumkin",
            user = userName,
            roles = roles,
            permissions = permissions
        });
    }

    /// <summary>
    /// Requires ManageProductStock permission (Seller role has this)
    /// </summary>
    [HttpGet("seller-only")]
    [RequirePermission("ManageProductStock")]
    public IActionResult SellerOnlyEndpoint()
    {
        var userName = User.Identity?.Name;
        return Ok(new
        {
            success = true,
            message = "Seller-only endpoint - faqat ManageProductStock permission bor userlar kiradi",
            user = userName
        });
    }

    /// <summary>
    /// Requires ManageUsers permission (only Admin/SuperAdmin have this)
    /// </summary>
    [HttpGet("admin-only")]
    [RequirePermission("ManageUsers")]
    public IActionResult AdminOnlyEndpoint()
    {
        var userName = User.Identity?.Name;
        return Ok(new
        {
            success = true,
            message = "Admin-only endpoint - faqat ManageUsers permission bor userlar kiradi",
            user = userName
        });
    }

    /// <summary>
    /// Requires ViewProducts permission (everyone has this - Customer, Seller, etc)
    /// </summary>
    [HttpGet("everyone")]
    [RequirePermission("ViewProducts")]
    public IActionResult EveryoneEndpoint()
    {
        var userName = User.Identity?.Name;
        return Ok(new
        {
            success = true,
            message = "Everyone endpoint - barcha userlar ViewProducts permission bor",
            user = userName
        });
    }

    /// <summary>
    /// Requires EITHER CreateProduct OR UpdateProduct permission (OR logic)
    /// </summary>
    [HttpGet("multiple-permissions")]
    [RequirePermission("CreateProduct", "UpdateProduct")]
    public IActionResult MultiplePermissionsEndpoint()
    {
        var userName = User.Identity?.Name;
        return Ok(new
        {
            success = true,
            message = "Multiple permissions endpoint - CreateProduct YOKI UpdateProduct permission kerak",
            user = userName
        });
    }
}
