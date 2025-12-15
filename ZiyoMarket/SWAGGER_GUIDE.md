# Swagger UI - Qo'llanma

## 🚀 Swagger ni ishga tushirish

1. Loyihani ishga tushiring:
```bash
cd src/ZiyoMarket.Api
dotnet run
```

2. Brauzerda oching:
```
https://localhost:5001/swagger
```

---

## 🔐 Token Olish va Ishlatish

### 1. Token Olish (Login)

**Endpoint:** `POST /api/auth/login`

**Request Body:**

#### Admin uchun:
```json
{
  "phoneOrEmail": "admin@ziyomarket.uz",
  "password": "Admin@123",
  "userType": "Admin"
}
```

#### Customer uchun:
```json
{
  "phoneOrEmail": "customer@example.com",
  "password": "password123",
  "userType": "Customer"
}
```

#### Seller uchun:
```json
{
  "phoneOrEmail": "seller@example.com",
  "password": "password123",
  "userType": "Seller"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
    "refreshToken": "refresh_token_string_here",
    "expiresAt": "2025-01-31T14:30:00Z",
    "user": {
      "id": 1,
      "firstName": "Admin",
      "lastName": "User",
      "phone": "+998901234567",
      "email": "admin@ziyomarket.uz",
      "userType": "Admin",
      "isActive": true
    }
  }
}
```

### 2. Token ni Swagger da ishlatish

Swagger UI da yuqoridagi **"Authorize"** tugmasini bosing (yashil qulf belgisi) va quyidagicha kiriting:

```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

⚠️ **Muhim:** `Bearer` so'zidan keyin **bitta probel** bo'lishi kerak!

![Swagger Authorize](https://i.imgur.com/example.png)

Keyin **"Authorize"** tugmasini bosing va barcha endpointlar uchun token avtomatik qo'shiladi.

---

## 📝 Test Misollar

### Test 1: Register (Yangi mijoz yaratish)

**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+998901234567",
  "email": "john.doe@example.com",
  "password": "password123",
  "address": "Toshkent, Yakkasaroy tumani"
}
```

### Test 2: Mahsulotlar ro'yxati (Authorization kerak emas)

**Endpoint:** `GET /api/product?pageNumber=1&pageSize=20`

### Test 3: Savatga qo'shish (Authorization kerak)

**Endpoint:** `POST /api/cart`

**Request:**
```json
{
  "productId": 1,
  "quantity": 2
}
```

**⚠️ Authorization kerak:** Avval login qiling va token ni Authorize da kiriting!

### Test 4: Buyurtma yaratish (Authorization kerak)

**Endpoint:** `POST /api/order`

**Request:**
```json
{
  "orderItems": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 50000
    }
  ],
  "paymentMethod": "Card",
  "deliveryType": "Home",
  "deliveryAddress": "Toshkent, Yakkasaroy tumani, Bobur ko'chasi 12",
  "cashbackToUse": 0,
  "customerNotes": "Iltimos, ertaga yetkazib bering"
}
```

---

## 🧪 Test Ma'lumotlari Yaratish

Agar database bo'sh bo'lsa, test ma'lumotlar yarating:

### 1. Kategoriyalar yaratish
**Endpoint:** `POST /api/category/seed`

### 2. Mahsulotlar yaratish
**Endpoint:** `POST /api/product/seed`

### 3. Sotuvchilar yaratish
**Endpoint:** `POST /api/seller/seed`

### 4. Buyurtmalar yaratish
**Endpoint:** `POST /api/order/seed`

---

## 🔍 Barcha Endpointlar

### Authentication (Authorization kerak emas)
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register
- `POST /api/auth/refresh-token` - Token yangilash
- `POST /api/auth/forgot-password` - Parolni unutdim
- `POST /api/auth/reset-password` - Parolni tiklash

### Authentication (Authorization kerak)
- `POST /api/auth/logout` - Logout
- `POST /api/auth/change-password` - Parolni o'zgartirish
- `GET /api/auth/me` - Profil ma'lumotlari

### Product (Ko'p endpointlar uchun Authorization kerak emas)
- `GET /api/product` - Mahsulotlar ro'yxati
- `GET /api/product/{id}` - Mahsulot detallari
- `GET /api/product/qr/{qrCode}` - QR kod orqali qidirish
- `POST /api/product` - Mahsulot yaratish (Admin)
- `PUT /api/product/{id}` - Mahsulotni yangilash (Admin)
- `DELETE /api/product/{id}` - Mahsulotni o'chirish (Admin)
- `POST /api/product/{productId}/like` - Like/Unlike (Customer)

### Cart (Customer only)
- `GET /api/cart` - Savat
- `POST /api/cart` - Savatga qo'shish
- `PUT /api/cart/{cartItemId}` - Miqdorni yangilash
- `DELETE /api/cart/{cartItemId}` - Savatdan o'chirish
- `DELETE /api/cart/clear` - Savatni tozalash

### Order
- `GET /api/order` - Buyurtmalar (o'zinikilar)
- `GET /api/order/{id}` - Buyurtma detallari
- `POST /api/order` - Buyurtma yaratish (Customer)
- `POST /api/order/seller` - Offline buyurtma (Seller)
- `PUT /api/order/{id}/status` - Status yangilash (Admin/Seller)
- `POST /api/order/{id}/cancel` - Bekor qilish

### Cashback (Customer only)
- `GET /api/cashback/summary` - Cashback xulosasi
- `GET /api/cashback/history` - Cashback tarixi
- `GET /api/cashback/available` - Mavjud cashback

### Support
- `GET /api/support/my-chats` - Mening chatlarim (Customer)
- `POST /api/support` - Chat yaratish (Customer)
- `GET /api/support/{chatId}/messages` - Xabarlar
- `POST /api/support/messages` - Xabar yuborish

### Reports (Admin only)
- `GET /api/report/dashboard` - Dashboard statistikasi
- `GET /api/report/sales` - Sotuv hisoboti
- `GET /api/report/inventory` - Inventar hisoboti

---

## ❓ Keng Tarqalgan Xatolar

### 401 Unauthorized
**Sabab:** Token berilmagan yoki yaroqsiz.
**Yechim:**
1. Avval login qiling
2. Token ni Swagger da "Authorize" orqali kiriting
3. Token muddati tugagan bo'lsa, qayta login qiling

### 403 Forbidden
**Sabab:** Siz bu endpoint ga ruxsat yo'q (masalan, Customer Admin endpointiga kirmoqchi)
**Yechim:** To'g'ri user type bilan login qiling

### 404 Not Found
**Sabab:** Resurs topilmadi (masalan, mahsulot ID mavjud emas)
**Yechim:** To'g'ri ID kiriting yoki avval seed endpoint orqali test ma'lumot yarating

### 400 Bad Request
**Sabab:** Request body noto'g'ri formatda
**Yechim:** Request body ni to'g'ri JSON formatda yuboring

---

## 🎯 Quick Start Checklist

- [ ] Loyihani ishga tushirish (`dotnet run`)
- [ ] Swagger ochish (https://localhost:5001/swagger)
- [ ] Test ma'lumotlar yaratish (seed endpoints)
- [ ] Login qilish (`POST /api/auth/login`)
- [ ] Token ni "Authorize" ga kiritish
- [ ] Biror protected endpoint test qilish

---

**Muammo yuzaga kelsa:**
1. Database connection string to'g'rimi? (appsettings.json)
2. Migration qo'llanganmi? (`dotnet ef database update`)
3. JWT settings to'g'rimi? (appsettings.json)
4. Token muddati tugaganmi? (Qayta login qiling)

---

**Muvaffaqiyatli test qiling! 🚀**
