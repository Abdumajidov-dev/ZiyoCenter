using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using ZiyoMarket.Api.Extensions;
using ZiyoMarket.Api.Helpers;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Service.DTOs.Auth;

var builder = WebApplication.CreateBuilder(args);

// ✅ Railway PORT configuration
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/ziyomarket-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting ZiyoMarket API on port {Port}", port);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use snake_case naming policy for JSON serialization
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
        // Optional: Configure other JSON options
        options.JsonSerializerOptions.WriteIndented = false; // Compact JSON
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

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

// ✅ PostgreSQL ulanish (Railway environment variable support)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL")
    ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string not found! Please set DATABASE_URL environment variable or configure DefaultConnection in appsettings.json");
}

Log.Information("Using database connection: {ConnectionInfo}",
    connectionString.Contains("railway") ? "Railway PostgreSQL" : "Local PostgreSQL");

builder.Services.AddDbContext<ZiyoMarketDbContext>(options =>
    options.UseNpgsql(connectionString));

// ✅ JWT sozlamalari
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new Exception("JwtSettings not configured properly");
}

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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCustomServices();

var app = builder.Build();

// ✅ Auto-run migrations on startup (Railway deployment)
if (app.Environment.IsProduction())
{
    try
    {
        Log.Information("🗄️  Running database migrations...");
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZiyoMarketDbContext>();
            dbContext.Database.Migrate();
            Log.Information("✅ Migrations completed successfully");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Migration failed: {Message}", ex.Message);
        // Don't throw - let app start anyway for debugging
    }
}

// ✅ Swagger'ni har doim yoqamiz (nafaqat Development'da)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ZiyoMarket API v1");
    options.RoutePrefix = "swagger"; // => /swagger
});

// ✅ HTTPS redirection faqat Development da (Railway proxy HTTPS handle qiladi)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllers();

// ✅ Root'ga "API ishlayapti" degan test endpoint
app.MapGet("/", () => Results.Ok("🚀 ZiyoMarket API is running! Visit /swagger"));

// ✅ Health check endpoint for Railway (lightweight - no DB check during startup)
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        service = "ZiyoMarket API"
    });
});

// ✅ Detailed health check with database (optional)
app.MapGet("/health/detailed", async (ZiyoMarketDbContext dbContext) =>
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
