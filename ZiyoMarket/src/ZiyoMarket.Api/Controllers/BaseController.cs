using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ZiyoMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Get current user ID from JWT token
    /// </summary>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get current user type from JWT token
    /// </summary>
    protected string GetCurrentUserType()
    {
        return User.FindFirst("UserType")?.Value ?? "";
    }

    /// <summary>
    /// Check if current user is authenticated
    /// </summary>
    protected bool IsAuthenticated()
    {
        return User.Identity?.IsAuthenticated ?? false;
    }
}
