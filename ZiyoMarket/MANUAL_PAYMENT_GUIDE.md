# Manual Payment Verification System (Bank O'tkazmasi)

> **Professional to'lov tasdiqlash tizimi** - Mijozlar to'g'ridan-to'g'ri admin kartasiga pul o'tkazib, admin tasdiqlaydi.

## 📋 Tizim Haqida

ZiyoMarket platformasi Click yoki Payme bilan integratsiya qilmasdan, **manual to'lov tasdiqlash tizimi** (Manual Payment Verification) ni ishlatadi. Bu keng tarqalgan va professional yondashuv hisoblanadi.

### Tizim Qanday Ishlaydi?

```
1. Mijoz buyurtma yaratadi
   ↓
2. Tizim admin karta raqamini ko'rsatadi
   ↓
3. Mijoz admin kartasiga pul o'tkazadi
   ↓
4. Mijoz screenshot yoki transaction reference yuboradi
   ↓
5. Admin to'lovni ko'rib chiqadi
   ↓
6. Admin tasdiqlaydi yoki rad etadi
   ↓
7. Buyurtma holati yangilanadi
```

---

## 🎯 Tizim Afzalliklari

✅ **Click/Payme integrat siya kerak emas** - Har qanday bank kartasiga o'tkazma qabul qilish mumkin
✅ **Xavfsiz** - Faqat admin tasdiqlagan to'lovlar qabul qilinadi
✅ **Moslashuvchan** - Har qanday to'lov usulini qo'llab-quvvatlaydi
✅ **Screenshot isboti** - Mijoz to'lov isbotini yuklaydi
✅ **Qayta urinishlar** - Agar rad etilsa, mijoz qayta to'lov yuborishi mumkin
✅ **Tarixni saqlash** - Barcha to'lov urinishlari saqlanadi

---

## 🛠 Texnik Arxitektura

### 1. Database Schema

#### `Orders` jadvali (yangilangan)
```sql
ALTER TABLE "Orders" ADD "PaymentStatus" integer NOT NULL DEFAULT 0;
```

**PaymentStatus enum qiymatlari:**
- `1` - Pending (Kutilmoqda)
- `2` - AwaitingProof (Isboti kutilmoqda)
- `3` - UnderReview (Ko'rib chiqilmoqda)
- `4` - Verified (Tasdiqlandi)
- `5` - Rejected (Rad etildi)
- `6` - Refunded (Qaytarildi)

#### `PaymentProofs` jadvali (yangi)
```sql
CREATE TABLE "PaymentProofs" (
    "Id" integer PRIMARY KEY,
    "OrderId" integer NOT NULL,
    "CustomerId" integer NOT NULL,
    "PaymentMethod" integer NOT NULL,
    "Amount" numeric NOT NULL,
    "TransactionReference" text,           -- Bank transaction reference
    "SenderCardNumber" text,               -- Oxirgi 4 ta raqam: **** 1234
    "ProofImageUrl" text,                  -- Screenshot path
    "PaymentDate" text,                    -- Mijoz to'lov qilgan sana
    "Status" integer NOT NULL,             -- PaymentStatus
    "CustomerNotes" text,
    "AdminNotes" text,
    "ReviewedAt" text,                     -- Admin ko'rgan vaqt
    "ReviewedBy" integer,                  -- Qaysi admin ko'rdi
    "CreatedAt" text NOT NULL,
    "UpdatedAt" text,
    "DeletedAt" text,
    "CreatedBy" integer,
    "UpdatedBy" integer,
    "DeletedBy" integer,
    CONSTRAINT "FK_PaymentProofs_Orders" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);
```

### 2. PaymentMethod Enum (yangilangan)
```csharp
public enum PaymentMethod
{
    Cash = 1,           // Naqd pul
    Card = 2,           // Bank kartasi (Click/Payme)
    Cashback = 3,       // Cashback dan
    Mixed = 4,          // Aralash
    Click = 5,          // Click tizimi
    BankTransfer = 6    // ✨ YANGI: Bank o'tkazmasi (Manual)
}
```

### 3. SystemSettings (Admin Karta Ma'lumotlari)

Admin panel orqali quyidagi sozlamalar kiritiladi:

```json
{
  "Payment.BankTransfer.CardNumber": "8600 1234 5678 9012",
  "Payment.BankTransfer.CardHolderName": "Abdulaziz Raximov",
  "Payment.BankTransfer.BankName": "Kapitalbank"
}
```

---

## 🔄 To'lov Jarayoni (Step-by-Step)

### QADM 1: Mijoz Buyurtma Yaratadi

**API Request:**
```http
POST /api/order/create
Authorization: Bearer {customer_token}
Content-Type: application/json

{
  "items": [
    {
      "product_id": 123,
      "quantity": 2,
      "unit_price": 50000
    }
  ],
  "payment_method": 6,  // BankTransfer
  "delivery_type": 2,   // HomeDelivery
  "delivery_address": "Toshkent sh., Chilonzor tumani",
  "customer_notes": "Eshik oldiga qoldiring"
}
```

**API Response:**
```json
{
  "status": true,
  "message": "Buyurtma muvaffaqiyatli yaratildi",
  "data": {
    "order_id": 45,
    "order_number": "ORD-20260311-789",
    "total_price": 100000,
    "final_price": 100000,
    "payment_status": 2,  // AwaitingProof
    "payment_method": 6,  // BankTransfer
    "admin_card_info": {
      "card_number": "8600 1234 5678 9012",
      "card_holder_name": "Abdulaziz Raximov",
      "bank_name": "Kapitalbank"
    },
    "instructions": "Ushbu karta raqamiga 100,000 so'm o'tkazing va screenshot yuklang"
  }
}
```

### QADAM 2: Mijoz Admin Kartasiga Pul O'tkazadi

Mijoz o'z mobile banking ilovasidan admin kartasiga pul o'tkazadi:
- **Summa:** 100,000 so'm
- **Qabul qiluvchi:** 8600 1234 5678 9012 (Abdulaziz Raximov)

### QADAM 3: Mijoz To'lov Isbotini Yuklaydi

**Mobile App ekrani:**
```
┌──────────────────────────────────┐
│  📸 To'lov Isbotini Yuklash      │
├──────────────────────────────────┤
│                                  │
│  [Screenshot yuklash]            │
│                                  │
│  Transaction Ref: 123456789      │
│  Jo'natuvchi karta: **** 5678    │
│  Summa: 100,000 so'm             │
│  Izoh: (ixtiyoriy)               │
│                                  │
│  [Yuborish]                      │
└──────────────────────────────────┘
```

**API Request:**
```http
POST /api/payment-proof/upload
Authorization: Bearer {customer_token}
Content-Type: multipart/form-data

{
  "order_id": 45,
  "amount": 100000,
  "transaction_reference": "123456789",
  "sender_card_number": "**** 5678",
  "payment_date": "2026-03-11 15:30:00",
  "proof_image": [binary_file],
  "customer_notes": "Soat 15:30 da o'tkazdim"
}
```

**API Response:**
```json
{
  "status": true,
  "message": "To'lov isboti yuklandi. Admin tasdiqlashi kutilmoqda.",
  "data": {
    "payment_proof_id": 12,
    "order_id": 45,
    "status": 3,  // UnderReview
    "uploaded_at": "2026-03-11 15:35:00",
    "estimated_review_time": "1-24 soat"
  }
}
```

### QADAM 4: Admin To'lovni Ko'rib Chiqadi

**Admin Panel ekrani:**
```
┌────────────────────────────────────────────┐
│  🔍 To'lov Tasdiqlash                      │
├────────────────────────────────────────────┤
│  Buyurtma: ORD-20260311-789                │
│  Mijoz: Jahongir Alimov (+998 90 123 45 67)│
│  Summa: 100,000 so'm                       │
│  Yuklangan: 2026-03-11 15:35:00            │
│                                            │
│  📷 Screenshot:                            │
│  [To'lov isboti rasmi]                     │
│                                            │
│  Transaction Ref: 123456789                │
│  Jo'natuvchi: **** 5678                    │
│  Mijoz izohi: "Soat 15:30 da o'tkazdim"    │
│                                            │
│  Admin izohi: ___________________          │
│                                            │
│  [✅ Tasdiqlash]  [❌ Rad etish]          │
└────────────────────────────────────────────┘
```

**Tasdiqlash API Request:**
```http
POST /api/payment-proof/verify/12
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "admin_notes": "To'lov tasdiqlandi. Bank hisобida ko'rindi."
}
```

**API Response:**
```json
{
  "status": true,
  "message": "To'lov tasdiqlandi",
  "data": {
    "payment_proof_id": 12,
    "order_id": 45,
    "status": 4,  // Verified
    "reviewed_by": 2,  // Admin ID
    "reviewed_at": "2026-03-11 16:00:00"
  }
}
```

**Rad etish API Request:**
```http
POST /api/payment-proof/reject/12
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "admin_notes": "Screenshot noaniq. Iltimos, aniqroq rasm yuklang."
}
```

### QADAM 5: Mijoz Xabardor Qilinadi

**Push Notification (mijozga):**
```
✅ To'lovingiz tasdiqlandi!

Buyurtma #ORD-20260311-789
To'lov summasi: 100,000 so'm
Buyurtmangiz tayyorlanmoqda.
```

yoki

**Push Notification (rad etilsa):**
```
❌ To'lovingiz rad etildi

Sabab: Screenshot noaniq
Iltimos, aniqroq rasm yuklang.
Qayta urinish: [Qayta yuklash]
```

---

## 📊 API Endpoints

### Customer Endpoints

#### 1. Admin Karta Ma'lumotlarini Olish
```http
GET /api/payment/bank-transfer-info
Authorization: Bearer {customer_token}

Response:
{
  "status": true,
  "data": {
    "card_number": "8600 1234 5678 9012",
    "card_holder_name": "Abdulaziz Raximov",
    "bank_name": "Kapitalbank",
    "instructions": "Ushbu karta raqamiga pul o'tkazing va screenshot yuklang"
  }
}
```

#### 2. To'lov Isbotini Yuklash
```http
POST /api/payment-proof/upload
Authorization: Bearer {customer_token}
Content-Type: multipart/form-data

Request:
- order_id: 45
- amount: 100000
- transaction_reference: "123456789"
- sender_card_number: "**** 5678"
- payment_date: "2026-03-11 15:30:00"
- proof_image: [file]
- customer_notes: "Izoh"

Response:
{
  "status": true,
  "message": "To'lov isboti yuklandi",
  "data": { ... }
}
```

#### 3. O'z To'lov Isbotlarini Ko'rish
```http
GET /api/payment-proof/my-proofs?order_id=45
Authorization: Bearer {customer_token}

Response:
{
  "status": true,
  "data": [
    {
      "id": 12,
      "order_id": 45,
      "amount": 100000,
      "status": 3,  // UnderReview
      "uploaded_at": "2026-03-11 15:35:00",
      "proof_image_url": "https://api.ziyomarket.uz/images/payment_proofs/abc123.jpg"
    }
  ]
}
```

### Admin Endpoints

#### 1. Tasdiqlash Kutayotgan To'lovlar
```http
GET /api/payment-proof/pending
Authorization: Bearer {admin_token}

Response:
{
  "status": true,
  "data": [
    {
      "id": 12,
      "order_number": "ORD-20260311-789",
      "customer_name": "Jahongir Alimov",
      "customer_phone": "+998 90 123 45 67",
      "amount": 100000,
      "transaction_reference": "123456789",
      "proof_image_url": "...",
      "uploaded_at": "2026-03-11 15:35:00",
      "status": 3  // UnderReview
    }
  ],
  "total_count": 5,
  "pending_amount": 500000
}
```

#### 2. To'lovni Tasdiqlash
```http
POST /api/payment-proof/verify/{id}
Authorization: Bearer {admin_token}

Request:
{
  "admin_notes": "To'lov tasdiqlandi"
}

Response:
{
  "status": true,
  "message": "To'lov tasdiqlandi",
  "data": { ... }
}
```

#### 3. To'lovni Rad Etish
```http
POST /api/payment-proof/reject/{id}
Authorization: Bearer {admin_token}

Request:
{
  "admin_notes": "Screenshot noaniq. Qayta yuklang."
}

Response:
{
  "status": true,
  "message": "To'lov rad etildi",
  "data": { ... }
}
```

#### 4. To'lov Tarixini Ko'rish
```http
GET /api/payment-proof/all?page=1&page_size=20&status=4
Authorization: Bearer {admin_token}

Response:
{
  "status": true,
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "page_size": 20,
    "total_count": 150,
    "total_pages": 8
  }
}
```

---

## 🎨 Mobile App UI/UX

### Mijoz Uchun Flow

#### Ekran 1: Buyurtma Yaratish
```
┌──────────────────────────────────┐
│  🛒 Buyurtmani Rasmiylashtirish  │
├──────────────────────────────────┤
│  Jami: 100,000 so'm              │
│                                  │
│  To'lov usuli:                   │
│  ○ Naqd                          │
│  ● Bank o'tkazmasi ✨            │
│  ○ Click                         │
│                                  │
│  [Buyurtma berish]               │
└──────────────────────────────────┘
```

#### Ekran 2: Admin Karta Ma'lumotlari
```
┌──────────────────────────────────┐
│  💳 To'lov Ma'lumotlari          │
├──────────────────────────────────┤
│  Ushbu kartaga o'tkazing:        │
│                                  │
│  8600 1234 5678 9012             │
│  Abdulaziz Raximov               │
│  Kapitalbank                     │
│                                  │
│  Summa: 100,000 so'm             │
│                                  │
│  [📋 Nusxa olish]               │
│  [To'lov qildim]                 │
└──────────────────────────────────┘
```

#### Ekran 3: To'lov Isbotini Yuklash
```
┌──────────────────────────────────┐
│  📸 To'lov Isbotini Yuklash      │
├──────────────────────────────────┤
│  Summa: 100,000 so'm             │
│                                  │
│  [Screenshot yuklash]            │
│  ✅ payment_proof.jpg            │
│                                  │
│  Transaction Ref (ixtiyoriy):    │
│  [_________________]             │
│                                  │
│  Jo'natuvchi karta:              │
│  [**** ____]                     │
│                                  │
│  Izoh (ixtiyoriy):               │
│  [_________________]             │
│                                  │
│  [Yuborish]                      │
└──────────────────────────────────┘
```

#### Ekran 4: Kutish Holati
```
┌──────────────────────────────────┐
│  ⏳ Admin Tasdiqlashi Kutilmoqda │
├──────────────────────────────────┤
│  Buyurtma: #ORD-20260311-789     │
│  Summa: 100,000 so'm             │
│                                  │
│  📷 Yuklangan screenshot         │
│  [Ko'rish]                       │
│                                  │
│  ℹ️ Admin 1-24 soat ichida       │
│     ko'rib chiqadi               │
│                                  │
│  [Boshqa screenshot yuklash]     │
└──────────────────────────────────┘
```

---

## 🔐 Xavfsizlik

### 1. Screenshot Himoyasi
- Screenshot'lar `wwwroot/images/payment_proofs/` papkasida saqlanadi
- Fayl nomi: `{order_id}_{timestamp}_{guid}.jpg`
- Faqat authenticated mijoz va admin ko'ra oladi
- Maksimal hajm: 5 MB
- Ruxsat etilgan format: JPG, PNG, PDF

### 2. Fraud Prevention
✅ Bir buyurtmaga bir necha marta to'lov isboti yuklash mumkin (rad etilsa, qayta urinish)
✅ Admin har bir to'lovni qo'lda tasdiqlaydi
✅ Tasdiqlangan to'lovlarni o'zgartirib bo'lmaydi
✅ Barcha o'zgarishlar audit log'da saqlanadi (CreatedBy, UpdatedBy, ReviewedBy)

### 3. Admin Huquqlari
- Faqat **SuperAdmin** va **Finance** rollari to'lovlarni tasdiqlashi mumkin
- Har bir tasdiq/rad etish admin izohi bilan birga keladi
- Admin har qanday vaqtda to'lov tarixini ko'ra oladi

---

## 📈 Statistika va Hisobotlar

### Admin Dashboard

```
┌─────────────────────────────────────────────┐
│  💰 To'lov Statistikasi (Bugun)             │
├─────────────────────────────────────────────┤
│  Kutilmoqda:      5 ta  (500,000 so'm)      │
│  Tasdiqlandi:    12 ta  (1,200,000 so'm)    │
│  Rad etildi:      2 ta  (150,000 so'm)      │
│  Qaytarildi:      1 ta  (50,000 so'm)       │
└─────────────────────────────────────────────┘
```

---

## 🚀 Deployment

### 1. Database Migration

Migration allaqachon yaratilgan va qo'llanilgan:

```bash
cd src/ZiyoMarket.Api
dotnet ef migrations add AddManualPaymentVerificationSystem --project ../ZiyoMarket.Data
dotnet ef database update --project ../ZiyoMarket.Data
```

### 2. SystemSettings Sozlash

Admin panel orqali yoki SQL orqali admin karta ma'lumotlarini kiriting:

```sql
INSERT INTO "SystemSettings" ("SettingKey", "SettingValue", "Description", "Category", "DataType", "IsEditable", "CreatedAt")
VALUES
  ('Payment.BankTransfer.Enabled', 'true', 'Bank o''tkazmasi yoqilganmi', 'Payment', 'Boolean', true, NOW()),
  ('Payment.BankTransfer.CardNumber', '8600 1234 5678 9012', 'Admin karta raqami', 'Payment', 'String', true, NOW()),
  ('Payment.BankTransfer.CardHolderName', 'Abdulaziz Raximov', 'Karta egasi', 'Payment', 'String', true, NOW()),
  ('Payment.BankTransfer.BankName', 'Kapitalbank', 'Bank nomi', 'Payment', 'String', true, NOW());
```

### 3. wwwroot Papka Yaratish

```bash
mkdir -p src/ZiyoMarket.Api/wwwroot/images/payment_proofs
```

### 4. Permissions Sozlash

Finance yoki Admin rollari uchun permission qo'shing:

```sql
-- Permission yaratish
INSERT INTO "Permissions" ("PermissionName", "Description", "Category", "CreatedAt")
VALUES ('VerifyPayments', 'To''lovlarni tasdiqlash/rad etish', 'Payment', NOW());

-- SuperAdmin rolliga permission berish
INSERT INTO "RolePermissions" ("RoleId", "PermissionId", "CreatedAt")
SELECT r."Id", p."Id", NOW()
FROM "Roles" r, "Permissions" p
WHERE r."RoleName" = 'SuperAdmin' AND p."PermissionName" = 'VerifyPayments';
```

---

## ✅ Xulosa

**Manual Payment Verification tizimi** professional, xavfsiz va ishonchli to'lov yechimi hisoblanadi.

### Keyingi Qadamlar:

1. ✅ Database migration yaratildi va qo'llanildi
2. ✅ Entity va enum'lar yaratildi
3. ✅ UnitOfWork yangilandi
4. ⏳ DTOs va Controller endpoint'lar yaratish kerak
5. ⏳ Mobile app integratsiyasi (Flutter)
6. ⏳ Admin panel UI (WPF yoki Web)

**Muallif:** ZiyoMarket Development Team
**Sana:** 2026-03-11
**Versiya:** 1.0.0
