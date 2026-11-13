using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ZiyoMarket.Data.UnitOfWorks;

namespace ZiyoMarket.Api.Helpers;

/// <summary>
/// Admin uchun ruxsat tekshiruvi (AdminPermissions katalogidan kod).
/// Faqat chaqiruvchi Admin bo'lsa ishlaydi: Customer/Seller rollar bilan boshqariladi, shuning uchun
/// "Admin,Seller" endpointlarida sotuvchiga ta'sir qilmaydi. SuperAdmin har doim o'tadi.
/// Bir nechta atribut qo'yilsa, hammasi talab qilinadi.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true || user.FindFirst("UserType")?.Value != "Admin")
            return; // autentifikatsiya va rollarni [Authorize] hal qiladi

        if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var adminId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Ruxsatlar token'da emas, bazadan o'qiladi: SuperAdmin olib tashlagan ruxsat darhol kuchga kiradi
        var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var admin = await unitOfWork.Admins.GetByIdAsync(adminId);

        if (admin == null || admin.IsDeleted || !admin.HasPermission(Permission))
        {
            context.Result = new ObjectResult(new { message = $"Ruxsat yo'q: {Permission}" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
