# ZiyoMarket API - Loyiha Holati va Ma'lumotnomasi

Ushbu hujjat ZiyoMarket platformasining hozirgi holati, uning imkoniyatlari va texnik tuzilishi haqida to'liq ma'lumot beradi. Loyiha Railway platformasida muvaffaqiyatli ishlamoqda.

## 🚀 Loyiha Maqsadi
ZiyoMarket - bu zamonaviy E-Commerce platformasi bo'lib, mahsulotlarni sotish, buyurtmalarni boshqarish, to'lovlarni amalga oshirish va foydalanuvchilar bilan samarali muloqot qilish uchun mo'ljallangan backend tizimi hisoblanadi.

## 🛠 Texnik Stack
- **Framework:** .NET 9 (Latest)
- **Database:** PostgreSQL (Railway Managed)
- **ORM:** Entity Framework Core
- **Documentation:** Swagger UI (OpenAPI 3.0)
- **Logging:** Serilog (Console va Faylga)
- **Image Processing:** WebP conversion & auto-resize

## ✅ Hozirgi Status (Nimalar ishlayapti?)

### 1. Avtorizatsiya va Foydalanuvchilar (Auth & Identity)
- **Holati:** To'liq ishlayapti.
- **Vazifasi:** Foydalanuvchilarni ro'yxatdan o'tkazish, login qilish, JWT token berish.
- **Identity Tizimi:** Foydalanuvchilar (`Users`), Rollar (`Roles`) va Huquqlar (`Permissions`) tizimi bazada to'liq tiklangan.

### 2. Mahsulotlar va Kategoriyalar (Products & Categories)
- **Holati:** To'liq ishlayapti.
- **Vazifasi:** Mahsulotlarni yaratish, tahrirlash, qidirish va kategoriyalarga ajratish.
- **Xususiyati:** Mahsulotlar va kategoriyalar o'rtasidagi bog'liqlik (`ProductCategories`) bazada to'g'ri sozlangan.

### 3. Savatcha va Buyurtmalar (Cart & Orders)
- **Holati:** Ishlayapti.
- **Vazifasi:** Foydalanuvchilar mahsulotlarni savatchaga qo'shishi va buyurtma rasmiylashtirishi mumkin.

### 4. Rasmlarni Yuklash (Multi-Media)
- **Holati:** To'liq qayta tiklandi (Swagger-da ko'rinadi).
- **Vazifasi:** Mahsulotlar, kategoriyalar va bannerlar uchun rasmlarni yuklash.
- **Texnik afzalligi:** Rasmlar avtomatik ravishda **WebP** formatiga o'giriladi (hajmi kichik, sifati yuqori) va kerakli o'lchamga keltiriladi.

### 5. To'lov Tizimi (Payment)
- **Holati:** Click tizimi bilan integratsiya qilingan.
- **Vazifasi:** Buyurtmalar uchun onlayn to'lovlarni qabul qilish.

### 6. SMS va Bildirishnomalar (SMS & Notifications)
- **Holati:** Ishga tushirilgan.
- **Vazifasi:** Foydalanuvchilarga SMS xabarlar va Firebase orqali Push-bildirishnomalar yuborish.

## 🛠 Yaqinda Amalga Oshirilgan Muhim Tuzatishlar (Railway Fixes)

1.  **Bazani Avtomatik Tiklash:** Railway-da ma'lumotlar bazasi jadvallari (Users, Roles, Permissions) yo'qligi sababli 500 xatoliklar kuzatilayotgan edi. `Program.cs` ga bazada jadvallar borligini tekshirib, yo'qlarni avtomatik yaratadigan "Robust StartUp" logikasi qo'shildi.
2.  **Swagger 500 Xatoligi:** `IFormFile` kutubxonasi bilan bog'liq ziddiyat sababli Swagger ishlamay qolgan edi. `ImageUploadController` refaktoring qilindi (DTO qo'shildi) va Swagger UI qayta tiklandi.
3.  **Migration Stabillash:** Eskirgan va xato migration'lar tozalab, bazaning hozirgi holatiga moslashtirildi (29 ta jadval to'liq mavjud).

## 📖 Swagger'dan Foydalanish
Loyiha dokumantatsiyasi va API sinovlari uchun quyidagi manzildan foydalaning:
👉 **[Swagger UI - Railway](https://ziyocenter-production.up.railway.app/swagger/index.html)**

---
*Loyiha har bir push`dan so'ng avtomatik ravishda Railway'ga deploy bo'ladi (Continuous Deployment).*
