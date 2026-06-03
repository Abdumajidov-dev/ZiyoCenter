using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ZiyoMarket.Api.Helpers;

/// <summary>
/// Flutter snake_case query param larini PascalCase ga map qiladi.
/// Misol: ?search_term=kitob → SearchTerm = "kitob"
/// </summary>
public class SnakeCaseQueryValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        var query = context.ActionContext.HttpContext.Request.Query;
        if (query?.Count > 0)
        {
            context.ValueProviders.Insert(0, new SnakeCaseQueryValueProvider(
                BindingSource.Query, query, CultureInfo.InvariantCulture));
        }
        return Task.CompletedTask;
    }
}

public class SnakeCaseQueryValueProvider : QueryStringValueProvider
{
    private readonly IQueryCollection _queryCollection;

    public SnakeCaseQueryValueProvider(
        BindingSource bindingSource,
        IQueryCollection values,
        CultureInfo culture)
        : base(bindingSource, values, culture)
    {
        _queryCollection = values;
    }

    public override ValueProviderResult GetValue(string key)
    {
        var result = base.GetValue(key);
        if (result == ValueProviderResult.None)
        {
            var snakeKey = ToSnakeCase(key);
            if (!string.Equals(snakeKey, key, StringComparison.OrdinalIgnoreCase))
                result = base.GetValue(snakeKey);
        }
        return result;
    }

    public override bool ContainsPrefix(string prefix)
    {
        if (base.ContainsPrefix(prefix))
            return true;

        var snakePrefix = ToSnakeCase(prefix);
        if (!string.Equals(snakePrefix, prefix, StringComparison.OrdinalIgnoreCase))
            return base.ContainsPrefix(snakePrefix);

        return false;
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }
}
