using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZiyoMarket.Api.Helpers;

/// <summary>
/// Swagger schema filter — property nomlarini snake_case ga o'giradi.
/// Swagger UI da ham to'g'ri nomlar ko'rinishi uchun kerak.
/// </summary>
public class SnakeCaseSchemaFilter : ISchemaFilter
{
    private readonly SnakeCaseNamingPolicy _namingPolicy = new();

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || schema.Properties.Count == 0)
            return;

        var converted = schema.Properties
            .ToDictionary(
                p => _namingPolicy.ConvertName(p.Key),
                p => p.Value
            );

        schema.Properties.Clear();
        foreach (var property in converted)
            schema.Properties.Add(property);
    }
}
