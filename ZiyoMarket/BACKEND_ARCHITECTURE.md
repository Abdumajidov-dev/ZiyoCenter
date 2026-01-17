# ZiyoMarket Backend API Architecture

**Generated**: 2026-01-12
**Purpose**: Complete backend architecture documentation for Claude Code

## Project Overview

**ZiyoMarket Backend API** - E-commerce platform REST API built with **ASP.NET Core 8.0**, **PostgreSQL**, **Entity Framework Core**, and **JWT Authentication**.

**Technology Stack:**
- ASP.NET Core 8.0 Web API
- PostgreSQL 15+ (Database)
- Entity Framework Core 9.0.10
- JWT Bearer Authentication
- Serilog (Logging)
- AutoMapper 12.0.1
- Swagger/OpenAPI
- Firebase Admin SDK (Push Notifications)
- Bogus (Test Data Generation)

## Quick Start Commands

### Development (Local)

```bash
# Navigate to backend root
cd /c/Users/abdum/OneDrive/Desktop/Kutubxona/ZiyoMarket

# Restore dependencies
dotnet restore

# Run PostgreSQL (Docker)
docker run --name ziyomarket-postgres -e POSTGRES_PASSWORD=2001 -p 5432:5432 -d postgres:15

# Run migrations
cd src/ZiyoMarket.Api
dotnet ef database update

# Run API (Port 8080)
dotnet run
# API will run on http://localhost:8080
# Swagger: http://localhost:8080/swagger
```

### Build

```bash
# Build entire solution
dotnet build ZiyoMarket.sln

# Build only API project
dotnet build src/ZiyoMarket.Api/ZiyoMarket.Api.csproj
```

### Migrations

```bash
# Create new migration (from API directory)
cd src/ZiyoMarket.Api
dotnet ef migrations add MigrationName --project ../ZiyoMarket.Data

# Apply migrations
dotnet ef database update --project ../ZiyoMarket.Data

# Remove last migration
dotnet ef migrations remove --project ../ZiyoMarket.Data

# Generate SQL script
dotnet ef migrations script --project ../ZiyoMarket.Data --output migration.sql
```

## Solution Structure

```
ZiyoMarket/
├── src/
│   ├── ZiyoMarket.Api/          # Presentation Layer (REST API)
│   │   ├── Controllers/         # API Controllers
│   │   │   ├── AuthController.cs
│   │   │   ├── Products/        # Product endpoints
│   │   │   ├── Orders/          # Order endpoints
│   │   │   ├── Users/           # User management
│   │   │   ├── Cart/            # Shopping cart
│   │   │   ├── Cashback/        # Cashback system
│   │   │   ├── Content/         # Content management
│   │   │   ├── Delivery/        # Delivery partners
│   │   │   ├── Notifications/   # Push notifications
│   │   │   ├── Reports/         # Reports & analytics
│   │   │   ├── Sms/             # SMS integration
│   │   │   └── Support/         # Customer support
│   │   ├── Extensions/          # Service registration extensions
│   │   ├── Helpers/             # Helper classes
│   │   ├── Middleware/          # Custom middleware
│   │   ├── Program.cs           # Application entry point
│   │   └── appsettings.json     # Configuration
│   │
│   ├── ZiyoMarket.Service/      # Business Logic Layer
│   │   ├── Services/            # Service implementations
│   │   ├── Interfaces/          # Service contracts
│   │   ├── DTOs/                # Data Transfer Objects
│   │   ├── Mapping/             # AutoMapper profiles
│   │   ├── Validators/          # FluentValidation
│   │   ├── Results/             # Result pattern
│   │   └── Exceptions/          # Custom exceptions
│   │
│   ├── ZiyoMarket.Data/         # Data Access Layer
│   │   ├── Context/             # DbContext
│   │   ├── Repositories/        # Repository implementations
│   │   ├── IRepositories/       # Repository interfaces
│   │   ├── UnitOfWorks/         # Unit of Work pattern
│   │   ├── Migrations/          # EF Core migrations
│   │   └── Seed/                # Database seeding
│   │
│   └── ZiyoMarket.Domain/       # Domain Layer
│       └── Entities/            # Domain entities
│
├── ZiyoMarket.sln               # Solution file
├── CLAUDE.md                    # Main documentation
├── BACKEND_ARCHITECTURE.md      # This file
└── migration.sql                # Latest migration script
```

## Architecture Layers

### 1. Domain Layer (`ZiyoMarket.Domain`)

**Purpose**: Core business entities and domain models.

**Key Entities**:
- User (Admin, Seller, Customer roles)
- Product, Category
- Order, OrderItem
- Cart, CartItem
- Cashback, CashbackTransaction
- Content (Banner, Article, FAQ, Video)
- Delivery, DeliveryPartner
- Notification
- Support, SupportMessage

### 2. Data Layer (`ZiyoMarket.Data`)

**Purpose**: Database access, EF Core configuration, repository pattern.

**Patterns**:
- Repository Pattern
- Unit of Work Pattern
- DbContext: `ZiyoMarketDbContext`

**Database**: PostgreSQL
**Connection String** (Local):
```
Server=localhost;Database=ZiyoNoorDb;User Id=postgres;Password=2001;Port=5432;
```

### 3. Service Layer (`ZiyoMarket.Service`)

**Purpose**: Business logic, DTOs, validators, mappings.

**Services**:
- `IAuthService` - Authentication, JWT tokens
- `IProductService` - Product management
- `ICategoryService` - Category management
- `IOrderService` - Order processing
- `IUserService` - User management
- `ICartService` - Shopping cart
- `ICashbackService` - Cashback system
- `IContentService` - Content management
- `IDeliveryService` - Delivery management
- `INotificationService` - Push notifications
- `IReportService` - Reports & analytics
- `ISupportService` - Customer support

**DTOs**: Located in `ZiyoMarket.Service/DTOs/`
**Validators**: FluentValidation for input validation
**Mapping**: AutoMapper profiles in `ZiyoMarket.Service/Mapping/`

### 4. API Layer (`ZiyoMarket.Api`)

**Purpose**: HTTP endpoints, Swagger, JWT middleware.

**Port**: 8080 (configurable via `PORT` environment variable)
**Base URL**: `http://localhost:8080/api/`
**Swagger**: `http://localhost:8080/swagger`

## API Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ZiyoNoorDb;User Id=postgres;Password=2001;Port=5432;"
  },
  "JwtSettings": {
    "SecretKey": "SuperSecretKeyForJwtDontShare123!",
    "Issuer": "ZiyoMarket",
    "Audience": "ZiyoMarketUsers",
    "AccessTokenExpirationMinutes": 1440
  },
  "EskizSms": {
    "BaseUrl": "https://notify.eskiz.uz/api",
    "Email": "your-email@example.com",
    "Password": "your-password",
    "IsDevelopment": true
  }
}
```

### Program.cs Key Features

1. **Port Configuration**: Reads `PORT` environment variable (default: 8080)
2. **Serilog Logging**: Logs to console and `Logs/ziyomarket-log.txt`
3. **Snake Case Convention**: All routes use snake_case (`api/product` not `api/Product`)
4. **Snake Case JSON**: All JSON properties use snake_case
5. **Enum as String**: Enums serialized as strings, not numbers
6. **JWT Authentication**: Bearer token authentication
7. **Swagger Always Enabled**: Even in production
8. **Auto Migrations**: Runs migrations on startup in production
9. **Health Checks**: `/health` and `/health/detailed` endpoints

## API Endpoints

All routes are **snake_case** due to `SlugifyParameterTransformer`:
- `api/Auth` → `api/auth`
- `api/Product` → `api/product`
- `api/Category` → `api/category`

### Authentication (`/api/auth`)

- `POST /api/auth/login` - Login (returns JWT token)
- `POST /api/auth/refresh-token` - Refresh expired token
- `POST /api/auth/register` - Register new user
- `POST /api/auth/logout` - Logout

### Products (`/api/product`)

- `GET /api/product` - Get products (pagination, filters)
- `GET /api/product/{id}` - Get product by ID
- `POST /api/product` - Create product [Admin]
- `PUT /api/product/{id}` - Update product [Admin]
- `DELETE /api/product/{id}` - Delete product [Admin]

### Categories (`/api/category`)

- `GET /api/category` - Get categories
- `GET /api/category/{id}` - Get category by ID
- `GET /api/category/tree` - Get hierarchical tree
- `POST /api/category` - Create category [Admin]
- `PUT /api/category/{id}` - Update category [Admin]
- `DELETE /api/category/{id}` - Delete category [Admin]

### Orders (`/api/order`)

- `GET /api/order` - Get orders (filters)
- `GET /api/order/{id}` - Get order details
- `POST /api/order` - Create order [Customer]
- `PUT /api/order/{id}/status` - Update status [Admin, Seller]
- `POST /api/order/{id}/confirm` - Confirm order [Seller]
- `POST /api/order/{id}/cancel` - Cancel order

### Users (`/api/user`)

- `GET /api/user` - Get users [Admin]
- `GET /api/user/{id}` - Get user by ID
- `POST /api/user` - Create user [Admin]
- `PUT /api/user/{id}` - Update user [Admin]
- `DELETE /api/user/{id}` - Delete user [Admin]

### Reports (`/api/report`)

- `GET /api/report/dashboard` - Dashboard statistics
- `GET /api/report/sales` - Sales report
- `GET /api/report/inventory` - Inventory report
- `GET /api/report/products/top` - Top selling products

### Cart (`/api/cart`)

- `GET /api/cart` - Get user cart
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update` - Update cart item
- `DELETE /api/cart/remove` - Remove item from cart
- `DELETE /api/cart/clear` - Clear cart

### Cashback (`/api/cashback`)

- `GET /api/cashback/balance` - Get user cashback balance
- `GET /api/cashback/transactions` - Get transactions
- `POST /api/cashback/apply` - Apply cashback to order

### Content (`/api/content`)

- `GET /api/content` - Get content (banners, articles, FAQs)
- `GET /api/content/type/{type}` - Get by type
- `POST /api/content` - Create content [Admin]
- `PUT /api/content/{id}` - Update content [Admin]
- `DELETE /api/content/{id}` - Delete content [Admin]

### Delivery (`/api/delivery`)

- `GET /api/delivery/partners` - Get delivery partners
- `POST /api/delivery/partners` - Create partner [Admin]
- `PUT /api/delivery/{orderId}/status` - Update delivery status
- `POST /api/delivery/{orderId}/assign` - Assign partner

### Notifications (`/api/notification`)

- `GET /api/notification` - Get user notifications
- `GET /api/notification/unread-count` - Get unread count
- `POST /api/notification/{id}/mark-read` - Mark as read
- `POST /api/notification/send` - Send push notification [Admin]

### Support (`/api/support`)

- `GET /api/support` - Get support chats
- `GET /api/support/{id}` - Get chat details
- `POST /api/support/{chatId}/message` - Send message
- `POST /api/support/{chatId}/close` - Close chat [Admin]

### SMS (`/api/sms`)

- `POST /api/sms/send` - Send SMS (Eskiz.uz integration)
- `GET /api/sms/status/{id}` - Check SMS status

## Important Implementation Details

### 1. Snake Case Convention

**Routes and JSON** both use snake_case:

```csharp
// Program.cs
options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
```

**Example**:
- Route: `api/product/get-by-category` (not `api/Product/GetByCategory`)
- JSON: `{"product_name": "Laptop", "is_active": true}`

### 2. JWT Authentication

**Token Generation** in `AuthService`:
- AccessToken expires in 1440 minutes (24 hours)
- Tokens include: `UserId`, `Email`, `Role`, `FullName`

**Token Validation**:
- Currently **disabled** for development: `ValidateIssuer = false`, `ValidateLifetime = false`
- **Production**: Enable validation

### 3. Database Migrations

**Auto-run on startup** (Production only):
```csharp
if (app.Environment.IsProduction())
{
    dbContext.Database.Migrate();
}
```

**Manual migrations**:
```bash
cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data
```

### 4. Railway Deployment Support

Program.cs automatically handles Railway PostgreSQL URL format:
```csharp
// Converts: postgresql://user:pass@host:port/db
// To: Host=host;Port=port;Database=db;Username=user;Password=pass;SSL Mode=Require;
```

### 5. CORS

Currently **not configured**. Add if needed:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

### 6. Firebase Push Notifications

Firebase Admin SDK configured in `firebase-service-account.json`.

### 7. Eskiz SMS Integration

SMS service configured in `appsettings.json` → `EskizSms`.

## Common Development Tasks

### Add New Controller

1. Create controller in `ZiyoMarket.Api/Controllers/`
2. Inherit from `BaseController` or `ControllerBase`
3. Add `[ApiController]` and `[Route("api/[controller]")]`
4. Inject services via constructor
5. Controller auto-discovered (no registration needed)

### Add New Service

1. Create interface in `ZiyoMarket.Service/Interfaces/`
2. Implement in `ZiyoMarket.Service/Services/`
3. Register in `Extensions/ServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<IMyService, MyService>();
```

### Add New Entity

1. Create entity in `ZiyoMarket.Domain/Entities/`
2. Add `DbSet<T>` to `ZiyoMarketDbContext`
3. Configure in `OnModelCreating()` if needed
4. Create migration:
```bash
cd src/ZiyoMarket.Api
dotnet ef migrations add AddMyEntity --project ../ZiyoMarket.Data
dotnet ef database update --project ../ZiyoMarket.Data
```

### Add New Repository

1. Create interface in `ZiyoMarket.Data/IRepositories/`
2. Implement in `ZiyoMarket.Data/Repositories/`
3. Register in `UnitOfWork` or DI container

## Testing Backend

### Health Checks

```bash
# Simple health check
curl http://localhost:8080/health

# Detailed (includes DB check)
curl http://localhost:8080/health/detailed
```

### Swagger

Open browser: `http://localhost:8080/swagger`

### Test Login

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@ziyomarket.uz",
    "password": "Admin@123"
  }'
```

## Integration with WPF Admin Panel

**WPF Client Location**: `/c/Users/abdum/OneDrive/Desktop/AdminPanel/ZiyoNurAdminPanel.Ui`

**WPF Configuration** (`appsettings.json`):
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080/api/"
  }
}
```

**Test Credentials**:
- Admin: `admin@ziyomarket.uz` / `Admin@123`

## Troubleshooting

### Port Already in Use

```bash
# Find process on port 8080
netstat -ano | findstr :8080

# Kill process
taskkill /PID <process_id> /F
```

### PostgreSQL Connection Issues

```bash
# Check if PostgreSQL is running
docker ps

# Restart PostgreSQL
docker restart ziyomarket-postgres

# View logs
docker logs ziyomarket-postgres
```

### Migration Errors

```bash
# Ensure correct directory
cd src/ZiyoMarket.Api

# Always use --project flag
dotnet ef database update --project ../ZiyoMarket.Data

# Drop database (WARNING: Deletes all data)
dotnet ef database drop --project ../ZiyoMarket.Data
```

### Swagger Not Loading

1. Check `app.UseSwagger()` is before `app.UseAuthentication()`
2. Verify `RoutePrefix = "swagger"` in `UseSwaggerUI()`
3. Access via `http://localhost:8080/swagger` (not `/swagger/index.html`)

## Best Practices

1. **Always use UTC** for DateTime values (PostgreSQL compatibility)
2. **Validate inputs** using FluentValidation
3. **Use DTOs** - Never expose entities directly
4. **Use AutoMapper** for entity ↔ DTO mapping
5. **Log errors** with Serilog
6. **Use async/await** for all I/O operations
7. **Return ApiResponse<T>** for consistent responses
8. **Use Repository pattern** for data access
9. **Use Unit of Work** for transactions
10. **Test with Swagger** before integrating with WPF

## Next Steps

1. **Start PostgreSQL**: `docker run --name ziyomarket-postgres -e POSTGRES_PASSWORD=2001 -p 5432:5432 -d postgres:15`
2. **Run Migrations**: `cd src/ZiyoMarket.Api && dotnet ef database update --project ../ZiyoMarket.Data`
3. **Start Backend**: `dotnet run` (from `src/ZiyoMarket.Api`)
4. **Test Health**: `curl http://localhost:8080/health`
5. **Open Swagger**: `http://localhost:8080/swagger`
6. **Start WPF Client**: (from ZiyoNurAdminPanel.Ui directory) `dotnet run`

---

**Last Updated**: 2026-01-12
**Maintained By**: Claude Code
**Purpose**: Quick reference for backend modifications
