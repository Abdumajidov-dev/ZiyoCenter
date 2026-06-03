# Seller (Sotuvchi) tomon — Flutter dasturchi uchun texnik topshiriq

## Umumiy ma'lumot

Seller — kutubxona xodimi. Uning asosiy ishi:
1. Mijoz keladi → telefon raqami yoki ism bo'yicha topadi (yoki yangi yaratadi)
2. Mijoz mahsulot tanlaydi → seller mahsulotni barcode skan qiladi yoki qidiradi
3. Savat tuzadi → mahsulotlarni qo'shadi
4. Chegirma beradi (kerak bo'lsa)
5. Buyurtmani tasdiqlaydi → shartnoma saqlanadi
6. Admin panel ichida bu buyurtma ko'rinadi

---

## Autentifikatsiya

### Login

```
POST /api/auth/login
Content-Type: application/json

{
  "phone_or_email": "+998901234567",
  "password": "123456",
  "user_type": "Seller"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "access_token": "eyJhbGciOiJ...",
    "user_id": 5,
    "full_name": "Ahmad Azimov",
    "role": "Seller",
    "user_type": "Seller"
  }
}
```

Keyingi barcha requestlarda header:
```
Authorization: Bearer {access_token}
```

---

## 1. Mahsulotlar

> Barcha mahsulot endpointlari `AllowAnonymous` — token bo'lmasa ham ishlaydi.
> Lekin seller tokeni bilan yuborish tavsiya etiladi.

### 1.1 Barcha mahsulotlar ro'yxati (pagination + filter)

```
GET /api/product?page_number=1&page_size=20
GET /api/product?page_number=1&page_size=20&category_id=3&in_stock=true
```

Query params (barchasi ixtiyoriy):

| Param | Tur | Tavsif |
|-------|-----|--------|
| `page_number` | int | Default: 1 |
| `page_size` | int | Default: 20, max: 100 |
| `category_id` | int | Category bo'yicha filter |
| `status` | string | `Active` / `OutOfStock` |
| `min_price` | decimal | Minimal narx |
| `max_price` | decimal | Maksimal narx |
| `in_stock` | bool | Faqat omborda borlarni ko'rsat |
| `low_stock` | bool | Kam qolganlarni ko'rsat |
| `search_term` | string | Qidiruv matni |
| `sort_by` | string | Sort maydoni nomi |
| `sort_descending` | bool | Teskari tartib |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Python dasturlash",
        "qr_code": "ZM-001-2024",
        "category_id": 2,
        "category_name": "Dasturlash",
        "price": 45000,
        "formatted_price": "45 000 so'm",
        "stock_quantity": 15,
        "status": "Active",
        "image_url": "https://...",
        "is_available": true,
        "is_low_stock": false,
        "barcode": "9781234567890",
        "manufacturer": "O'zDavNashr"
      }
    ],
    "total_count": 150,
    "page_number": 1,
    "page_size": 20,
    "total_pages": 8
  }
}
```

### 1.2 Nom yoki barcode bo'yicha qidiruv

```
GET /api/product/search?search_term=python
GET /api/product/search?search_term=python&category_id=2
```

**Response:** `ProductListDto` listini qaytaradi (pagination yo'q, max 20 ta).

### 1.3 QR-kod / barcode bo'yicha izlash

> Kamera bilan skan qilinganda shu endpoint ishlatiladi.

```
GET /api/product/qr/{qrCode}
```

Misol:
```
GET /api/product/qr/ZM-001-2024
GET /api/product/qr/9781234567890
```

**Response:** `ProductDetailDto` (bitta mahsulot to'liq ma'lumotlari bilan).

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Python dasturlash",
    "qr_code": "ZM-001-2024",
    "price": 45000,
    "stock_quantity": 15,
    "status": "Active",
    "is_available": true,
    "barcode": "9781234567890",
    "publisher": "O'zDavNashr",
    "language": "Uzbek",
    "page_count": 320
  }
}
```

### 1.4 Category bo'yicha mahsulotlar

```
GET /api/product/category/{categoryId}
```

---

## 2. Categorylar ro'yxati

```
GET /api/category
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Barcha kitoblar",
      "parent_id": null,
      "is_active": true,
      "product_count": 240
    },
    {
      "id": 2,
      "name": "Dasturlash",
      "parent_id": 1,
      "is_active": true,
      "product_count": 45
    }
  ]
}
```

---

## 3. Mijozlar (Customers)

> Seller quyidagi amallarni bajara oladi:
> - Ko'rish, qidirish, yangi yaratish ✅
> - O'chirish, yangilash, status o'zgartirish ❌ (faqat Admin)

### 3.1 Mijozlar ro'yxati

```
GET /api/customer?page_number=1&page_size=20
GET /api/customer?search_term=Ahmad
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 10,
        "first_name": "Ahmad",
        "last_name": "Karimov",
        "phone": "+998901234567",
        "is_active": true,
        "cashback_balance": 12500,
        "total_orders": 7
      }
    ],
    "total_count": 85,
    "page_number": 1,
    "page_size": 20
  }
}
```

### 3.2 Mijozni telefon raqami bo'yicha tez qidirish

```
GET /api/customer/search?search_term=+99890123
GET /api/customer/search?search_term=Ahmad
```

> Telefon raqami, ism yoki familya bo'yicha qidiradi.
> Max 20 ta natija qaytaradi.

### 3.3 Mijozni ID bo'yicha olish

```
GET /api/customer/{id}
```

### 3.4 Yangi mijoz yaratish

> Mijoz yo'q bo'lsa, seller tez ro'yxatdan o'tkaza oladi.

```
POST /api/customer
Authorization: Bearer {token}
Content-Type: application/json

{
  "first_name": "Sardor",
  "last_name": "Toshmatov",
  "phone": "+998901112233",
  "password": "123456"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Customer created successfully",
  "data": {
    "id": 42,
    "first_name": "Sardor",
    "last_name": "Toshmatov",
    "phone": "+998901112233",
    "cashback_balance": 0,
    "is_active": true
  }
}
```

> **Muhim:** Agar telefon raqami allaqachon mavjud bo'lsa, `409 Conflict` qaytadi.

---

## 4. Buyurtma yaratish (Offline savdo)

### 4.1 Chegirma sabablarini olish

> Buyurtma yaratishdan oldin chegirma sabablarini yuklab oling (bir marta).

```
GET /api/order/discount-reasons
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Doimiy mijoz",
      "description": "Doimiy mijoz uchun maxsus chegirma",
      "is_active": true,
      "max_discount_percentage": 20,
      "max_discount_amount": null,
      "is_seller_only": true,
      "display_order": 1
    },
    {
      "id": 2,
      "name": "Ko'plab xarid",
      "description": "3ta va undan ko'p mahsulot olganda",
      "is_active": true,
      "max_discount_percentage": null,
      "max_discount_amount": 50000,
      "is_seller_only": false,
      "display_order": 2
    }
  ]
}
```

### 4.2 Buyurtma yaratish (savat + chegirma bitta requestda)

```
POST /api/order/seller
Authorization: Bearer {token}
Content-Type: application/json

{
  "customer_id": 42,
  "create_from_cart": false,
  "items": [
    { "product_id": 1, "quantity": 2 },
    { "product_id": 7, "quantity": 1 },
    { "product_id": 12, "quantity": 1 }
  ],
  "payment_method": "Cash",
  "delivery_type": "Pickup",
  "discount_amount": 5000,
  "seller_notes": "Mijoz 100 ming berdi, ruchka (5 ming) bepul"
}
```

**Request maydonlari:**

| Maydon | Majburiy | Tavsif |
|--------|----------|--------|
| `customer_id` | ✅ | Mijoz ID |
| `create_from_cart` | ✅ | `false` = to'g'ridan items; `true` = mijoz savati |
| `items` | `create_from_cart=false` da ✅ | Mahsulotlar ro'yxati |
| `items[].product_id` | ✅ | Mahsulot ID |
| `items[].quantity` | ✅ | Miqdori |
| `payment_method` | ✅ | `Cash`, `Card`, `Cashback`, `Mixed` |
| `delivery_type` | ✅ | Doim `Pickup` (do'kon savdosi) |
| `discount_amount` | ❌ | Chegirma summasi (so'mda). 0 = chegirmasi |
| `cashback_to_use` | ❌ | Cashback ishlatish summasi. 0 = ishlatmaslik |
| `seller_notes` | ❌ | Qo'shimcha izoh |
| `customer_notes` | ❌ | Mijoz izohi |

**`payment_method` qiymatlari:**
- `Cash` — naqd pul
- `Card` — plastik karta
- `Cashback` — cashback balans bilan to'lash
- `Mixed` — aralash (qisman naqd, qisman cashback)

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "id": 105,
    "customer_id": 42,
    "customer_name": "Sardor Toshmatov",
    "seller_id": 5,
    "seller_name": "Ahmad Azimov",
    "status": "Confirmed",
    "total_price": 105000,
    "discount_applied": 5000,
    "cashback_used": 0,
    "final_price": 100000,
    "formatted_final_price": "100 000 so'm",
    "payment_method": "Cash",
    "delivery_type": "Pickup",
    "order_items": [
      {
        "id": 201,
        "product_id": 1,
        "product_name": "Python dasturlash",
        "quantity": 2,
        "unit_price": 45000,
        "discount_applied": 0,
        "total_price": 90000
      },
      {
        "id": 202,
        "product_id": 12,
        "product_name": "Ruchka",
        "quantity": 1,
        "unit_price": 5000,
        "discount_applied": 5000,
        "total_price": 0
      }
    ]
  }
}
```

> **Muhim:** Offline buyurtma yaratilganda status avtomatik `Confirmed` bo'ladi (tasdiqlash kerak emas).

---

## 5. Chegirma (Discount) — alohida qo'llash

> Agar buyurtma yaratilgandan keyin chegirma qo'shish kerak bo'lsa.

```
POST /api/order/discount
Authorization: Bearer {token}
Content-Type: application/json

{
  "order_id": 105,
  "discount_amount": 5000,
  "discount_reason_id": 1,
  "notes": "Mijoz 100 ming berdi"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Discount applied successfully"
}
```

> **Muhim:** Seller faqat o'z roli doirasidagi chegirma bera oladi.
> - Oddiy `Seller` roli: maksimal 20% chegirma
> - `Manager` roli: cheksiz chegirma

---

## 6. Seller o'z buyurtmalarini ko'rish

```
GET /api/order
Authorization: Bearer {token}
```

Seller faqat o'zi yaratgan buyurtmalarni ko'radi (boshqasinikilar ko'rinmaydi).

```
GET /api/order?page_number=1&page_size=20
GET /api/order?date_from=2025-01-01&date_to=2025-12-31
```

```
GET /api/order/{id}
```

---

## 7. Buyurtma holati o'zgartirish

> Offline savdoda odatda kerak bo'lmaydi, lekin kerak bo'lsa:

```
PUT /api/order/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

"Delivered"
```

**Holat qiymatlari:**
```
Pending → Confirmed → Preparing → ReadyForPickup/Shipped → Delivered/Cancelled
```

---

## To'liq ish jarayoni (flowchart)

```
1. [Login] POST /api/auth/login (user_type: "Seller")
     ↓ access_token olindi

2. [Categorylar yuklash] GET /api/category
     ↓ (bir marta, lokal cache)

3. [Chegirma sabablari yuklash] GET /api/order/discount-reasons
     ↓ (bir marta, lokal cache)

4. [Mijoz qidirish] GET /api/customer/search?search_term={telefon}
     ├─ Topildi → customer_id olindi
     └─ Topilmadi → [Yangi mijoz] POST /api/customer

5. [Mahsulot qo'shish — skan]
     ├─ Barcode skan → GET /api/product/qr/{barcode}
     ├─ Nom qidiruv → GET /api/product/search?search_term={nom}
     └─ Lokal savat (Flutter tomonida saqlash)

6. [Buyurtma yaratish] POST /api/order/seller
     {
       customer_id, items, payment_method, delivery_type,
       discount_amount (agar chegirma bo'lsa)
     }
     ↓ Order ID olindi

7. [Chek chiqarish — ixtiyoriy]
     GET /api/order/{id} → to'liq ma'lumot chop etish uchun
```

---

## Xato kodlari

| HTTP kod | Ma'nosi | Misol |
|----------|---------|-------|
| 200 | Muvaffaqiyatli | - |
| 201 | Yangi yaratildi | Buyurtma, mijoz |
| 400 | Noto'g'ri so'rov | `"Stock quantity insufficient"` |
| 401 | Token yo'q/muddati o'tgan | Qayta login kerak |
| 403 | Ruxsat yo'q | Seller Admin endpointga kirmoqchi |
| 404 | Topilmadi | `"Product not found"` |
| 409 | Takror | `"Phone number already exists"` |
| 500 | Server xatosi | Log tekshiring |

---

## Flutter model sinflari uchun namunalar

### ProductModel
```dart
class ProductModel {
  final int id;
  final String name;
  final String qrCode;
  final String? barcode;
  final int categoryId;
  final String categoryName;
  final double price;
  final String formattedPrice;
  final int stockQuantity;
  final String status;
  final String? imageUrl;
  final bool isAvailable;

  // snake_case JSON dan o'qish
  factory ProductModel.fromJson(Map<String, dynamic> json) => ProductModel(
    id: json['id'],
    name: json['name'],
    qrCode: json['qr_code'],
    barcode: json['barcode'],
    categoryId: json['category_id'],
    categoryName: json['category_name'],
    price: (json['price'] as num).toDouble(),
    formattedPrice: json['formatted_price'],
    stockQuantity: json['stock_quantity'],
    status: json['status'],
    imageUrl: json['image_url'],
    isAvailable: json['is_available'],
  );
}
```

### CreateOrderRequest
```dart
class CreateOrderRequest {
  final int customerId;
  final bool createFromCart;
  final List<OrderItemRequest> items;
  final String paymentMethod;
  final String deliveryType;
  final double discountAmount;
  final String? sellerNotes;

  Map<String, dynamic> toJson() => {
    'customer_id': customerId,
    'create_from_cart': createFromCart,
    'items': items.map((e) => e.toJson()).toList(),
    'payment_method': paymentMethod,
    'delivery_type': deliveryType,
    'discount_amount': discountAmount,
    if (sellerNotes != null) 'seller_notes': sellerNotes,
  };
}

class OrderItemRequest {
  final int productId;
  final int quantity;

  Map<String, dynamic> toJson() => {
    'product_id': productId,
    'quantity': quantity,
  };
}
```

---

## Muhim eslatmalar

1. **Token muddati:** 24 soat. Muddati o'tsa 401 qaytadi → qayta login.

2. **Savat Flutter tomonida saqlang:** Server tomonida seller savatiga alohida endpoint yo'q. Offline savdoda `POST /api/order/seller` ga to'g'ridan `items` yuboriladi. Seller savatini Flutter ilovasi ichida (xotirada yoki lokal DB da) boshqaring.

3. **snake_case:** Barcha JSON fieldlari `snake_case` — `firstName` emas, `first_name`.

4. **Chegirma mantigi:** Jami narx 105 ming, mijoz 100 ming bermoqchi → `discount_amount: 5000`. Qaysi mahsulot "bepul" bo'lishini admin ko'rganda buyurtma itemlari ichida ko'radi.

5. **Cashback:** Mijozning cashback balansi `GET /api/customer/{id}` da `cashback_balance` fieldida. Agar ishlatmoqchi bo'lsa, `cashback_to_use` fieldini to'ldiring.

6. **Offline buyurtma statusi:** `POST /api/order/seller` bilan yaratilgan buyurtma avtomatik `Confirmed` holatiga o'tadi — alohida tasdiqlash kerak emas.
