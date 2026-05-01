# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ZiyoMarket is a professional multi-user e-commerce platform built with ASP.NET Core 8.0 and PostgreSQL. It supports three user types (Customers, Sellers, Admins) with features including offline/online sales, cashback rewards, delivery tracking, and a comprehensive admin panel.

**Tech Stack:**
- ASP.NET Core 8.0 Web API
- Entity Framework Core 9.0 with PostgreSQL (Npgsql)
- JWT Authentication
- AutoMapper for object mapping
- Serilog for logging
- Swagger/OpenAPI for API documentation
- BCrypt.Net for password hashing

## Solution Structure

The solution follows Clean Architecture with 5 projects:

```
ZiyoMarket/
├── src/
│   ├── ZiyoMarket.Api/          # REST API Controllers, Program.cs, Extensions
│   ├── ZiyoMarket.Service/       # Business Logic Layer (Services, DTOs, Interfaces, Mapping)
│   ├── ZiyoMarket.Data/          # Data Access Layer (DbContext, Repositories, UnitOfWork)
│   ├── ZiyoMarket.Domain/        # Domain Layer (Entities, Enums, Common)
│   └── ZiyoMarket.AdminPanel/    # WPF Desktop Admin Panel
```

**Dependency Flow:** Api → Service → Data → Domain

## Architecture Patterns

### 1. Repository Pattern with Unit of Work

The codebase uses a generic repository pattern for all entities:

- **Generic Repository:** `Repository<T>` provides CRUD operations for all entities inheriting from `BaseEntity`
- **Unit of Work:** `IUnitOfWork` coordinates multiple repositories and manages transactions
- All database operations go through repositories, never direct DbContext access in services
- **Transaction Support:** UnitOfWork provides `BeginTransactionAsync()`, `CommitTransactionAsync()`, and `RollbackTransactionAsync()` for explicit transaction management

**Example Usage in Services:**
```csharp
// Simple operations
var product = await _unitOfWork.Products.GetByIdAsync(id);
await _unitOfWork.Products.InsertAsync(newProduct);
await _unitOfWork.SaveChangesAsync(); // Commit changes

// Complex transactions
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Orders.InsertAsync(order);
    await _unitOfWork.OrderItems.AddRangeAsync(orderItems);
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### 2. Service Layer Pattern

All business logic resides in service classes implementing interfaces:

- Services are registered in `ServiceExtension.cs`
- Each service handles one domain area (Products, Orders, Cashback, etc.)
- Services use DTOs (Data Transfer Objects) for input/output, not domain entities
- AutoMapper handles entity ↔ DTO transformations

**Service Registration:** All services are scoped and registered in `Api/Extensions/ServiceExtension.cs`

### 3. Entity Base Classes

Two base classes live in `Domain/Common/`:

- **`BaseEntity`** — provides `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt` (all timestamps stored as `"yyyy-MM-dd HH:mm:ss"` strings), `IsDeleted` computed property, and `Delete()`/`Restore()`/`MarkAsUpdated()` methods.
- **`BaseAuditableEntity : BaseEntity`** — adds `CreatedBy`, `UpdatedBy`, `DeletedBy` (user IDs). Most important domain entities (`Product`, `Order`, `Customer`, `Seller`, `Admin`, `Category`, etc.) use this class. Use `BaseAuditableEntity` when tracking who created/modified the record; use `BaseEntity` for supporting entities that don't need user tracking (e.g., `OrderItem`, `CartItem`).

**Soft Delete:**
- Global query filter in `ZiyoMarketDbContext` automatically excludes soft-deleted records
- Use repository `SoftDelete(entity)` or `SoftDeleteRange(entities)` for soft delete
- Use repository `Delete(entity)` or `DeleteAsync(id)` for hard delete (use cautiously)
- `SaveChangesAsync` override in DbContext automatically sets audit timestamps

### 4. API Controller Structure

Controllers inherit from `BaseController` and follow RESTful conventions:

- Use `[Authorize]` attribute for protected endpoints
- Return `ActionResult<T>` for type-safe responses
- Use standard HTTP status codes (200, 201, 400, 404, 401, 403)
- Controllers are thin - delegate all logic to services
- Most controllers live in `Controllers/<Domain>/` subdirectories, but `AuthController` and `SeedDataController` are at the `Controllers/` root level

**BaseController Helper Methods:**
```csharp
// Access current user information from JWT claims
int userId = GetCurrentUserId();           // Gets user ID from token
string userType = GetCurrentUserType();    // Gets user type (Customer/Seller/Admin)
bool isAuth = IsAuthenticated();           // Check if user is authenticated
```

### 5. Snake Case JSON Serialization

The API uses snake_case for all JSON responses (not PascalCase or camelCase):

- C# property `FirstName` → JSON `first_name`
- C# property `TotalAmount` → JSON `total_amount`
- Custom `SnakeCaseNamingPolicy` and `SnakeCaseSchemaFilter` configured in `Program.cs` (both in `Api/Helpers/`)
- **Important:** `PropertyNameCaseInsensitive = false` — request body properties must use exact snake_case, no case flexibility
- Swagger UI also renders property names in snake_case (via `SnakeCaseSchemaFilter`)

## File Naming Conventions

Some legacy Service layer files use a numeric prefix. Do not create new files with this pattern:

- `Service/Interfaces/11_Interfaces_IAuthService.cs` — auth service interface (edit in-place)
- `Service/Interfaces/12_Interfaces_IProductService.cs` — product service interface (edit in-place)
- `Service/Interfaces/13_Interfaces_IOrderCashbackCartServices.cs` — **empty stub**, do not use; Order/Cart/Cashback interfaces are in their own unnumbered files (`IOrderService.cs`, `ICartService.cs`, `ICashbackService.cs`)
- `Service/Validators/14_Validators_FluentValidators.cs` — **all** FluentValidation validators (edit in-place)
- `Service/Services/17_Services_ProductService.cs` — product service implementation (edit in-place)

**New interfaces and services:** Use normal naming (`IFooService.cs`, `FooService.cs`) — the numbered pattern is legacy.

When adding new validators, add them to `14_Validators_FluentValidators.cs`. Do not create separate validator files.

## Common Development Commands

### Build and Run

```bash
# Build the solution
dotnet build

# Run the API (from src/ZiyoMarket.Api)
cd src/ZiyoMarket.Api
dotnet run

# Clean build artifacts
dotnet clean
```

### Auto-Migration on Startup

`Program.cs` automatically runs `db.Database.MigrateAsync()` and `DataSeeder.SeedAsync()` on every startup. This means:
- Pending migrations are applied automatically when the API starts
- Default seed data (SystemSettings, Categories, DiscountReasons, DeliveryPartners, Admin user) is inserted if not present
- Manual `dotnet ef database update` is only needed if you want to apply migrations without starting the API

### Database Migrations

**IMPORTANT:** Always run EF Core commands from `src/ZiyoMarket.Api` directory, specifying the Data project:

```bash
# Create a new migration
cd src/ZiyoMarket.Api
dotnet ef migrations add MigrationName --project ../ZiyoMarket.Data

# Apply migrations to database
dotnet ef database update --project ../ZiyoMarket.Data

# Remove last migration (if not applied)
dotnet ef migrations remove --project ../ZiyoMarket.Data
```

**Note:** On Windows, use backslashes: `cd src\ZiyoMarket.Api` and `..\ZiyoMarket.Data`.

### Database Setup

```bash
psql -U postgres
CREATE DATABASE "ZiyoDb";
\q

cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data
```

### Deployment

For production deployment to Railway.app, see [DEPLOYMENT.md](./DEPLOYMENT.md).

## Configuration

### Connection Strings

Located in `src/ZiyoMarket.Api/appsettings.json`.

**WARNING:** The current `appsettings.json` has a Railway.app production connection string committed to source control. For local development, override it using user secrets:

```bash
dotnet user-secrets init --project src/ZiyoMarket.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ZiyoDb;User Id=postgres;Password=YOUR_PASSWORD;" --project src/ZiyoMarket.Api
```

### JWT Settings

JWT configuration in `appsettings.json`:
- **Access Token:** 1440 minutes (24 hours)
- No refresh token endpoint — users must re-login when the token expires
- Secret key should be changed for production

## Key Domain Concepts

### User Types and Authorization

Three distinct user types with different capabilities:

1. **Customer** - End users who purchase products online
2. **Seller** - Staff who create offline orders in physical stores
3. **Admin** - System administrators with full access

**JWT Claims:** Token includes `UserId`, `Email`, `UserType` claims for authorization

### Product Entity Extended Fields

The `Product` entity (which targets a bookstore context) has fields beyond typical e-commerce products:
- **Book metadata:** `Publisher`, `PublishYear`, `PageCount`, `Language`, `Edition`
- **Multiple images:** `ImageUrls` (List<string>) in addition to the primary `ImageUrl`
- **Multiple categories:** `CategoryIds` (List<int>) in addition to the primary `CategoryId`
- **Other:** `Barcode`, `Manufacturer`, `Weight`, `Dimensions`, `DisplayOrder`, `SearchText`
- Business methods on the entity handle stock changes and auto-update `Status` (Active/OutOfStock)

### Cashback System

- 2% cashback on delivered orders
- Cashback transactions tracked in `CashbackTransaction` entity
- Types: Earned (from orders), Used (in purchases), Expired
- Cashback can be used as payment method in orders

### Order Workflow (7 States)

```
Pending → Confirmed → Preparing → ReadyForPickup/Shipped → Delivered/Cancelled
```

**Critical:** When order status changes to `Delivered`, trigger cashback calculation (2% of order total)

### Online vs Offline Orders

- **Online Orders:** Created by customers via mobile app (`OrderType.Online`)
- **Offline Orders:** Created by sellers in physical stores (`OrderType.Offline`)
- Offline orders skip certain workflow steps

## Important Implementation Notes

### When Adding New Entities

1. Create entity in appropriate `Domain/Entities/` subdirectory (Users, Products, Orders, etc.)
2. Inherit from `BaseAuditableEntity` if you need to track who created/modified the record; inherit from `BaseEntity` for simpler supporting entities
3. Add `DbSet<NewEntity>` to `ZiyoMarketDbContext.cs`
4. Create entity configuration class in `Data/Configurations/` if needed (for complex mappings)
5. Create migration: `dotnet ef migrations add AddNewEntity --project ../ZiyoMarket.Data`
6. Update database: `dotnet ef database update --project ../ZiyoMarket.Data`
7. Create corresponding DTOs in `Service/DTOs/` (CreateDto, UpdateDto, ResultDto)
8. Add AutoMapper profile mappings in `Service/Mapping/MappingProfiles.cs`
9. Create service interface in `Service/Interfaces/INewEntityService.cs`
10. Create service implementation in `Service/Services/NewEntityService.cs`
11. Add repository property to `IUnitOfWork` interface and `UnitOfWork` class
12. Register service in `Api/Extensions/ServiceExtension.cs`
13. Create controller in the appropriate `Api/Controllers/<Domain>/` subdirectory inheriting from `BaseController`

### Authentication Flow

**Public endpoints (no auth required):**
- `POST /api/auth/login` — login with `{phone_or_email, password, user_type}`; returns `AccessToken`. Admin uses username or email; Customer/Seller use phone or email.
- `POST /api/auth/register` — universal registration with `{first_name, last_name, phone, password, user_type, username?, address?}`. Customer: open. Admin/Seller: requires Admin JWT.
- `POST /api/auth/password-reset/request` — request password reset code
- `POST /api/auth/password-reset/confirm` — confirm reset with code and new password
- `POST /api/auth/verification/send` — send verification code to phone/email
- `POST /api/auth/verification/verify` — verify the code
- `POST /api/auth/validate-token` — validate a JWT token
- `POST /api/auth/dev/create-admin` — create admin (Development environment only)

**Protected endpoints (Bearer token required):**
- `GET /api/auth/profile` — get current user profile
- `POST /api/auth/logout` — logout current user
- `POST /api/auth/change-password` — change password
- `POST /api/auth/change-role` — move user between roles (Admin only)

**Authorized Requests:** Include `Authorization: Bearer {AccessToken}` header

**Logout** is stateless — JWT is removed client-side. Server-side logout only logs the action.

**Password reset / verification codes** are currently printed to the console (`Console.WriteLine`) only — SMS/Email integration is not yet implemented (marked as TODO in `AuthService`). When integrating a real SMS provider, update `RequestPasswordResetAsync` and `SendVerificationCodeAsync` in `AuthService.cs`.

### Seed Data for Testing

The `SeedDataController` provides endpoints to populate test data for development (no authentication required):

- `POST /api/seeddata/seed-all` — seeds all data types at once (recommended for initial setup)
- Individual endpoints for specific entity types (check `Controllers/SeedDataController.cs` for the full list)

## API Structure

**Base URL:** `https://localhost:5001/api/`
**Swagger:** `https://localhost:5001/swagger` (always enabled, not just in Development)
**Health Check:** `GET /health` — checks database connectivity

### Main Controllers

- **ProductController** — Product CRUD, search, QR code lookup, stock management
- **CategoryController** — Hierarchical category management
- **CartController** — Shopping cart operations
- **OrderController** — Order CRUD, status updates, order processing
- **CashbackController** — Cashback history, balance, expiring cashback
- **DeliveryController** — Delivery partners, tracking
- **SupportController** — Customer support chats
- **NotificationController** / **NotificationApiController** — Push notifications (two controllers in `Controllers/Notifications/`)
- **ContentController** — CMS (blogs, news, FAQs)
- **ReportController** — Sales reports, analytics
- **SellerController** — Seller management (Admin only)
- **AdminController** — Admin user management (**SuperAdmin role required**)
- **CustomerController** — Customer management (Admin/SuperAdmin only): CRUD, search, toggle-status, `GET/{id}/orders`
- **ImageUploadController** — `POST /api/image_upload?type=product|category` — uploads to `wwwroot/uploads/{type}/{year}/{month}/`, max 5MB, allowed: jpg/jpeg/png/gif/webp; returns `{ file_path, full_url }`

## Common Patterns in This Codebase

### Custom Exceptions

Use typed exceptions from `Service/Exceptions/` instead of throwing generic exceptions:

- `NotFoundException(entityName, id)` — 404
- `BusinessRuleException(message)` — 400
- `DuplicateException(entityName, field, value)` — 409
- `UnauthorizedException(message)` — 401
- `ForbiddenException(message)` — 403
- `InsufficientStockException(productName, requested, available)` — 400
- `InsufficientCashbackException(requested, available)` — 400
- `OrderCannotBeCancelledException(orderId, currentStatus)` — 400
- `ExternalServiceException(serviceName, message)` — 503

### Result Pattern

There are two `Result` implementations — use the **Service layer** one in services:

- **`ZiyoMarket.Service.Results.Result<T>`** — used in services. Has `.Data` property for the returned value, `.StatusCode`, and factory methods: `Success(data)`, `Failure(error)`, `NotFound(msg)`, `Unauthorized(msg)`, `Forbidden(msg)`, `Conflict(msg)`.
- **`ZiyoMarket.Domain.Common.Result<T>`** — simpler version with `.Value` property; rarely needed in service/controller code.

```csharp
// In service (use Service layer Result)
return Result<ProductDto>.Success(dto);
return Result<ProductDto>.NotFound("Product not found");

// Access data in controller
var result = await _service.GetByIdAsync(id);
if (!result.IsSuccess) return NotFound(result.Message);
return Ok(result.Data);   // .Data, not .Value

// PaginationResponse
return new PaginationResponse<ProductDto>(items, totalCount, page, pageSize);
```

### FluentValidation

All validators are in **one file**: `Service/Validators/14_Validators_FluentValidators.cs`. Add new validators to that same file — do not create separate validator files. Existing validators: `LoginRequestValidator`, `RegisterCustomerValidator`, `CreateProductValidator`, `UpdateProductValidator`, `CreateOrderValidator`, `UpdateStockValidator`, `ApplyDiscountValidator`.

### TimeHelper

Use `TimeHelper.GetCurrentServerTime()` (in `Service/Helpers/TimeHelper.cs`) when you need the local server timestamp. It returns `DateTime.UtcNow + 5 hours` (Tashkent/UTC+5). Use this consistently in business logic instead of `DateTime.UtcNow` directly.

### Error Handling

Services return DTOs or throw custom exceptions. Controllers should wrap service calls in try-catch:

```csharp
try
{
    var result = await _service.GetById(id);
    return Ok(result);
}
catch (NotFoundException ex)
{
    return NotFound(ex.Message);
}
catch (Exception ex)
{
    return BadRequest(ex.Message);
}
```

### Pagination

Use `GetPagedAsync` method from repository for paginated results. Returns a tuple with items and total count:

```csharp
var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
    page: 1,              // Page number (1-based)
    pageSize: 20,         // Items per page
    filter: p => p.Status == ProductStatus.Active,  // Optional filter
    orderBy: q => q.OrderByDescending(p => p.CreatedAt)  // Optional sorting
);
```

### Eager Loading Relationships

Use `includes` parameter for loading related entities:

```csharp
var order = await _unitOfWork.Orders.GetByIdAsync(
    id,
    includes: new[] { "OrderItems.Product", "Customer", "OrderDelivery" }
);
```

### Common Repository Methods

All repositories inherit from `Repository<T>` and provide these methods:

**Create:** `InsertAsync(entity)` / `AddAsync(entity)`, `AddRangeAsync(entities)`

**Read:** `GetByIdAsync(id, includes)`, `GetAllAsync()`, `FindAsync(predicate)`, `FirstOrDefaultAsync(predicate)`, `SelectAsync(expression, includes)`, `SelectAllAsync(expression, includes)`, `ExistsAsync(predicate)` / `AnyAsync(expression)`, `CountAsync(predicate)`

**Update:** `Update(entity, id)`, `UpdateAsync(entity)`, `UpdateRange(entities)`

**Delete:** `DeleteAsync(id)`, `Delete(entity)`, `DeleteRange(entities)`, `SoftDelete(entity)`, `SoftDeleteRange(entities)`

### Available Repository Properties in UnitOfWork

- `Customers`, `Sellers`, `Admins` — User management
- `Products`, `Categories`, `ProductLikes`, `CartItems` — Product catalog
- `Orders`, `OrderItems`, `OrderDiscounts`, `DiscountReasons`, `CashbackTransactions` — Order processing
- `DeliveryPartners`, `OrderDeliveries` — Delivery management
- `Notifications` — Push notifications
- `SupportChats`, `SupportMessages` — Customer support
- `Contents`, `SystemSettings`, `DailySalesSummaries` — System configuration

## Testing

**Default SuperAdmin Credentials** (seeded automatically on first startup):
- Username: `Bek`
- Phone: `+998882641919`
- Password: `2641919`
- UserType: `Admin`

Login via: `POST /api/auth/login` with `{"phone_or_email": "Bek", "password": "2641919", "user_type": "Admin"}`

**Note:** Admin login uses username or email (not phone). Customer/Seller login uses phone or email.

**Swagger UI:** Use "Authorize" button with format: `Bearer {your-token-here}`

## Logging

Serilog logs to console output and `Logs/ziyomarket-log.txt` (daily rolling).

Add logging via constructor injection: `ILogger<ClassName>`

## Desktop Admin Panel

WPF desktop application in `src/ZiyoMarket.AdminPanel/` that consumes the ZiyoMarket API via HTTP client.

```bash
cd src/ZiyoMarket.AdminPanel
dotnet run
```
