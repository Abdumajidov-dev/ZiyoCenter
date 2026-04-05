# 🧠 MASTER PROJECT DOCUMENTATION (SINGLE SOURCE OF TRUTH)

---

# 📌 PROJECT OVERVIEW

**ZiyoMarket** — multi-platform tizim:

* Backend: ASP.NET Core (REST API)
* Admin Panel: WPF (MVVM)
* Mobile: Flutter

Tizim quyidagilarni boshqaradi:

* Mahsulotlar (Products)
* Buyurtmalar (Orders)
* Foydalanuvchilar (Users)
* To‘lovlar (Payments)
* Bildirishnomalar (Notifications)

---

# 🧰 TECH STACK

## Backend

* ASP.NET Core
* Entity Framework Core
* PostgreSQL
* AutoMapper
* Result Pattern

## Admin Panel (WPF)

* MVVM
* Material Design
* HttpClient (custom service)

## Mobile

* Flutter (mustaqil ishlaydi)

## External Services

* Firebase FCM (push notifications)
* Eskiz SMS
* Click.uz (to‘lov)

---

# 🏗️ ARCHITECTURE

Clean Architecture asosida:

```
API (Presentation Layer)
   ↓
Service (Business Logic)
   ↓
Data (Repository + EF Core)
   ↓
Domain (Entities)
```

---

# 📂 PROJECT STRUCTURE

## 🔹 Backend

```
Domain/
  ├── Entities/
  ├── Enums/
  └── Common/

Data/
  ├── Context/
  ├── Repositories/
  ├── UnitOfWork/
  └── Migrations/

Service/
  ├── DTOs/
  ├── Interfaces/
  ├── Services/
  └── Mapping/

Api/
  ├── Controllers/
  ├── Middleware/
  └── Extensions/
```

---

## 🔹 WPF (Admin Panel)

```
Views/
ViewModels/
Services/
Resources/
```

---

# 🔄 DATA FLOW

## Backend Flow

1. Client → HTTP request
2. Controller → requestni qabul qiladi
3. Service → business logic bajariladi
4. Repository → DB bilan ishlaydi
5. DbContext → PostgreSQL
6. Result<T> → response qaytariladi

---

## WPF Flow (MVVM)

1. View (XAML)
2. ViewModel (Command)
3. Service (API call)
4. HttpClient
5. Backend API
6. Response → ViewModel → UI

---

# 🧱 CORE COMPONENTS

* Generic Repository
* UnitOfWork
* AutoMapper Profiles
* Result<T> pattern
* BaseController (HandleResult)

---

# 📏 NAMING CONVENTIONS

## DTO

* CreateXDto
* UpdateXDto
* XResultDto

## Service

* IXService
* XService

## Controller

* XController

---

# ⚙️ PATTERNS

* Clean Architecture
* Repository Pattern
* UnitOfWork
* MVVM (WPF)
* Result Pattern

---

# ❗ CURRENT PROBLEMS

## 🔴 Critical Issues

### 1. Date string sifatida saqlangan

* Query sekin
* EF Core muammo
* Yechim: DateTime migration

---

### 2. Computed properties muammo

Masalan:

* IsLowStock

❌ Query ichida ishlamaydi
✅ Faqat memory ichida ishlatish kerak

---

### 3. Duplicate User System

* Eski: Customer / Seller / Admin
* Yangi: User / Role / Permission

❌ Konflikt bor
✅ RBAC ga to‘liq o‘tish kerak

---

## 🟡 Moderate Issues

* Code duplication bor
* Ba’zi service’larda logic aralashgan
* RBAC to‘liq tugallanmagan

---

# 🚨 DEVELOPMENT RULES (MUHIM)

## ❌ TAQIQLANADI

* Controller’da business logic yozish
* Repository’da business logic yozish
* Code duplication
* Query ichida Date parsing
* Computed property queryda ishlatish

---

## ✅ MAJBURIY

* Barcha business logic → Service layer
* Soft delete ishlatish
* Naming conventionlarga amal qilish
* Result<T> pattern ishlatish

---

# 🗄️ DATABASE RULES

* Soft delete default
* Hard delete faqat majbur bo‘lsa
* DateTime ishlatish
* Clean migrations

---

# 🧩 NEW FEATURE QO‘SHISH

## New Entity

1. Domain → Entity yaratish
2. Data → DbSet + Migration
3. Service → DTO + Service
4. API → Controller

---

## New Endpoint

1. Service → method
2. Controller → action

---

## New Business Logic

* Faqat Service ichida yoziladi

---

# 🔐 SAFE EXTENSION STRATEGY

### 1. Analyze

* Mavjud kodni o‘rgan

### 2. Plan

* DTO
* Service
* Controller

### 3. Implement

* DB → Service → API → UI

---

# 🔁 DEVELOPMENT WORKFLOW

1. Analyze existing code
2. Duplicate qilma
3. Extend qil
4. Namingga amal qil
5. Swaggerda test qil
6. UI bilan tekshir

---

# 📊 CURRENT SYSTEM STATUS

* Controllers: ~28
* Services: ~23
* Entities: ~30
* Enums: ~17

---

# 🧠 AI USAGE RULES

Agar AI ishlatilsa:

* Shu file asosida ishlashi shart
* Yangi structure o‘ylab topmasligi kerak
* Faqat extend qilish kerak
* Har doim Service layerga yozishi kerak

---

# 🏁 FINAL NOTE

Bu hujjat:

👉 loyihaning yagona haqiqat manbasi
👉 barcha developerlar uchun qo‘llanma
👉 AI uchun instruction

❗ Har qanday yangi o‘zgarish — shu file asosida bo‘lishi shart
