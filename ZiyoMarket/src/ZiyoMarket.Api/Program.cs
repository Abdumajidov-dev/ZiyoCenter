using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using ZiyoMarket.Api.Extensions;
using ZiyoMarket.Api.Helpers;
using ZiyoMarket.Api.Settings;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Data.Seed;
using ZiyoMarket.Service.DTOs.Auth;

var builder = WebApplication.CreateBuilder(args);

// Fix PostgreSQL DateTime Local vs UTC mapping errors
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/ziyomarket-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Barcha request/response lar snake_case bo'lishi uchun
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
        // Deserialization da ham naming policy qo'llaniladi; snake_case bo'lmagan nomlar qabul qilinmaydi
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// To'lov sozlamalari
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection("PaymentSettings"));

builder.Services.AddEndpointsApiExplorer();

// ✅ Swagger sozlamalari
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ZiyoMarket API",
        Version = "v1",
        Description = "ZiyoMarket E-Commerce Platform API",
        Contact = new OpenApiContact
        {
            Name = "ZiyoMarket Team",
            Email = "support@ziyomarket.uz"
        }
    });

    // Swagger schema da property nomlarini snake_case qilish
    options.SchemaFilter<SnakeCaseSchemaFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ PostgreSQL ulanish
builder.Services.AddDbContext<ZiyoMarketDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ JWT sozlamalari
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.MapInboundClaims = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = jwtSettings.ValidateLifetime,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddCustomServices();

var app = builder.Build();

// Seed default data (SuperAdmin, categories, system settings, etc.)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ZiyoMarketDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DataSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database seed/migration xatosi: {Message}", ex.Message);
    }
}

// ✅ Swagger’ni har doim yoqamiz (nafaqat Development’da)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ZiyoMarket API v1");
    options.RoutePrefix = "swagger"; // => /swagger
});

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot/uploads/ uchun
app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllers();

// ✅ Root'ga "API ishlayapti" degan test endpoint
app.MapGet("/", () => Results.Ok("🚀 ZiyoMarket API is running! Visit /swagger"));

// ✅ Health check endpoint for Railway
app.MapGet("/health", async (ZiyoMarketDbContext dbContext) =>
{
    try
    {
        // Check database connectivity
        await dbContext.Database.CanConnectAsync();
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            database = "connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "unhealthy",
            timestamp = DateTime.UtcNow,
            database = "disconnected",
            error = ex.Message
        }, statusCode: 503);
    }
});

app.Run();
