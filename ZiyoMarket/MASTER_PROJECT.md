# MASTER PROJECT DOCUMENTATION
## ZiyoMarket E-Commerce Platform

**Version:** 1.0
**Last Updated:** 2026-03-17
**Status:** Production-ready with ongoing development

---

# PROJECT OVERVIEW

ZiyoMarket is a professional multi-user e-commerce platform supporting:
- **Online Sales:** Customers purchase via mobile app
- **Offline Sales:** Sellers create orders in physical stores
- **Admin Management:** Comprehensive admin panel for system oversight
- **Cashback System:** 2% cashback on delivered orders
- **Multi-device Support:** Push notifications across devices
- **Payment Options:** Cash, Card, Bank Transfer, Cashback
- **Delivery Tracking:** Multiple delivery partners and statuses

**Primary Users:**
1. **Customers** - End users purchasing products via mobile
2. **Sellers** - Store staff creating offline orders
3. **Admins** - System administrators managing the platform

---

# TECH STACK

## Backend (REST API)
- **Framework:** ASP.NET Core Web API (.NET 8.0)
- **Database:** PostgreSQL 15+ (via Npgsql.EntityFrameworkCore)
- **ORM:** Entity Framework Core 8.0
- **Authentication:** JWT Bearer Tokens
- **Mapping:** AutoMapper 12.0
- **Logging:** Serilog (File + Console)
- **Documentation:** Swagger/OpenAPI 3.0
- **Security:** BCrypt.Net for password hashing
- **Push Notifications:** Firebase Admin SDK (FCM)
- **SMS:** Eskiz.uz API
- **Payments:** Click.uz integration

## Admin Panel (Desktop Application)
- **Framework:** WPF (.NET 8.0)
- **Architecture:** MVVM (CommunityToolkit.Mvvm)
- **UI Components:** MaterialDesignInXaml 5.0
- **DI Container:** Microsoft.Extensions.DependencyInjection
- **HTTP Client:** System.Net.Http
- **Serialization:** System.Text.Json (snake_case)
- **Logging:** Custom file-based logger

## Mobile (Separate Repository)
- **Framework:** Flutter (Dart)
- **NOT covered in this documentation**

---

# ARCHITECTURE

## Overall System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ZiyoMarket Ecosystem                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────┐       ┌─────────────┐      ┌─────────────┐│
│  │   Flutter   │       │  WPF Admin  │      │   Future    ││
│  │  Mobile App │◄─────►│    Panel    │      │  Web Admin  ││
│  │  (iOS/Andr) │       │  (Windows)  │      │  (Planned)  ││
│  └─────────────┘       └─────────────┘      └─────────────┘│
│         │                      │                     │       │
│         └──────────────────────┴─────────────────────┘       │
│                                │                              │
│                        ┌───────▼────────┐                    │
│                        │  ASP.NET Core  │                    │
│                        │   REST API     │                    │
│                        │   (Port 8080)  │                    │
│                        └───────┬────────┘                    │
│                                │                              │
│                   ┌────────────┴──────────────┐             │
│                   │                            │             │
│            ┌──────▼──────┐            ┌───────▼────────┐   │
│            │  PostgreSQL │            │  External APIs  │   │
│            │  Database   │            │  - Firebase FCM │   │
│            │             │            │  - Eskiz SMS    │   │
│            └─────────────┘            │  - Click.uz     │   │
│                                        └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Backend Layer Architecture (Clean Architecture)

```
┌──────────────────────────────────────────────────────────┐
│                    CLIENT LAYER                          │
│          (Mobile App / Admin Panel / Web)                │
└────────────────────┬─────────────────────────────────────┘
                     │ HTTP/JSON (snake_case)
                     │
┌────────────────────▼─────────────────────────────────────┐
│              PRESENTATION LAYER (Api)                     │
│  - Controllers (28 controllers)                           │
│  - Middleware (Global exception handler)                  │
│  - Extensions (Service registration, routing)             │
│  - BaseController (GetCurrentUserId, HandleResult)        │
└────────────────────┬─────────────────────────────────────┘
                     │ DTOs
                     │
┌────────────────────▼─────────────────────────────────────┐
│            BUSINESS LOGIC LAYER (Service)                 │
│  - Services (23 services)                                 │
│  - DTOs (Create, Update, Result, List)                    │
│  - Mapping (AutoMapper profiles)                          │
│  - Result Pattern (Success/Failure)                       │
│  - Validation & Business Rules                            │
└────────────────────┬─────────────────────────────────────┘
                     │ Entities
                     │
┌────────────────────▼─────────────────────────────────────┐
│             DATA ACCESS LAYER (Data)                      │
│  - DbContext (ZiyoMarketDbContext)                        │
│  - Repository<T> (Generic repository)                     │
│  - UnitOfWork (Transaction coordination)                  │
│  - Configurations (Fluent API)                            │
│  - Migrations (EF Core)                                   │
└────────────────────┬─────────────────────────────────────┘
                     │ ADO.NET / Npgsql
                     │
┌────────────────────▼─────────────────────────────────────┐
│               DOMAIN LAYER (Domain)                       │
│  - Entities (30 entities)                                 │
│  - Enums (17 enums)                                       │
│  - BaseEntity (Soft delete, timestamps)                   │
│  - Value Objects                                           │
└──────────────────────────────────────────────────────────┘
                     │
                     ▼
              ┌──────────────┐
              │  PostgreSQL  │
              │   Database   │
              └──────────────┘
```

**Dependency Flow:** Api → Service → Data → Domain (strict one-way)

**Key Principles:**
1. **Separation of Concerns:** Each layer has distinct responsibility
2. **Dependency Inversion:** Higher layers depend on abstractions (interfaces)
3. **Single Responsibility:** Each service/controller handles one area
4. **DRY:** Generic Repository eliminates data access duplication
5. **Testability:** Interface-based design enables mocking

---

# FOLDER STRUCTURE

## Backend Structure

```
ZiyoMarket/
│
├── src/
│   ├── ZiyoMarket.Domain/           # Domain Layer
│   │   ├── Common/                  # Base classes (BaseEntity, Result)
│   │   ├── Entities/                # Domain entities (30 entities)
│   │   │   ├── Users/              # Customer, Seller, Admin, User, Role, Permission
│   │   │   ├── Products/           # Product, Category, ProductCategory, Images
│   │   │   ├── Orders/             # Order, OrderItem, Discount, Cashback, PaymentProof
│   │   │   ├── Delivery/           # DeliveryPartner, OrderDelivery
│   │   │   ├── Notifications/      # Notification, DeviceToken, SmsLog
│   │   │   ├── Support/            # SupportChat, SupportMessage
│   │   │   ├── Contents/           # Content (CMS)
│   │   │   └── Systems/            # SystemSetting, DailySalesSummary
│   │   └── Enums/                   # 17 enums (OrderStatus, PaymentMethod, etc.)
│   │
│   ├── ZiyoMarket.Data/             # Data Access Layer
│   │   ├── Context/                 # ZiyoMarketDbContext.cs
│   │   ├── Repositories/            # Repository<T> implementation
│   │   ├── UnitOfWorks/             # UnitOfWork implementation
│   │   ├── Configurations/          # Fluent API configurations
│   │   └── Migrations/              # EF Core migrations
│   │
│   ├── ZiyoMarket.Service/          # Business Logic Layer
│   │   ├── Services/                # 23 service implementations
│   │   ├── Interfaces/              # Service interfaces
│   │   ├── DTOs/                    # 21 DTO categories (~100+ DTOs)
│   │   │   ├── Products/           # CreateProductDto, ProductListDto, etc.
│   │   │   ├── Orders/             # CreateOrderDto, OrderDetailDto, etc.
│   │   │   ├── Auth/               # LoginDto, RegisterDto, etc.
│   │   │   └── ... (18 more)       # Customers, Sellers, Cashback, etc.
│   │   ├── Mapping/                 # AutoMapper profiles
│   │   ├── Results/                 # Result pattern implementation
│   │   └── Helpers/                 # FileUploadSettings, EskizSmsClient
│   │
│   └── ZiyoMarket.Api/              # Presentation Layer
│       ├── Controllers/             # 28 REST controllers
│       │   ├── Auth/               # AuthController
│       │   ├── Products/           # ProductController, CategoryController
│       │   ├── Orders/             # OrderController, CartController
│       │   ├── Content/            # ContentController, BannerController
│       │   ├── Payments/           # ClickController, PaymentProofController
│       │   └── ... (8 more)        # Delivery, Support, Notifications, etc.
│       ├── Extensions/              # ServiceExtension, RouteTransformer
│       ├── Middleware/              # GlobalExceptionMiddleware
│       ├── Common/                  # ApiResponse, BaseController
│       ├── wwwroot/                 # Static files (images)
│       │   └── images/             # products/, categories/, banners/, users/
│       ├── Program.cs               # Application entry point
│       ├── appsettings.json         # Configuration (DB, JWT, SMS, Payment)
│       └── firebase-service-account.json  # Firebase credentials (gitignored)
│
└── CreateAdminTool/                 # Utility console app for creating admins
```

## Admin Panel Structure

```
ZiyoNurAdminPanel.Ui/
│
├── ViewModels/                      # MVVM ViewModels (25 total)
│   ├── Auth/                        # LoginViewModel
│   ├── Dashboard/                   # DashboardViewModel
│   ├── Products/                    # ProductsViewModel, ProductEditViewModel
│   ├── Categories/                  # CategoriesViewModel
│   ├── Orders/                      # OrdersViewModel, OrderDetailViewModel
│   ├── Customers/                   # CustomersViewModel
│   ├── Sellers/                     # SellersViewModel, SellerEditViewModel
│   ├── Admins/                      # AdminsViewModel, AdminEditViewModel
│   ├── Roles/                       # RolesViewModel, RoleDialogViewModel
│   ├── Banners/                     # BannersViewModel, BannerEditViewModel
│   ├── Content/                     # ContentViewModel
│   ├── Delivery/                    # DeliveryViewModel
│   ├── Support/                     # SupportViewModel
│   ├── Notifications/               # NotificationsViewModel
│   ├── Reports/                     # ReportsViewModel
│   └── Common/                      # ViewModelBase
│
├── Views/                           # XAML Views (23 total)
│   ├── Auth/                        # LoginWindow.xaml
│   ├── Dashboard/                   # DashboardView.xaml
│   ├── Products/                    # ProductsView.xaml, ProductEditDialog.xaml
│   ├── Categories/                  # CategoriesView.xaml, CategoryEditDialog.xaml
│   ├── Orders/                      # OrdersView.xaml, OrderDetailDialog.xaml
│   ├── Customers/                   # CustomersView.xaml
│   ├── Sellers/                     # SellersView.xaml, SellerEditDialog.xaml
│   ├── Admins/                      # AdminsView.xaml, AdminEditDialog.xaml
│   ├── Roles/                       # RolesView.xaml, RoleDialog.xaml
│   ├── Banners/                     # BannersView.xaml, BannerEditDialog.xaml
│   ├── Content/                     # ContentView.xaml
│   ├── Delivery/                    # DeliveryView.xaml
│   ├── Support/                     # SupportView.xaml
│   ├── Notifications/               # NotificationsView.xaml
│   ├── Reports/                     # ReportsView.xaml
│   └── MainWindow.xaml              # Main application window
│
├── Services/                        # API Services (23 total)
│   ├── Http/                        # ZiyoMarketHttpClient (central HTTP client)
│   ├── Interfaces/                  # Service interfaces
│   └── Implementations/             # Service implementations
│       ├── AuthService.cs           # Login, logout, token refresh
│       ├── ProductService.cs        # Product CRUD API calls
│       ├── OrderService.cs          # Order management
│       ├── BannerService.cs         # Banner management
│       ├── DashboardService.cs      # Dashboard statistics
│       ├── NavigationService.cs     # View navigation (Singleton)
│       ├── ExportService.cs         # Excel/PDF export
│       └── ... (16 more)
│
├── Models/                          # DTOs and Domain Models
│   ├── DTOs/                        # Data Transfer Objects (mirroring backend)
│   ├── Enums/                       # Enums (mirroring backend)
│   ├── Requests/                    # API request models
│   └── Responses/                   # API response models
│
├── Infrastructure/                  # Cross-cutting Concerns
│   ├── Configuration/               # ApiSettings, AppSettings, CacheSettings
│   ├── DependencyInjection/         # ServiceCollectionExtensions
│   ├── Logging/                     # LoggerService (file-based)
│   ├── Caching/                     # MemoryCacheService
│   └── Session/                     # SessionManager (JWT token storage)
│
├── Helpers/                         # Utility Classes
│   └── Converters/                  # XAML value converters (15 converters)
│       ├── BooleanConverters.cs    # BoolToVisibility, InverseBool, BoolToColor
│       ├── CollectionConverters.cs # CollectionContains
│       └── NotificationConverters.cs # Notification-specific converters
│
├── Resources/                       # Styles, Themes, Images
├── Assets/                          # Static assets
├── App.xaml                         # Application definition
├── App.xaml.cs                      # DI container setup
└── appsettings.json                 # Configuration (API base URL, theme)
```

**Key Folders Explained:**

**Backend:**
- **Domain/Entities:** Pure business entities with no dependencies
- **Data/Repositories:** Data access abstraction (generic Repository<T>)
- **Service/Services:** Business logic, validation, mapping
- **Api/Controllers:** HTTP endpoints, thin layer delegating to services
- **Api/wwwroot:** Static file hosting (images, documents)

**Admin Panel:**
- **ViewModels:** MVVM presentation logic, commands, data binding
- **Views:** XAML UI definitions
- **Services:** API client wrappers for backend communication
- **Infrastructure:** DI, logging, session management
- **Helpers/Converters:** XAML value converters for data binding

---

# EXISTING COMPONENTS

## Backend Services (23 Total)

| Service | Responsibility | Key Methods |
|---------|---------------|-------------|
| **AuthService** | Authentication, JWT tokens | Login, Register, RefreshToken, ResetPassword |
| **ProductService** | Product CRUD, search, stock | Create, Update, Delete, Search, GetByQR, UpdateStock |
| **CategoryService** | Hierarchical categories | Create, Update, Delete, GetHierarchy, GetSubcategories |
| **OrderService** | Order processing, status | CreateOrder, UpdateStatus, GetOrders, CancelOrder |
| **CartService** | Shopping cart operations | AddToCart, RemoveFromCart, UpdateQuantity, ClearCart |
| **CashbackService** | Cashback tracking | GetHistory, GetBalance, GetExpiring, CalculateCashback |
| **CustomerService** | Customer management | GetAll, GetById, GetStats, UpdateProfile |
| **SellerService** | Seller management | GetAll, GetById, GetPerformance, Create, Update |
| **AdminService** | Admin management | GetAll, GetById, Create, Update, Delete |
| **DeliveryService** | Delivery tracking | GetPartners, AssignDelivery, UpdateStatus |
| **SupportService** | Customer support chat | CreateChat, SendMessage, GetChats, CloseChat |
| **NotificationService** | Push notifications | SendNotification, SendBatch, GetHistory |
| **DeviceTokenService** | FCM token management | RegisterToken, GetDevices, CleanupExpired |
| **SmsService** | SMS sending (Eskiz.uz) | SendSMS, SendVerificationCode, VerifyCode |
| **ContentService** | CMS (Banners, Blogs, News, FAQ) | Create, Update, Delete, Publish, GetByType |
| **ReportService** | Sales reports, analytics | GetDashboardStats, GetSalesReport, GetChartData |
| **FileUploadService** | File upload to wwwroot | UploadImage, DeleteFile, GetFileUrl |
| **ClickService** | Click.uz payment integration | PreparePayment, VerifyPayment, CheckStatus |
| **FirebaseService** | Firebase SDK initialization | InitializeFirebase (Singleton) |
| **PermissionManagementService** | RBAC permissions | AssignPermissions, RemovePermissions, GetUserPermissions |
| **RoleManagementService** | RBAC roles | CreateRole, UpdateRole, AssignRole, GetRoles |
| **RolePermissionSeedService** | Seed default roles/permissions | SeedDefaultRoles, SeedDefaultPermissions |

## Backend Controllers (28 Total)

| Controller | Base Route | Authentication | Purpose |
|-----------|-----------|----------------|---------|
| **AuthController** | `/api/auth` | ❌ Public | Register, Login, RefreshToken, ResetPassword |
| **ProductController** | `/api/product` | ✅ Seller/Admin | Product CRUD, Search, QR lookup, Stock |
| **CategoryController** | `/api/category` | ✅ Seller/Admin | Category CRUD, Hierarchy |
| **SubcategoryController** | `/api/subcategory` | ✅ Seller/Admin | Subcategory operations |
| **CartController** | `/api/cart` | ✅ Customer | Add, Remove, Update, Clear cart |
| **OrderController** | `/api/order` | ✅ Customer/Seller/Admin | Order CRUD, Status updates |
| **CashbackController** | `/api/cashback` | ✅ Customer | History, Balance, Expiring |
| **ClickController** | `/api/click` | ❌ Public (webhooks) | Click.uz payment callbacks |
| **PaymentProofController** | `/api/payment-proof` | ✅ Customer/Admin | Upload/review manual payment proofs |
| **AdminController** | `/api/admin` | ✅ Admin | Admin CRUD |
| **SellerController** | `/api/seller` | ✅ Admin | Seller CRUD |
| **DeliveryController** | `/api/delivery` | ✅ Seller/Admin | Delivery partners, tracking |
| **ContentController** | `/api/content` | ✅ Admin | Blog, News, FAQ management |
| **BannerController** | `/api/banner` | ✅ Admin | Banner CRUD, Publish, Unpublish |
| **NotificationController** | `/api/notification` | ✅ Customer/Seller/Admin | In-app notifications |
| **NotificationApiController** | `/api/notification-api` | ✅ Admin | Notification API |
| **PushNotificationController** | `/api/push-notification` | ✅ Admin | FCM push notifications |
| **SupportController** | `/api/support` | ✅ Customer/Admin | Customer support chat |
| **ReportController** | `/api/report` | ✅ Admin | Sales reports, analytics |
| **PermissionController** | `/api/permission` | ✅ Admin | Permission management |
| **RoleManagementController** | `/api/role-management` | ✅ Admin | Role management |
| **SmsController** | `/api/sms` | ✅ Admin | Send SMS, verification codes |
| **FileUploadController** | `/api/file-upload` | ✅ Authenticated | Upload/delete/get files |
| **ImageUploadController** | `/api/image-upload` | ✅ Authenticated | Legacy image upload |
| **TestImageController** | `/api/test-image` | ✅ Authenticated | Image upload testing |
| **SeedDataController** | `/api/seed-data` | ❌ Public (dev only) | Seed test data |
| **AdminSeedController** | `/api/admin-seed` | ❌ Public (dev only) | Seed admin data |
| **CustomerController** | `/api/customer` | ✅ Admin | Customer management |

## Admin Panel ViewModels (25 Total)

| ViewModel | Purpose | Key Commands |
|-----------|---------|-------------|
| **LoginViewModel** | Authentication | LoginCommand |
| **MainViewModel** | Main window navigation | Navigate{Module}Command (15 commands) |
| **DashboardViewModel** | Statistics, charts | LoadStatsCommand, RefreshCommand |
| **ProductsViewModel** | Product list | LoadProductsCommand, CreateCommand, EditCommand, DeleteCommand |
| **ProductEditViewModel** | Create/Edit product dialog | SaveCommand, UploadImageCommand, SelectCategoriesCommand |
| **CategoriesViewModel** | Category tree | LoadCategoriesCommand, CreateCommand, EditCommand, DeleteCommand |
| **OrdersViewModel** | Order list, filtering | LoadOrdersCommand, FilterCommand, UpdateStatusCommand |
| **OrderDetailViewModel** | Order detail dialog | LoadOrderCommand, CancelOrderCommand |
| **CustomersViewModel** | Customer list | LoadCustomersCommand, ViewStatsCommand |
| **SellersViewModel** | Seller list | LoadSellersCommand, CreateCommand, EditCommand, DeleteCommand |
| **SellerEditViewModel** | Create/Edit seller dialog | SaveCommand |
| **AdminsViewModel** | Admin list | LoadAdminsCommand, CreateCommand, EditCommand, DeleteCommand |
| **AdminEditViewModel** | Create/Edit admin dialog | SaveCommand |
| **PermissionManagementViewModel** | Permission assignment | LoadPermissionsCommand, AssignCommand, RemoveCommand |
| **RolesViewModel** | Role list | LoadRolesCommand, CreateCommand, EditCommand, DeleteCommand |
| **RoleDialogViewModel** | Create/Edit role dialog | SaveCommand, SelectPermissionsCommand |
| **BannersViewModel** | Banner list | LoadBannersCommand, CreateCommand, EditCommand, DeleteCommand, PublishCommand, UnpublishCommand, SeedCommand |
| **BannerEditViewModel** | Create/Edit banner dialog | SaveCommand, UploadImageCommand, SelectImageCommand |
| **ContentViewModel** | CMS content | LoadContentCommand, CreateCommand, EditCommand, DeleteCommand, PublishCommand |
| **DeliveryViewModel** | Delivery tracking | LoadDeliveriesCommand, AssignDeliveryCommand, UpdateStatusCommand |
| **SupportViewModel** | Support chat | LoadChatsCommand, SendMessageCommand, CloseChatCommand |
| **NotificationsViewModel** | Send push notifications | LoadNotificationsCommand, SendCommand, SendBatchCommand |
| **ReportsViewModel** | Sales reports, export | LoadReportsCommand, ExportCommand, GenerateChartCommand |

## Domain Entities (30 Total)

### User Entities (8)
| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| **Customer** | End users | Id, FirstName, LastName, Phone, Email, CashbackBalance, PasswordHash |
| **Seller** | Store staff | Id, FirstName, LastName, Phone, Email, PasswordHash, StoreId |
| **Admin** | Administrators | Id, FirstName, LastName, Email, PasswordHash, IsSuperAdmin |
| **User** | **NEW** Unified user | Id, FirstName, LastName, Email, Phone, PasswordHash, UserType |
| **Role** | **NEW** User roles | Id, Name, Description, IsActive |
| **Permission** | **NEW** Permissions | Id, Name, Description, Category |
| **UserRole** | **NEW** User-Role M:N | UserId, RoleId |
| **RolePermission** | **NEW** Role-Permission M:N | RoleId, PermissionId |

### Product Entities (6)
| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| **Product** | Products for sale | Id, Name, Description, Price, DiscountPrice, StockQuantity, MinStockLevel, QRCode, IsActive |
| **Category** | Hierarchical categories | Id, Name, ParentId, Level, DisplayOrder |
| **ProductCategory** | Product-Category M:N | ProductId, CategoryId |
| **ProductImage** | Product images | Id, ProductId, ImageUrl, DisplayOrder, IsPrimary |
| **ProductLike** | Customer product likes | Id, CustomerId, ProductId, CreatedAt |
| **CartItem** | Shopping cart items | Id, CustomerId, ProductId, Quantity, UnitPrice |

### Order Entities (6)
| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| **Order** | Customer orders | Id, CustomerId, SellerId, OrderNumber, TotalPrice, FinalPrice, Status, PaymentMethod, PaymentStatus, OrderDate |
| **OrderItem** | Order line items | Id, OrderId, ProductId, Quantity, UnitPrice, Subtotal, DiscountAmount |
| **OrderDiscount** | Discounts applied | Id, OrderId, DiscountReasonId, Amount, AppliedBySellerId |
| **DiscountReason** | Predefined discount reasons | Id, Name, Description, IsActive |
| **CashbackTransaction** | Cashback history | Id, CustomerId, OrderId, Amount, TransactionType, ExpiryDate |
| **PaymentProof** | Manual payment proofs | Id, OrderId, CustomerId, ImageUrl, TransactionReference, Status, ReviewedByAdminId |

### Delivery Entities (2)
| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| **DeliveryPartner** | Delivery providers | Id, Name, Phone, ContactPerson, IsActive |
| **OrderDelivery** | Delivery tracking | Id, OrderId, DeliveryPartnerId, Status, EstimatedDeliveryDate, ActualDeliveryDate, TrackingNumber |

### Notification Entities (3)
| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| **Notification** | In-app notifications | Id, UserId, UserType, Type, Title, Message, IsRead |
| **DeviceToken** | FCM device tokens | Id, UserId, UserType, Token, DeviceOS, DeviceModel, LastUsedAt |
| **SmsLog** | SMS history | Id, RecipientPhone, RecipientId, RecipientType, Message, Purpose, Status, ProviderId |

### Support Entities (2)
| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| **SupportChat** | Support chat sessions | Id, CustomerId, Subject, Status, AssignedToAdminId |
| **SupportMessage** | Chat messages | Id, SupportChatId, SenderId, SenderType, Message, IsRead |

### Content & System Entities (3)
| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| **Content** | CMS (Banners, Blogs, News, FAQ) | Id, Type, Title, Description, ImageUrl, ExternalUrl, IsPublished, TargetAudience, ViewCount, ClickCount, ValidFrom, ValidUntil, SortOrder |
| **SystemSetting** | Key-value settings | Id, Key, Value, Description |
| **DailySalesSummary** | Daily aggregates | Id, Date, TotalOrders, TotalRevenue, TotalCashbackGiven |

## Data Transfer Objects (100+ Total)

### Naming Pattern

**Input DTOs (for Create/Update):**
- `Create{Entity}Dto` - Required fields for creation
- `Update{Entity}Dto` - Updatable fields + Id

**Output DTOs (for Read):**
- `{Entity}ResultDto` - Single entity detail
- `{Entity}ListDto` - Summary for lists
- `{Entity}DetailDto` - Full detail view

### Example: Product DTOs

```csharp
// Input
CreateProductDto {
    string Name
    string Description
    decimal Price
    decimal? DiscountPrice
    int StockQuantity
    int MinStockLevel
    List<int> CategoryIds
    List<string> ImageUrls
    bool IsActive
}

// Output (List)
ProductListDto {
    int Id
    string Name
    decimal Price
    decimal? DiscountPrice
    string ImageUrl
    bool IsAvailable
    bool IsLowStock
    List<string> CategoryNames
}

// Output (Detail)
ProductDetailDto : ProductListDto {
    // Inherits all from ProductListDto +
    string Description
    int StockQuantity
    int MinStockLevel
    string QRCode
    bool IsLikedByCurrentUser
    List<int> CategoryIds
    List<string> CategoryPaths
    List<string> ImageUrls
    DateTime CreatedAt
    DateTime UpdatedAt
}
```

---

# DATA FLOW

## Request Flow (Backend)

```
┌─────────────┐
│   Client    │ (Mobile/Admin Panel)
│  (HTTP/JSON)│
└──────┬──────┘
       │ 1. HTTP Request
       │    POST /api/product
       │    Header: Authorization: Bearer {token}
       │    Body: CreateProductDto (snake_case JSON)
       │
       ▼
┌──────────────────────┐
│  Middleware Pipeline │
├──────────────────────┤
│ 1. Routing           │ Convert snake_case URL to PascalCase controller
│ 2. Authentication    │ Validate JWT token, extract claims
│ 3. Authorization     │ Check [Authorize] attribute
│ 4. Model Binding     │ Deserialize JSON to DTO (snake_case → PascalCase)
│ 5. Validation        │ ModelState validation
└──────┬───────────────┘
       │ 2. Route to Controller
       │
       ▼
┌──────────────────────┐
│  ProductController   │ (Presentation Layer)
├──────────────────────┤
│ [HttpPost]           │
│ public async Task<IActionResult> Create(CreateProductDto dto)
│ {
│   int userId = GetCurrentUserId(); // Extract from JWT
│   var result = await _productService.CreateAsync(dto, userId);
│   return HandleResult(result); // Convert Result<T> to ApiResponse
│ }
└──────┬───────────────┘
       │ 3. Call Service
       │
       ▼
┌──────────────────────┐
│  ProductService      │ (Business Logic Layer)
├──────────────────────┤
│ public async Task<Result<ProductDetailDto>> CreateAsync(CreateProductDto dto, int createdBy)
│ {
│   // 1. Validation
│   if (string.IsNullOrEmpty(dto.Name))
│     return Result<ProductDetailDto>.BadRequest("Name is required");
│
│   // 2. Map DTO to Entity
│   var product = _mapper.Map<Product>(dto);
│   product.CreatedBy = createdBy;
│
│   // 3. Business Logic
│   product.QRCode = GenerateQRCode();
│
│   // 4. Persistence
│   await _unitOfWork.Products.InsertAsync(product);
│
│   // 5. Save & Commit
│   await _unitOfWork.SaveChangesAsync();
│
│   // 6. Map Entity to DTO
│   var resultDto = _mapper.Map<ProductDetailDto>(product);
│   return Result<ProductDetailDto>.Success(resultDto);
│ }
└──────┬───────────────┘
       │ 4. Access Data via UnitOfWork
       │
       ▼
┌──────────────────────┐
│    UnitOfWork        │ (Data Access Coordination)
├──────────────────────┤
│ IRepository<Product> Products { get; }
│
│ await SaveChangesAsync() → DbContext.SaveChangesAsync()
│ - Auto-set CreatedAt/UpdatedAt timestamps
│ - Soft delete logic
└──────┬───────────────┘
       │ 5. Repository Operations
       │
       ▼
┌──────────────────────┐
│ Repository<Product>  │ (Generic Repository)
├──────────────────────┤
│ public async Task InsertAsync(Product entity)
│ {
│   await _dbSet.AddAsync(entity);
│ }
└──────┬───────────────┘
       │ 6. EF Core
       │
       ▼
┌──────────────────────┐
│  ZiyoMarketDbContext │ (DbContext)
├──────────────────────┤
│ DbSet<Product> Products { get; set; }
│
│ - Global query filter (DeletedAt == null)
│ - SaveChangesAsync override (auto timestamps)
│ - Entity configurations (Fluent API)
└──────┬───────────────┘
       │ 7. ADO.NET / Npgsql
       │
       ▼
┌──────────────────────┐
│    PostgreSQL        │
│     Database         │
└──────────────────────┘
       │ 8. Return rows
       │
       ▼
    (Back up the chain)
       │
       ▼
┌──────────────────────┐
│  Response Pipeline   │
├──────────────────────┤
│ 1. Map Entity to DTO │ AutoMapper
│ 2. Wrap in Result<T> │ Result.Success(dto)
│ 3. Convert to ApiResponse │ HandleResult(result)
│ 4. Serialize to JSON │ Snake_case naming policy
│ 5. Return HTTP 200   │
└──────┬───────────────┘
       │ 9. HTTP Response
       │    Status: 200 OK
       │    Body: {
       │      "status": true,
       │      "message": "Success",
       │      "data": { product_detail_dto (snake_case) }
       │    }
       ▼
┌─────────────┐
│   Client    │
└─────────────┘
```

## Data Flow (Admin Panel)

```
┌─────────────┐
│  User Input │ (Button click, form submit)
└──────┬──────┘
       │ 1. User Action
       │
       ▼
┌──────────────────────┐
│   View (XAML)        │ (Presentation)
├──────────────────────┤
│ <Button Command="{Binding CreateProductCommand}" />
│ <TextBox Text="{Binding ProductName}" />
└──────┬───────────────┘
       │ 2. Data Binding
       │
       ▼
┌──────────────────────┐
│  ViewModel           │ (MVVM Logic)
├──────────────────────┤
│ public ICommand CreateProductCommand { get; }
│
│ private async Task CreateProduct()
│ {
│   IsLoading = true;
│
│   var dto = new CreateProductDto {
│     Name = ProductName,
│     Price = ProductPrice,
│     // ... other fields
│   };
│
│   var result = await _productService.CreateAsync(dto);
│
│   if (result.Status) {
│     // Success
│     await LoadProducts(); // Refresh list
│     CloseDialog();
│   } else {
│     // Error
│     ErrorMessage = result.Message;
│   }
│
│   IsLoading = false;
│ }
└──────┬───────────────┘
       │ 3. Call Service
       │
       ▼
┌──────────────────────┐
│  ProductService      │ (API Client)
│  (Admin Panel)       │
├──────────────────────┤
│ public async Task<ApiResponse<ProductDetailDto>> CreateAsync(CreateProductDto dto)
│ {
│   return await _httpClient.PostAsync<ProductDetailDto>("/product", dto);
│ }
└──────┬───────────────┘
       │ 4. HTTP Request
       │
       ▼
┌──────────────────────┐
│ ZiyoMarketHttpClient │ (Central HTTP Client)
├──────────────────────┤
│ public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
│ {
│   // 1. Add Authorization header
│   _httpClient.DefaultRequestHeaders.Authorization =
│     new AuthenticationHeaderValue("Bearer", _sessionManager.AccessToken);
│
│   // 2. Serialize to JSON (snake_case)
│   var json = JsonSerializer.Serialize(data, _jsonOptions);
│
│   // 3. Send HTTP POST
│   var response = await _httpClient.PostAsync(
│     $"{_apiSettings.BaseUrl}{endpoint}",
│     new StringContent(json, Encoding.UTF8, "application/json")
│   );
│
│   // 4. Deserialize response
│   var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
│   return apiResponse;
│ }
└──────┬───────────────┘
       │ 5. HTTP/JSON
       │
       ▼
┌──────────────────────┐
│  Backend API         │ (ASP.NET Core)
│  (See Backend Flow)  │
└──────┬───────────────┘
       │ 6. HTTP Response
       │
       ▼
┌──────────────────────┐
│  ViewModel           │ (Update UI State)
├──────────────────────┤
│ Products.Add(newProduct);
│ StatusMessage = "Product created successfully";
└──────┬───────────────┘
       │ 7. Property Changed
       │
       ▼
┌──────────────────────┐
│   View (XAML)        │ (UI Update)
├──────────────────────┤
│ DataGrid refreshes automatically via data binding
└──────────────────────┘
```

## Authentication Flow

```
┌─────────────┐
│  Login Form │
└──────┬──────┘
       │ 1. User enters credentials
       │    Email: admin@ziyomarket.uz
       │    Password: Admin@123
       ▼
┌──────────────────────┐
│  LoginViewModel      │
├──────────────────────┤
│ LoginCommand executes:
│ var dto = new LoginDto { Email, Password };
│ var result = await _authService.LoginAsync(dto);
└──────┬───────────────┘
       │ 2. API Call
       ▼
┌──────────────────────┐
│  Backend API         │
│  POST /api/auth/login│
├──────────────────────┤
│ AuthService.LoginAsync(dto)
│ 1. Find user by email
│ 2. Verify password (BCrypt)
│ 3. Generate JWT tokens
│ 4. Return tokens
└──────┬───────────────┘
       │ 3. Response
       │    {
       │      "status": true,
       │      "data": {
       │        "access_token": "eyJhbGci...",
       │        "refresh_token": "dGhpcyBpc...",
       │        "user_id": 2,
       │        "email": "admin@ziyomarket.uz",
       │        "user_type": "Admin"
       │      }
       │    }
       ▼
┌──────────────────────┐
│  SessionManager      │ (Token Storage)
├──────────────────────┤
│ AccessToken = response.Data.AccessToken
│ RefreshToken = response.Data.RefreshToken
│ UserId = response.Data.UserId
│ UserEmail = response.Data.Email
│ IsAuthenticated = true
└──────┬───────────────┘
       │ 4. Navigate
       ▼
┌──────────────────────┐
│  MainWindow          │ (Dashboard)
└──────────────────────┘

All subsequent API calls include:
Header: Authorization: Bearer {AccessToken}
```

---

# PATTERNS & CONVENTIONS

## Architecture Style

**Backend:** Clean Architecture (Onion Architecture)
- **Domain-Centric:** Domain entities have no dependencies
- **Dependency Inversion:** Higher layers depend on interfaces
- **Separation of Concerns:** Each layer has distinct responsibility

**Frontend:** MVVM (Model-View-ViewModel)
- **Data Binding:** Two-way binding between View and ViewModel
- **Commands:** ICommand pattern for user actions
- **Dependency Injection:** Constructor injection for services

## Naming Rules

### Backend

**Entities:** Singular, PascalCase
- `Product`, `Order`, `Customer`
- Junction tables: `ProductCategory`, `UserRole`

**Services:**
- Interface: `I{Entity}Service` → `IProductService`
- Implementation: `{Entity}Service` → `ProductService`
- Methods: `{Action}Async` → `GetByIdAsync`, `CreateAsync`

**DTOs:**
- Input: `Create{Entity}Dto`, `Update{Entity}Dto`
- Output: `{Entity}ResultDto`, `{Entity}ListDto`, `{Entity}DetailDto`

**Controllers:**
- Name: `{Entity}Controller` → `ProductController`
- Route: `/api/{entity}` → `/api/product` (snake_case via transformer)
- Actions: `GetById`, `Create`, `Update`, `Delete`

**Repository Methods:**
- `GetByIdAsync(id)`, `GetAllAsync()`, `FindAsync(predicate)`
- `InsertAsync(entity)`, `UpdateAsync(entity)`, `DeleteAsync(id)`
- `SoftDelete(entity)`, `SaveAsync()`

**Database:**
- Tables: PascalCase → `Products`, `Orders`
- Columns: PascalCase → `Id`, `Name`, `CreatedAt`
- Foreign Keys: `{Entity}Id` → `CustomerId`, `ProductId`

### Frontend (Admin Panel)

**ViewModels:**
- Name: `{Feature}ViewModel` → `ProductsViewModel`, `DashboardViewModel`
- Properties: PascalCase → `Products`, `SelectedProduct`, `IsLoading`
- Commands: `{Action}Command` → `LoadProductsCommand`, `SaveCommand`

**Views:**
- UserControl: `{Feature}View.xaml` → `ProductsView.xaml`
- Dialog: `{Feature}Dialog.xaml` → `ProductEditDialog.xaml`
- Window: `{Feature}Window.xaml` → `LoginWindow.xaml`

**Services:**
- Interface: `I{Feature}Service` → `IProductService`
- Implementation: `{Feature}Service` → `ProductService`

**Models:**
- DTOs mirror backend (PascalCase in C#, snake_case in JSON)

## JSON Serialization

**Backend:**
- **Output:** Snake_case via `SnakeCaseNamingPolicy`
  - `FirstName` → `"first_name"`
  - `TotalAmount` → `"total_amount"`
- **Input:** Auto-maps snake_case → PascalCase

**Frontend:**
- **Output:** Snake_case (matches backend)
- **Input:** Auto-maps snake_case → PascalCase

**Example:**
```json
// API Response (Backend)
{
  "status": true,
  "message": "Success",
  "data": {
    "product_id": 1,
    "product_name": "Laptop",
    "unit_price": 1500000,
    "is_available": true,
    "created_at": "2026-03-17 10:30:00"
  }
}

// C# DTO (Backend/Frontend)
public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Code Patterns

### Repository Pattern (Backend)

```csharp
// Service layer
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<ProductDto>> GetByIdAsync(int id)
    {
        // Use repository via UnitOfWork
        var product = await _unitOfWork.Products.GetByIdAsync(id,
            includes: new[] { "ProductCategories.Category", "Images" });

        if (product == null)
            return Result<ProductDto>.NotFound("Product not found");

        var dto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }
}
```

### MVVM Pattern (Frontend)

```csharp
// ViewModel
public class ProductsViewModel : ViewModelBase
{
    private ObservableCollection<ProductListDto> _products;
    public ObservableCollection<ProductListDto> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    public ICommand LoadProductsCommand { get; }

    public ProductsViewModel(IProductService productService)
    {
        LoadProductsCommand = new AsyncRelayCommand(LoadProducts);
    }

    private async Task LoadProducts()
    {
        IsLoading = true;
        var response = await _productService.GetAllAsync();
        Products = new ObservableCollection<ProductListDto>(response.Data);
        IsLoading = false;
    }
}

// View (XAML)
<Button Command="{Binding LoadProductsCommand}" Content="Refresh" />
<DataGrid ItemsSource="{Binding Products}" />
```

### Result Pattern (Backend)

```csharp
// Service returns Result<T>
public async Task<Result<OrderDto>> CreateOrderAsync(CreateOrderDto dto)
{
    // Validation
    if (dto.Items == null || !dto.Items.Any())
        return Result<OrderDto>.BadRequest("Order must have items");

    // Business logic
    var order = Order.Create(dto);
    await _unitOfWork.Orders.InsertAsync(order);
    await _unitOfWork.SaveChangesAsync();

    // Success
    var orderDto = _mapper.Map<OrderDto>(order);
    return Result<OrderDto>.Success(orderDto);
}

// Controller auto-converts to ApiResponse
[HttpPost]
public async Task<IActionResult> Create(CreateOrderDto dto)
{
    var result = await _orderService.CreateOrderAsync(dto);
    return HandleResult(result); // Returns ApiResponse<OrderDto>
}
```

### Dependency Injection (Both)

```csharp
// Backend (ServiceExtension.cs)
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Frontend (ServiceCollectionExtensions.cs)
services.AddTransient<IProductService, ProductService>();
services.AddTransient<ProductsViewModel>();
services.AddTransient<ProductsView>();
services.AddSingleton<ZiyoMarketHttpClient>();
```

---

# CURRENT PROBLEMS

## 🔴 CRITICAL ISSUES

### 1. String-based Date Storage

**Problem:** All dates stored as strings in "yyyy-MM-dd HH:mm:ss" format

**Impact:**
- ❌ Cannot use `DateTime.Parse()` in EF Core LINQ queries
- ❌ No database date functions (date arithmetic, filtering)
- ❌ Performance degradation on date queries
- ❌ Sorting requires parsing
- ❌ Timezone handling unclear

**Current Workaround (Applied):**
```csharp
// ❌ WRONG - EF Core translation error
var orders = await _unitOfWork.Orders
    .FindAsync(o => DateTime.Parse(o.OrderDate) >= startDate);

// ✅ CORRECT - String comparison
var startDateStr = startDate.ToString("yyyy-MM-dd");
var orders = await _unitOfWork.Orders
    .FindAsync(o => string.Compare(o.OrderDate, startDateStr) >= 0);
```

**Long-term Solution:** Database migration to DateTime columns

**Migration Complexity:** ⚠️ **HIGH** - affects all 30 tables

**Files Affected:**
- `Domain/Common/BaseEntity.cs` (CreatedAt, UpdatedAt, DeletedAt)
- `Domain/Entities/Orders/Order.cs` (OrderDate)
- All services using date filtering (7+ services)
- All mappers parsing dates (9+ profiles)

**Recommendation:** Prioritize migration in next major version

---

### 2. Computed Properties in EF Queries

**Problem:** Computed properties like `IsDeleted`, `IsLowStock`, `IsOutOfStock` cannot be used in LINQ queries

**Error:** "The LINQ expression could not be translated"

**Examples:**
```csharp
// Entity (Product.cs)
public bool IsLowStock => StockQuantity <= MinStockLevel;
public bool IsOutOfStock => StockQuantity <= 0;

// ❌ WRONG - Runtime error
var lowStockProducts = await _unitOfWork.Products
    .FindAsync(p => p.IsLowStock);

// ✅ CORRECT
var lowStockProducts = await _unitOfWork.Products
    .FindAsync(p => p.StockQuantity <= p.MinStockLevel);
```

**Current Status:** Fixed in ReportService.cs, may exist elsewhere

**Solution:** Always use direct property comparisons in queries, computed properties only for in-memory checks

---

### 3. Duplicate User Systems

**Problem:** Two user authentication systems coexist

**Legacy System (Current Production):**
- `Customer`, `Seller`, `Admin` (3 separate tables)
- Used by AuthController, all services

**New RBAC System (Partial Implementation):**
- `User`, `Role`, `Permission`, `UserRole`, `RolePermission` (5 tables)
- Partially implemented controllers/services
- No migration path defined

**Impact:**
- 🔴 Inconsistent authorization logic
- 🔴 Code duplication (2 authentication flows)
- 🔴 Confusion for new developers
- 🔴 Difficult to maintain

**Recommendation:**
1. Complete RBAC implementation
2. Create migration tool (Customer/Seller/Admin → User)
3. Deprecate legacy tables
4. Update all services to use unified system

---

## 🟡 MODERATE ISSUES

### 4. Code Duplication

**Date Parsing Logic:**
- Duplicated in 9+ AutoMapper profiles
- Manual try-catch in each mapper

**Solution:** Extract to helper extension method
```csharp
public static class DateExtensions
{
    public static DateTime ParseOrDefault(this string? dateStr, DateTime defaultValue = default)
    {
        return DateTime.TryParse(dateStr, out var date) ? date : defaultValue;
    }
}
```

**Validation Logic:**
- Phone number validation in multiple services
- Email validation in multiple services

**Solution:** Create `ValidationHelper` class with reusable methods

---

### 5. File Upload Limitations

**Problem:** Railway deployment - files in `wwwroot/` are ephemeral (deleted on redeploy)

**Current Status:** Working in development, **broken in production**

**Impact:**
- ❌ Product images lost on redeploy
- ❌ Banner images lost
- ❌ User avatars lost
- ❌ Payment proof screenshots lost

**Solutions:**
1. **Railway Volumes** - Mount persistent volume (recommended)
2. **Cloud Storage** - Migrate to AWS S3 / Cloudinary / Azure Blob Storage
3. **External CDN** - Use dedicated image hosting

**Recommendation:** Implement cloud storage (S3) before production launch

---

### 6. Security Concerns

**Hardcoded Secrets:**
- ⚠️ Firebase service account JSON in codebase (mitigated by `.gitignore`)
- ⚠️ Privileged user verification codes in `appsettings.json`

**Token Storage:**
- ⚠️ Admin panel stores tokens in-memory (`SessionManager`)
- ⚠️ No token encryption
- ⚠️ No token refresh on expiry (partially implemented)

**File Upload:**
- ⚠️ Validation relies on file extension only
- ⚠️ No magic byte checking
- ⚠️ No virus scanning
- ⚠️ No rate limiting

**Recommendations:**
1. Use environment variables for all secrets
2. Encrypt tokens in SessionManager
3. Implement file magic byte validation
4. Add antivirus scanning integration
5. Add rate limiting on file upload endpoints

---

### 7. Architectural Inconsistencies

**Multiple Result Implementations:**
- `Domain.Common.Result`
- `Service.Results.Result`

**Recommendation:** Standardize on one implementation

**Inconsistent Async Methods:**
- Some services throw exceptions
- Some return `Result<T>`

**Recommendation:** Standardize on Result pattern for all service methods

**Legacy Files:**
- `ImageUploadController.cs` (redundant with FileUploadController)
- `TestImageController.cs` (dev-only, should be removed)

**Recommendation:** Remove unused controllers

---

## 🟢 MINOR ISSUES

### 8. Missing Documentation

**API Documentation:**
- ✅ Swagger enabled
- ⚠️ Some endpoints lack XML comments
- ⚠️ No example requests/responses

**Code Documentation:**
- ⚠️ Some complex business logic lacks comments
- ⚠️ No architecture decision records (ADRs)

**Recommendation:** Add XML documentation to all public APIs

---

### 9. Testing

**Current Status:**
- ❌ No unit tests
- ❌ No integration tests
- ❌ Manual testing only

**Recommendation:**
1. Add unit tests for critical services (OrderService, AuthService)
2. Add integration tests for key workflows
3. Use xUnit + Moq + FluentAssertions

---

### 10. Performance

**Potential Issues:**
- ⚠️ No caching (except SMS verification codes)
- ⚠️ N+1 query potential in some endpoints
- ⚠️ No query optimization analysis

**Recommendation:**
1. Implement response caching for frequently accessed data
2. Add EF Core query logging to detect N+1 queries
3. Add database indexes on frequently queried columns

---

# RULES FOR FUTURE DEVELOPMENT

## ⚠️ CRITICAL RULES - NEVER VIOLATE

### 1. ❌ DO NOT Rewrite Existing Working Code

**Rule:** If code works, DO NOT rewrite it from scratch

**Reason:** Risk of introducing bugs, breaking existing functionality

**Allowed:**
- ✅ Refactoring for clarity
- ✅ Extracting duplicated logic
- ✅ Fixing bugs

**Not Allowed:**
- ❌ Rewriting entire services
- ❌ Changing architecture without approval
- ❌ Replacing working patterns with "better" ones

**Example:**
```csharp
// ❌ WRONG - Don't replace working repository with Dapper
// Just because you prefer Dapper doesn't mean you should rewrite

// ✅ CORRECT - Extend existing repository if needed
public interface IRepository<T>
{
    // Existing methods...
    Task<T> GetByIdAsync(int id);

    // Add new method if needed
    Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids);
}
```

---

### 2. ✅ ALWAYS Extend Instead of Replace

**Rule:** Add new features by extending existing code, not replacing it

**Pattern:**
1. Understand existing implementation
2. Identify extension point
3. Add new functionality via inheritance, composition, or new methods
4. Maintain backward compatibility

**Example:**
```csharp
// ✅ CORRECT - Extend existing service
public class ProductService : IProductService
{
    // Existing methods...
    public Task<Result<ProductDto>> GetByIdAsync(int id) { ... }

    // Add new method
    public Task<Result<List<ProductDto>>> GetByBarcodeAsync(string barcode)
    {
        // New functionality
    }
}

// ❌ WRONG - Don't replace existing GetByIdAsync with "better" version
// unless there's a critical bug
```

---

### 3. 🎯 Follow Existing Patterns

**Rule:** Use the same patterns as existing code

**Patterns to Follow:**

**Service Layer:**
- All services implement interface (`I{Entity}Service`)
- All methods return `Result<T>`
- All methods are async (`async Task<Result<T>>`)
- All services use `IUnitOfWork` for data access

**Controllers:**
- All inherit from `BaseController`
- All use `[Authorize]` for protected endpoints
- All use `HandleResult()` for response mapping
- All return `IActionResult`

**DTOs:**
- Input: `Create{Entity}Dto`, `Update{Entity}Dto`
- Output: `{Entity}ResultDto`, `{Entity}ListDto`, `{Entity}DetailDto`

**Repository:**
- Use generic `Repository<T>` via `IUnitOfWork`
- No custom repositories without approval

**Example:**
```csharp
// ✅ CORRECT - New service follows existing pattern
public interface IDiscountService
{
    Task<Result<DiscountDto>> GetByIdAsync(int id);
    Task<Result<DiscountDto>> CreateAsync(CreateDiscountDto dto);
    Task<Result<DiscountDto>> UpdateAsync(int id, UpdateDiscountDto dto);
    Task<Result> DeleteAsync(int id);
}

public class DiscountService : IDiscountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    // Follow existing service pattern...
}

// ❌ WRONG - Don't create custom repository unless absolutely necessary
public class DiscountRepository : IDiscountRepository
{
    // Don't do this - use generic Repository<Discount>
}
```

---

### 4. 📝 Naming Conventions are STRICT

**Rule:** Follow existing naming conventions exactly

**Backend:**
```csharp
// Entity (singular, PascalCase)
public class Discount { }

// Service interface
public interface IDiscountService { }

// Service implementation
public class DiscountService : IDiscountService { }

// DTOs
public class CreateDiscountDto { }
public class UpdateDiscountDto { }
public class DiscountResultDto { }
public class DiscountListDto { }

// Controller
[Route("api/[controller]")] // Will be /api/discount (snake_case)
public class DiscountController : BaseController { }

// Repository (use generic)
IRepository<Discount> via IUnitOfWork.Discounts
```

**Frontend:**
```csharp
// ViewModel
public class DiscountsViewModel : ViewModelBase { }
public class DiscountEditViewModel : ViewModelBase { }

// View
DiscountsView.xaml
DiscountEditDialog.xaml

// Service
public interface IDiscountService { }
public class DiscountService : IDiscountService { }

// Commands
public ICommand LoadDiscountsCommand { get; }
public ICommand CreateDiscountCommand { get; }
```

---

### 5. 🔒 Security Rules

**Authentication:**
- ✅ ALWAYS use JWT Bearer tokens
- ✅ ALWAYS add `[Authorize]` to protected endpoints
- ❌ NEVER store passwords in plain text (use BCrypt)
- ❌ NEVER expose sensitive data in DTOs

**Authorization:**
```csharp
// ✅ CORRECT - Proper authorization
[Authorize] // Requires any authenticated user
public class OrderController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        int customerId = GetCurrentUserId(); // From JWT
        string userType = GetCurrentUserType(); // Customer/Seller/Admin

        // Business logic can check userType if needed
    }
}

// ❌ WRONG - Don't bypass authorization
[AllowAnonymous] // Only use for login, register, public endpoints
public async Task<IActionResult> DeleteOrder(int id)
{
    // NEVER allow unauthorized deletion
}
```

**Input Validation:**
- ✅ ALWAYS validate all user input
- ✅ ALWAYS use DTOs (never bind directly to entities)
- ✅ ALWAYS sanitize file uploads

---

### 6. 🗄️ Database Rules

**Soft Delete:**
```csharp
// ✅ CORRECT - Use soft delete
public async Task<Result> DeleteProductAsync(int id)
{
    var product = await _unitOfWork.Products.GetByIdAsync(id);
    if (product == null)
        return Result.NotFound("Product not found");

    _unitOfWork.Products.SoftDelete(product);
    await _unitOfWork.SaveChangesAsync();
    return Result.Success("Product deleted");
}

// ❌ WRONG - Don't use hard delete unless absolutely necessary
_unitOfWork.Products.Delete(product); // Permanent deletion
```

**Queries:**
```csharp
// ✅ CORRECT - Use DeletedAt in queries
var products = await _unitOfWork.Products
    .FindAsync(p => p.DeletedAt == null && p.IsActive);

// ❌ WRONG - Don't use computed properties in queries
var products = await _unitOfWork.Products
    .FindAsync(p => !p.IsDeleted); // Runtime error!
```

**Date Handling:**
```csharp
// ✅ CORRECT - Use string comparison
var startDateStr = startDate.ToString("yyyy-MM-dd");
var orders = await _unitOfWork.Orders
    .FindAsync(o => string.Compare(o.OrderDate, startDateStr) >= 0);

// ❌ WRONG - Don't parse dates in queries
var orders = await _unitOfWork.Orders
    .FindAsync(o => DateTime.Parse(o.OrderDate) >= startDate); // Error!
```

---

### 7. 🏗️ Where to Add New Logic

**New Entity:**
1. Create entity in `Domain/Entities/{Category}/{Entity}.cs`
2. Inherit from `BaseEntity`
3. Add `DbSet<{Entity}>` to `ZiyoMarketDbContext`
4. Create migration: `dotnet ef migrations add Add{Entity} --project ../ZiyoMarket.Data`
5. Create DTOs in `Service/DTOs/{Category}/`
6. Add AutoMapper profile in `Service/Mapping/MappingProfiles.cs`
7. Create service interface in `Service/Interfaces/I{Entity}Service.cs`
8. Create service implementation in `Service/Services/{Entity}Service.cs`
9. Add repository property to `IUnitOfWork` and `UnitOfWork`
10. Register service in `Api/Extensions/ServiceExtension.cs`
11. Create controller in `Api/Controllers/{Category}/{Entity}Controller.cs`

**New Endpoint:**
1. Add method to existing service interface
2. Implement method in service
3. Add controller action
4. Test via Swagger

**New ViewModel (Admin Panel):**
1. Create ViewModel in `ViewModels/{Feature}/{Name}ViewModel.cs`
2. Create View in `Views/{Feature}/{Name}View.xaml`
3. Create service in `Services/Implementations/{Feature}Service.cs` (if needed)
4. Register ViewModel, View, Service in `ServiceCollectionExtensions.cs`
5. Add navigation command in `MainViewModel.cs`

**New Business Rule:**
- Add to existing service method
- **NEVER in controller or repository**

---

## 🚀 DEVELOPMENT WORKFLOW

### Step 1: Analyze

**Before writing any code:**

1. **Understand the requirement**
   - What is the user asking for?
   - What existing feature does it relate to?

2. **Search existing codebase**
   - Is there similar functionality already?
   - Which entities/services are involved?
   - What patterns are used?

3. **Identify extension points**
   - Which service needs the new method?
   - Does it need a new entity or DTO?
   - Will it require a new controller endpoint?

**Example:**
```
User Request: "Add bulk product import from Excel"

Analysis:
1. Relates to: Product management
2. Existing code: ProductService, ProductController
3. New code needed:
   - Service method: ImportProductsFromExcelAsync()
   - Controller endpoint: POST /api/product/import
   - DTO: ImportProductDto (with Excel file)
4. Pattern: Follow existing file upload pattern (FileUploadService)
5. No new entities needed
```

---

### Step 2: Plan

**Create a detailed plan:**

1. **List required changes**
   - New methods
   - New DTOs
   - New endpoints
   - Database changes

2. **Identify dependencies**
   - What services are needed?
   - What libraries are needed?
   - What database changes are needed?

3. **Estimate complexity**
   - Simple: Add method to existing service
   - Moderate: Add new entity + service
   - Complex: Add new subsystem

**Example Plan:**
```markdown
## Bulk Product Import Feature

### Backend Changes
1. Add DTO: `Service/DTOs/Products/ImportProductDto.cs`
   - ExcelFile (IFormFile)
   - OverwriteExisting (bool)

2. Add service method: `IProductService.ImportFromExcelAsync(ImportProductDto dto)`
   - Parse Excel file (EPPlus library)
   - Validate data
   - Create/update products in transaction
   - Return Result<ImportResultDto> (success count, error count)

3. Add controller endpoint: `ProductController.ImportFromExcel(ImportProductDto dto)`
   - [HttpPost("import")]
   - [Authorize(Roles = "Admin,Seller")]
   - Return ApiResponse<ImportResultDto>

4. Add NuGet package: EPPlus 7.0

### Admin Panel Changes
1. Add ViewModel method: `ProductsViewModel.ImportFromExcelCommand`
2. Add UI: Button in ProductsView.xaml
3. Add Dialog: ImportProductsDialog.xaml (file picker)

### Testing
1. Create test Excel file with sample products
2. Test valid data
3. Test invalid data (missing fields)
4. Test duplicate products
```

---

### Step 3: Approve (If Major Change)

**Checklist for approval:**

- ⚠️ **Major changes require approval:**
  - New architecture patterns
  - New external dependencies
  - Database schema changes
  - Breaking API changes
  - Performance-critical code

- ✅ **Minor changes don't require approval:**
  - New endpoint in existing controller
  - New service method following existing pattern
  - UI improvements
  - Bug fixes

**How to request approval:**
1. Create GitHub issue or discussion
2. Present the plan (Step 2)
3. Explain why existing code can't be extended
4. Wait for review

---

### Step 4: Implement

**Implementation order:**

**Backend:**
1. Database (if needed)
   - Create entity
   - Add migration
   - Apply migration

2. DTOs
   - Create input DTOs
   - Create output DTOs
   - Add AutoMapper mappings

3. Service
   - Add method to interface
   - Implement method
   - Add unit tests (if applicable)

4. Controller
   - Add endpoint
   - Add XML documentation
   - Test in Swagger

5. Register
   - Add to DI container (if new service)

**Frontend:**
1. Service
   - Add API client method

2. ViewModel
   - Add properties
   - Add commands
   - Add business logic

3. View
   - Add UI elements
   - Add data bindings

4. Register
   - Add to DI container (if new)

**Testing:**
1. Manual testing via Swagger (backend)
2. Manual testing via Admin Panel (frontend)
3. Verify error handling
4. Verify validation

**Example Implementation:**
```csharp
// Step 1: DTO
public class ImportProductDto
{
    public IFormFile ExcelFile { get; set; }
    public bool OverwriteExisting { get; set; }
}

public class ImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; }
}

// Step 2: Service Interface
public interface IProductService
{
    // ... existing methods
    Task<Result<ImportResultDto>> ImportFromExcelAsync(ImportProductDto dto);
}

// Step 3: Service Implementation
public class ProductService : IProductService
{
    public async Task<Result<ImportResultDto>> ImportFromExcelAsync(ImportProductDto dto)
    {
        // 1. Validate file
        if (dto.ExcelFile == null)
            return Result<ImportResultDto>.BadRequest("File is required");

        // 2. Parse Excel
        using var stream = dto.ExcelFile.OpenReadStream();
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];

        // 3. Process rows
        var result = new ImportResultDto { Errors = new List<string>() };
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                // Parse row, create product, handle errors
                result.TotalRows++;

                try
                {
                    var product = ParseProductFromRow(worksheet, row);
                    await _unitOfWork.Products.InsertAsync(product);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result<ImportResultDto>.Success(result);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

// Step 4: Controller
[HttpPost("import")]
[Authorize(Roles = "Admin,Seller")]
public async Task<IActionResult> ImportFromExcel([FromForm] ImportProductDto dto)
{
    var result = await _productService.ImportFromExcelAsync(dto);
    return HandleResult(result);
}
```

---

### Step 5: Document

**Documentation checklist:**

- ✅ Add XML comments to public methods
- ✅ Update CLAUDE.md if architecture changed
- ✅ Update API_RESPONSE_FORMAT.md if response changed
- ✅ Add example requests to Swagger
- ✅ Update admin panel documentation if UI changed

**Example:**
```csharp
/// <summary>
/// Import products from Excel file
/// </summary>
/// <param name="dto">Excel file and import options</param>
/// <returns>Import result with success/error counts</returns>
/// <response code="200">Products imported successfully</response>
/// <response code="400">Invalid file or data</response>
/// <response code="401">Unauthorized</response>
[HttpPost("import")]
[ProducesResponseType(typeof(ApiResponse<ImportResultDto>), 200)]
[ProducesResponseType(typeof(ApiResponse<object>), 400)]
public async Task<IActionResult> ImportFromExcel([FromForm] ImportProductDto dto)
{
    var result = await _productService.ImportFromExcelAsync(dto);
    return HandleResult(result);
}
```

---

## 🔥 COMMON PITFALLS TO AVOID

### ❌ Don't Use Computed Properties in LINQ

```csharp
// ❌ WRONG
var lowStock = await _unitOfWork.Products.FindAsync(p => p.IsLowStock);

// ✅ CORRECT
var lowStock = await _unitOfWork.Products.FindAsync(p => p.StockQuantity <= p.MinStockLevel);
```

### ❌ Don't Parse Dates in LINQ

```csharp
// ❌ WRONG
var orders = await _unitOfWork.Orders.FindAsync(o => DateTime.Parse(o.OrderDate) >= startDate);

// ✅ CORRECT
var startDateStr = startDate.ToString("yyyy-MM-dd");
var orders = await _unitOfWork.Orders.FindAsync(o => string.Compare(o.OrderDate, startDateStr) >= 0);
```

### ❌ Don't Create Custom Repositories

```csharp
// ❌ WRONG - Don't do this
public class ProductRepository : IProductRepository
{
    public Task<List<Product>> GetLowStockAsync() { ... }
}

// ✅ CORRECT - Add method to service instead
public class ProductService : IProductService
{
    public async Task<Result<List<ProductDto>>> GetLowStockAsync()
    {
        var products = await _unitOfWork.Products
            .FindAsync(p => p.StockQuantity <= p.MinStockLevel);
        // ...
    }
}
```

### ❌ Don't Put Business Logic in Controllers

```csharp
// ❌ WRONG
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    // Don't calculate totals here
    decimal total = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
    decimal discount = total * 0.1m;
    // ...
}

// ✅ CORRECT - Business logic in service
public class OrderService
{
    public async Task<Result<OrderDto>> CreateAsync(CreateOrderDto dto)
    {
        // Calculate here
        decimal total = CalculateTotal(dto.Items);
        decimal discount = CalculateDiscount(total);
        // ...
    }
}
```

### ❌ Don't Expose Entities in API

```csharp
// ❌ WRONG - Don't return entity directly
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var product = await _unitOfWork.Products.GetByIdAsync(id);
    return Ok(product); // Exposes internal structure
}

// ✅ CORRECT - Always return DTO
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _productService.GetByIdAsync(id); // Returns Result<ProductDto>
    return HandleResult(result); // Returns ApiResponse<ProductDto>
}
```

### ❌ Don't Hardcode Connection Strings

```csharp
// ❌ WRONG
var connectionString = "Server=localhost;Database=ZiyoNoorDb;User Id=postgres;Password=mypassword";

// ✅ CORRECT - Use configuration
var connectionString = Configuration.GetConnectionString("DefaultConnection");
```

---

# SAFE EXTENSION STRATEGY

## Adding a New Feature - Step by Step

### Example: Add Product Reviews Feature

**Step 1: Analyze**

```
Requirement: Customers can leave reviews (rating 1-5 + text) on products

Related entities:
- Product (existing)
- Customer (existing)

New entity needed:
- ProductReview (new)

Services affected:
- ProductService (add methods to get reviews)

New service:
- ProductReviewService (create, update, delete reviews)
```

**Step 2: Plan**

```markdown
## Product Reviews Feature - Implementation Plan

### Database Changes
1. Create entity: `ProductReview`
   - Id (int, PK)
   - ProductId (int, FK → Product)
   - CustomerId (int, FK → Customer)
   - Rating (int, 1-5)
   - ReviewText (string, nullable)
   - CreatedAt, UpdatedAt, DeletedAt (from BaseEntity)

2. Add navigation properties:
   - Product.Reviews → List<ProductReview>
   - Customer.Reviews → List<ProductReview>

3. Create migration: `AddProductReview`

### DTOs
1. `CreateProductReviewDto` (ProductId, Rating, ReviewText)
2. `UpdateProductReviewDto` (Id, Rating, ReviewText)
3. `ProductReviewResultDto` (Id, ProductId, ProductName, CustomerName, Rating, ReviewText, CreatedAt)
4. `ProductReviewListDto` (Id, CustomerName, Rating, ReviewText, CreatedAt)

### Services
1. Create `IProductReviewService`
   - Task<Result<ProductReviewResultDto>> CreateAsync(CreateProductReviewDto dto, int customerId)
   - Task<Result<ProductReviewResultDto>> UpdateAsync(int id, UpdateProductReviewDto dto, int customerId)
   - Task<Result> DeleteAsync(int id, int customerId)
   - Task<Result<List<ProductReviewListDto>>> GetByProductIdAsync(int productId, int page, int pageSize)
   - Task<Result<ProductReviewResultDto>> GetByIdAsync(int id)

2. Extend `IProductService`
   - Add AverageRating to ProductDetailDto

### Controllers
1. Create `ProductReviewController`
   - POST /api/product-review (create)
   - PUT /api/product-review/{id} (update)
   - DELETE /api/product-review/{id} (delete)
   - GET /api/product-review/product/{productId} (get by product)
   - GET /api/product-review/{id} (get by id)

### Admin Panel
1. Add `ProductReviewsViewModel`
2. Add `ProductReviewsView` (DataGrid showing all reviews)
3. Add ability to delete inappropriate reviews (admin only)

### Authorization
- Create: Customer only
- Update: Customer (own reviews only)
- Delete: Customer (own reviews) OR Admin (any review)
- Read: Public (no auth required)
```

**Step 3: Implement (Detailed)**

**3.1 Create Entity**

```csharp
// File: Domain/Entities/Products/ProductReview.cs
namespace ZiyoMarket.Domain.Entities.Products;

public class ProductReview : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int Rating { get; set; } // 1-5

    public string? ReviewText { get; set; }

    // Navigation
    // (CreatedAt, UpdatedAt, DeletedAt from BaseEntity)
}
```

**3.2 Update Related Entities**

```csharp
// File: Domain/Entities/Products/Product.cs
public class Product : BaseEntity
{
    // ... existing properties

    // Add navigation property
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}

// File: Domain/Entities/Users/Customer.cs
public class Customer : BaseEntity
{
    // ... existing properties

    // Add navigation property
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}
```

**3.3 Update DbContext**

```csharp
// File: Data/Context/ZiyoMarketDbContext.cs
public class ZiyoMarketDbContext : DbContext
{
    // ... existing DbSets

    public DbSet<ProductReview> ProductReviews { get; set; }
}
```

**3.4 Create Migration**

```bash
cd src/ZiyoMarket.Api
dotnet ef migrations add AddProductReview --project ../ZiyoMarket.Data
dotnet ef database update --project ../ZiyoMarket.Data
```

**3.5 Create DTOs**

```csharp
// File: Service/DTOs/Products/CreateProductReviewDto.cs
public class CreateProductReviewDto
{
    public int ProductId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? ReviewText { get; set; }
}

// File: Service/DTOs/Products/UpdateProductReviewDto.cs
public class UpdateProductReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
}

// File: Service/DTOs/Products/ProductReviewResultDto.cs
public class ProductReviewResultDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// File: Service/DTOs/Products/ProductReviewListDto.cs
public class ProductReviewListDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**3.6 Add AutoMapper Mappings**

```csharp
// File: Service/Mapping/MappingProfiles.cs
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // ... existing mappings

        // ProductReview mappings
        CreateMap<CreateProductReviewDto, ProductReview>();
        CreateMap<UpdateProductReviewDto, ProductReview>();
        CreateMap<ProductReview, ProductReviewResultDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"));
        CreateMap<ProductReview, ProductReviewListDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"));
    }
}
```

**3.7 Create Service Interface**

```csharp
// File: Service/Interfaces/IProductReviewService.cs
public interface IProductReviewService
{
    Task<Result<ProductReviewResultDto>> CreateAsync(CreateProductReviewDto dto, int customerId);
    Task<Result<ProductReviewResultDto>> UpdateAsync(int id, UpdateProductReviewDto dto, int customerId);
    Task<Result> DeleteAsync(int id, int userId, string userType);
    Task<Result<List<ProductReviewListDto>>> GetByProductIdAsync(int productId, int page = 1, int pageSize = 20);
    Task<Result<ProductReviewResultDto>> GetByIdAsync(int id);
}
```

**3.8 Implement Service**

```csharp
// File: Service/Services/ProductReviewService.cs
public class ProductReviewService : IProductReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductReviewService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductReviewResultDto>> CreateAsync(CreateProductReviewDto dto, int customerId)
    {
        // 1. Validation
        if (dto.Rating < 1 || dto.Rating > 5)
            return Result<ProductReviewResultDto>.BadRequest("Rating must be between 1 and 5");

        // 2. Check if product exists
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
            return Result<ProductReviewResultDto>.NotFound("Product not found");

        // 3. Check if customer already reviewed this product
        var existingReview = await _unitOfWork.ProductReviews
            .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId && r.CustomerId == customerId && r.DeletedAt == null);

        if (existingReview != null)
            return Result<ProductReviewResultDto>.BadRequest("You have already reviewed this product");

        // 4. Map and create
        var review = _mapper.Map<ProductReview>(dto);
        review.CustomerId = customerId;

        await _unitOfWork.ProductReviews.InsertAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // 5. Load related data for DTO
        var createdReview = await _unitOfWork.ProductReviews.GetByIdAsync(review.Id,
            includes: new[] { "Product", "Customer" });

        var resultDto = _mapper.Map<ProductReviewResultDto>(createdReview);
        return Result<ProductReviewResultDto>.Success(resultDto);
    }

    public async Task<Result<ProductReviewResultDto>> UpdateAsync(int id, UpdateProductReviewDto dto, int customerId)
    {
        // 1. Validation
        if (dto.Rating < 1 || dto.Rating > 5)
            return Result<ProductReviewResultDto>.BadRequest("Rating must be between 1 and 5");

        // 2. Get review
        var review = await _unitOfWork.ProductReviews.GetByIdAsync(id);
        if (review == null)
            return Result<ProductReviewResultDto>.NotFound("Review not found");

        // 3. Authorization check (only owner can update)
        if (review.CustomerId != customerId)
            return Result<ProductReviewResultDto>.Forbidden("You can only update your own reviews");

        // 4. Update
        review.Rating = dto.Rating;
        review.ReviewText = dto.ReviewText;

        _unitOfWork.ProductReviews.Update(review, id);
        await _unitOfWork.SaveChangesAsync();

        // 5. Load and return
        var updatedReview = await _unitOfWork.ProductReviews.GetByIdAsync(id,
            includes: new[] { "Product", "Customer" });

        var resultDto = _mapper.Map<ProductReviewResultDto>(updatedReview);
        return Result<ProductReviewResultDto>.Success(resultDto);
    }

    public async Task<Result> DeleteAsync(int id, int userId, string userType)
    {
        var review = await _unitOfWork.ProductReviews.GetByIdAsync(id);
        if (review == null)
            return Result.NotFound("Review not found");

        // Authorization: Owner OR Admin
        if (userType != "Admin" && review.CustomerId != userId)
            return Result.Forbidden("You can only delete your own reviews");

        _unitOfWork.ProductReviews.SoftDelete(review);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Review deleted successfully");
    }

    public async Task<Result<List<ProductReviewListDto>>> GetByProductIdAsync(int productId, int page = 1, int pageSize = 20)
    {
        var (reviews, totalCount) = await _unitOfWork.ProductReviews.GetPagedAsync(
            page: page,
            pageSize: pageSize,
            filter: r => r.ProductId == productId && r.DeletedAt == null,
            orderBy: q => q.OrderByDescending(r => r.CreatedAt),
            includes: new[] { "Customer" }
        );

        var dtos = _mapper.Map<List<ProductReviewListDto>>(reviews);
        return Result<List<ProductReviewListDto>>.Success(dtos);
    }

    public async Task<Result<ProductReviewResultDto>> GetByIdAsync(int id)
    {
        var review = await _unitOfWork.ProductReviews.GetByIdAsync(id,
            includes: new[] { "Product", "Customer" });

        if (review == null)
            return Result<ProductReviewResultDto>.NotFound("Review not found");

        var dto = _mapper.Map<ProductReviewResultDto>(review);
        return Result<ProductReviewResultDto>.Success(dto);
    }
}
```

**3.9 Update UnitOfWork**

```csharp
// File: Data/UnitOfWorks/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    // ... existing repositories

    IRepository<ProductReview> ProductReviews { get; }
}

// File: Data/UnitOfWorks/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly ZiyoMarketDbContext _context;

    // ... existing repositories

    public IRepository<ProductReview> ProductReviews =>
        new Repository<ProductReview>(_context);
}
```

**3.10 Register Service**

```csharp
// File: Api/Extensions/ServiceExtension.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // ... existing services

    services.AddScoped<IProductReviewService, ProductReviewService>();

    return services;
}
```

**3.11 Create Controller**

```csharp
// File: Api/Controllers/Products/ProductReviewController.cs
[Route("api/[controller]")]
[ApiController]
public class ProductReviewController : BaseController
{
    private readonly IProductReviewService _reviewService;

    public ProductReviewController(IProductReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Create a product review
    /// </summary>
    [HttpPost]
    [Authorize] // Customer only
    public async Task<IActionResult> Create(CreateProductReviewDto dto)
    {
        int customerId = GetCurrentUserId();
        string userType = GetCurrentUserType();

        if (userType != "Customer")
            return ErrorResponse("Only customers can create reviews");

        var result = await _reviewService.CreateAsync(dto, customerId);
        return HandleResult(result);
    }

    /// <summary>
    /// Update a product review
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateProductReviewDto dto)
    {
        int customerId = GetCurrentUserId();
        dto.Id = id;

        var result = await _reviewService.UpdateAsync(id, dto, customerId);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a product review
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetCurrentUserId();
        string userType = GetCurrentUserType();

        var result = await _reviewService.DeleteAsync(id, userId, userType);
        return HandleResult(result);
    }

    /// <summary>
    /// Get reviews for a product
    /// </summary>
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProductId(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _reviewService.GetByProductIdAsync(productId, page, pageSize);
        return HandleResult(result);
    }

    /// <summary>
    /// Get review by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _reviewService.GetByIdAsync(id);
        return HandleResult(result);
    }
}
```

**Step 4: Test**

1. Run migrations
2. Start API
3. Open Swagger
4. Test each endpoint:
   - Create review (as customer)
   - Get reviews for product
   - Update review (as owner)
   - Delete review (as owner or admin)

**Step 5: Document**

Update CLAUDE.md:
```markdown
### Product Entities (7) // Updated from 6

| Entity | Purpose | Key Fields |
|--------|---------|-----------|
| ... existing entities ...
| **ProductReview** | Customer product reviews | Id, ProductId, CustomerId, Rating (1-5), ReviewText |
```

---

This is the complete, production-ready master documentation for the ZiyoMarket e-commerce platform.

---

**Document End**

Generated: 2026-03-17
Author: AI Analysis
For: Future Development Teams