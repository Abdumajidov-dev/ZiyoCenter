# ZiyoMarket - WPF Admin Panel uchun To'liq Qo'llanma

## Loyiha haqida umumiy ma'lumot

ZiyoMarket - bu ASP.NET Core 8.0 da yozilgan professional elektron savdo platformasi. Loyiha Clean Architecture asosida qurilgan va PostgreSQL ma'lumotlar bazasidan foydalanadi.

### Texnologiyalar:
- **Backend Framework:** ASP.NET Core 8.0
- **Ma'lumotlar Bazasi:** PostgreSQL 14+
- **ORM:** Entity Framework Core 8.0
- **Autentifikatsiya:** JWT (JSON Web Tokens)
- **Parol Shifrlash:** BCrypt.Net-Next
- **API Hujjatlari:** Swagger/OpenAPI

---

## 📁 Loyiha Strukturasi

```
ZiyoMarket/
├── src/
│   ├── ZiyoMarket.Domain/         # Entities, Enums, Base klasslar
│   ├── ZiyoMarket.Data/           # DbContext, Repositories, UnitOfWork
│   ├── ZiyoMarket.Service/        # Business Logic, DTOs, Validators
│   └── ZiyoMarket.Api/            # Controllers, Middleware
└── database_seed.sql              # Test ma'lumotlari
```

### Statistika:
- **20+ Entity** - to'liq business model
- **14 Controller** - 150+ REST API endpoint
- **13 Service** - business logic
- **60+ DTO** - data transfer objects
- **14 Enum** - type-safe operatsiyalar

---

## 🗄️ Ma'lumotlar Bazasi Strukturasi

### Asosiy Jadvallar:

#### 1. Foydalanuvchilar
- **Customer** - mijozlar (cashback tizimi bilan)
- **Seller** - sotuvchilar (offline buyurtmalar uchun)
- **Admin** - administratorlar (to'liq huquqlar)

#### 2. Mahsulotlar
- **Product** - mahsulotlar (QR kod bilan)
- **Category** - kategoriyalar (hierarchical)
- **CartItem** - savatdagi mahsulotlar
- **ProductLike** - sevimli mahsulotlar

#### 3. Buyurtmalar
- **Order** - online va offline buyurtmalar
- **OrderItem** - buyurtma qatorlari
- **OrderDiscount** - chegirmalar (sabablari bilan)
- **DiscountReason** - chegirma sabablari
- **CashbackTransaction** - cashback tarixi

#### 4. Qo'llab-quvvatlash
- **SupportChat** - mijozlar bilan chat
- **SupportMessage** - chat xabarlari

#### 5. Yetkazib berish
- **DeliveryPartner** - yetkazib berish xizmatlari
- **OrderDelivery** - yetkazib berish ma'lumotlari

#### 6. Tizim
- **Notification** - bildirishnomalar
- **Content** - blog, banner, e'lonlar
- **DailySalesSummary** - kunlik hisobotlar
- **SystemSetting** - tizim sozlamalari

---

## 🔐 Autentifikatsiya va Avtorizatsiya

### JWT Konfiguratsiyasi

```json
{
  "JwtSettings": {
    "SecretKey": "SuperSecretKeyForJwtDontShare123!",
    "Issuer": "ZiyoMarket",
    "Audience": "ZiyoMarketUsers",
    "AccessTokenExpirationMinutes": 1440,  // 24 soat
    "RefreshTokenExpirationDays": 7
  }
}
```

### Foydalanuvchi Rollari

| Rol | Huquqlar |
|-----|----------|
| **Admin** | To'liq tizim huquqi, hisobotlar, foydalanuvchilarni boshqarish |
| **Seller** | Offline buyurtmalar yaratish, o'z ko'rsatkichlarini ko'rish |
| **Customer** | Xarid qilish, buyurtmalarni kuzatish, sevimlilar/savat |

### Login Jarayoni

1. **Login qilish:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "phoneOrEmail": "admin@ziyomarket.uz",
  "password": "Admin@123",
  "userType": "Admin"
}
```

2. **Javob:**
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "fullName": "Admin User",
    "email": "admin@ziyomarket.uz",
    "userType": "Admin",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
  },
  "statusCode": 200
}
```

3. **Keyingi so'rovlarda token ishlatish:**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Test Foydalanuvchilar

| Email | Parol | Rol |
|-------|-------|-----|
| admin@ziyomarket.uz | Admin@123 | Admin |
| john@example.com | password123 | Customer |
| jane@example.com | password123 | Customer |

---

## 🚀 Asosiy API Endpointlar

### Base URL
```
https://localhost:5001/api/
```

---

### 1️⃣ Autentifikatsiya (/auth)

| Endpoint | Method | Auth | Tavsif |
|----------|--------|------|--------|
| `/auth/login` | POST | ❌ | Tizimga kirish |
| `/auth/register` | POST | ❌ | Yangi mijoz ro'yxatdan o'tish |
| `/auth/refresh-token` | POST | ❌ | Token yangilash |
| `/auth/change-password` | POST | ✅ | Parolni o'zgartirish |
| `/auth/forgot-password` | POST | ❌ | Parolni tiklash |
| `/auth/me` | GET | ✅ | Joriy foydalanuvchi |
| `/auth/logout` | POST | ✅ | Tizimdan chiqish |

---

### 2️⃣ Mahsulotlar (/product)

| Endpoint | Method | Auth | Rol | Tavsif |
|----------|--------|------|-----|--------|
| `/product` | GET | ❌ | - | Mahsulotlar ro'yxati |
| `/product/{id}` | GET | ❌ | - | Mahsulot ma'lumotlari |
| `/product/qr/{qrCode}` | GET | ❌ | - | QR kod bo'yicha |
| `/product/search` | GET | ❌ | - | Qidirish |
| `/product` | POST | ✅ | Admin | Yangi mahsulot |
| `/product/{id}` | PUT | ✅ | Admin | Tahrirlash |
| `/product/{id}` | DELETE | ✅ | Admin | O'chirish |

**GET /product Parametrlari:**
```
pageNumber=1
pageSize=20
searchTerm=telefon
categoryId=5
minPrice=100000
maxPrice=5000000
status=Active
sortBy=Price
sortOrder=Ascending
```

**POST /product Request:**
```json
{
  "name": "Samsung Galaxy A54",
  "description": "128GB, 6GB RAM",
  "price": 3500000,
  "stockQuantity": 50,
  "categoryId": 1,
  "imageUrl": "https://example.com/image.jpg",
  "qrCode": "SAMSUNG-A54-001",
  "status": "Active"
}
```

---

### 3️⃣ Kategoriyalar (/category)

| Endpoint | Method | Auth | Tavsif |
|----------|--------|------|--------|
| `/category` | GET | ❌ | Barcha kategoriyalar |
| `/category/{id}` | GET | ❌ | Kategoriya ma'lumotlari |
| `/category/{id}/products` | GET | ❌ | Kategoriya mahsulotlari |
| `/category` | POST | ✅ | Yangi kategoriya |
| `/category/{id}` | PUT | ✅ | Tahrirlash |
| `/category/{id}` | DELETE | ✅ | O'chirish |

---

### 4️⃣ Buyurtmalar (/order)

| Endpoint | Method | Auth | Tavsif |
|----------|--------|------|--------|
| `/order` | GET | ✅ | Buyurtmalar ro'yxati |
| `/order/{id}` | GET | ✅ | Buyurtma ma'lumotlari |
| `/order` | POST | ✅ | Yangi buyurtma (Customer) |
| `/order/offline` | POST | ✅ | Offline buyurtma (Seller) |
| `/order/{id}/status` | PUT | ✅ | Status o'zgartirish |
| `/order/{id}/cancel` | POST | ✅ | Bekor qilish |
| `/order/{id}/confirm` | POST | ✅ | Tasdiqlash |
| `/order/discount` | POST | ✅ | Chegirma berish |
| `/order/summary` | GET | ✅ | Umumiy statistika |

**GET /order Parametrlari:**
```
pageNumber=1
pageSize=20
status=Pending
customerId=123
startDate=2025-01-01
endDate=2025-01-31
minTotal=100000
maxTotal=10000000
sortBy=CreatedAt
sortOrder=Descending
```

**Buyurtma Statuslari:**
```
Pending → Confirmed → Preparing → ReadyForPickup/Shipped → Delivered
                                                          ↘ Cancelled
```

**PUT /order/{id}/status Request:**
```json
{
  "status": "Confirmed"
}
```

**POST /order/discount Request:**
```json
{
  "orderId": 123,
  "discountAmount": 50000,
  "discountReasonId": 1,
  "notes": "VIP mijoz chegirmasi"
}
```

---

### 5️⃣ Hisobotlar (/report)

#### Dashboard
```http
GET /report/dashboard
```
**Javob:**
```json
{
  "success": true,
  "data": {
    "totalRevenue": 15000000,
    "totalOrders": 450,
    "totalCustomers": 150,
    "totalProducts": 80,
    "todayRevenue": 500000,
    "todayOrders": 15,
    "pendingOrders": 5,
    "lowStockProducts": 3
  }
}
```

#### Sotuv Hisobotlari
```http
GET /report/sales?startDate=2025-01-01&endDate=2025-01-31
GET /report/sales/chart?startDate=2025-01-01&endDate=2025-01-31&interval=Daily
GET /report/sales/daily
```

#### Mahsulot Hisobotlari
```http
GET /report/products/top?startDate=2025-01-01&endDate=2025-01-31&count=10
GET /report/products/analytics
```

#### Ombor Hisobotlari
```http
GET /report/inventory
GET /report/inventory/low-stock?threshold=10
```

#### Mijozlar Hisobotlari
```http
GET /report/customers/analytics
GET /report/customers/top?startDate=2025-01-01&endDate=2025-01-31&count=10
GET /report/customers/activity?customerId=123
```

#### Sotuvchilar Hisobotlari
```http
GET /report/sellers/performance?sellerId=456&startDate=2025-01-01&endDate=2025-01-31
```

---

### 6️⃣ Qo'llab-quvvatlash (/support)

| Endpoint | Method | Auth | Tavsif |
|----------|--------|------|--------|
| `/support` | GET | ✅ | Chatlar ro'yxati |
| `/support/{chatId}` | GET | ✅ | Chat ma'lumotlari |
| `/support/{chatId}/messages` | GET | ✅ | Chat xabarlari |
| `/support` | POST | ✅ | Yangi chat |
| `/support/messages` | POST | ✅ | Xabar yuborish |
| `/support/{chatId}/status` | PUT | ✅ | Status o'zgartirish |

**Chat Statuslari:**
- Open - Yangi
- InProgress - Ishlanmoqda
- Closed - Yopilgan
- Escalated - Yuqori darajaga ko'tarilgan

**Priority Darajalari:**
- Low - Past
- Medium - O'rta
- High - Yuqori
- Urgent - Shoshilinch

---

### 7️⃣ Cashback (/cashback)

| Endpoint | Method | Auth | Tavsif |
|----------|--------|------|--------|
| `/cashback/balance` | GET | ✅ | Balans ma'lumotlari |
| `/cashback/transactions` | GET | ✅ | Tranzaksiyalar tarixi |

**Cashback Qoidalari:**
- Avtomatik 2% cashback har bir buyurtmadan
- 1 yil muddati (keyin amal qilmaydi)
- Keyingi xaridlarda ishlatish mumkin
- Tranzaksiya turlari: Earned, Used, Expired

---

### 8️⃣ Bildirishnomalar (/notification)

| Endpoint | Method | Auth | Tavsif |
|----------|--------|------|--------|
| `/notification` | GET | ✅ | Bildirishnomalar ro'yxati |
| `/notification/unread-count` | GET | ✅ | O'qilmagan soni |
| `/notification/{id}/read` | POST | ✅ | O'qilgan deb belgilash |
| `/notification/{id}` | DELETE | ✅ | O'chirish |

---

### 9️⃣ Yetkazib berish (/delivery)

| Endpoint | Method | Auth | Tavsif |
|----------|--------|------|--------|
| `/delivery/partners` | GET | ✅ | Yetkazib beruvchilar |
| `/delivery/partners/{id}` | GET | ✅ | Partner ma'lumotlari |
| `/delivery/order/{orderId}` | GET | ✅ | Buyurtma yetkazilishi |
| `/delivery/partners` | POST | ✅ | Yangi partner |
| `/delivery/order` | POST | ✅ | Yetkazib berishni belgilash |

---

## 📊 Response Format

### Muvaffaqiyatli Javob
```json
{
  "success": true,
  "message": "Operatsiya muvaffaqiyatli",
  "data": { /* ma'lumotlar */ },
  "statusCode": 200
}
```

### Xato Javobi
```json
{
  "success": false,
  "message": "Xatolik haqida ma'lumot",
  "errors": ["xato 1", "xato 2"],
  "statusCode": 400
}
```

### Pagination Javobi
```json
{
  "success": true,
  "data": {
    "items": [ /* obyektlar */ ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

## 💻 WPF da API Client Yaratish

### 1. NuGet Paketlar

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

### 2. API Response Klasslari

```csharp
// BaseResponse.cs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; }
}

// PaginatedResponse.cs
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

### 3. API Client Asosiy Klass

```csharp
// ZiyoMarketApiClient.cs
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ZiyoMarketApiClient
{
    private readonly HttpClient _httpClient;
    private string _accessToken;
    private string _refreshToken;

    public ZiyoMarketApiClient(string baseUrl = "https://localhost:5001/api/")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

    // Token o'rnatish
    public void SetToken(string accessToken, string refreshToken = null)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    // Login
    public async Task<ApiResponse<LoginResponse>> LoginAsync(
        string phoneOrEmail,
        string password,
        string userType = "Admin")
    {
        var request = new
        {
            phoneOrEmail,
            password,
            userType
        };

        var response = await PostAsync<LoginResponse>("auth/login", request);

        if (response.Success)
        {
            SetToken(response.Data.AccessToken, response.Data.RefreshToken);
        }

        return response;
    }

    // Logout
    public async Task<ApiResponse<object>> LogoutAsync()
    {
        var response = await PostAsync<object>("auth/logout", null);
        if (response.Success)
        {
            _accessToken = null;
            _refreshToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        return response;
    }

    // GET so'rov
    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiResponse<T>>(content);
            }
            else
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Xatolik: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // POST so'rov
    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
            }
            else
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Xatolik: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // PUT so'rov
    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Xatolik: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // DELETE so'rov
    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<T>>(content);
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Xatolik: {ex.Message}",
                StatusCode = 500
            };
        }
    }
}
```

### 4. Login Response Model

```csharp
// LoginResponse.cs
public class LoginResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string UserType { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
```

---

## 🎯 WPF Admin Panel Feature Ro'yxati

### 1. Dashboard (Bosh Sahifa)
**Endpoint:** `GET /report/dashboard`

Ko'rsatish kerak:
- Jami daromad
- Jami buyurtmalar
- Jami mijozlar
- Jami mahsulotlar
- Bugungi statistika
- Kutilayotgan buyurtmalar
- Kamayib qolgan mahsulotlar (Low Stock)

**WPF UI Elementi:** Cards, Charts (LiveCharts)

---

### 2. Buyurtmalar Boshqaruvi
**Endpointlar:**
- `GET /order` - ro'yxat
- `GET /order/{id}` - detallar
- `PUT /order/{id}/status` - status o'zgartirish
- `POST /order/{id}/confirm` - tasdiqlash
- `POST /order/{id}/cancel` - bekor qilish
- `POST /order/discount` - chegirma berish

**Funksiyalar:**
- Buyurtmalarni filtr qilish (status, sana, mijoz)
- Status o'zgartirish (Pending → Confirmed → ...)
- Chegirma berish
- Buyurtma detallari ko'rish
- Qidirish

**WPF UI Elementlari:** DataGrid, ComboBox, DatePicker

---

### 3. Mahsulotlar Boshqaruvi
**Endpointlar:**
- `GET /product` - ro'yxat
- `POST /product` - yangi mahsulot
- `PUT /product/{id}` - tahrirlash
- `DELETE /product/{id}` - o'chirish

**Funksiyalar:**
- Mahsulotlarni ko'rish (pagination bilan)
- Yangi mahsulot qo'shish
- Tahrirlash
- O'chirish
- Qidirish va filter
- QR kod yaratish

**WPF UI Elementlari:** DataGrid, Form Dialog

---

### 4. Kategoriyalar Boshqaruvi
**Endpointlar:**
- `GET /category` - ro'yxat
- `POST /category` - yangi
- `PUT /category/{id}` - tahrirlash
- `DELETE /category/{id}` - o'chirish

**WPF UI Elementlari:** TreeView yoki ListBox

---

### 5. Ombor Boshqaruvi
**Endpointlar:**
- `GET /report/inventory` - ombor holati
- `GET /report/inventory/low-stock` - kamaygan mahsulotlar

**Funksiyalar:**
- Ombor holati monitoring
- Kamaygan mahsulotlar uchun ogohlantirish
- Mahsulot miqdorini o'zgartirish

**WPF UI Elementlari:** DataGrid, Alerts

---

### 6. Hisobotlar
**Endpointlar:**
- `GET /report/sales` - sotuv hisoboti
- `GET /report/sales/chart` - grafik
- `GET /report/products/top` - top mahsulotlar
- `GET /report/customers/top` - top mijozlar

**Funksiyalar:**
- Kunlik/Haftalik/Oylik hisobotlar
- Grafik va diagrammalar
- Excel export (WPF da yaratish)

**WPF UI Elementlari:** LiveCharts, DataGrid, DatePicker

---

### 7. Mijozlar Boshqaruvi
**Endpointlar:**
- `GET /report/customers/analytics` - mijozlar statistikasi
- `GET /report/customers/top` - top mijozlar
- `GET /report/customers/activity?customerId={id}` - faollik

**WPF UI Elementlari:** DataGrid

---

### 8. Qo'llab-quvvatlash (Support)
**Endpointlar:**
- `GET /support` - chatlar
- `GET /support/{chatId}/messages` - xabarlar
- `POST /support/messages` - xabar yuborish
- `PUT /support/{chatId}/status` - status o'zgartirish

**Funksiyalar:**
- Mijozlar chatlarini ko'rish
- Xabar yuborish
- Chat statusini o'zgartirish (Open → InProgress → Closed)
- Priority belgilash

**WPF UI Elementlari:** ListBox (chats), ListBox (messages), TextBox

---

### 9. Bildirishnomalar
**Endpointlar:**
- `GET /notification` - barcha bildirishnomalar
- `GET /notification/unread-count` - o'qilmaganlar soni
- `POST /notification/{id}/read` - o'qilgan

**WPF UI Elementlari:** Notification Panel, Badge (o'qilmagan soni)

---

## 🔧 Muhim Enumlar

```csharp
// UserType.cs
public enum UserType
{
    Customer,
    Seller,
    Admin
}

// OrderStatus.cs
public enum OrderStatus
{
    Pending,          // Kutilmoqda
    Confirmed,        // Tasdiqlangan
    Preparing,        // Tayyorlanmoqda
    ReadyForPickup,   // Olib ketishga tayyor
    Shipped,          // Yo'lda
    Delivered,        // Yetkazildi
    Cancelled         // Bekor qilingan
}

// ProductStatus.cs
public enum ProductStatus
{
    Active,      // Faol
    Inactive,    // Nofaol
    OutOfStock,  // Omborda yo'q
    Deleted      // O'chirilgan
}

// PaymentMethod.cs
public enum PaymentMethod
{
    Cash,      // Naqd
    Card,      // Karta
    Cashback,  // Cashback
    Mixed      // Aralash
}

// DeliveryType.cs
public enum DeliveryType
{
    Pickup,   // Olib ketish
    Postal,   // Pochta
    Courier   // Kuryer
}

// SupportChatStatus.cs
public enum SupportChatStatus
{
    Open,        // Ochiq
    InProgress,  // Ishlanmoqda
    Closed,      // Yopilgan
    Escalated    // Yuqoriga ko'tarilgan
}

// SupportChatPriority.cs
public enum SupportChatPriority
{
    Low,      // Past
    Medium,   // O'rta
    High,     // Yuqori
    Urgent    // Shoshilinch
}
```

---

## 📦 WPF Loyiha Strukturasi (Tavsiya)

```
ZiyoMarketWPF/
├── Models/                   # API response modellari
│   ├── ApiResponse.cs
│   ├── LoginResponse.cs
│   ├── ProductDto.cs
│   ├── OrderDto.cs
│   └── ...
├── Services/                 # API client va business logic
│   ├── ZiyoMarketApiClient.cs
│   ├── AuthService.cs
│   ├── ProductService.cs
│   ├── OrderService.cs
│   └── ReportService.cs
├── ViewModels/              # MVVM ViewModels
│   ├── LoginViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── ProductsViewModel.cs
│   ├── OrdersViewModel.cs
│   └── ...
├── Views/                   # XAML oynalari
│   ├── LoginWindow.xaml
│   ├── MainWindow.xaml
│   ├── DashboardView.xaml
│   ├── ProductsView.xaml
│   ├── OrdersView.xaml
│   └── ...
├── Helpers/                 # Yordamchi klasslar
│   ├── RelayCommand.cs
│   ├── ObservableObject.cs
│   └── Converters/
└── App.xaml
```

---

## 🎨 WPF UI Library Tavsiyalari

### 1. MaterialDesignInXaml
```xml
<PackageReference Include="MaterialDesignThemes" Version="5.0.0" />
<PackageReference Include="MaterialDesignColors" Version="3.0.0" />
```

### 2. LiveCharts (Grafik uchun)
```xml
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc2" />
```

### 3. CommunityToolkit.Mvvm
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

---

## 🚦 Loyihani Ishga Tushirish

### 1. Backend ishga tushirish

```bash
# Database yaratish
psql -U postgres
CREATE DATABASE "ZiyoDb";

# Migrations
cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data

# Test ma'lumotlarni yuklash
psql -U postgres -d ZiyoDb -f ../../database_seed.sql

# Backend ishga tushirish
dotnet run
```

Backend manzil: `https://localhost:5001`
Swagger: `https://localhost:5001/swagger`

### 2. Ma'lumotlar Bazasi Connection String

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ZiyoDb;Username=postgres;Password=2001"
  }
}
```

---

## 💡 WPF Development Bo'yicha Maslahatlar

### 1. Async/Await ishlatish
```csharp
// YAXShI ✅
private async void LoadProducts()
{
    var response = await _apiClient.GetAsync<PaginatedResponse<ProductDto>>("product");
    if (response.Success)
    {
        Products = response.Data.Items;
    }
}

// YOMON ❌
private void LoadProducts()
{
    var response = _apiClient.GetAsync<PaginatedResponse<ProductDto>>("product").Result;
    // UI freeze bo'ladi!
}
```

### 2. Error Handling
```csharp
try
{
    var response = await _apiClient.GetAsync<DashboardDto>("report/dashboard");
    if (response.Success)
    {
        Dashboard = response.Data;
    }
    else
    {
        MessageBox.Show(response.Message, "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
catch (Exception ex)
{
    MessageBox.Show($"Xatolik: {ex.Message}", "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

### 3. MVVM Pattern ishlatish
```csharp
// ViewModel
public class ProductsViewModel : ObservableObject
{
    private readonly ZiyoMarketApiClient _apiClient;
    private ObservableCollection<ProductDto> _products;

    public ObservableCollection<ProductDto> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    public ICommand LoadProductsCommand { get; }
    public ICommand AddProductCommand { get; }

    public ProductsViewModel(ZiyoMarketApiClient apiClient)
    {
        _apiClient = apiClient;
        LoadProductsCommand = new RelayCommand(async () => await LoadProductsAsync());
        AddProductCommand = new RelayCommand(async () => await AddProductAsync());
    }

    private async Task LoadProductsAsync()
    {
        var response = await _apiClient.GetAsync<PaginatedResponse<ProductDto>>("product");
        if (response.Success)
        {
            Products = new ObservableCollection<ProductDto>(response.Data.Items);
        }
    }
}
```

### 4. Pagination
```csharp
// Pagination ViewModel
public class PaginationViewModel : ObservableObject
{
    private int _currentPage = 1;
    private int _totalPages;
    private int _pageSize = 20;

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            SetProperty(ref _currentPage, value);
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        set => SetProperty(ref _totalPages, value);
    }

    public bool CanGoNext => CurrentPage < TotalPages;
    public bool CanGoPrevious => CurrentPage > 1;

    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public PaginationViewModel()
    {
        NextPageCommand = new RelayCommand(() => CurrentPage++, () => CanGoNext);
        PreviousPageCommand = new RelayCommand(() => CurrentPage--, () => CanGoPrevious);
    }
}
```

---

## 🔒 Security Best Practices

### 1. Token xavfsiz saqlash
```csharp
// Tokenni xotirada saqlang (Session davomida)
// XAML da SecureString ishlatish

public class AuthService
{
    private string _accessToken;
    private string _refreshToken;

    public void SaveTokens(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;

        // Agar dastur yopilganda eslab qolish kerak bo'lsa:
        // Windows Credential Manager ishlatish tavsiya qilinadi
    }

    public void ClearTokens()
    {
        _accessToken = null;
        _refreshToken = null;
    }
}
```

### 2. HTTPS ishlatish
```csharp
// Faqat HTTPS
var apiClient = new ZiyoMarketApiClient("https://localhost:5001/api/");
```

### 3. Token Expiration Handling
```csharp
// Token muddati tugasa avtomatik refresh qilish
public async Task<ApiResponse<T>> GetWithRefreshAsync<T>(string endpoint)
{
    var response = await GetAsync<T>(endpoint);

    if (response.StatusCode == 401) // Unauthorized
    {
        // Token muddati tugagan
        var refreshed = await RefreshTokenAsync();
        if (refreshed)
        {
            response = await GetAsync<T>(endpoint); // Qayta urinish
        }
    }

    return response;
}
```

---

## 📝 Data Models (DTOs)

### ProductDto
```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string ImageUrl { get; set; }
    public string QrCode { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### OrderDto
```csharp
public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int? SellerId { get; set; }
    public string SellerName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string PaymentMethod { get; set; }
    public string DeliveryType { get; set; }
    public string Status { get; set; }
    public string ShippingAddress { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}
```

### DashboardDto
```csharp
public class DashboardDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
}
```

---

## ⚙️ Sozlamalar

### Backend URL o'zgartirish
```csharp
// Development
var apiClient = new ZiyoMarketApiClient("https://localhost:5001/api/");

// Production
var apiClient = new ZiyoMarketApiClient("https://api.ziyomarket.uz/api/");
```

### App.config
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="ApiBaseUrl" value="https://localhost:5001/api/" />
    <add key="PageSize" value="20" />
    <add key="RequestTimeout" value="30" />
  </appSettings>
</configuration>
```

---

## 🧪 Test Qilish

### Postman Collection
Backend bilan test qilish uchun Postman ishlatishingiz mumkin:

1. Postman ochish
2. Import → File → `ZiyoMarket.postman_collection.json` (agar mavjud bo'lsa)
3. Yoki qo'lda so'rovlar yaratish:
   - Environment yaratish (base_url, token)
   - Login so'rovi yuborish
   - Token olish
   - Boshqa endpointlarni test qilish

### WPF Test Strategiyasi
1. Birinchi Login ni test qiling
2. Keyin Dashboard ni test qiling
3. Har bir feature ni alohida test qiling
4. Error scenariolarni test qiling

---

## 📞 Qo'shimcha Ma'lumot

### API Hujjatlari
Backend ishga tushganda Swagger orqali:
```
https://localhost:5001/swagger
```

### Ma'lumotlar Bazasi
PostgreSQL Admin Panel (pgAdmin4) orqali ko'rish mumkin:
```
Host: localhost
Port: 5432
Database: ZiyoDb
User: postgres
Password: 2001
```

---

## ✅ Development Checklist

WPF loyihasini boshlashdan oldin:

- [ ] Backend ishga tushganini tekshiring (https://localhost:5001/swagger)
- [ ] Ma'lumotlar bazasida test ma'lumotlar borligini tekshiring
- [ ] Admin login qila olishingizni tekshiring (Postman orqali)
- [ ] Visual Studio yoki Rider da yangi WPF loyihasi yarating
- [ ] NuGet paketlarni o'rnating (HTTP, JSON, MaterialDesign)
- [ ] ZiyoMarketApiClient klassini yarating
- [ ] Login oynasini yarating va test qiling
- [ ] MainWindow yarating (Dashboard bilan)
- [ ] Keyingi featurelarga o'ting

---

## 🎓 Keyingi Qadamlar

1. **WPF loyihasi yaratish**
   ```bash
   dotnet new wpf -n ZiyoMarketWPF
   ```

2. **Kerakli paketlarni o'rnatish**
   ```bash
   dotnet add package MaterialDesignThemes
   dotnet add package MaterialDesignColors
   dotnet add package Newtonsoft.Json
   dotnet add package CommunityToolkit.Mvvm
   dotnet add package LiveChartsCore.SkiaSharpView.WPF
   ```

3. **API Client yaratish**
   - Yuqoridagi `ZiyoMarketApiClient` klassini nusxalash

4. **Login oynasini yaratish**
   - LoginWindow.xaml
   - LoginViewModel.cs

5. **MainWindow yaratish**
   - Dashboard
   - Navigation Menu
   - Content Area

6. **Feature-feature qo'shish**
   - Products
   - Orders
   - Reports
   - va hokazo...

---

## 🤝 Yordam

Agar savollaringiz bo'lsa:
1. Swagger dokumentatsiyasini tekshiring: https://localhost:5001/swagger
2. Backend kodini o'rganing: `src/ZiyoMarket.Api/Controllers/`
3. Service kodlarini ko'ring: `src/ZiyoMarket.Service/Services/`

---

**Omad tilaymiz! WPF Admin Panel yaratishda muvaffaqiyatlar! 🚀**
