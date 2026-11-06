using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ZiyoMarket.Api.Helpers;

/// <summary>
/// Test ma'lumot yaratadigan/o'chiradigan endpointlarni faqat Development muhitida ochadi.
/// Production'da ular 404 qaytaradi — aks holda masalan clear-all butun bazani o'chirardi,
/// cashback/seed esa mijozga bepul keshbek berardi.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DevelopmentOnlyAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (!env.IsDevelopment())
            context.Result = new NotFoundResult();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
