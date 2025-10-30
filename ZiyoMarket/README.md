# ZiyoMarket - E-Commerce Platform

> **Professional multi-user e-commerce system with offline/online sales, cashback rewards, delivery tracking, and comprehensive admin panel**

## 📚 Dokumentatsiya

| Dokument | Kimlar Uchun | Tavsif |
|----------|--------------|--------|
| **[QUICK_START.md](./QUICK_START.md)** | 🚀 Barchaga | 5 daqiqada ishga tushirish, test credentials, tezkor qo'llanma |
| **[SWAGGER_GUIDE.md](./SWAGGER_GUIDE.md)** | 🧪 Testerlar | Swagger ishlatish, token olish, API test qilish |
| **[BACKEND_DEVELOPER_GUIDE.md](./BACKEND_DEVELOPER_GUIDE.md)** | 💻 Backend | Arxitektura, entities, services, 150+ endpoints |
| **[FLUTTER_DEVELOPER_GUIDE.md](./FLUTTER_DEVELOPER_GUIDE.md)** | 📱 Flutter | API integration, models, state management, FCM |
| **[DESKTOP_ADMIN_GUIDE.md](./DESKTOP_ADMIN_GUIDE.md)** | 🖥️ Desktop | Admin panel, reports, dashboard, charts |

---

## 🎯 Tezkor Boshlash

```bash
# 1. Database yaratish
psql -U postgres
CREATE DATABASE "ZiyoDb";
\q

# 2. Migration qo'llash
cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data

# 3. Test ma'lumotlar qo'shish
psql -U postgres -d ZiyoDb -f ../../database_seed.sql

# 4. Loyihani ishga tushirish
dotnet run

# 5. Swagger ni oching
# https://localhost:5001/swagger
```

**Default Admin:** `admin@ziyomarket.uz` / `Admin@123`

Batafsil: [QUICK_START.md](./QUICK_START.md)

---

## 📋 Loyiha Haqida

ZiyoMarket - bu zamonaviy elektron tijorat platformasi bo'lib, uch turdagi foydalanuvchilarni qo'llab-quvvatlaydi:
- **Mijozlar (Customers)** - Mahsulot xarid qilish, cashback yig'ish, buyurtmalarni kuzatish
- **Sotuvchilar (Sellers)** - Do'kon orqali offline sotuvlar amalga oshirish
- **Administratorlar (Admins)** - Tizimni to'liq boshqarish, hisobotlar, analitika

## 🏗️ Arxitektura

### Texnologiyalar

**Backend:**
- ASP.NET Core 8.0
- Entity Framework Core 8
- PostgreSQL Database
- JWT Authentication
- AutoMapper
- FluentValidation
- Swagger/OpenAPI

**Arxitektura Pattern:**
```
┌─────────────────────────────────────────────┐
│         ZiyoMarket.Api (Controllers)        │ ← REST API Layer
├─────────────────────────────────────────────┤
│      ZiyoMarket.Service (Business Logic)    │ ← Service Layer
├─────────────────────────────────────────────┤
│    ZiyoMarket.Data (Repository & UnitOfWork)│ ← Data Access Layer
├─────────────────────────────────────────────┤
│     ZiyoMarket.Domain (Entities & Enums)    │ ← Domain Layer
├─────────────────────────────────────────────┤
│          PostgreSQL Database (ZiyoDb)       │ ← Database
└─────────────────────────────────────────────┘
```

## 🚀 Asosiy Imkoniyatlar

### 1. E-Commerce Core
- ✅ Mahsulot katalogi (kategoriyalar, qidiruv, filtrlash)
- ✅ QR kod orqali mahsulot qidirish
- ✅ Savat (Shopping Cart) funksionalligi
- ✅ Online va offline buyurtmalar
- ✅ Mahsulotlarni yoqtirish (Like) tizimi

### 2. Moliyaviy Tizim
- ✅ **Cashback Tizimi** - 2% cashback har bir yetkazilgan buyurtmadan
- ✅ Cashback balansini kuzatish va ishlatish
- ✅ Chegirmalar (Discounts) tizimi
- ✅ Turli to'lov usullari (Naqd, Karta, Cashback, Aralash)

### 3. Buyurtma Boshqaruvi
- ✅ Buyurtma holati kuzatuvi (7 bosqichli workflow)
- ✅ Online buyurtmalar (mijoz tomonidan)
- ✅ Offline buyurtmalar (sotuvchi tomonidan do'konda)
- ✅ Buyurtma tarixi va detallari

### 4. Yetkazib Berish
- ✅ Yetkazib berish hamkorlari (Delivery Partners)
- ✅ Tracking kod orqali kuzatish
- ✅ Yetkazib berish holati yangilanishlari
- ✅ Uyga va ofisga yetkazib berish

### 5. Qo'llab-quvvatlash
- ✅ Mijozlar uchun chat tizimi
- ✅ Admin tomonidan javob berish
- ✅ Chat tarixini saqlash
- ✅ Muammolarni kategoriyalash va teglar

### 6. Bildirishnomalar
- ✅ Buyurtma holati yangilanishlari
- ✅ Push notifications (FCM support)
- ✅ Tizim bildirishnomalar
- ✅ O'qilgan/o'qilmagan status

### 7. Kontent Boshqaruvi
- ✅ Blog postlar
- ✅ Yangiliklar
- ✅ FAQ (Ko'p beriladigan savollar)
- ✅ Shartlar va qoidalar

### 8. Hisobotlar va Analitika (Admin)
- ✅ Kunlik/davr bo'yicha sotuv hisobotlari
- ✅ Eng ko'p sotiladigan mahsulotlar
- ✅ Inventar hisobotlari
- ✅ Mijozlar analitikasi
- ✅ Dashboard statistikasi

## 📊 Ma'lumotlar Bazasi Strukturasi

### Asosiy Jadvallar (20+ Entity)

```
Users:
├── Customers (Mijozlar)
├── Sellers (Sotuvchilar)
└── Admins (Administratorlar)

Products:
├── Products (Mahsulotlar)
├── Categories (Kategoriyalar - ierarxik)
├── CartItems (Savat elementlari)
└── ProductLikes (Yoqtirilgan mahsulotlar)

Orders:
├── Orders (Buyurtmalar)
├── OrderItems (Buyurtma elementlari)
├── OrderDiscounts (Chegirmalar)
└── CashbackTransactions (Cashback operatsiyalari)

Delivery:
├── DeliveryPartners (Yetkazib berish hamkorlari)
└── OrderDeliveries (Yetkazib berish kuzatuvi)

Support:
├── SupportChats (Qo'llab-quvvatlash chatlar)
└── SupportMessages (Chat xabarlari)

System:
├── Notifications (Bildirishnomalar)
├── Contents (Kontent)
├── DailySalesSummaries (Kunlik savdo xulosalari)
└── SystemSettings (Tizim sozlamalari)
```

## 🔐 Autentifikatsiya va Avtorizatsiya

### Foydalanuvchi Turlari va Ruxsatlar

| Rol | Imkoniyatlar |
|-----|-------------|
| **Customer** | Xarid qilish, savat, buyurtmalar, cashback, qo'llab-quvvatlash |
| **Seller** | Offline buyurtmalar yaratish, sotuv statistikasi |
| **Admin** | To'liq tizim boshqaruvi, hisobotlar, foydalanuvchilar boshqaruvi |

### JWT Token
- **Access Token:** 1440 daqiqa (24 soat)
- **Refresh Token:** Token yangilash uchun
- **Claims:** UserId, Email, UserType, Role

## 🌐 API Endpointlar (13 Controller)

### Base URL
```
https://localhost:5001/api/
```

### Endpoint Kategoriyalari

| Controller | Endpoint Pattern | Asosiy Vazifalar |
|------------|-----------------|------------------|
| **Auth** | `/api/auth/*` | Login, Register, Token yangilash, Parolni tiklash |
| **Product** | `/api/product/*` | Mahsulotlar CRUD, Qidiruv, Stok boshqaruvi, Like |
| **Category** | `/api/category/*` | Kategoriyalar CRUD, Ierarxiya |
| **Cart** | `/api/cart/*` | Savat CRUD, Jami narx hisoblash |
| **Order** | `/api/order/*` | Buyurtmalar CRUD, Status yangilash, Chegirmalar |
| **Cashback** | `/api/cashback/*` | Cashback tarix, Balans, Muddati tugaydigan cashback |
| **Delivery** | `/api/delivery/*` | Yetkazib berish hamkorlari, Tracking |
| **Support** | `/api/support/*` | Chat yaratish, Xabarlar, Status yangilash |
| **Notification** | `/api/notification/*` | Bildirishnomalar, O'qilgan belgilash |
| **Content** | `/api/content/*` | Blog, News, FAQ, Policy CRUD |
| **Seller** | `/api/seller/*` | Sotuvchilar CRUD, Performance |
| **Report** | `/api/report/*` | Sotuv, Inventar, Mijozlar hisobotlari |
| **Customer** | `/api/customer/*` | Mijozlar CRUD, Analitika |

## 🔄 Buyurtma Workflow

```
1. Pending (Kutilmoqda)
   ↓
2. Confirmed (Tasdiqlangan)
   ↓
3. Preparing (Tayyorlanmoqda)
   ↓
4. ReadyForPickup (Olib ketishga tayyor) / 5. Shipped (Yuborilgan)
   ↓
6. Delivered (Yetkazildi) / 7. Cancelled (Bekor qilindi)
```

## 📱 Client Application Turlari

### 1. Mobile App (Flutter)
- Mijozlar uchun
- Mahsulot ko'rish, xarid qilish
- Buyurtma kuzatuvi
- Cashback tizimi
- Push notifications

### 2. Seller App (Flutter - opsional)
- Sotuvchilar uchun
- Offline buyurtmalar yaratish
- Sotuv statistikasi

### 3. Desktop Admin Panel
- Administratorlar uchun
- To'liq tizim boshqaruvi
- Hisobotlar va analitika
- Foydalanuvchilar boshqaruvi
- Kontent boshqaruvi

## 💾 Database Configuration

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ZiyoDb;User Id=postgres;Password=2001;"
  }
}
```

**Migration Commands:**
```bash
# Migration yaratish
dotnet ef migrations add MigrationName --project src/ZiyoMarket.Data --startup-project src/ZiyoMarket.Api

# Database yangilash
dotnet ef database update --project src/ZiyoMarket.Data --startup-project src/ZiyoMarket.Api
```

## 🛠️ Ishga Tushirish

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 14+
- Visual Studio 2022 / VS Code / Rider

### Installation

1. **Repository ni clone qiling:**
```bash
git clone [repository-url]
cd ZiyoMarket
```

2. **Database yarating:**
```bash
# PostgreSQL da ZiyoDb database yarating
psql -U postgres
CREATE DATABASE "ZiyoDb";
```

3. **Connection string ni sozlang:**
`src/ZiyoMarket.Api/appsettings.json` faylida connection stringni o'zgartiring

4. **Migration ni qo'llang:**
```bash
cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data
```

5. **Loyihani ishga tushiring:**
```bash
dotnet run
```

6. **Swagger UI ni oching:**
```
https://localhost:5001/swagger
```

## 🧪 Test Ma'lumotlari Yaratish

Har bir entity uchun seed endpoint mavjud:

```bash
POST /api/category/seed      # Kategoriyalar
POST /api/product/seed       # Mahsulotlar
POST /api/seller/seed        # Sotuvchilar
POST /api/order/seed         # Buyurtmalar
POST /api/cashback/seed      # Cashback
POST /api/support/seed/chats # Support chatlar
POST /api/notification/seed  # Bildirishnomalar
POST /api/content/seed       # Kontent
```

## 📖 Dokumentatsiya Fayllari

Har bir jamoa a'zosi uchun maxsus qo'llanma:

- **[BACKEND_DEVELOPER_GUIDE.md](./BACKEND_DEVELOPER_GUIDE.md)** - Backend dasturchilar uchun to'liq qo'llanma
- **[FLUTTER_DEVELOPER_GUIDE.md](./FLUTTER_DEVELOPER_GUIDE.md)** - Flutter dasturchilar uchun API integration qo'llanmasi
- **[DESKTOP_ADMIN_GUIDE.md](./DESKTOP_ADMIN_GUIDE.md)** - Desktop admin panel dasturchilar uchun qo'llanma

## 🔒 Xavfsizlik

- ✅ BCrypt password hashing
- ✅ JWT token-based authentication
- ✅ Role-based authorization
- ✅ Soft delete (ma'lumotlar fizik o'chirilmaydi)
- ✅ Audit trails (kim, qachon yaratdi/o'zgartirdi)
- ✅ SQL injection himoyasi (EF Core)

## 📊 Loyiha Statistikasi

```
Entitylar:        20+
Controllerlar:    14
Servislar:        13
Endpointlar:      150+
Enums:            14
DTOs:             60+
Migrations:       15+
```

## 🤝 Jamoa Tarkibi

- **Backend Developer** - ASP.NET Core, PostgreSQL, API development
- **Flutter Developer (Mobile)** - Customer mobile app
- **Flutter Developer (Seller - optional)** - Seller mobile app
- **Desktop Developer** - Admin panel (Windows/Mac/Linux)

## 📞 Support

Muammolar yoki savollar uchun:
- Support chat tizimidan foydalaning
- Email: support@ziyomarket.uz (example)

## 📄 License

[License turini belgilang]

---

**Version:** 1.0.0
**Last Updated:** 2025-01-30
**Status:** Production Ready
