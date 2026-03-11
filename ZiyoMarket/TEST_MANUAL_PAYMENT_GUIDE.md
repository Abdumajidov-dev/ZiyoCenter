# Manual Payment Verification - Test Qo'llanmasi

## ✅ Qilingan Ishlar

### 1. Backend Tizim
- ✅ `PaymentStatus` enum yaratildi (6 ta holat)
- ✅ `PaymentMethod.BankTransfer` qo'shildi
- ✅ `PaymentProof` entity yaratildi
- ✅ `PaymentProofs` jadvali database'ga qo'shildi
- ✅ `Order.PaymentStatus` qo'shildi
- ✅ DTOs yaratildi (8 ta DTO)
- ✅ AutoMapper mapping'lar qo'shildi
- ✅ `PaymentProofController` yaratildi (9 ta endpoint)
- ✅ UnitOfWork'ga `PaymentProofs` repository qo'shildi

### 2. Endpoint'lar
1. `GET /api/payment-proof/bank-transfer-info` - Admin karta ma'lumotlari (Public)
2. `POST /api/payment-proof/upload` - To'lov isbotini yuklash (Customer)
3. `GET /api/payment-proof/my-proofs` - O'z to'lov isbotlarini ko'rish (Customer)
4. `GET /api/payment-proof/pending` - Kutayotgan to'lovlar (Admin)
5. `POST /api/payment-proof/verify/{id}` - To'lovni tasdiqlash (Admin)
6. `POST /api/payment-proof/reject/{id}` - To'lovni rad etish (Admin)
7. `GET /api/payment-proof/all` - Barcha to'lovlar (Admin, pagination)
8. `GET /api/payment-proof/stats` - Statistika (Admin)

---

## 🧪 Test Qilish (Swagger)

### QADM 1: API'ni Ishga Tushirish

```bash
cd src/ZiyoMarket.Api
dotnet run
```

Keyin brauzerni oching:
```
http://localhost:8080/swagger
```

---

### QADAM 2: Admin Karta Ma'lumotlarini Sozlash

**Agar database'ga kirish imkoniyatingiz yo'q bo'lsa**, SystemSettings endpoint orqali qo'shing:

**Option 1: SQL Orqali (Agar psql bor bo'lsa)**

```sql
INSERT INTO "SystemSettings" ("SettingKey", "SettingValue", "Description", "Category", "DataType", "IsEditable", "CreatedAt")
VALUES
  ('Payment.BankTransfer.Enabled', 'true', 'Bank o''tkazmasi yoqilganmi', 'Payment', 'Boolean', true, NOW()),
  ('Payment.BankTransfer.CardNumber', '8600 1234 5678 9012', 'Sizning karta raqamingiz!', 'Payment', 'String', true, NOW()),
  ('Payment.BankTransfer.CardHolderName', 'Sizning ismingiz', 'Karta egasi', 'Payment', 'String', true, NOW()),
  ('Payment.BankTransfer.BankName', 'Kapitalbank', 'Bank nomi', 'Payment', 'String', true, NOW())
ON CONFLICT ("SettingKey") DO UPDATE
  SET "SettingValue" = EXCLUDED."SettingValue";
```

**Option 2: PostgreSQL GUI (pgAdmin, DBeaver, etc.) Orqali**

Table: `SystemSettings`
Insert qiling:
- `SettingKey`: `Payment.BankTransfer.CardNumber`
- `SettingValue`: `8600 1234 5678 9012` (O'zingizniki)
- `Description`: `Admin karta raqami`
- `Category`: `Payment`
- `DataType`: `String`
- `IsEditable`: `true`
- `CreatedAt`: `2026-03-11 00:00:00`

Xuddi shunday:
- `Payment.BankTransfer.CardHolderName` → `Sizning ismingiz`
- `Payment.BankTransfer.BankName` → `Kapitalbank`
- `Payment.BankTransfer.Enabled` → `true`

---

### QADAM 3: Swagger Orqali Test Qilish

#### Test 1: Admin Karta Ma'lumotlarini Olish (Public Endpoint)

Swagger'da:
```
GET /api/payment-proof/bank-transfer-info
```

**Click "Try it out"** → **Execute**

**Expected Response:**
```json
{
  "status": true,
  "message": "Admin karta ma'lumotlari",
  "data": {
    "card_number": "8600 1234 5678 9012",
    "card_holder_name": "Sizning ismingiz",
    "bank_name": "Kapitalbank",
    "instructions": "Ushbu karta raqamiga pul o'tkazing va to'lov isbotini (screenshot) yuklang"
  }
}
```

✅ Agar bu ishlasa, backend to'g'ri ishlayapti!

---

#### Test 2: Login Qilish (Customer Token Olish)

Avval customer sifatida login qilishimiz kerak:

**Swagger'da:**
```
POST /api/auth/login
```

**Request Body:**
```json
{
  "phone": "+998941033001",
  "password": "Test@123"
}
```

**Response:**
```json
{
  "status": true,
  "message": "Login muvaffaqiyatli",
  "data": {
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refresh_token": "...",
    "user_type": "Customer"
  }
}
```

**Access Token'ni nusxa oling!** Bu kerak bo'ladi.

---

#### Test 3: Authorize Qilish

Swagger'ning yuqori o'ng tomonidagi **"Authorize" 🔓** tugmasini bosing.

Value ga:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

(Token'ni `Bearer ` so'zidan keyin qo'ying)

**Authorize** → **Close**

---

#### Test 4: Buyurtma Yaratish (BankTransfer)

**Swagger'da:**
```
POST /api/order/create
```

**Request Body:**
```json
{
  "items": [
    {
      "product_id": 1,
      "quantity": 2,
      "unit_price": 50000
    }
  ],
  "payment_method": 6,
  "delivery_type": 2,
  "delivery_address": "Toshkent sh., Chilonzor tumani",
  "customer_notes": "Test buyurtma"
}
```

**Response:**
```json
{
  "status": true,
  "message": "Buyurtma yaratildi",
  "data": {
    "order_id": 45,
    "order_number": "ORD-20260311-789",
    "payment_status": 2,
    "payment_method": 6
  }
}
```

**Order ID'ni eslab qoling!** (Masalan: 45)

---

#### Test 5: To'lov Isbotini Yuklash

⚠️ **Diqqat:** Screenshot yuklash uchun Swagger'da `multipart/form-data` ishlatishingiz kerak.

**Swagger'da:**
```
POST /api/payment-proof/upload
```

**Parameters:**
- `order_id`: 45
- `amount`: 100000
- `transaction_reference`: "12345"
- `sender_card_number`: "**** 5678"
- `payment_date`: "2026-03-11 15:30:00"
- `proof_image`: [Fayl tanlang - PNG yoki JPG]
- `customer_notes`: "Test to'lov"

**Execute**

**Expected Response:**
```json
{
  "status": true,
  "message": "To'lov isboti yuklandi. Admin tasdiqlashi kutilmoqda.",
  "data": {
    "payment_proof_id": 12,
    "order_id": 45,
    "status": 3,
    "status_text": "Ko'rib chiqilmoqda",
    "uploaded_at": "2026-03-11 18:00:00"
  }
}
```

✅ To'lov isboti yuklandi!

---

#### Test 6: Admin Sifatida Tasdiqlash

Avval Admin sifatida login qiling:

**Swagger'da:**
```
POST /api/auth/login
```

**Request Body:**
```json
{
  "phone": "+998711234567",
  "password": "Admin@123"
}
```

Token olganingizdan keyin, **Authorize** qiling.

---

#### Test 7: Kutayotgan To'lovlarni Ko'rish (Admin)

**Swagger'da:**
```
GET /api/payment-proof/pending
```

**Execute**

**Response:**
```json
{
  "status": true,
  "message": "5 ta kutayotgan to'lov",
  "data": {
    "proofs": [
      {
        "id": 12,
        "order_number": "ORD-20260311-789",
        "customer_name": "Test User",
        "amount": 100000,
        "status": 3,
        "proof_image_url": "images/payment_proofs/45_20260311180000_abc123.jpg"
      }
    ],
    "stats": {
      "total_count": 5,
      "pending_amount": 500000
    }
  }
}
```

---

#### Test 8: To'lovni Tasdiqlash (Admin)

**Swagger'da:**
```
POST /api/payment-proof/verify/12
```

**Request Body:**
```json
{
  "admin_notes": "To'lov tasdiqlandi. Bank hisobida ko'rindi."
}
```

**Execute**

**Response:**
```json
{
  "status": true,
  "message": "To'lov tasdiqlandi",
  "data": {
    "id": 12,
    "order_id": 45,
    "status": 4,
    "status_text": "Tasdiqlandi",
    "reviewed_by": 2,
    "reviewed_at": "2026-03-11 18:05:00"
  }
}
```

✅ To'lov tasdiqlandi!

---

#### Test 9: Statistika (Admin)

**Swagger'da:**
```
GET /api/payment-proof/stats
```

**Response:**
```json
{
  "status": true,
  "message": "To'lov statistikasi",
  "data": {
    "pending_count": 4,
    "pending_amount": 400000,
    "verified_count": 1,
    "verified_amount": 100000,
    "rejected_count": 0,
    "rejected_amount": 0,
    "total_count": 5,
    "total_amount": 500000
  }
}
```

---

## 🎯 Test Natijalari

Agar barcha testlar muvaffaqiyatli bo'lsa:

✅ Backend to'liq ishlayapti
✅ Admin karta ma'lumotlari olinmoqda
✅ Customer to'lov isbotini yuklaydi
✅ Admin tasdiqlaydi/rad etadi
✅ Statistika to'g'ri hisoblanmoqda

---

## 📱 Mobile App Uchun

Mobile app'dan test qilish uchun:

**Base URL:** `http://localhost:8080` (yoki server IP)

**Flutter Example:**
```dart
// 1. Admin karta ma'lumotlarini olish
final response = await http.get(
  Uri.parse('$baseUrl/api/payment-proof/bank-transfer-info'),
);

// 2. To'lov isbotini yuklash
var request = http.MultipartRequest(
  'POST',
  Uri.parse('$baseUrl/api/payment-proof/upload'),
);
request.headers['Authorization'] = 'Bearer $token';
request.fields['order_id'] = '45';
request.fields['amount'] = '100000';
request.files.add(await http.MultipartFile.fromPath(
  'proof_image',
  imagePath,
));
final response = await request.send();
```

---

## 🔍 Troubleshooting

### Problem 1: "Admin karta ma'lumotlari topilmadi"
**Solution:** SystemSettings'ga ma'lumotlarni kiriting (QADAM 2)

### Problem 2: "To'lov isboti yuklanmadi"
**Solution:**
- Screenshot 5MB dan kichik bo'lishi kerak
- Format: JPG, PNG, PDF
- wwwroot/images/payment_proofs papkasi mavjudligini tekshiring

### Problem 3: "Unauthorized"
**Solution:** Authorize tugmasini bosib, token kiriting (Bearer tokeningiz)

---

## 📝 Eslatma

**Database ma'lumotlari:**
- Database: `ZiyoNoorDb`
- Fayllar: `C:\Users\abdum\OneDrive\Desktop\Kutubxona\ZiyoMarket\database_payment_settings.sql`
- wwwroot: `src/ZiyoMarket.Api/wwwroot/images/payment_proofs/`

---

**Muallif:** ZiyoMarket Development Team
**Sana:** 2026-03-11
**Test Versiya:** 1.0
