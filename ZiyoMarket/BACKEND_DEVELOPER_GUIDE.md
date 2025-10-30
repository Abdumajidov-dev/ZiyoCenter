# Backend Developer Guide - ZiyoMarket

> **To'liq qo'llanma backend dasturchilari uchun: Arxitektura, Kod strukturasi, Best Practices**

## 📚 Mundarija

1. [Loyiha Arxitekturasi](#loyiha-arxitekturasi)
2. [Kod Strukturasi](#kod-strukturasi)
3. [Database Schema va Entity Relationships](#database-schema-va-entity-relationships)
4. [API Endpoints - To'liq Ma'lumotnoma](#api-endpoints-toliq-malumotnoma)
5. [Service Layer va Business Logic](#service-layer-va-business-logic)
6. [Authentication va Authorization](#authentication-va-authorization)
7. [DTOs va AutoMapper](#dtos-va-automapper)
8. [Validation va Error Handling](#validation-va-error-handling)
9. [Best Practices](#best-practices)
10. [Migration va Database Management](#migration-va-database-management)
11. [Testing va Debugging](#testing-va-debugging)

---

## 🏗️ Loyiha Arxitekturasi

### 4-Tier Clean Architecture

```
┌─────────────────────────────────────────────┐
│  ZiyoMarket.Api (Presentation Layer)        │
│  - Controllers                              │
│  - Middleware                               │
│  - Filters                                  │
│  - Program.cs & Startup Configuration      │
├─────────────────────────────────────────────┤
│  ZiyoMarket.Service (Business Logic Layer)  │
│  - Services (IAuthService, IProductService) │
│  - DTOs (Data Transfer Objects)            │
│  - Validators (FluentValidation)           │
│  - Mapping Profiles (AutoMapper)           │
│  - Result Pattern Implementation           │
├─────────────────────────────────────────────┤
│  ZiyoMarket.Data (Data Access Layer)        │
│  - DbContext (EF Core Configuration)       │
│  - Repositories (Generic Repository)       │
│  - UnitOfWork Pattern                      │
│  - Migrations                              │
├─────────────────────────────────────────────┤
│  ZiyoMarket.Domain (Domain Layer)           │
│  - Entities (Domain Models)                │
│  - Enums                                   │
│  - Common Base Classes                     │
│  - Business Logic Methods                 │
└─────────────────────────────────────────────┘
```

### Design Patterns Qo'llanilgan

1. **Repository Pattern** - Data access abstraction
2. **Unit of Work Pattern** - Transaction management
3. **Result Pattern** - Consistent error handling
4. **Dependency Injection** - Loose coupling
5. **AutoMapper** - Object-to-object mapping
6. **Generic Repository** - Code reusability

---

## 📁 Kod Strukturasi

### Proyekt Fayl Strukturasi

```
ZiyoMarket/
├── src/
│   ├── ZiyoMarket.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── BaseAuditableEntity.cs
│   │   │   └── Result.cs
│   │   ├── Entities/
│   │   │   ├── Users/
│   │   │   │   ├── Customer.cs
│   │   │   │   ├── Seller.cs
│   │   │   │   └── Admin.cs
│   │   │   ├── Products/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── Category.cs
│   │   │   │   ├── CartItem.cs
│   │   │   │   └── ProductLike.cs
│   │   │   ├── Orders/
│   │   │   │   ├── Order.cs
│   │   │   │   ├── OrderItem.cs
│   │   │   │   ├── OrderDiscount.cs
│   │   │   │   ├── DiscountReason.cs
│   │   │   │   └── CashbackTransaction.cs
│   │   │   ├── Delivery/
│   │   │   │   ├── DeliveryPartner.cs
│   │   │   │   └── OrderDelivery.cs
│   │   │   ├── Support/
│   │   │   │   ├── SupportChat.cs
│   │   │   │   └── SupportMessage.cs
│   │   │   ├── Notifications/
│   │   │   │   └── Notification.cs
│   │   │   ├── Contents/
│   │   │   │   └── Content.cs
│   │   │   └── Systems/
│   │   │       ├── DailySalesSummary.cs
│   │   │       └── SystemSetting.cs
│   │   └── Enums/
│   │       ├── OrderStatus.cs
│   │       ├── ProductStatus.cs
│   │       ├── PaymentMethod.cs
│   │       ├── DeliveryType.cs
│   │       ├── DeliveryStatus.cs
│   │       ├── UserType.cs
│   │       ├── NotificationType.cs
│   │       ├── SupportChatStatus.cs
│   │       ├── ContentType.cs
│   │       └── CashbackTransactionType.cs
│   │
│   ├── ZiyoMarket.Data/
│   │   ├── Context/
│   │   │   └── ZiyoMarketDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── IRepository.cs
│   │   │   └── Repository.cs
│   │   ├── UnitOfWorks/
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Migrations/
│   │   │   └── [Generated Migration Files]
│   │   └── Extensions/
│   │       └── ServiceExtensions.cs
│   │
│   ├── ZiyoMarket.Service/
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IProductService.cs
│   │   │   ├── ICategoryService.cs
│   │   │   ├── ICartService.cs
│   │   │   ├── IOrderService.cs
│   │   │   ├── ICustomerService.cs
│   │   │   ├── ISellerService.cs
│   │   │   ├── ICashbackService.cs
│   │   │   ├── IDeliveryService.cs
│   │   │   ├── INotificationService.cs
│   │   │   ├── ISupportService.cs
│   │   │   ├── IContentService.cs
│   │   │   └── IReportService.cs
│   │   ├── Services/
│   │   │   └── [13 Service Implementations]
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Products/
│   │   │   ├── Orders/
│   │   │   ├── Cart/
│   │   │   ├── Cashback/
│   │   │   ├── Sellers/
│   │   │   ├── Customers/
│   │   │   ├── Delivery/
│   │   │   ├── Support/
│   │   │   ├── Notifications/
│   │   │   ├── Content/
│   │   │   ├── Reports/
│   │   │   └── Common/
│   │   ├── Mapping/
│   │   │   └── AutoMapperProfile.cs
│   │   ├── Validators/
│   │   │   └── [FluentValidation Validators]
│   │   ├── Results/
│   │   │   └── Result.cs
│   │   ├── Exceptions/
│   │   │   └── [Custom Exceptions]
│   │   └── Helpers/
│   │       └── TimeHelper.cs
│   │
│   └── ZiyoMarket.Api/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── ProductController.cs
│       │   ├── CategoryController.cs
│       │   ├── CartController.cs
│       │   ├── OrderController.cs
│       │   ├── CashbackController.cs
│       │   ├── DeliveryController.cs
│       │   ├── SupportController.cs
│       │   ├── NotificationController.cs
│       │   ├── ContentController.cs
│       │   ├── SellerController.cs
│       │   ├── CustomerController.cs
│       │   └── ReportController.cs
│       ├── Extensions/
│       │   └── ServiceExtensions.cs
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── ZiyoMarket.Api.http
│
└── ZiyoMarket.sln
```

---

## 🗄️ Database Schema va Entity Relationships

### 1. Domain Entities va Relationships

#### BaseEntity va BaseAuditableEntity

**Fayl:** `src/ZiyoMarket.Domain/Common/BaseEntity.cs`

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } // Soft delete
    public bool IsDeleted => DeletedAt.HasValue;
}

public abstract class BaseAuditableEntity : BaseEntity
{
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
```

**Key Features:**
- Soft delete support (`DeletedAt`)
- Automatic timestamp tracking
- Audit trail (CreatedBy, UpdatedBy)

#### Customer Entity

**Fayl:** `src/ZiyoMarket.Domain/Entities/Users/Customer.cs`

```csharp
public class Customer : BaseAuditableEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; } // Unique
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Address { get; set; }
    public string FcmToken { get; set; } // Firebase Cloud Messaging
    public decimal CashbackBalance { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Order> Orders { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
    public ICollection<ProductLike> ProductLikes { get; set; }
    public ICollection<CashbackTransaction> CashbackTransactions { get; set; }
    public ICollection<SupportChat> SupportChats { get; set; }
    public ICollection<Notification> Notifications { get; set; }

    // Computed Property
    public string FullName => $"{FirstName} {LastName}";
}
```

**DbContext Configuration:**
```csharp
modelBuilder.Entity<Customer>(entity =>
{
    entity.HasIndex(e => e.Phone).IsUnique();
    entity.HasIndex(e => e.Email);
    entity.Property(e => e.CashbackBalance).HasColumnType("decimal(18,2)");
    entity.HasQueryFilter(e => e.DeletedAt == null); // Global soft delete filter
});
```

#### Product Entity

**Fayl:** `src/ZiyoMarket.Domain/Entities/Products/Product.cs`

```csharp
public class Product : BaseAuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string QrCode { get; set; } // Unique
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string ImageUrl { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public decimal? Weight { get; set; }
    public string Dimensions { get; set; }
    public string Manufacturer { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Category Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
    public ICollection<ProductLike> ProductLikes { get; set; }

    // Business Logic Methods
    public bool IsLowStock() => StockQuantity <= MinStockLevel && StockQuantity > 0;
    public bool IsOutOfStock() => StockQuantity == 0;
    public bool IsAvailableForSale() => IsActive && Status == ProductStatus.Active && StockQuantity > 0;

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new ArgumentException("Price must be positive");
        Price = newPrice;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        StockQuantity += quantity;
        if (StockQuantity > 0 && Status == ProductStatus.OutOfStock)
            Status = ProductStatus.Active;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (quantity > StockQuantity) throw new InvalidOperationException("Not enough stock");
        StockQuantity -= quantity;
        if (StockQuantity == 0)
            Status = ProductStatus.OutOfStock;
    }
}
```

#### Order Entity

**Fayl:** `src/ZiyoMarket.Domain/Entities/Orders/Order.cs`

```csharp
public class Order : BaseAuditableEntity
{
    public string OrderNumber { get; set; } // Format: ORD-20250118-001
    public int CustomerId { get; set; }
    public int? SellerId { get; set; } // NULL for online orders
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalPrice { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal CashbackUsed { get; set; }
    public decimal FinalPrice { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }
    public DeliveryType? DeliveryType { get; set; }
    public string DeliveryAddress { get; set; }
    public decimal? DeliveryFee { get; set; }
    public string CustomerNotes { get; set; }
    public string SellerNotes { get; set; }
    public string AdminNotes { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; }
    public Seller Seller { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<OrderDiscount> OrderDiscounts { get; set; }
    public OrderDelivery OrderDelivery { get; set; }
    public ICollection<CashbackTransaction> CashbackTransactions { get; set; }

    // Computed Properties
    public bool IsOnlineOrder => !SellerId.HasValue;
    public bool IsOfflineOrder => SellerId.HasValue;
    public bool IsPaid => PaidAt.HasValue;
    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
    public bool IsCompleted => Status == OrderStatus.Delivered;
    public bool IsCancelled => Status == OrderStatus.Cancelled;
    public bool RequiresPayment => !IsPaid && PaymentMethod != PaymentMethod.Cash;
    public bool RequiresDelivery => DeliveryType == ZiyoMarket.Domain.Enums.DeliveryType.Home ||
                                    DeliveryType == ZiyoMarket.Domain.Enums.DeliveryType.Office;
}
```

**Order Status Workflow:**
```
Pending (1)
  → Confirmed (2)
  → Preparing (3)
  → ReadyForPickup (4) / Shipped (5)
  → Delivered (6) / Cancelled (7)
```

#### Category Entity (Hierarchical)

**Fayl:** `src/ZiyoMarket.Domain/Entities/Products/Category.cs`

```csharp
public class Category : BaseAuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int? ParentId { get; set; } // Self-referencing for hierarchy
    public string ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Category ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; }
    public ICollection<Product> Products { get; set; }
}
```

**DbContext Configuration:**
```csharp
modelBuilder.Entity<Category>(entity =>
{
    entity.HasOne(e => e.ParentCategory)
          .WithMany(e => e.SubCategories)
          .HasForeignKey(e => e.ParentId)
          .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
});
```

### 2. Enums

#### OrderStatus Enum

**Fayl:** `src/ZiyoMarket.Domain/Enums/OrderStatus.cs`

```csharp
public enum OrderStatus
{
    Pending = 1,          // Kutilmoqda
    Confirmed = 2,        // Tasdiqlangan
    Preparing = 3,        // Tayyorlanmoqda
    ReadyForPickup = 4,   // Olib ketishga tayyor
    Shipped = 5,          // Yuborilgan
    Delivered = 6,        // Yetkazildi
    Cancelled = 7         // Bekor qilindi
}
```

#### PaymentMethod Enum

```csharp
public enum PaymentMethod
{
    Cash = 1,      // Naqd
    Card = 2,      // Karta
    Cashback = 3,  // Cashback
    Mixed = 4      // Aralash (Cashback + Card/Cash)
}
```

#### ProductStatus Enum

```csharp
public enum ProductStatus
{
    Active,        // Faol
    Inactive,      // Nofaol
    OutOfStock,    // Tugagan
    Discontinued   // Ishlab chiqarilmaydi
}
```

### 3. DbContext Configuration

**Fayl:** `src/ZiyoMarket.Data/Context/ZiyoMarketDbContext.cs`

```csharp
public class ZiyoMarketDbContext : DbContext
{
    public ZiyoMarketDbContext(DbContextOptions<ZiyoMarketDbContext> options)
        : base(options) { }

    // DbSets (20+ entities)
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<ProductLike> ProductLikes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderDiscount> OrderDiscounts { get; set; }
    public DbSet<DiscountReason> DiscountReasons { get; set; }
    public DbSet<CashbackTransaction> CashbackTransactions { get; set; }
    public DbSet<DeliveryPartner> DeliveryPartners { get; set; }
    public DbSet<OrderDelivery> OrderDeliveries { get; set; }
    public DbSet<SupportChat> SupportChats { get; set; }
    public DbSet<SupportMessage> SupportMessages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<DailySalesSummary> DailySalesSummaries { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filter - Soft Delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(BaseEntity.DeletedAt)),
                    Expression.Constant(null, typeof(DateTime?))
                );
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                    Expression.Lambda(body, parameter)
                );
            }
        }

        // Entity Configurations
        ConfigureCustomer(modelBuilder);
        ConfigureSeller(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureOrder(modelBuilder);
        // ... other configurations
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-populate audit fields
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

---

## 🔌 API Endpoints - To'liq Ma'lumotnoma

### 1. Authentication Endpoints (`AuthController`)

**Base Path:** `/api/auth`

#### POST `/api/auth/login`
**Vazifasi:** Foydalanuvchini tizimga kiritish

**Request Body:**
```json
{
  "phoneOrEmail": "user@example.com",
  "password": "password123",
  "userType": "Customer"  // Customer | Seller | Admin
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh_token_string",
    "expiresAt": "2025-01-31T14:30:00Z",
    "user": {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "phone": "+998901234567",
      "email": "john@example.com",
      "userType": "Customer",
      "cashbackBalance": 50000,
      "isActive": true
    }
  },
  "message": "Login successful",
  "statusCode": 200
}
```

**Service Method:**
```csharp
public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
}
```

**Controller Implementation:**
```csharp
[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
{
    var result = await _authService.LoginAsync(dto);
    if (!result.IsSuccess)
        return BadRequest(result);
    return Ok(result);
}
```

#### POST `/api/auth/register`
**Vazifasi:** Yangi mijoz ro'yxatdan o'tkazish

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+998901234567",
  "email": "john@example.com",
  "password": "password123",
  "address": "Toshkent, Yakkasaroy"
}
```

**Validation Rules:**
- FirstName: Required, MinLength(2), MaxLength(50)
- Phone: Required, Unique, Phone format
- Email: Required, Valid email, Unique
- Password: Required, MinLength(6)

#### POST `/api/auth/refresh-token`
**Vazifasi:** Access token ni yangilash

**Request Body:**
```json
{
  "refreshToken": "refresh_token_string"
}
```

#### POST `/api/auth/change-password`
**Vazifasi:** Parolni o'zgartirish
**Authorization:** Required

**Request Body:**
```json
{
  "currentPassword": "old_password",
  "newPassword": "new_password"
}
```

---

### 2. Product Endpoints (`ProductController`)

**Base Path:** `/api/product`

#### GET `/api/product`
**Vazifasi:** Mahsulotlar ro'yxatini olish (filtrlash va pagination bilan)

**Query Parameters:**
```
pageNumber: int = 1
pageSize: int = 20
searchTerm: string = ""
categoryId: int? = null
minPrice: decimal? = null
maxPrice: decimal? = null
sortBy: string = "Name"  // Name | Price | CreatedAt | MostLiked
sortOrder: string = "Ascending"  // Ascending | Descending
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Kitob: C# in Depth",
        "description": "Advanced C# programming",
        "qrCode": "PRD-001",
        "price": 125000,
        "stockQuantity": 50,
        "categoryId": 5,
        "categoryName": "Dasturlash kitoblari",
        "status": "Active",
        "imageUrl": "https://example.com/image.jpg",
        "isLowStock": false,
        "isLiked": false,
        "likesCount": 15
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**Service Method:**
```csharp
public interface IProductService
{
    Task<Result<PaginationResponse<ProductListDto>>> GetProductsAsync(
        ProductFilterRequest filter,
        int? currentUserId = null
    );
}
```

#### GET `/api/product/{id}`
**Vazifasi:** Mahsulot detallari

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Kitob: C# in Depth",
    "description": "Advanced C# programming book...",
    "qrCode": "PRD-001",
    "price": 125000,
    "stockQuantity": 50,
    "categoryId": 5,
    "category": {
      "id": 5,
      "name": "Dasturlash kitoblari",
      "parentCategoryName": "Kitoblar"
    },
    "status": "Active",
    "imageUrl": "https://example.com/image.jpg",
    "weight": 0.5,
    "dimensions": "15x21cm",
    "manufacturer": "Manning Publications",
    "isLowStock": false,
    "isLiked": true,
    "likesCount": 15,
    "createdAt": "2025-01-15T10:30:00Z",
    "updatedAt": "2025-01-18T14:20:00Z"
  }
}
```

#### GET `/api/product/qr/{qrCode}`
**Vazifasi:** QR kod orqali mahsulotni topish

**Example:** `GET /api/product/qr/PRD-001`

#### POST `/api/product`
**Vazifasi:** Yangi mahsulot yaratish
**Authorization:** Admin

**Request Body:**
```json
{
  "name": "Kitob: C# in Depth",
  "description": "Advanced C# programming",
  "qrCode": "PRD-001",
  "price": 125000,
  "stockQuantity": 50,
  "categoryId": 5,
  "imageUrl": "https://example.com/image.jpg",
  "weight": 0.5,
  "dimensions": "15x21cm",
  "manufacturer": "Manning Publications",
  "minStockLevel": 10
}
```

#### POST `/api/product/{productId}/like`
**Vazifasi:** Mahsulotni like/unlike qilish
**Authorization:** Customer

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "isLiked": true,
    "likesCount": 16
  },
  "message": "Product liked successfully"
}
```

#### POST `/api/product/{productId}/stock/add`
**Vazifasi:** Stok qo'shish
**Authorization:** Admin/Seller

**Request Body:**
```json
{
  "quantity": 20,
  "notes": "New shipment received"
}
```

---

### 3. Cart Endpoints (`CartController`)

**Base Path:** `/api/cart`
**Authorization:** Customer only

#### GET `/api/cart`
**Vazifasi:** Savat elementlarini olish

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "productId": 5,
        "productName": "Kitob: C# in Depth",
        "productImage": "https://example.com/image.jpg",
        "unitPrice": 125000,
        "quantity": 2,
        "subTotal": 250000,
        "isAvailable": true,
        "stockQuantity": 50,
        "addedAt": "2025-01-18T10:30:00Z"
      }
    ],
    "totalItems": 1,
    "totalQuantity": 2,
    "totalPrice": 250000
  }
}
```

#### POST `/api/cart`
**Vazifasi:** Savatga mahsulot qo'shish

**Request Body:**
```json
{
  "productId": 5,
  "quantity": 2
}
```

**Business Logic:**
- Agar mahsulot allaqachon savatda bo'lsa, quantity ni oshiradi
- Stok miqdorini tekshiradi
- Mahsulot mavjudligini va faolligini tekshiradi

#### PUT `/api/cart/{cartItemId}`
**Vazifasi:** Savat elementini yangilash (quantity)

**Request Body:**
```json
{
  "quantity": 3
}
```

#### DELETE `/api/cart/{cartItemId}`
**Vazifasi:** Savatdan olib tashlash

#### GET `/api/cart/total`
**Vazifasi:** Savat jami narxini hisoblash

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalItems": 3,
    "totalQuantity": 5,
    "totalPrice": 450000
  }
}
```

---

### 4. Order Endpoints (`OrderController`)

**Base Path:** `/api/order`

#### POST `/api/order`
**Vazifasi:** Yangi online buyurtma yaratish
**Authorization:** Customer

**Request Body:**
```json
{
  "orderItems": [
    {
      "productId": 5,
      "quantity": 2,
      "unitPrice": 125000
    },
    {
      "productId": 8,
      "quantity": 1,
      "unitPrice": 200000
    }
  ],
  "paymentMethod": "Card",  // Cash | Card | Cashback | Mixed
  "deliveryType": "Home",   // Pickup | Home | Office
  "deliveryAddress": "Toshkent, Yakkasaroy tumani, Bobur ko'chasi 12",
  "cashbackToUse": 5000,
  "customerNotes": "Iltimos, ehtiyotkorlik bilan o'rang"
}
```

**Business Logic:**
1. Savatdan mahsulotlarni olish (agar orderItems bo'sh bo'lsa)
2. Mahsulot mavjudligini va stok miqdorini tekshirish
3. TotalPrice ni hisoblash
4. Cashback ishlatish (agar so'ralgan bo'lsa)
5. FinalPrice ni hisoblash
6. OrderNumber generatsiya qilish (format: `ORD-20250118-001`)
7. Order va OrderItems yaratish
8. Mahsulot stoklarini kamaytirish
9. Savatni tozalash

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "orderNumber": "ORD-20250118-001",
    "orderDate": "2025-01-18T14:30:00Z",
    "status": "Pending",
    "totalPrice": 450000,
    "discountApplied": 0,
    "cashbackUsed": 5000,
    "finalPrice": 445000,
    "paymentMethod": "Card",
    "deliveryType": "Home",
    "deliveryAddress": "Toshkent, Yakkasaroy...",
    "items": [
      {
        "productId": 5,
        "productName": "Kitob: C# in Depth",
        "quantity": 2,
        "unitPrice": 125000,
        "subTotal": 250000
      }
    ]
  },
  "message": "Order created successfully"
}
```

#### POST `/api/order/seller`
**Vazifasi:** Offline buyurtma yaratish (do'konda)
**Authorization:** Seller

**Request Body:**
```json
{
  "customerPhone": "+998901234567",
  "orderItems": [
    {
      "productId": 5,
      "quantity": 2
    }
  ],
  "paymentMethod": "Cash",
  "discountAmount": 10000,
  "discountReasonId": 1,
  "sellerNotes": "VIP mijoz, 10% chegirma"
}
```

**Business Logic:**
1. Mijozni topish (phone orqali)
2. Mahsulot stok tekshirish
3. Chegirma qo'llash
4. Buyurtma yaratish (status: Confirmed)
5. Stok kamaytirish

#### GET `/api/order/{id}`
**Vazifasi:** Buyurtma detallari
**Authorization:** Customer (faqat o'zining), Seller (o'zinikilar), Admin (hammasi)

#### GET `/api/order`
**Vazifasi:** Buyurtmalar ro'yxati (filtrlash bilan)

**Query Parameters:**
```
pageNumber: int = 1
pageSize: int = 20
status: OrderStatus? = null
customerId: int? = null
sellerId: int? = null
startDate: DateTime? = null
endDate: DateTime? = null
searchTerm: string = ""  // OrderNumber, CustomerName
```

**Authorization Logic:**
- Customer: Faqat o'z buyurtmalari
- Seller: Faqat o'zi yaratgan buyurtmalar
- Admin: Barcha buyurtmalar

#### PUT `/api/order/{id}/status`
**Vazifasi:** Buyurtma statusini yangilash
**Authorization:** Admin, Seller (faqat o'zinikilar)

**Request Body:**
```json
{
  "status": "Confirmed",  // OrderStatus enum
  "notes": "Tasdiqlandi, tayyorlanmoqda"
}
```

**Status Workflow Validation:**
```
Pending → Confirmed → Preparing → ReadyForPickup/Shipped → Delivered
                                                          ↘ Cancelled
```

#### POST `/api/order/{id}/cancel`
**Vazifasi:** Buyurtmani bekor qilish
**Authorization:** Customer (faqat Pending/Confirmed), Admin (istalgan vaqt)

**Business Logic:**
1. Status Cancelled ga o'zgartirish
2. Mahsulot stoklarini qaytarish
3. Ishlatilgan cashback ni qaytarish
4. Bildirishnoma yuborish

---

### 5. Cashback Endpoints (`CashbackController`)

**Base Path:** `/api/cashback`
**Authorization:** Customer

#### GET `/api/cashback/summary`
**Vazifasi:** Cashback xulosasi

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "currentBalance": 50000,
    "totalEarned": 125000,
    "totalUsed": 75000,
    "totalExpired": 0,
    "expiringIn30Days": 5000,
    "pendingFromOrders": 3000
  }
}
```

#### GET `/api/cashback/history`
**Vazifasi:** Cashback tarixi (pagination bilan)

**Query Parameters:**
```
pageNumber: int = 1
pageSize: int = 20
transactionType: CashbackTransactionType? = null  // Earned | Used | Expired | Refunded
startDate: DateTime? = null
endDate: DateTime? = null
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "amount": 5000,
        "transactionType": "Earned",
        "orderId": 123,
        "orderNumber": "ORD-20250118-001",
        "description": "2% cashback from order ORD-20250118-001",
        "createdAt": "2025-01-18T14:30:00Z",
        "expiresAt": "2026-01-18T14:30:00Z",
        "isExpired": false
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 50,
    "totalPages": 3
  }
}
```

**Cashback Business Logic:**

```csharp
public class CashbackService : ICashbackService
{
    public async Task<decimal> CalculateCashbackAsync(decimal orderAmount)
    {
        // 2% cashback
        var cashbackPercentage = await GetCashbackPercentageFromSettingsAsync();
        return orderAmount * (cashbackPercentage / 100);
    }

    public async Task CreateCashbackTransactionAsync(Order order)
    {
        if (order.Status != OrderStatus.Delivered)
            return;

        var cashbackAmount = await CalculateCashbackAsync(order.FinalPrice);

        var transaction = new CashbackTransaction
        {
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            Amount = cashbackAmount,
            TransactionType = CashbackTransactionType.Earned,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1), // 1 yil amal qiladi
            IsUsed = false
        };

        await _repository.AddAsync(transaction);

        // Customer balansini yangilash
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        customer.CashbackBalance += cashbackAmount;

        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

### 6. Support Endpoints (`SupportController`)

**Base Path:** `/api/support`

#### POST `/api/support`
**Vazifasi:** Yangi support chat yaratish
**Authorization:** Customer

**Request Body:**
```json
{
  "title": "Buyurtma yetib kelmadi",
  "description": "ORD-20250118-001 buyurtmam 3 kundan beri yetib kelmadi",
  "category": "Delivery",
  "priority": "High"  // Low | Medium | High | Urgent
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": 15,
    "customerId": 1,
    "title": "Buyurtma yetib kelmadi",
    "description": "...",
    "status": "Open",
    "priority": "High",
    "category": "Delivery",
    "createdAt": "2025-01-18T15:00:00Z"
  }
}
```

#### GET `/api/support/{chatId}/messages`
**Vazifasi:** Chat xabarlarini olish
**Authorization:** Customer (o'ziniki), Admin

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "chatId": 15,
      "senderId": 1,
      "senderType": "Customer",
      "senderName": "John Doe",
      "message": "Buyurtmam qachon yetib keladi?",
      "isRead": true,
      "attachments": [],
      "createdAt": "2025-01-18T15:00:00Z"
    },
    {
      "id": 2,
      "chatId": 15,
      "senderId": 5,
      "senderType": "Admin",
      "senderName": "Support Team",
      "message": "Buyurtmangizni tekshiryapmiz, 1 soat ichida javob beramiz",
      "isRead": true,
      "attachments": [],
      "createdAt": "2025-01-18T15:05:00Z"
    }
  ]
}
```

#### POST `/api/support/messages`
**Vazifasi:** Chat ga xabar yuborish
**Authorization:** Customer (o'ziniki), Admin

**Request Body:**
```json
{
  "chatId": 15,
  "message": "Tushundim, kutaman",
  "attachments": ["https://example.com/screenshot.jpg"]
}
```

---

### 7. Report Endpoints (`ReportController`)

**Base Path:** `/api/report`
**Authorization:** Admin

#### GET `/api/report/dashboard`
**Vazifasi:** Dashboard statistikasi

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalRevenue": 15000000,
    "totalOrders": 450,
    "totalCustomers": 1250,
    "totalProducts": 350,
    "todayRevenue": 450000,
    "todayOrders": 15,
    "pendingOrders": 25,
    "lowStockProducts": 12,
    "revenueGrowth": 15.5,  // %
    "ordersGrowth": 12.3,   // %
    "topSellingProducts": [
      {
        "productId": 5,
        "productName": "Kitob: C# in Depth",
        "totalSold": 150,
        "revenue": 18750000
      }
    ],
    "recentOrders": [...]
  }
}
```

#### GET `/api/report/sales`
**Vazifasi:** Sotuv hisoboti

**Query Parameters:**
```
startDate: DateTime (required)
endDate: DateTime (required)
groupBy: string = "Day"  // Day | Week | Month
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "period": {
      "startDate": "2025-01-01T00:00:00Z",
      "endDate": "2025-01-31T23:59:59Z"
    },
    "summary": {
      "totalRevenue": 15000000,
      "totalOrders": 450,
      "totalItems": 1250,
      "averageOrderValue": 33333,
      "totalDiscounts": 500000,
      "totalCashbackUsed": 150000
    },
    "chartData": [
      {
        "date": "2025-01-01",
        "revenue": 350000,
        "orders": 12,
        "items": 35
      },
      {
        "date": "2025-01-02",
        "revenue": 420000,
        "orders": 15,
        "items": 42
      }
    ]
  }
}
```

#### GET `/api/report/products/top`
**Vazifasi:** Eng ko'p sotiladigan mahsulotlar

**Query Parameters:**
```
startDate: DateTime? = null
endDate: DateTime? = null
limit: int = 10
```

#### GET `/api/report/inventory`
**Vazifasi:** Inventar hisoboti

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalProducts": 350,
    "totalStock": 15000,
    "totalValue": 125000000,
    "lowStockProducts": 12,
    "outOfStockProducts": 3,
    "byCategory": [
      {
        "categoryId": 5,
        "categoryName": "Dasturlash kitoblari",
        "productCount": 45,
        "totalStock": 550,
        "totalValue": 6875000
      }
    ]
  }
}
```

---

## 🛠️ Service Layer va Business Logic

### Repository Pattern

**Interface:** `src/ZiyoMarket.Data/Repositories/IRepository.cs`

```csharp
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    // Read Operations
    Task<TEntity> GetByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    // Write Operations
    Task<TEntity> AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);

    // Pagination
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = ""
    );
}
```

### Unit of Work Pattern

**Interface:** `src/ZiyoMarket.Data/UnitOfWorks/IUnitOfWork.cs`

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

**Usage Example:**

```csharp
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = _unitOfWork.Repository<Order>();
        _productRepository = _unitOfWork.Repository<Product>();
    }

    public async Task<Result<OrderDetailDto>> CreateOrderAsync(CreateOrderDto dto, int customerId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Validate products and stock
            foreach (var item in dto.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    return Result<OrderDetailDto>.Failure("Product not found");

                if (product.StockQuantity < item.Quantity)
                    return Result<OrderDetailDto>.Failure($"Not enough stock for {product.Name}");
            }

            // 2. Create order
            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = await GenerateOrderNumberAsync(),
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                // ... other properties
            };

            await _orderRepository.AddAsync(order);

            // 3. Update product stocks
            foreach (var item in dto.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                product.DecreaseStock(item.Quantity);
                await _productRepository.UpdateAsync(product);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result<OrderDetailDto>.Success(mappedOrder);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<OrderDetailDto>.Failure(ex.Message);
        }
    }
}
```

### Result Pattern

**Fayl:** `src/ZiyoMarket.Domain/Common/Result.cs`

```csharp
public class Result
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; }

    public static Result Success(string message = "Operation successful")
    {
        return new Result
        {
            IsSuccess = true,
            Message = message,
            StatusCode = 200
        };
    }

    public static Result Failure(string message, int statusCode = 400)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = new List<string> { message }
        };
    }
}

public class Result<T> : Result
{
    public T Data { get; set; }

    public static Result<T> Success(T data, string message = "Operation successful")
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = 200
        };
    }

    public static new Result<T> Failure(string message, int statusCode = 400)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = new List<string> { message }
        };
    }
}
```

---

## 🔐 Authentication va Authorization

### JWT Configuration

**Fayl:** `src/ZiyoMarket.Api/appsettings.json`

```json
{
  "JwtSettings": {
    "Key": "SuperSecretKeyForJwtDontShare123!",
    "Issuer": "ZiyoMarket",
    "Audience": "ZiyoMarketUsers",
    "ExpirationMinutes": 1440
  }
}
```

### JWT Token Generation

**Service Method:**

```csharp
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public string GenerateJwtToken(UserProfileDto user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserType", user.UserType.ToString()),
            new Claim(ClaimTypes.Role, user.UserType.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"])
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Authorization in Controllers

**Base Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    protected string GetCurrentUserType()
    {
        return User.FindFirst("UserType")?.Value ?? "";
    }

    protected bool IsAuthenticated()
    {
        return User.Identity?.IsAuthenticated ?? false;
    }

    protected bool IsCustomer()
    {
        return GetCurrentUserType() == "Customer";
    }

    protected bool IsSeller()
    {
        return GetCurrentUserType() == "Seller";
    }

    protected bool IsAdmin()
    {
        return GetCurrentUserType() == "Admin";
    }
}
```

**Usage in Controllers:**

```csharp
[Authorize(Roles = "Customer")]
public class CartController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.GetCartItemsAsync(customerId);
        return Ok(result);
    }
}

[Authorize(Roles = "Admin")]
public class ReportController : BaseController
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        // Only admins can access
        var result = await _reportService.GetDashboardStatisticsAsync();
        return Ok(result);
    }
}

[Authorize(Roles = "Admin,Seller")]
public class OrderController : BaseController
{
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        // Both admin and seller can update, but with different permissions
        var currentUserId = GetCurrentUserId();
        var currentUserType = GetCurrentUserType();

        var result = await _orderService.UpdateOrderStatusAsync(
            id, dto, currentUserId, currentUserType
        );

        return Ok(result);
    }
}
```

---

## 🎯 DTOs va AutoMapper

### AutoMapper Profile

**Fayl:** `src/ZiyoMarket.Service/Mapping/AutoMapperProfile.cs`

```csharp
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Customer mappings
        CreateMap<Customer, UserProfileDto>()
            .ForMember(dest => dest.FullName,
                       opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.UserType,
                       opt => opt.MapFrom(src => UserType.Customer));

        CreateMap<RegisterCustomerDto, Customer>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // Product mappings
        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.CategoryName,
                       opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.IsLowStock,
                       opt => opt.MapFrom(src => src.IsLowStock()))
            .ForMember(dest => dest.LikesCount,
                       opt => opt.MapFrom(src => src.ProductLikes.Count));

        CreateMap<Product, ProductDetailDto>()
            .IncludeBase<Product, ProductListDto>()
            .ForMember(dest => dest.Category,
                       opt => opt.MapFrom(src => src.Category));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        // Order mappings
        CreateMap<Order, OrderDetailDto>()
            .ForMember(dest => dest.Customer,
                       opt => opt.MapFrom(src => src.Customer))
            .ForMember(dest => dest.Items,
                       opt => opt.MapFrom(src => src.OrderItems));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName,
                       opt => opt.MapFrom(src => src.Product.Name));

        // And more mappings...
    }
}
```

### DTO Examples

**Product DTOs:**

```csharp
// List DTO (minimal data)
public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string QrCode { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; }
    public ProductStatus Status { get; set; }
    public string ImageUrl { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsLiked { get; set; }
    public int LikesCount { get; set; }
}

// Detail DTO (full data)
public class ProductDetailDto : ProductListDto
{
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto Category { get; set; }
    public decimal? Weight { get; set; }
    public string Dimensions { get; set; }
    public string Manufacturer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Create DTO
public class CreateProductDto
{
    [Required]
    [MinLength(2), MaxLength(200)]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string QrCode { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string ImageUrl { get; set; }
    public decimal? Weight { get; set; }
    public string Dimensions { get; set; }
    public string Manufacturer { get; set; }
}
```

---

## ✅ Validation va Error Handling

### FluentValidation

**Fayl:** `src/ZiyoMarket.Service/Validators/CreateProductValidator.cs`

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.QrCode)
            .NotEmpty().WithMessage("QR code is required")
            .Matches(@"^PRD-\d{3}$").WithMessage("QR code must be in format PRD-XXX");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category must be selected");
    }
}
```

### Global Error Handling Middleware

**Fayl:** `src/ZiyoMarket.Api/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var result = exception switch
        {
            ArgumentException => new Result
            {
                IsSuccess = false,
                Message = exception.Message,
                StatusCode = 400,
                Errors = new List<string> { exception.Message }
            },
            UnauthorizedAccessException => new Result
            {
                IsSuccess = false,
                Message = "Unauthorized access",
                StatusCode = 401
            },
            KeyNotFoundException => new Result
            {
                IsSuccess = false,
                Message = "Resource not found",
                StatusCode = 404
            },
            _ => new Result
            {
                IsSuccess = false,
                Message = "Internal server error",
                StatusCode = 500,
                Errors = new List<string> { exception.Message }
            }
        };

        context.Response.StatusCode = result.StatusCode;
        return context.Response.WriteAsJsonAsync(result);
    }
}
```

---

## 🎓 Best Practices

### 1. Naming Conventions

```csharp
// Entities: PascalCase, singular
public class Product { }
public class Order { }

// DTOs: PascalCase, descriptive suffix
public class CreateProductDto { }
public class ProductDetailDto { }
public class ProductListDto { }

// Interfaces: IPascalCase
public interface IProductService { }
public interface IRepository<T> { }

// Private fields: _camelCase
private readonly IProductService _productService;

// Public properties: PascalCase
public string FirstName { get; set; }

// Methods: PascalCase, verb-based
public async Task<Result> CreateProductAsync() { }
public async Task<Product> GetProductByIdAsync(int id) { }
```

### 2. Async/Await Pattern

```csharp
// Always use Async suffix for async methods
public async Task<Result<Product>> GetProductByIdAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    return Result<Product>.Success(product);
}

// Avoid async void (except event handlers)
// ❌ Bad
public async void ProcessOrder() { }

// ✅ Good
public async Task ProcessOrderAsync() { }
```

### 3. Dependency Injection

```csharp
// Register services in Program.cs or ServiceExtensions
public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Scoped: Per request lifecycle
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        // Singleton: Application lifetime
        services.AddSingleton<ICacheService, CacheService>();

        // Transient: New instance every time
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}
```

### 4. Repository Pattern Usage

```csharp
// ✅ Good: Using repository
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;

    public async Task<Product> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

// ❌ Bad: Direct DbContext usage in service
public class ProductService
{
    private readonly ZiyoMarketDbContext _context;

    public async Task<Product> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }
}
```

### 5. Error Handling

```csharp
// ✅ Good: Using Result pattern
public async Task<Result<Product>> CreateProductAsync(CreateProductDto dto)
{
    try
    {
        // Validation
        if (dto == null)
            return Result<Product>.Failure("Invalid data");

        // Business logic
        var product = _mapper.Map<Product>(dto);
        await _repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result<Product>.Success(product);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating product");
        return Result<Product>.Failure(ex.Message, 500);
    }
}

// ❌ Bad: Throwing exceptions
public async Task<Product> CreateProductAsync(CreateProductDto dto)
{
    var product = _mapper.Map<Product>(dto);
    await _repository.AddAsync(product);
    return product; // No error handling
}
```

---

## 🗂️ Migration va Database Management

### Migration Commands

```bash
# Yangi migration yaratish
dotnet ef migrations add MigrationName \
    --project src/ZiyoMarket.Data \
    --startup-project src/ZiyoMarket.Api

# Database ni yangilash
dotnet ef database update \
    --project src/ZiyoMarket.Data \
    --startup-project src/ZiyoMarket.Api

# Migration ni bekor qilish
dotnet ef migrations remove \
    --project src/ZiyoMarket.Data \
    --startup-project src/ZiyoMarket.Api

# Ma'lum bir migration gacha rollback qilish
dotnet ef database update MigrationName \
    --project src/ZiyoMarket.Data \
    --startup-project src/ZiyoMarket.Api

# SQL script generatsiya qilish
dotnet ef migrations script \
    --project src/ZiyoMarket.Data \
    --startup-project src/ZiyoMarket.Api \
    --output migration.sql
```

### Seed Data

**Test ma'lumotlar yaratish:**

```bash
# Admin user yaratish (manual)
POST /api/auth/register
{
  "firstName": "Admin",
  "lastName": "User",
  "phone": "+998901234567",
  "email": "admin@ziyomarket.uz",
  "password": "Admin@123"
}

# Kategoriyalar yaratish
POST /api/category/seed

# Mahsulotlar yaratish
POST /api/product/seed

# Sotuvchilar yaratish
POST /api/seller/seed

# Buyurtmalar yaratish
POST /api/order/seed
```

---

## 🧪 Testing va Debugging

### API Testing with HTTP File

**Fayl:** `src/ZiyoMarket.Api/ZiyoMarket.Api.http`

```http
### Variables
@baseUrl = https://localhost:5001/api
@token = your_jwt_token_here

### 1. Login
POST {{baseUrl}}/auth/login
Content-Type: application/json

{
  "phoneOrEmail": "admin@ziyomarket.uz",
  "password": "Admin@123",
  "userType": "Admin"
}

### 2. Get Products (with token)
GET {{baseUrl}}/product?pageNumber=1&pageSize=20
Authorization: Bearer {{token}}

### 3. Create Product
POST {{baseUrl}}/product
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Test Product",
  "description": "Test description",
  "qrCode": "PRD-999",
  "price": 50000,
  "stockQuantity": 100,
  "categoryId": 1
}
```

### Logging

```csharp
public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;

    public async Task<Result<Product>> CreateProductAsync(CreateProductDto dto)
    {
        _logger.LogInformation("Creating product: {ProductName}", dto.Name);

        try
        {
            // Business logic
            _logger.LogDebug("Product created successfully with ID: {ProductId}", product.Id);
            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName}", dto.Name);
            return Result<Product>.Failure(ex.Message, 500);
        }
    }
}
```

### Debug Configuration

**appsettings.Development.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ZiyoDb;User Id=postgres;Password=2001;"
  }
}
```

---

## 📚 Qo'shimcha Resurslar

- **Swagger UI:** `https://localhost:5001/swagger`
- **Database:** PostgreSQL (ZiyoDb)
- **EF Core Documentation:** https://docs.microsoft.com/ef/core/
- **ASP.NET Core Documentation:** https://docs.microsoft.com/aspnet/core/

---

**Version:** 1.0.0
**Last Updated:** 2025-01-30
**Author:** ZiyoMarket Development Team
