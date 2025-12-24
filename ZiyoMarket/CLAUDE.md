# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ZiyoMarket is a professional multi-user e-commerce platform built with .NET 10.0 and PostgreSQL. It supports three user types (Customers, Sellers, Admins) with features including offline/online sales, cashback rewards, delivery tracking, and a comprehensive admin panel.

**Tech Stack:**
- .NET 10.0 (ASP.NET Core Web API)
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

### 3. Soft Delete Pattern

All entities inherit from `BaseEntity` which implements soft delete:

**CRITICAL:** Never use `IsDeleted` property in LINQ queries - it's a computed property that cannot be translated to SQL by EF Core. Always use `DeletedAt == null` for queries.

```csharp
// ❌ WRONG - Will throw runtime error
var products = await _unitOfWork.Products.FindAsync(p => !p.IsDeleted);

// ✅ CORRECT - Translates to SQL properly
var products = await _unitOfWork.Products.FindAsync(p => p.DeletedAt == null);
```

- Entities have `DeletedAt`, `CreatedAt`, `UpdatedAt` fields (stored as strings in "yyyy-MM-dd HH:mm:ss" format)
- Global query filter in `ZiyoMarketDbContext` automatically excludes soft-deleted records (where `DeletedAt == null`)
- Use `IsDeleted` property only for in-memory checks after loading entities
- Use repository `SoftDelete(entity)` or `SoftDeleteRange(entities)` for soft delete
- Use repository `Delete(entity)` or `DeleteAsync(id)` for hard delete (use cautiously)
- `SaveChangesAsync` override in DbContext automatically sets audit timestamps

### 4. API Controller Structure

Controllers inherit from `BaseController` and follow RESTful conventions:

- Use `[Authorize]` attribute for protected endpoints
- Return `ActionResult<T>` for type-safe responses
- Use standard HTTP status codes (200, 201, 400, 404, 401, 403)
- Controllers are thin - delegate all logic to services

**BaseController Helper Methods:**
```csharp
// Access current user information from JWT claims
int userId = GetCurrentUserId();           // Gets user ID from token
string userType = GetCurrentUserType();    // Gets user type (Customer/Seller/Admin)
bool isAuth = IsAuthenticated();           // Check if user is authenticated
```

### 5. Snake Case Convention (API URLs and JSON)

The API uses snake_case for both URL routes and JSON payloads:

**URL Routes:**
- Controller routes are automatically converted to snake_case via `SlugifyParameterTransformer`
- `/api/auth` (not `/api/Auth`), `/api/product` (not `/api/Product`)
- Both PascalCase and snake_case work due to ASP.NET Core's case-insensitive routing, but snake_case is standard

**JSON Serialization:**
- C# property `FirstName` → JSON `"first_name"`
- C# property `TotalAmount` → JSON `"total_amount"`
- Custom `SnakeCaseNamingPolicy` configured in `Program.cs`
- Request bodies should also use snake_case: `{"first_name": "John", "last_name": "Doe"}`
- This is transparent to developers - just use C# naming conventions in code, serialization handles conversion

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

# View migration SQL
dotnet ef migrations script --project ../ZiyoMarket.Data
```

**Note:** On Windows, use `cd src\ZiyoMarket.Api` and `..\ZiyoMarket.Data` with backslashes.

### Database Setup

```bash
# Create database in PostgreSQL
psql -U postgres
CREATE DATABASE "ZiyoDb";
\q

# Apply migrations
cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data

# Seed test data (optional - use Swagger seed endpoints)
# POST /api/seeddata/seed-all
```

### Deployment

For production deployment to Railway.app, see [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed instructions on:
- Setting up Railway projects
- Configuring PostgreSQL database
- Environment variables and secrets
- Build configuration and troubleshooting

## Configuration

### Connection Strings

Located in `src/ZiyoMarket.Api/appsettings.json`:

- **Local Development:** `Server=localhost;Database=ZiyoDb;User Id=postgres;Password=YOUR_PASSWORD;`
- **Production:** Currently configured for Render.com PostgreSQL (update for your environment)

**IMPORTANT:** Never commit sensitive connection strings or secrets. Use environment variables or user secrets for local development:

```bash
# Use user secrets (recommended for local dev)
dotnet user-secrets init --project src/ZiyoMarket.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING" --project src/ZiyoMarket.Api
```

### JWT Settings

JWT configuration in `appsettings.json`:
- **Access Token:** 1440 minutes (24 hours)
- **Refresh Token:** 7 days
- Secret key should be changed for production

## Key Domain Concepts

### User Types and Authorization

Three distinct user types with different capabilities:

1. **Customer** - End users who purchase products online
2. **Seller** - Staff who create offline orders in physical stores
3. **Admin** - System administrators with full access

**JWT Claims:** Token includes `UserId`, `Email`, `UserType` claims for authorization

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
2. Inherit from `BaseEntity` to get soft delete and audit fields
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
13. Create controller in `Api/Controllers/` inheriting from `BaseController`

### When Adding New Controllers

1. Inherit from `BaseController`
2. Use constructor injection for services
3. Add `[Authorize]` for protected endpoints
4. Return appropriate HTTP status codes
5. Use DTOs, never expose domain entities directly
6. Add XML comments for Swagger documentation

### Authentication Flow

1. **Register:** POST `/api/auth/register` with user details
2. **Login:** POST `/api/auth/login` returns `AccessToken` and `RefreshToken`
3. **Authorized Requests:** Include `Authorization: Bearer {AccessToken}` header
4. **Refresh Token:** POST `/api/auth/refresh-token` when access token expires

### Seed Data for Testing

The `SeedDataController` provides endpoints to populate test data for development:

**Usage in Swagger:**
1. Start the API (`dotnet run` in `src/ZiyoMarket.Api`)
2. Navigate to `http://localhost:8080/swagger`
3. No authentication needed for seed endpoints
4. Execute POST requests to seed specific data types

**Available Seed Endpoints:**
- POST `/api/seed_data/seed-all` - Seeds all data types at once (recommended for initial setup)
- Individual seed endpoints for specific entity types (check SeedDataController for complete list)

## API Structure

**Base URL (Development):** `http://localhost:8080/api/`
**Base URL (Production):** Configured via Railway/Docker
**Swagger UI:** `http://localhost:8080/swagger`

**Note:** The API runs on port 8080 by default (configurable via `PORT` environment variable for cloud deployment).

### Main Controllers (snake_case URLs)

All controller routes use snake_case format:

- **`/api/auth`** - Registration, login, token refresh, password reset
- **`/api/product`** - Product CRUD, search, QR code lookup, stock management
- **`/api/category`** - Hierarchical category management
- **`/api/cart`** - Shopping cart operations
- **`/api/order`** - Order CRUD, status updates, order processing
- **`/api/cashback`** - Cashback history, balance, expiring cashback
- **`/api/delivery`** - Delivery partners, tracking
- **`/api/support`** - Customer support chats
- **`/api/notification`** - Push notifications
- **`/api/content`** - CMS (blogs, news, FAQs)
- **`/api/report`** - Sales reports, analytics
- **`/api/customer`** - Customer management (Admin)
- **`/api/seller`** - Seller management (Admin)
- **`/api/admin`** - Admin user management
- **`/api/seed_data`** - Development seed data endpoints

## Common Patterns in This Codebase

### Error Handling

Services return DTOs or throw exceptions. Controllers should wrap service calls in try-catch:

```csharp
try
{
    var result = await _service.GetById(id);
    return Ok(result);
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
// items: IEnumerable<Product> - the page of results
// totalCount: int - total number of items matching the filter
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

**Create:**
- `InsertAsync(entity)` / `AddAsync(entity)` - Add single entity
- `AddRangeAsync(entities)` - Add multiple entities

**Read:**
- `GetByIdAsync(id, includes)` - Get by ID with optional eager loading
- `GetAllAsync()` - Get all entities
- `FindAsync(predicate)` - Find entities matching condition
- `FirstOrDefaultAsync(predicate)` - Get first match or null
- `SelectAsync(expression, includes)` - Select single with includes
- `SelectAllAsync(expression, includes)` - Select multiple with includes
- `ExistsAsync(predicate)` / `AnyAsync(expression)` - Check existence
- `CountAsync(predicate)` - Count entities

**Update:**
- `Update(entity, id)` - Update by ID
- `UpdateAsync(entity)` - Update entity
- `UpdateRange(entities)` - Update multiple entities

**Delete:**
- `DeleteAsync(id)` - Hard delete by ID
- `Delete(entity)` - Hard delete entity
- `DeleteRange(entities)` - Hard delete multiple entities
- `SoftDelete(entity)` - Soft delete (sets DeletedAt)
- `SoftDeleteRange(entities)` - Soft delete multiple entities

## Testing

**Default Admin Credentials:**
- Email: `admin@ziyomarket.uz`
- Password: `Admin@123`

**Swagger UI:** Use "Authorize" button with format: `Bearer {your-token-here}`

## Logging

Serilog is configured to log to:
- Console output
- File: `Logs/ziyomarket-log.txt` (daily rolling)

Add logging in services/controllers via constructor injection: `ILogger<ClassName>`

## Desktop Admin Panel

The solution includes a separate desktop admin panel project (`ZiyoMarket.AdminPanel`) for Windows desktop administration:

- Built with WPF (Windows Presentation Foundation)
- Consumes the ZiyoMarket API via HTTP client
- Provides admin dashboard, reports, and management interfaces
- Located in `src/ZiyoMarket.AdminPanel/`

**Run Admin Panel:**
```bash
cd src/ZiyoMarket.AdminPanel
dotnet run
```

## Important Development Notes

### API URL and Response Format

**All API endpoints and JSON use snake_case:**
- **URLs:** `/api/auth/login`, `/api/product/search`, `/api/order/create` (lowercase)
- **Request Bodies:** `{"first_name": "John", "last_name": "Doe", "phone_number": "+998..."}`
- **Response Bodies:** `{"user_id": 1, "created_at": "2025-01-24 10:30:00", "is_active": true}`

When testing with Postman/Swagger/curl, always use snake_case for property names in request bodies.

### Available Repository Properties in UnitOfWork

When working with services, these repositories are available through `_unitOfWork`:
- `Customers`, `Sellers`, `Admins` - User management
- `Products`, `Categories`, `ProductLikes`, `CartItems` - Product catalog
- `Orders`, `OrderItems`, `OrderDiscounts`, `DiscountReasons`, `CashbackTransactions` - Order processing
- `DeliveryPartners`, `OrderDeliveries` - Delivery management
- `Notifications` - Push notifications
- `SupportChats`, `SupportMessages` - Customer support
- `Contents`, `SystemSettings`, `DailySalesSummaries` - System configuration
