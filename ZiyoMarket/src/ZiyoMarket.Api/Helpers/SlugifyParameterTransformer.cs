using System.Text.RegularExpressions;

namespace ZiyoMarket.Api.Helpers;

/// <summary>
/// Transforms controller and action names to snake_case for URLs
/// Example: AuthController -> auth, ProductController -> product
/// </summary>
public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null)
            return null;

        var str = value.ToString();
        if (string.IsNullOrEmpty(str))
            return str;

        // Convert PascalCase to snake_case
        // AuthController -> auth_controller -> auth (Controller suffix removed by framework)
        // ProductList -> product_list
        return Regex.Replace(str, "([a-z])([A-Z])", "$1_$2").ToLower();
    }
}
