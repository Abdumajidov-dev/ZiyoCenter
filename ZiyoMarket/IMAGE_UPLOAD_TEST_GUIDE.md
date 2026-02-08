# 🧪 Image Upload Test Guide - Category va Product

## ✅ Tayyor!

Barcha kod yozildi va build muvaffaqiyatli o'tdi. Endi test qilish vaqti!

---

## 🚀 1. API'ni Ishga Tushirish

```bash
cd src\ZiyoMarket.Api
dotnet run
```

Kutilayotgan output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:8080
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## 🌐 2. Swagger UI'ni Ochish

Browser'da quyidagi URL'ni oching:
```
http://localhost:8080/swagger
```

---

## 🔑 3. Token Olish (Authorization)

### 3.1 Admin Login
1. Swagger'da **Auth** section'ni toping
2. **POST /api/auth/login** ni bosing
3. **Try it out** bosing
4. Request body:
   ```json
   {
     "phone": "1111",
     "password": "Admin@123"
   }
   ```
5. **Execute** bosing
6. Response'dan `access_token` ni nusxalang

### 3.2 Token'ni Authorize Qilish
1. Swagger UI'da yuqoridagi **Authorize** tugmasini bosing
2. Quyidagi formatda kiriting:
   ```
   Bearer NUSXALANGAN_TOKEN
   ```
3. **Authorize** bosing
4. **Close** bosing

---

## 📸 4. CATEGORY bilan Image Upload Test

### Test 1: Asosiy Category Yaratish (Image bilan)

1. **TestImage** section'ni toping
2. **POST /api/test_image/create-category-with-image** ni bosing
3. **Try it out** bosing
4. Parametrlarni to'ldiring:
   - `name`: Kitoblar
   - `description`: Barcha turdagi kitoblar
   - `parentId`: (bo'sh qoldiring)
   - `imageFile`: **Choose File** bosib rasm tanlang

5. **Execute** bosing

**Kutilayotgan Response:**
```json
{
  "message": "Category created successfully",
  "category": {
    "id": 1,
    "name": "Kitoblar",
    "description": "Barcha turdagi kitoblar",
    "parent_id": null,
    "image_url": "images/categories/abc123-def456.jpg",
    "image": "http://localhost:8080/images/categories/abc123-def456.jpg",
    "created_at": "2026-02-08 15:30:00"
  }
}
```

### Test 2: Ichki Category Yaratish (Parent bilan)

1. **POST /api/test_image/create-category-with-image** ni yana bosing
2. **Try it out** bosing
3. Parametrlar:
   - `name`: Diniy Kitoblar
   - `description`: Islomiy adabiyotlar
   - `parentId`: 1 (yuqorida yaratilgan Kitoblar ID'si)
   - `imageFile`: Boshqa rasm tanlang

4. **Execute** bosing

**Kutilayotgan Response:**
```json
{
  "message": "Category created successfully",
  "category": {
    "id": 2,
    "name": "Diniy Kitoblar",
    "description": "Islomiy adabiyotlar",
    "parent_id": 1,
    "image_url": "images/categories/xyz789-ghi012.jpg",
    "image": "http://localhost:8080/images/categories/xyz789-ghi012.jpg",
    "created_at": "2026-02-08 15:32:00"
  }
}
```

### Test 3: Category Image'ni Yangilash

1. **PUT /api/test_image/update-category-image/{categoryId}** ni bosing
2. **Try it out** bosing
3. Parametrlar:
   - `categoryId`: 1
   - `imageFile`: Yangi rasm tanlang

4. **Execute** bosing

**Kutilayotgan Response:**
```json
{
  "message": "Category image updated successfully",
  "category": {
    "id": 1,
    "name": "Kitoblar",
    "image_url": "images/categories/new123-new456.jpg",
    "image": "http://localhost:8080/images/categories/new123-new456.jpg"
  }
}
```

### Test 4: Barcha Categorylarni Ko'rish

1. **GET /api/test_image/categories-with-images** ni bosing
2. **Try it out** bosing
3. **Execute** bosing

**Kutilayotgan Response:**
```json
{
  "count": 2,
  "categories": [
    {
      "id": 1,
      "name": "Kitoblar",
      "description": "Barcha turdagi kitoblar",
      "parent_id": null,
      "image_url": "images/categories/abc123.jpg",
      "image": "http://localhost:8080/images/categories/abc123.jpg",
      "is_active": true
    },
    {
      "id": 2,
      "name": "Diniy Kitoblar",
      "description": "Islomiy adabiyotlar",
      "parent_id": 1,
      "image_url": "images/categories/xyz789.jpg",
      "image": "http://localhost:8080/images/categories/xyz789.jpg",
      "is_active": true
    }
  ]
}
```

---

## 📦 5. PRODUCT bilan Image Upload Test

### Test 5: Product Yaratish (Image va Category bilan)

1. **POST /api/test_image/create-product-with-image** ni bosing
2. **Try it out** bosing
3. Parametrlar:
   - `name`: Qur'oni Karim
   - `description`: Arab tilida Qur'on
   - `qrCode`: QR001
   - `price`: 50000
   - `stockQuantity`: 100
   - `categoryIds`: [1, 2] (array sifatida)
   - `imageFile`: Product rasmi tanlang

4. **Execute** bosing

**Kutilayotgan Response:**
```json
{
  "message": "Product created successfully",
  "product": {
    "id": 1,
    "name": "Qur'oni Karim",
    "description": "Arab tilida Qur'on",
    "qr_code": "QR001",
    "price": 50000,
    "stock_quantity": 100,
    "category_ids": [1, 2],
    "image_url": "images/products/prod123-prod456.jpg",
    "image": "http://localhost:8080/images/products/prod123-prod456.jpg",
    "created_at": "2026-02-08 15:35:00"
  }
}
```

### Test 6: Product Image'ni Yangilash

1. **PUT /api/test_image/update-product-image/{productId}** ni bosing
2. **Try it out** bosing
3. Parametrlar:
   - `productId`: 1
   - `imageFile`: Yangi product rasmi

4. **Execute** bosing

**Kutilayotgan Response:**
```json
{
  "message": "Product image updated successfully",
  "product": {
    "id": 1,
    "name": "Qur'oni Karim",
    "image_url": "images/products/newprod123.jpg",
    "image": "http://localhost:8080/images/products/newprod123.jpg"
  }
}
```

### Test 7: Barcha Productlarni Ko'rish

1. **GET /api/test_image/products-with-images** ni bosing
2. **Try it out** bosing
3. **Execute** bosing

**Kutilayotgan Response:**
```json
{
  "count": 1,
  "products": [
    {
      "id": 1,
      "name": "Qur'oni Karim",
      "description": "Arab tilida Qur'on",
      "qr_code": "QR001",
      "price": 50000,
      "stock_quantity": 100,
      "image_url": "images/products/prod123.jpg",
      "image": "http://localhost:8080/images/products/prod123.jpg",
      "is_active": true
    }
  ]
}
```

---

## 🖼️ 6. Rasmlarni Browser'da Ko'rish

### Category Rasmi:
Response'dan `image` URL'ni nusxalang va browser'da oching:
```
http://localhost:8080/images/categories/abc123-def456.jpg
```

### Product Rasmi:
```
http://localhost:8080/images/products/prod123-prod456.jpg
```

Rasm to'g'ri ko'rinishi kerak! ✅

---

## 📂 7. Fayllarni Disk'da Tekshirish

Windows Explorer'da ochish:
```
C:\Users\abdum\OneDrive\Desktop\Kutubxona\ZiyoMarket\src\ZiyoMarket.Api\wwwroot\images
```

Quyidagi papkalar ichida rasmlar paydo bo'lishi kerak:
- `categories/` - category rasmlari
- `products/` - product rasmlari

---

## ✅ 8. Test Checklist

- [ ] ✅ API muvaffaqiyatli ishga tushdi
- [ ] ✅ Swagger UI ochildi
- [ ] ✅ Token olindi va authorize qilindi
- [ ] ✅ Asosiy category image bilan yaratildi
- [ ] ✅ Ichki category parent bilan yaratildi
- [ ] ✅ Category image yangilandi
- [ ] ✅ Barcha categorylar ko'rildi (image URL'lar bilan)
- [ ] ✅ Product image va categorylar bilan yaratildi
- [ ] ✅ Product image yangilandi
- [ ] ✅ Barcha productlar ko'rildi (image URL'lar bilan)
- [ ] ✅ Category rasmi browser'da ochildi
- [ ] ✅ Product rasmi browser'da ochildi
- [ ] ✅ Diskdagi wwwroot/images papkasida rasmlar bor

---

## 🐛 Troubleshooting

### Issue 1: "401 Unauthorized" Error
**Solution:** Token'ni qaytadan oling va Authorize qiling

### Issue 2: Rasm browser'da ochilmayapti
**Solution:**
- `image_url` to'g'ri yozilganini tekshiring
- `app.UseStaticFiles()` Program.cs'da borligini tekshiring

### Issue 3: "File too large" error
**Solution:** Rasmni 5MB dan kichik qiling

### Issue 4: categoryIds array qanday yuborish kerak?
**Swagger'da:**
```
[1, 2]
```
**cURL'da:**
```bash
-F "categoryIds=1" -F "categoryIds=2"
```

---

## 📊 Test Natijalari

Barcha testlar muvaffaqiyatli o'tgandan keyin:

**Database:**
- Categories jadvali: 2 ta yozuv (image_url bilan)
- Products jadvali: 1 ta yozuv (image_url bilan)
- ProductCategories jadvali: 2 ta yozuv (many-to-many)

**File System:**
- `wwwroot/images/categories/` - 2+ rasm fayl
- `wwwroot/images/products/` - 1+ rasm fayl

---

## 🎉 Muvaffaqiyat Mezonlari

✅ **PASSED** agar:
1. Barcha endpoint'lar 200 OK qaytaradi
2. Response'da `image` va `image_url` mavjud
3. Browser'da rasmlar ochiladi
4. Diskdagi fayllar mavjud
5. Database'da path'lar to'g'ri saqlangan

---

**Status:** ✅ **READY FOR TESTING**
**Date:** 2026-02-08
**Prepared by:** Claude Code Assistant
