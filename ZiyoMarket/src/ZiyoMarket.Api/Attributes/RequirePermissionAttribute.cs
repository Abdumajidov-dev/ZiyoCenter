using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ZiyoMarket.Api.Attributes;

/// <summary>
/// Permission-based authorization attribute
/// Foydalanuvchi kerakli permission'ga ega ekanligini tekshiradi
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _permissions;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // User authenticated ekanligini tekshirish
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "Tizimga kirish talab qilinadi"
            });
            return;
        }

        // Foydalanuvchining permissionlarini olish
        var userPermissions = context.HttpContext.User
            .FindAll("Permission")
            .Select(c => c.Value)
            .ToList();

        // SuperAdmin barchaga ruxsat beriladi
        var roles = context.HttpContext.User
            .FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (roles.Contains("SuperAdmin"))
            return;

        // Kamida bitta kerakli permission borligini tekshirish
        var hasPermission = _permissions.Any(p => userPermissions.Contains(p));

        if (!hasPermission)
        {
            context.Result = new ObjectResult(new
            {
                success = false,
                message = "Sizda bu amalni bajarish uchun ruxsat yo'q",
                required_permissions = _permissions
            })
            {
                StatusCode = 403
            };
        }
    }
}
