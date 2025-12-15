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

### 3. Soft Delete Pattern

All entities inherit from `BaseEntity` which implements soft delete:

- Entities have `DeletedAt`, `CreatedAt`, `UpdatedAt` fields (stored as strings in "yyyy-MM-dd HH:mm:ss" format)
- Global query filter in `ZiyoMarketDbContext` automatically excludes soft-deleted records
- Use `entity.Delete()` for soft delete, not repository `Delete()` for hard delete
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

### 5. Snake Case JSON Serialization

The API uses snake_case for all JSON responses (not PascalCase or camelCase):

- C# property `FirstName` → JSON `first_name`
- C# property `TotalAmount` → JSON `total_amount`
- Custom `SnakeCaseNamingPolicy` configured in `Program.cs`
- This is transparent to developers - just use C# naming conventions in code

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

1. Create entity in appropriate `Domain/Entities/` subdirectory
2. Inherit from `BaseEntity`
3. Add `DbSet<NewEntity>` to `ZiyoMarketDbContext`
4. Create entity configuration class in `Data/Configurations/` if needed
5. Create migration and update database
6. Create corresponding DTOs in `Service/DTOs/`
7. Add AutoMapper profile mappings in `Service/Mapping/MappingProfiles.cs`
8. Create service interface and implementation
9. Register service in `ServiceExtension.cs`

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
2. Navigate to `https://localhost:5001/swagger`
3. No authentication needed for seed endpoints
4. Execute POST requests to seed specific data types

**Available Seed Endpoints:**
- POST `/api/seeddata/seed-all` - Seeds all data types at once (recommended for initial setup)
- Individual seed endpoints for specific entity types (check SeedDataController for complete list)

## API Structure

**Base URL:** `https://localhost:5001/api/`
**Swagger:** `https://localhost:5001/swagger`

### Main Controllers

- **AuthController** - Registration, login, token refresh
- **ProductController** - Product CRUD, search, QR code lookup, stock management
- **CategoryController** - Hierarchical category management
- **CartController** - Shopping cart operations
- **OrderController** - Order CRUD, status updates, order processing
- **CashbackController** - Cashback history, balance, expiring cashback
- **DeliveryController** - Delivery partners, tracking
- **SupportController** - Customer support chats
- **NotificationController** - Push notifications
- **ContentController** - CMS (blogs, news, FAQs)
- **ReportController** - Sales reports, analytics
- **CustomerController** - Customer management (Admin)
- **SellerController** - Seller management (Admin)
- **AdminController** - Admin user management

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

Use `GetPagedAsync` method from repository for paginated results:

```csharp
var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
    page: 1,
    pageSize: 20,
    filter: p => p.Status == ProductStatus.Active,
    orderBy: q => q.OrderByDescending(p => p.CreatedAt)
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
- `SoftDelete(entity)` - Soft delete (sets DeletedAt)
- `DeleteRange(entities)` - Delete multiple entities

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

### API Response Format

All API responses use snake_case JSON formatting. When testing with tools like Postman or Swagger:
- Request body properties should use snake_case: `{"first_name": "John"}`
- Response properties will be in snake_case: `{"user_id": 1, "created_at": "..."}`

### Available Repository Properties in UnitOfWork

When working with services, these repositories are available through `_unitOfWork`:
- `Customers`, `Sellers`, `Admins` - User management
- `Products`, `Categories`, `ProductLikes`, `CartItems` - Product catalog
- `Orders`, `OrderItems`, `OrderDiscounts`, `DiscountReasons`, `CashbackTransactions` - Order processing
- `DeliveryPartners`, `OrderDeliveries` - Delivery management
- `Notifications` - Push notifications
- `SupportChats`, `SupportMessages` - Customer support
- `Contents`, `SystemSettings`, `DailySalesSummaries` - System configuration
