# ZiyoMarket - Quick Start Guide

## 🚀 Tezkor Ishga Tushirish (5 daqiqa)

### 1️⃣ Database yaratish va migration qo'llash

```bash
# PostgreSQL ga ulanish
psql -U postgres

# Database yaratish
CREATE DATABASE "ZiyoDb";

# Chiqish
\q
```

### 2️⃣ Migration ni qo'llash

```bash
# Loyiha papkasiga o'tish
cd C:\Users\abdum\OneDrive\Desktop\Kutubxona\ZiyoMarket

# Migration qo'llash
cd src\ZiyoMarket.Api
dotnet ef database update --project ..\ZiyoMarket.Data
```

### 3️⃣ Test ma'lumotlar qo'shish

**Option A: SQL Script orqali (Tezroq)**

```bash
# PostgreSQL ga ulanish va script ishga tushirish
psql -U postgres -d ZiyoDb -f database_seed.sql
```

**Option B: Swagger orqali (Qo'lda)**

Loyihani ishga tushiring va Swagger dan seed endpointlarni chaqiring (pastga qarang).

### 4️⃣ Loyihani ishga tushirish

```bash
# API papkasida
cd src\ZiyoMarket.Api
dotnet run
```

✅ Loyiha ishga tushdi! Browser oching:
- Swagger UI: https://localhost:5001/swagger
- API Base URL: https://localhost:5001/api

---

## 🔑 Default Test Credentials

### Admin Login
```
Email: admin@ziyomarket.uz
Password: Admin@123
UserType: Admin
```

### Test Customer
```
Email: john@example.com
Password: password123
UserType: Customer
```

### Test Seller
```
Email: seller1@example.com
Password: password123
UserType: Seller
```

---

## 📝 Swagger da Birinchi Test

### Qadam 1: Swagger ni oching
```
https://localhost:5001/swagger
```

### Qadam 2: Login qiling

1. **POST /api/auth/login** endpointini oching
2. "Try it out" tugmasini bosing
3. Quyidagi ma'lumotni kiriting:

```json
{
  "phoneOrEmail": "admin@ziyomarket.uz",
  "password": "Admin@123",
  "userType": "Admin"
}
```

4. "Execute" tugmasini bosing
5. Response dan **accessToken** ni ko'chirib oling

### Qadam 3: Token ni Authorize ga qo'shish

1. Swagger yuqori qismidagi **"Authorize"** tugmasini bosing (yashil qulf)
2. Quyidagi formatda kiriting:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
⚠️ **Muhim:** `Bearer` so'zidan keyin probel bo'lishi kerak!

3. "Authorize" tugmasini bosing
4. "Close" tugmasini bosing

✅ Endi barcha protected endpointlar ishlaydi!

### Qadam 4: Biror endpoint test qiling

Masalan: **GET /api/auth/me** - sizning profil ma'lumotlaringiz

---

## 🎯 Test Ma'lumotlar Yaratish (agar SQL script ishlamagan bo'lsa)

Swagger orqali quyidagi seed endpointlarni ketma-ket chaqiring:

1. **POST /api/category/seed** - Kategoriyalar yaratish
2. **POST /api/product/seed** - Mahsulotlar yaratish
3. **POST /api/seller/seed** - Sotuvchilar yaratish
4. **POST /api/order/seed** - Buyurtmalar yaratish
5. **POST /api/cashback/seed** - Cashback tranzaksiyalar
6. **POST /api/notification/seed** - Bildirishnomalar
7. **POST /api/support/seed/chats** - Support chatlar

---

## 🧪 API Test Workflow (Customer sifatida)

### 1. Register (Yangi customer yaratish)
**POST /api/auth/register**
```json
{
  "firstName": "Test",
  "lastName": "User",
  "phone": "+998909999999",
  "email": "test@example.com",
  "password": "password123",
  "address": "Toshkent"
}
```

### 2. Login
**POST /api/auth/login**
```json
{
  "phoneOrEmail": "test@example.com",
  "password": "password123",
  "userType": "Customer"
}
```
**Token ni Authorize ga qo'shish!**

### 3. Mahsulotlarni ko'rish
**GET /api/product?pageNumber=1&pageSize=20**

### 4. Mahsulot detallari
**GET /api/product/1**

### 5. Savatga qo'shish
**POST /api/cart**
```json
{
  "productId": 1,
  "quantity": 2
}
```

### 6. Savatni ko'rish
**GET /api/cart**

### 7. Buyurtma yaratish
**POST /api/order**
```json
{
  "orderItems": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 250000
    }
  ],
  "paymentMethod": "Card",
  "deliveryType": "Home",
  "deliveryAddress": "Toshkent, Yakkasaroy tumani, Bobur ko'chasi 12",
  "customerNotes": "Iltimos, tez yetkazib bering"
}
```

### 8. Buyurtmalarimni ko'rish
**GET /api/order**

### 9. Cashback balansini ko'rish
**GET /api/cashback/summary**

---

## 🔍 Muammolarni Bartaraf Qilish

### ❌ Muammo: "Failed to connect to database"

**Yechim:**
1. PostgreSQL ishga tushganini tekshiring:
```bash
# Windows
net start postgresql-x64-14

# Yoki pgAdmin orqali tekshiring
```

2. Connection string to'g'rimi? (`src/ZiyoMarket.Api/appsettings.json`):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ZiyoDb;User Id=postgres;Password=2001;"
}
```

### ❌ Muammo: "401 Unauthorized"

**Yechim:**
1. Login qilgansizmi?
2. Token ni "Authorize" ga to'g'ri kiritdingizmi?
3. Token muddati tugaganmi? (Qayta login qiling)

### ❌ Muammo: "Database 'ZiyoDb' does not exist"

**Yechim:**
```bash
psql -U postgres
CREATE DATABASE "ZiyoDb";
\q
```

### ❌ Muammo: "Pending migrations"

**Yechim:**
```bash
cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data
```

### ❌ Muammo: "Admin user not found"

**Yechim:**
```bash
# SQL script ni qayta ishga tushiring
psql -U postgres -d ZiyoDb -f database_seed.sql
```

---

## 📚 Keyingi Qadamlar

1. ✅ [BACKEND_DEVELOPER_GUIDE.md](./BACKEND_DEVELOPER_GUIDE.md) - Backend ishlab chiqish
2. ✅ [FLUTTER_DEVELOPER_GUIDE.md](./FLUTTER_DEVELOPER_GUIDE.md) - Mobile app ishlab chiqish
3. ✅ [DESKTOP_ADMIN_GUIDE.md](./DESKTOP_ADMIN_GUIDE.md) - Admin panel ishlab chiqish
4. ✅ [SWAGGER_GUIDE.md](./SWAGGER_GUIDE.md) - Swagger to'liq qo'llanma

---

## 🎓 Video Qo'llanmalar (Tavsiya)

1. ASP.NET Core JWT Authentication
2. PostgreSQL Setup
3. Entity Framework Core Migrations
4. Swagger API Testing

---

## 💡 Pro Tips

1. **Hot Reload:** Development paytida kod o'zgarishi bilan avtomatik restart:
```bash
dotnet watch run
```

2. **Database Reset:**
```bash
dotnet ef database drop --force --project ../ZiyoMarket.Data
dotnet ef database update --project ../ZiyoMarket.Data
psql -U postgres -d ZiyoDb -f database_seed.sql
```

3. **Logs ko'rish:** Console da barcha requestlar ko'rinadi

4. **JWT Token Decode:** https://jwt.io da token ni decode qilib ko'ring

---

## 📞 Yordam

Muammo yuzaga kelsa:
1. Dokumentatsiyani o'qing
2. GitHub issues ga yozing
3. Team lead bilan bog'laning

---

**Happy Coding! 🚀**

**Version:** 1.0.0
**Last Updated:** 2025-01-30
