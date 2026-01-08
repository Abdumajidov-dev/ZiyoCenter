# SMS Integration Guide - Eskiz.uz

ZiyoMarket loyihasiga SMS xizmati (Eskiz.uz) to'liq integratsiya qilindi. Bu qo'llanma SMS xizmatidan foydalanish va sozlashni ko'rsatadi.

## 📋 Qo'shilgan Komponentlar

### 1. Domain Layer (ZiyoMarket.Domain)

**Entities:**
- `SmsLog` - SMS yuborish tarixini saqlash uchun entity
  - Telefon raqami, SMS matni, maqsad, holat
  - Eskiz.uz message ID va xatolik xabarlari
  - Foydalanuvchi ma'lumotlari (agar ma'lum bo'lsa)

**Enums:**
- `SmsPurpose` - SMS yuborish maqsadi
  - Registration, PasswordReset, LoginVerification
  - OrderConfirmation, OrderStatusChange, DeliveryNotification
  - Marketing, Other
- `SmsStatus` - SMS holati
  - Pending, Sent, Delivered, Failed, NotDelivered

### 2. Data Layer (ZiyoMarket.Data)

**DbContext:**
- `SmsLogs` DbSet qo'shildi
- Database migration: `AddSmsLog`

**Repository:**
- `IRepository<SmsLog>` - Generic repository pattern
- `UnitOfWork` ga SmsLogs qo'shildi

### 3. Service Layer (ZiyoMarket.Service)

**DTOs:**
- `SendSmsDto` - SMS yuborish uchun
- `SendVerificationCodeDto` - Verification code yuborish uchun
- `VerifyCodeDto` - Kodni tekshirish uchun
- `SmsLogDto` - SMS log natijasi
- `SmsResultDto` - SMS yuborish natijasi
- `VerificationResultDto` - Verification natijasi
- `SmsStatisticsDto` - SMS statistikasi

**Interfaces:**
- `ISmsService` - SMS xizmati interface

**Services:**
- `SmsService` - Asosiy SMS xizmati
  - SendSmsAsync - SMS yuborish
  - SendVerificationCodeAsync - 6 raqamli kod yuborish
  - VerifyCodeAsync - Kodni tekshirish
  - SendBulkSmsAsync - Ko'plab SMS yuborish
  - GetSmsLogsAsync - SMS loglarni olish
  - GetUserSmsLogsAsync - Foydalanuvchi SMS loglarini olish
  - GetSmsStatisticsAsync - SMS statistikasi

**Helpers:**
- `EskizSmsClient` - Eskiz.uz API client
  - AuthenticateAsync - Avtomatik autentifikatsiya (token 30 kun amal qiladi)
  - SendSmsAsync - SMS yuborish
  - GetSmsStatusAsync - SMS holatini tekshirish

### 4. API Layer (ZiyoMarket.Api)

**Controllers:**
- `SmsController` - SMS API endpoints
  - POST `/api/sms/send` - SMS yuborish (Admin)
  - POST `/api/sms/send-verification-code` - Verification code yuborish (Public)
  - POST `/api/sms/verify-code` - Kodni tekshirish (Public)
  - POST `/api/sms/send-bulk` - Ko'plab SMS yuborish (Admin)
  - GET `/api/sms/logs` - SMS loglarni olish (Admin)
  - GET `/api/sms/my-logs` - Mening SMS loglarim (Authenticated)
  - GET `/api/sms/statistics` - SMS statistikasi (Admin)

**Configuration:**
- `appsettings.json` ga Eskiz.uz sozlamalari qo'shildi

## ⚙️ Sozlash (Configuration)

### 1. Eskiz.uz Account

1. [Eskiz.uz](https://eskiz.uz) da ro'yxatdan o'ting
2. Email va parolingizni oling
3. API access uchun ariza yuboring

### 2. appsettings.json

```json
"EskizSms": {
  "BaseUrl": "https://notify.eskiz.uz/api",
  "Email": "your-email@example.com",        // O'zgartiring
  "Password": "your-password",              // O'zgartiring
  "CallbackUrl": "",                        // Ixtiyoriy
  "IsDevelopment": true                     // Production'da false qiling
}
```

**MUHIM:**
- `Email` va `Password` ni o'z Eskiz.uz ma'lumotlaringiz bilan almashtiring
- Production'da `IsDevelopment: false` qiling (verification code qaytarilmaydi)
- Credentials'larni environment variables yoki user secrets orqali saqlang

### 3. Environment Variables (Production)

```bash
# Railway/Docker uchun
ESKIZSMS__EMAIL=your-email@example.com
ESKIZSMS__PASSWORD=your-password
ESKIZSMS__ISDEVELOPMENT=false
```

## 🚀 Foydalanish (Usage)

### 1. SMS Yuborish (Admin)

**Request:**
```http
POST /api/sms/send
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "phone_number": "+998901234567",
  "message": "Buyurtmangiz qabul qilindi. Tez orada aloqaga chiqamiz.",
  "purpose": 4,  // OrderConfirmation
  "user_id": 5,
  "user_type": "Customer"
}
```

**Response:**
```json
{
  "is_success": true,
  "message": "SMS sent successfully",
  "data": {
    "success": true,
    "message": "SMS sent successfully",
    "message_id": "12345678",
    "sms_log_id": 42
  }
}
```

### 2. Verification Code Yuborish (Registration)

**Request:**
```http
POST /api/sms/send-verification-code
Content-Type: application/json

{
  "phone_number": "+998901234567",
  "purpose": 1  // Registration
}
```

**Response (Development):**
```json
{
  "is_success": true,
  "data": {
    "success": true,
    "message": "Tasdiqlash kodi yuborildi",
    "code": "123456",  // Faqat development'da
    "expires_at": "2025-01-08T13:15:00Z"
  }
}
```

**Response (Production):**
```json
{
  "is_success": true,
  "data": {
    "success": true,
    "message": "Tasdiqlash kodi yuborildi",
    "code": null,  // Production'da null
    "expires_at": "2025-01-08T13:15:00Z"
  }
}
```

### 3. Verification Code Tekshirish

**Request:**
```http
POST /api/sms/verify-code
Content-Type: application/json

{
  "phone_number": "+998901234567",
  "code": "123456",
  "purpose": 1  // Registration
}
```

**Response:**
```json
{
  "is_success": true,
  "data": true,
  "message": "Operation successful"
}
```

**Error Response:**
```json
{
  "is_success": false,
  "message": "Tasdiqlash kodi noto'g'ri",
  "errors": ["Tasdiqlash kodi noto'g'ri"]
}
```

### 4. SMS Logs (Admin)

**Request:**
```http
GET /api/sms/logs?pageNumber=1&pageSize=50&purpose=1&status=2
Authorization: Bearer {admin-token}
```

**Response:**
```json
{
  "is_success": true,
  "data": [
    {
      "id": 1,
      "phone_number": "+998901234567",
      "message": "ZiyoMarket ro'yxatdan o'tish kodi: 123456. Kod 5 daqiqa amal qiladi.",
      "purpose": "Registration",
      "status": "Sent",
      "provider": "Eskiz.uz",
      "provider_message_id": "12345678",
      "error_message": null,
      "sent_at": "2025-01-08 13:00:00",
      "user_id": 5,
      "user_type": "Customer",
      "created_at": "2025-01-08 13:00:00"
    }
  ],
  "message": "Jami 1 ta SMS log topildi"
}
```

### 5. SMS Statistics (Admin)

**Request:**
```http
GET /api/sms/statistics?startDate=2025-01-01&endDate=2025-01-31
Authorization: Bearer {admin-token}
```

**Response:**
```json
{
  "is_success": true,
  "data": {
    "total_sent": 150,
    "delivered": 145,
    "failed": 3,
    "pending": 2,
    "by_purpose": {
      "Registration": 80,
      "PasswordReset": 30,
      "OrderConfirmation": 40
    },
    "by_status": {
      "Sent": 148,
      "Failed": 2
    }
  }
}
```

## 🔐 Authentication Flow (SMS bilan)

### 1. Registration Flow

```
1. POST /api/sms/send-verification-code
   - phone_number: "+998901234567"
   - purpose: Registration (1)

2. Foydalanuvchi SMS orqali 6 raqamli kod oladi

3. POST /api/sms/verify-code
   - phone_number: "+998901234567"
   - code: "123456"
   - purpose: Registration (1)

4. Agar kod to'g'ri bo'lsa, ro'yxatdan o'tish davom etadi
   POST /api/auth/register
   - phone_number: "+998901234567"
   - ... (boshqa ma'lumotlar)
```

### 2. Password Reset Flow

```
1. POST /api/sms/send-verification-code
   - phone_number: "+998901234567"
   - purpose: PasswordReset (2)

2. POST /api/sms/verify-code
   - phone_number: "+998901234567"
   - code: "123456"
   - purpose: PasswordReset (2)

3. Agar kod to'g'ri bo'lsa, parolni o'zgartirish davom etadi
   POST /api/auth/reset-password
   - phone_number: "+998901234567"
   - new_password: "NewPassword123!"
```

## 📊 Database Schema

```sql
CREATE TABLE "SmsLogs" (
    "Id" SERIAL PRIMARY KEY,
    "PhoneNumber" TEXT NOT NULL,
    "Message" TEXT NOT NULL,
    "Purpose" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "Provider" TEXT NOT NULL,
    "ProviderMessageId" TEXT,
    "ErrorMessage" TEXT,
    "SentAt" TEXT,
    "UserId" INTEGER,
    "UserType" TEXT,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);
```

## 🎯 Use Cases

### 1. Ro'yxatdan o'tishda SMS verification
```csharp
// Registration endpoint'da
var verificationResult = await _smsService.SendVerificationCodeAsync(new SendVerificationCodeDto
{
    PhoneNumber = dto.PhoneNumber,
    Purpose = SmsPurpose.Registration
});

// Foydalanuvchi kodni kiritgandan keyin
var verifyResult = await _smsService.VerifyCodeAsync(new VerifyCodeDto
{
    PhoneNumber = dto.PhoneNumber,
    Code = dto.Code,
    Purpose = SmsPurpose.Registration
});
```

### 2. Buyurtma tasdiqlash SMS
```csharp
var smsResult = await _smsService.SendSmsAsync(new SendSmsDto
{
    PhoneNumber = customer.PhoneNumber,
    Message = $"Buyurtmangiz #{order.Id} qabul qilindi. Umumiy summa: {order.TotalAmount:N0} so'm",
    Purpose = SmsPurpose.OrderConfirmation,
    UserId = customer.Id,
    UserType = UserType.Customer.ToString()
});
```

### 3. Buyurtma holati o'zgarganda
```csharp
var message = order.Status switch
{
    OrderStatus.Confirmed => "Buyurtmangiz tasdiqlandi",
    OrderStatus.Preparing => "Buyurtmangiz tayyorlanmoqda",
    OrderStatus.Shipped => "Buyurtmangiz yo'lda",
    OrderStatus.Delivered => "Buyurtmangiz yetkazildi. Rahmat!",
    _ => "Buyurtma holati yangilandi"
};

await _smsService.SendSmsAsync(new SendSmsDto
{
    PhoneNumber = customer.PhoneNumber,
    Message = $"{message}. Buyurtma #{order.Id}",
    Purpose = SmsPurpose.OrderStatusChange,
    UserId = customer.Id,
    UserType = UserType.Customer.ToString()
});
```

## 🔒 Security Notes

1. **Credentials:** Eskiz.uz email/password ni environment variables yoki user secrets da saqlang
2. **Rate Limiting:** Bulk SMS yuborishda har bir SMS o'rtasida 100ms kutiladi
3. **Verification Codes:**
   - 6 raqamli kod
   - 5 daqiqa amal qiladi
   - Memory cache'da saqlanadi
   - Bir marta ishlatiladi (verify qilingandan keyin o'chiriladi)
4. **Phone Number Validation:** +998XXXXXXXXX format talab qilinadi
5. **Authorization:** Admin endpoints uchun `[Authorize(Roles = "Admin")]`

## 📝 Development vs Production

### Development Mode (`IsDevelopment: true`)
- Verification code response'da qaytariladi
- Test qilish oson
- Debugging uchun qulay

### Production Mode (`IsDevelopment: false`)
- Verification code qaytarilmaydi (faqat SMS orqali)
- Xavfsizlik yuqori
- Real foydalanuvchilar uchun

## 🐛 Troubleshooting

### SMS yuborilmayapti
1. Eskiz.uz credentials to'g'ri ekanligini tekshiring
2. Eskiz.uz balance'ni tekshiring
3. API log'larni ko'ring (`src/ZiyoMarket.Api/Logs/`)
4. Database'dagi `SmsLogs` jadvalini tekshiring

### Verification code ishlamayapti
1. Kod 5 daqiqa ichida kiritilganligini tekshiring
2. Telefon raqami to'g'ri formatda ekanligini tekshiring (+998XXXXXXXXX)
3. Purpose bir xil ekanligini tekshiring (send va verify'da)

### SMS log saqlanmayapti
1. Migration apply qilinganligini tekshiring: `dotnet ef database update`
2. DbContext'da `SmsLogs` DbSet bor ekanligini tekshiring
3. UnitOfWork'da `SmsLogs` repository bor ekanligini tekshiring

## 📚 API Endpoints Summary

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/sms/send` | Admin | SMS yuborish |
| POST | `/api/sms/send-verification-code` | Public | Verification code yuborish |
| POST | `/api/sms/verify-code` | Public | Kodni tekshirish |
| POST | `/api/sms/send-bulk` | Admin | Ko'plab SMS yuborish |
| GET | `/api/sms/logs` | Admin | SMS loglarni olish |
| GET | `/api/sms/my-logs` | Authenticated | O'z SMS loglarni olish |
| GET | `/api/sms/statistics` | Admin | SMS statistikasi |

## 🎉 To'liq integratsiya qilindi!

SMS xizmati ZiyoMarket loyihasiga to'liq integratsiya qilindi va ishlatishga tayyor!

**Keyingi qadamlar:**
1. Eskiz.uz credentials'larni sozlang
2. Migration'ni apply qiling (✅ Bajarildi)
3. Swagger'da test qiling: `http://localhost:8080/swagger`
4. AuthService ga SMS verification qo'shing (opsional)
5. Order service'ga SMS notification qo'shing (opsional)

**Test qilish:**
```bash
# API'ni ishga tushiring
cd src/ZiyoMarket.Api
dotnet run

# Swagger'ni oching
http://localhost:8080/swagger
```
