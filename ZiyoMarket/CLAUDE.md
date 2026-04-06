# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ZiyoMarket is a professional multi-user e-commerce platform built with .NET 8.0 and PostgreSQL. It supports three user types (Customers, Sellers, Admins) with features including offline/online sales, cashback rewards, delivery tracking, and a comprehensive admin panel.

**Tech Stack:**
- .NET 8.0 (ASP.NET Core Web API)
- Entity Framework Core 8.0 with PostgreSQL (Npgsql)
- JWT Authentication
- AutoMapper for object mapping
- Serilog for logging
- Swagger/OpenAPI for API documentation
- BCrypt.Net for password hashing
- **Firebase Admin SDK** for push notifications (FCM)

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

// Unified response helpers (returns ApiResponse<T> format)
return SuccessResponse(data, "Success message");     // Success with data
return SuccessResponse("Success message");           // Success without data
return ErrorResponse("Error message");               // Error response
return HandleResult(serviceResult);                  // Auto-handle service Result<T>
```

**Unified Response Format:**
All API responses follow a consistent format with `status`, `message`, and `data` fields:
```json
{
  "status": true,
  "message": "Success message",
  "data": { /* actual data here */ }
}
```
See `API_RESPONSE_FORMAT.md` for complete documentation.

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

**On Windows (Current Environment):**
```bash
# Build the solution
dotnet build

# Run the API
cd src\ZiyoMarket.Api
dotnet run

# Clean build artifacts (close Visual Studio first to avoid DLL locks)
dotnet clean
```

**On Linux/Mac:**
```bash
# Build the solution
dotnet build

# Run the API
cd src/ZiyoMarket.Api
dotnet run

# Clean build artifacts
dotnet clean
```

**Important for Windows:** If you encounter "file is being used by another process" errors during build/migration commands:
1. Close Visual Studio completely
2. Stop all running API instances (check Task Manager for `dotnet.exe` processes)
3. Wait a few seconds for processes to fully terminate
4. Run the command again

This is a common issue on Windows where Visual Studio or IIS Express locks DLL files.

### Database Migrations

**IMPORTANT:** Always run EF Core commands from `src/ZiyoMarket.Api` directory, specifying the Data project.

**On Windows (Current Environment):**
```bash
# Navigate to API directory
cd src\ZiyoMarket.Api

# Create a new migration
dotnet ef migrations add MigrationName --project ..\ZiyoMarket.Data

# Apply migrations to database
dotnet ef database update --project ..\ZiyoMarket.Data

# Remove last migration (if not applied)
dotnet ef migrations remove --project ..\ZiyoMarket.Data

# View migration SQL
dotnet ef migrations script --project ..\ZiyoMarket.Data
```

**On Linux/Mac:**
```bash
# Navigate to API directory
cd src/ZiyoMarket.Api

# Create a new migration
dotnet ef migrations add MigrationName --project ../ZiyoMarket.Data

# Apply migrations to database
dotnet ef database update --project ../ZiyoMarket.Data
```

**Auto-Migration on Startup:** The application automatically runs pending migrations on startup (configured in Program.cs). This ensures Railway deployments always have the latest schema.

### Database Setup

```bash
# Create database in PostgreSQL
psql -U postgres
CREATE DATABASE "ZiyoNoorDb";
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

- **Local Development:** `Server=localhost;Database=ZiyoNoorDb;User Id=postgres;Password=YOUR_PASSWORD;Port=5432;`
- **Production:** Use Railway environment variables (see DEPLOYMENT.md)

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

### Firebase Push Notifications

**Architecture:** Professional device token management with separate `DeviceTokens` table

**Setup:**
1. Firebase service account file: `src/ZiyoMarket.Api/firebase-service-account.json`
2. **CRITICAL:** This file is in `.gitignore` - never commit it to git!
3. Firebase Admin SDK is registered as Singleton in `ServiceExtension.cs`

**Database Schema:**
- `DeviceTokens` table: Stores FCM tokens for all user devices
- Supports multiple devices per user (professional approach)
- Auto-cleanup of expired tokens (> 60 days)
- Tracks device metadata (OS, version, last used)

**API Endpoints:**
```
POST /api/push-notification/register-token      [Authorize]
POST /api/push-notification/send                [Admin]
POST /api/push-notification/send-batch          [Admin]
POST /api/push-notification/send-topic          [Admin]
GET  /api/push-notification/my-devices          [Authorize]
POST /api/push-notification/logout-all-devices  [Authorize]
POST /api/push-notification/cleanup-expired     [Admin]
```

**Usage Example (Admin sending notification):**
```csharp
POST /api/push-notification/send
{
  "user_id": 5,
  "title": "Yangi buyurtma",
  "message": "Sizning buyurtmangiz qabul qilindi",
  "data": {
    "order_id": "12345",
    "type": "order_created"
  }
}
```

**Flutter Integration:**
- Mobile app registers device token on login/app launch
- Token stored in `DeviceTokens` table with device metadata
- Backend sends to all user's active devices automatically
- See `FIREBASE_PUSH_NOTIFICATION_GUIDE.md` for complete Flutter integration

**Important Notes:**
- One user can have multiple device tokens (multi-device support)
- Tokens auto-expire after 60 days of inactivity
- Admin can send to specific users, batch users, or topics
- Topics: `all_customers`, `new_products`, `promotions`

### File Upload System (wwwroot)

**Architecture:** Professional static file management with local and cloud storage support

**Setup:**
1. Static files served from `src/ZiyoMarket.Api/wwwroot/` folder
2. Organized by category: products, categories, banners, users
3. Configured in `appsettings.json` under `FileUploadSettings`

**Folder Structure:**
```
wwwroot/
└── images/
    ├── products/      # Product images
    ├── categories/    # Category images
    ├── banners/       # Banner/promotional images
    ├── users/         # User avatars
    └── temp/          # Temporary uploads
```

**API Endpoints:**
```
POST /api/file_upload/product               [Authorize]
POST /api/file_upload/product/multiple      [Authorize]
POST /api/file_upload/category              [Authorize]
POST /api/file_upload/banner                [Authorize]
POST /api/file_upload/banner/multiple       [Authorize]
POST /api/file_upload/user/avatar           [Authorize]
DELETE /api/file_upload?filePath=...        [Authorize]
POST /api/file_upload/delete-multiple       [Authorize]
GET /api/file_upload/url?filePath=...       [Public]
```

**Validation:**
- **Max File Size:** 5MB (configurable)
- **Allowed Extensions:** .jpg, .jpeg, .png, .gif, .webp, .svg
- **Authentication:** Required for all upload/delete operations
- **Unique Names:** GUID-based filenames prevent conflicts

**Usage Example:**
```csharp
// Upload image
var uploadResult = await _fileUploadService.UploadImageAsync(file, ImageCategory.Product);

// Save to entity
product.ImageUrl = uploadResult.FilePath; // "images/products/abc123.jpg"
await _unitOfWork.SaveChangesAsync();

// Get full URL for display
var fullUrl = _fileUploadService.GetFileUrl(product.ImageUrl);
// Result: "http://localhost:8080/images/products/abc123.jpg"
```

**Entity Integration:**
- `Product.ImageUrl` - Product images
- `Category.ImageUrl` - Category images
- `User.ImageUrl` - User avatars (new unified system)
- Store relative paths in DB: `"images/products/abc123.jpg"`
- Service generates full URLs dynamically based on environment

**Production Deployment:**
- **Railway:** Files are ephemeral (deleted on redeploy)
- **Solution 1:** Use Railway Volumes for persistent storage
- **Solution 2:** Migrate to cloud storage (AWS S3, Cloudinary, Azure Blob)

See `FILE_UPLOAD_GUIDE.md` for complete documentation and examples.

### Payment Integration (Click.uz)

**Architecture:** Click payment gateway integration for online payments

**Setup:**
1. Configure Click credentials in `appsettings.json`:
   ```json
   "ClickSettings": {
     "ServiceId": "your-service-id",
     "MerchantId": "your-merchant-id",
     "SecretKey": "your-secret-key",
     "MerchantUserId": "your-merchant-user-id"
   }
   ```
2. **CRITICAL:** Store credentials in environment variables or user secrets in production
3. Click service is registered as Scoped in `ServiceExtension.cs`

**Features:**
- Payment request preparation
- Payment verification
- Transaction status checking
- Integration with order system

### SMS Integration (Eskiz.uz)

**Architecture:** Professional SMS service with `SmsLog` table and Eskiz.uz API integration

**Setup:**
1. Configure Eskiz.uz credentials in `appsettings.json`:
   ```json
   "EskizSms": {
     "BaseUrl": "https://notify.eskiz.uz/api",
     "Email": "your-email@example.com",
     "Password": "your-password",
     "CallbackUrl": "",
     "IsDevelopment": true
   }
   ```
2. **CRITICAL:** Store credentials in environment variables or user secrets in production
3. SMS service is registered as Scoped in `ServiceExtension.cs`

**Database Schema:**
- `SmsLogs` table: Stores all SMS sending history
- Tracks purpose (Registration, PasswordReset, OrderConfirmation, etc.)
- Records status (Pending, Sent, Delivered, Failed)
- Stores provider message ID and error messages

**API Endpoints:**
```
POST /api/sms/send                         [Admin]
POST /api/sms/send-verification-code       [Public]
POST /api/sms/verify-code                  [Public]
POST /api/sms/send-bulk                    [Admin]
GET  /api/sms/logs                         [Admin]
GET  /api/sms/my-logs                      [Authorize]
GET  /api/sms/statistics                   [Admin]
```

**Usage Example (Verification Code):**
```csharp
// Send verification code
POST /api/sms/send-verification-code
{
  "phone_number": "+998901234567",
  "purpose": 1  // Registration
}

// Verify code
POST /api/sms/verify-code
{
  "phone_number": "+998901234567",
  "code": "123456",
  "purpose": 1
}
```

**Features:**
- 6-digit verification codes (5 minutes expiry)
- Memory cache for code storage
- Development mode returns code in response
- Automatic authentication with Eskiz.uz (token cached for 30 days)
- Rate limiting for bulk SMS (100ms delay between sends)
- SMS statistics and logging
- **Privileged Users:** Hardcoded verification codes for specific phone numbers (configured in `appsettings.json` -> `PrivilegedUsers`)

**Important Notes:**
- Verification codes expire after 5 minutes
- Phone number format: +998XXXXXXXXX
- Development mode: `IsDevelopment: true` returns code in response
- Production mode: Code only sent via SMS
- Privileged users bypass SMS sending and use hardcoded codes (for testing/demo)
- See `SMS_INTEGRATION_GUIDE.md` for detailed documentation

## Key Domain Concepts

### User Types and Authorization

Three distinct user types with different capabilities:

1. **Customer** - End users who purchase products online
2. **Seller** - Staff who create offline orders in physical stores
3. **Admin** - System administrators with full access

**JWT Claims:** Token includes `UserId`, `Email`, `UserType` claims for authorization

**User System Architecture:**
- **New System (in development):** `User`, `Role`, `Permission`, `UserRole`, `RolePermission` tables provide flexible RBAC
- **Legacy System (current):** `Customer`, `Seller`, `Admin` separate tables (will be deprecated)
- Both systems coexist in DbContext; new code should prepare for migration to unified user system

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

**Alternative: CreateAdminTool Utility**

The `CreateAdminTool` is a standalone console application for creating admin users directly in the database (useful when API is not running):

```bash
# Navigate to the tool directory
cd CreateAdminTool

# Update connection string in Program.cs if needed
# Default: Server=localhost;Database=ZiyoNoorDb;User Id=postgres;Password=2001;Port=5432;

# Run the tool
dotnet run

# Creates admin user:
# Username: testadmin
# Phone: +998901234568
# Password: Admin@123
```

**Note:** This tool bypasses the API and writes directly to the database. Use only for initial setup or emergency admin creation.

## API Structure

**Base URL (Development):** `http://localhost:8080/api/`
**Base URL (Production):** Configured via Railway/Docker
**Swagger UI:** `http://localhost:8080/swagger`

**Note:** The API runs on port 8080 by default (configurable via `PORT` environment variable for cloud deployment).

### Health Check Endpoints

The API provides health check endpoints for monitoring:

```bash
# Basic health check (lightweight, no DB check)
GET /health
# Returns: {"status": "healthy", "timestamp": "...", "service": "ZiyoMarket API"}

# Detailed health check (includes database connectivity)
GET /health/detailed
# Returns: {"status": "healthy", "timestamp": "...", "database": "connected"}
```

Use `/health` for load balancer health checks and `/health/detailed` for monitoring dashboards.

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
- **`/api/banner`** - Banner management (Create, Read, Update, Delete, Publish, Unpublish)
- **`/api/report`** - Sales reports, analytics
- **`/api/customer`** - Customer management (Admin)
- **`/api/seller`** - Seller management (Admin)
- **`/api/admin`** - Admin user management
- **`/api/seed_data`** - Development seed data endpoints

## Common Patterns in This Codebase

### Error Handling

Services return `Result<T>` objects (not throwing exceptions for business logic errors). Controllers use `HandleResult()` for automatic response formatting:

```csharp
// Recommended approach - Service returns Result<T>
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _service.GetById(id);
    return HandleResult(result); // Automatically formats ApiResponse
}

// Alternative approach for try-catch scenarios
try
{
    var result = await _service.GetById(id);
    return SuccessResponse(result, "Success");
}
catch (Exception ex)
{
    return ErrorResponse(ex.Message);
}
```

**Note:** Global exception handler middleware catches uncaught exceptions and returns them in unified `ApiResponse` format.

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

The project has a **separate WPF admin panel** project for Windows desktop administration:

- **Location:** `C:\Users\abdum\OneDrive\Desktop\AdminPanel\ZiyoNurAdminPanel.Ui`
- Built with WPF (Windows Presentation Foundation) and .NET 8
- Uses MaterialDesign themes for modern UI
- Follows MVVM pattern with CommunityToolkit.Mvvm
- Consumes the ZiyoMarket API via HTTP client
- Provides admin dashboard, reports, and management interfaces

**Run Admin Panel:**
```bash
cd C:\Users\abdum\OneDrive\Desktop\AdminPanel\ZiyoNurAdminPanel.Ui
dotnet run --project ZiyoNurAdminPanel.Ui/ZiyoNurAdminPanel.Ui.csproj
```

**Features:**
- Full CRUD for Products, Categories, Orders, Customers, Sellers, Admins
- Banner management (NEW) - Create, edit, publish, unpublish banners
- Dashboard with statistics
- Role and permission management
- Reports and analytics
- File upload support
- Real-time notifications

**API Configuration:**
- Update `appsettings.json` → `ApiSettings.BaseUrl` to point to backend API
- Default: `http://localhost:8081/api/`

See admin panel's `CLAUDE.md` for detailed documentation.

## Performance Optimization

### Caching System

**Architecture:** In-memory caching for high-traffic endpoints

**Setup:**
1. Cache service: `src/ZiyoMarket.Service/Services/CacheService.cs`
2. Uses `IMemoryCache` (no external dependencies)
3. Registered in `ServiceExtension.cs`

**Cached Endpoints:**
- `GET /api/product/{id}` - Product details (5 minutes)
- `GET /api/category` - All categories (1 hour)
- `GET /api/category/tree` - Category hierarchy (1 hour)

**Features:**
- ✅ Automatic cache invalidation on create/update/delete
- ✅ Sliding expiration (popular items stay longer)
- ✅ 80-90% faster response times
- ✅ 70% reduction in database queries
- ✅ Transparent to API consumers (no changes needed)

**Cache Invalidation:**
```csharp
// Product updated
await _cacheService.RemoveAsync(string.Format(PRODUCT_DETAIL_KEY, id));
await _cacheService.RemoveByPrefixAsync(PRODUCT_PREFIX);
```

See `CACHE_IMPLEMENTATION_GUIDE.md` for complete documentation.

---

## Important Development Notes

### API URL and Response Format

**All API endpoints and JSON use snake_case:**
- **URLs:** `/api/auth/login`, `/api/product/search`, `/api/order/create` (lowercase)
- **Request Bodies:** `{"first_name": "John", "last_name": "Doe", "phone_number": "+998..."}`
- **Response Bodies:** `{"user_id": 1, "created_at": "2025-01-24 10:30:00", "is_active": true}`

When testing with Postman/Swagger/curl, always use snake_case for property names in request bodies.

### Available Repository Properties in UnitOfWork

When working with services, these repositories are available through `_unitOfWork`:
- `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions` - New unified user system (in development)
- `Customers`, `Sellers`, `Admins` - Legacy user management (current)
- `Products`, `Categories`, `ProductCategories`, `ProductLikes`, `CartItems` - Product catalog
- `Orders`, `OrderItems`, `OrderDiscounts`, `DiscountReasons`, `CashbackTransactions` - Order processing
- `DeliveryPartners`, `OrderDeliveries` - Delivery management
- `Notifications`, `DeviceTokens`, `SmsLogs` - Notifications (Push & SMS)
- `SupportChats`, `SupportMessages` - Customer support
- `Contents`, `SystemSettings`, `DailySalesSummaries` - System configuration and CMS
- `Banners` (via Contents with ContentType.Banner) - Marketing banners

## Project Status and Context Files

For comprehensive project understanding, refer to these additional documentation files:

- **`PROJECT_STATUS.md`** - Current development status, pending tasks, known issues, and sprint progress
- **`API_RESPONSE_FORMAT.md`** - Unified API response format specification and Flutter integration guide
- **`FIREBASE_PUSH_NOTIFICATION_GUIDE.md`** - Complete guide for Firebase/FCM integration (Flutter + Backend)
- **`SMS_INTEGRATION_GUIDE.md`** - Complete guide for SMS integration with Eskiz.uz (Verification codes, notifications)
- **`FILE_UPLOAD_GUIDE.md`** - Complete guide for file upload system with wwwroot static files
- **`SECURITY_WARNING.md`** - Security alerts and action items (check regularly)
- **`DEPLOYMENT.md`** - Production deployment guide for Railway.app
- **`BACKEND_DEVELOPER_GUIDE.md`** - Detailed backend development guide (150+ endpoints)
- **`FLUTTER_DEVELOPER_GUIDE.md`** - Flutter/mobile integration guide with API examples

**IMPORTANT:** When resuming work after a break, always check `PROJECT_STATUS.md` for current blockers and pending migrations.
