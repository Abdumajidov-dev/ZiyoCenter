using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using ZiyoMarket.Api.Extensions;
using ZiyoMarket.Api.Filters;
using ZiyoMarket.Api.Helpers;
using ZiyoMarket.Api.Middleware;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Service.DTOs.Auth;

var builder = WebApplication.CreateBuilder(args);

// ✅ Railway PORT configuration
// ✅ Railway PORT configuration
// var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
// builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/ziyomarket-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Log.Information("Starting ZiyoMarket API on port {Port}", port);

// Add services to the container
builder.Services.AddControllers(options =>
{
    // ✅ Convert controller routes to snake_case (api/Auth -> api/auth, api/Product -> api/product)
    options.Conventions.Add(new Microsoft.AspNetCore.Mvc.ApplicationModels.RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

    // ✅ Add custom validation filter for consistent error responses
    options.Filters.Add<ValidationFilter>();
})
    .AddJsonOptions(options =>
    {
        // Use snake_case naming policy for JSON serialization
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();

        // Enum'larni string sifatida serialize qilish (1, 2, 3 emas, "Banner", "Video", "Article")
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        // Optional: Configure other JSON options
        options.JsonSerializerOptions.WriteIndented = false; // Compact JSON
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ✅ Disable automatic ModelState validation (we use ValidationFilter instead)
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
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

    // ✅ Support for file uploads in Swagger
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
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

// ✅ Parse Railway PostgreSQL URL format to Npgsql connection string
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    try
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
        Log.Information("✅ Converted Railway PostgreSQL URL to connection string");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to parse Railway PostgreSQL URL");
        throw;
    }
}

Log.Information("Using database connection: {ConnectionInfo}",
    connectionString.Contains("railway") || connectionString.Contains("gondola") ? "Railway PostgreSQL" : "Local PostgreSQL");

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
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = false,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCustomServices();

// ✅ Payment Services
builder.Services.Configure<ZiyoMarket.Service.DTOs.Payments.Click.ClickSettings>(builder.Configuration.GetSection("ClickSettings"));
builder.Services.AddScoped<ZiyoMarket.Service.Interfaces.IClickService, ZiyoMarket.Service.Services.ClickService>();

// ✅ File Upload Services
builder.Services.Configure<ZiyoMarket.Service.Helpers.FileUploadSettings>(builder.Configuration.GetSection("FileUploadSettings"));
builder.Services.AddScoped<ZiyoMarket.Service.Interfaces.IFileUploadService, ZiyoMarket.Service.Services.FileUploadService>();
builder.Services.AddHttpContextAccessor(); // Required for FileUploadService to get base URL

var app = builder.Build();

// ✅ Auto-run migrations on startup (Railway deployment)
try
{
    Log.Information("🗄️  Running database migrations...");
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ZiyoMarketDbContext>();

        // ✅ DIRECT FIX: Ensure ProductCategories table exists before any migration
        // This handles Railway where the table was never created
        try
        {
            dbContext.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""ProductCategories"" (
                    ""Id"" integer GENERATED BY DEFAULT AS IDENTITY,
                    ""ProductId"" integer NOT NULL,
                    ""CategoryId"" integer NOT NULL,
                    ""CreatedAt"" text NOT NULL,
                    ""UpdatedAt"" text,
                    ""DeletedAt"" text,
                    CONSTRAINT ""PK_ProductCategories"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_ProductCategories_Categories_CategoryId"" 
                        FOREIGN KEY (""CategoryId"") REFERENCES ""Categories"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_ProductCategories_Products_ProductId"" 
                        FOREIGN KEY (""ProductId"") REFERENCES ""Products"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_ProductCategories_ProductId"" ON ""ProductCategories"" (""ProductId"");
                CREATE INDEX IF NOT EXISTS ""IX_ProductCategories_CategoryId"" ON ""ProductCategories"" (""CategoryId"");
            ");
            Log.Information("✅ ProductCategories table ensured");
        }
        catch (Exception tableEx)
        {
            Log.Warning("⚠️ ProductCategories ensure failed: {Message}", tableEx.Message);
        }

        // ✅ Check pending migrations
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            Log.Information("📋 Found {Count} pending migrations: {Migrations}", 
                pendingMigrations.Count, string.Join(", ", pendingMigrations));
            
            try
            {
                dbContext.Database.Migrate();
                Log.Information("✅ Migrations completed successfully");
            }
            catch (Exception ex) when (ex.Message.Contains("42P07") || ex.Message.Contains("already exists"))
            {
                // Tables already exist but __EFMigrationsHistory is empty.
                // Mark ALL known migrations as applied, then retry to apply only newer ones.
                Log.Warning("⚠️  Migration failed because tables already exist. Fixing __EFMigrationsHistory...");

                try
                {
                    // Ensure the history table exists first
                    dbContext.Database.ExecuteSqlRaw(
                        "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (" +
                        "\"MigrationId\" character varying(150) NOT NULL, " +
                        "\"ProductVersion\" character varying(32) NOT NULL, " +
                        "CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY (\"MigrationId\"));");

                    // Mark all migrations that are known to already be applied in the DB
                    var migrationsToMark = new[]
                    {
                        "20251215182414_FreshInitialMigration",
                        "20260105181500_AddDeviceTokenTable",
                        "20260108080108_AddSmsLog",
                        "20260113071923_AddUnifiedUserRolePermissionSystem",
                        "20260130152620_AddProductCategoriesTable",
                        "20260306160001_AddProductCategoriesTableV2"
                    };

                    foreach (var migrationId in migrationsToMark)
                    {
                        dbContext.Database.ExecuteSqlRaw(
                            $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
                            $"VALUES ('{migrationId}', '9.0.10') ON CONFLICT DO NOTHING;");
                        Log.Information("✅ Marked migration as applied: {MigrationId}", migrationId);
                    }

                    Log.Information("✅ All existing migrations marked. Retrying remaining migrations...");

                    // Now apply any truly new migrations
                    dbContext.Database.Migrate();
                    Log.Information("✅ Remaining migrations completed successfully");
                }
                catch (Exception retryEx)
                {
                    Log.Error(retryEx, "❌ Failed to fix and retry migrations");
                }
            }
        }
        else
        {
            Log.Information("✅ No pending migrations");
        }
    }
}
catch (Exception ex)
{
    Log.Error(ex, "❌ General Migration error: {Message}", ex.Message);
    // Let app start anyway for debugging
}

// ✅ Global exception handler middleware (must be first)
app.UseMiddleware<GlobalExceptionMiddleware>();

// ✅ Swagger'ni har doim yoqamiz (nafaqat Development'da)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ZiyoMarket API v1");
    options.RoutePrefix = "swagger"; // => /swagger
});

// ✅ HTTPS redirection (disabled for Railway, enabled for local)
// Railway proxy handles HTTPS, but local dev needs it
// app.UseHttpsRedirection(); // Disabled - not required

// ✅ Static files serving (images, CSS, JS, etc.)
app.UseStaticFiles(); // Serves files from wwwroot folder

app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllers();

// ✅ Root'ga "API ishlayapti" degan test endpoint
app.MapGet("/", () => Results.Ok("🚀 ZiyoMarket API is running! Visit /swagger"));

// ✅ Diagnostic endpoint to see what tables exist (Temporary for debugging)
app.MapGet("/debug/tables", async (ZiyoMarketDbContext dbContext) =>
{
    try 
    {
        var tables = new List<string>();
        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;";
            await dbContext.Database.OpenConnectionAsync();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
        }
        return Results.Ok(new { count = tables.Count, tables });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

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
