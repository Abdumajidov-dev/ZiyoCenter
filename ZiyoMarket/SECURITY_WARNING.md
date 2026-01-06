# ⚠️ XAVFSIZLIK OGOHLANTIRISHI

## KRITIK: Firebase Service Account Kalit Fayli

Siz Firebase service account kalitingizni **oshkor qildingiz**! Bu juda xavfli.

### 🔴 Darhol qilinadigan ishlar:

1. **Firebase Console ga kiring:**
   - https://console.firebase.google.com/project/market-f0779/settings/serviceaccounts/adminsdk

2. **Joriy kalitni o'chiring:**
   - "Service accounts" → "firebase-adminsdk-fbsvc@market-f0779.iam.gserviceaccount.com"
   - "Actions" → "Delete key" → Tasdiqlang

3. **Yangi kalit yarating:**
   - "Generate new private key" tugmasini bosing
   - Yangi `firebase-service-account.json` faylini yuklab oling

4. **Yangi kalitni loyihaga qo'shing:**
   - Yangi faylni `src/ZiyoMarket.Api/firebase-service-account.json` ga nusxa oling
   - **HECH QACHON** uni git ga commit qilmang (`.gitignore` da allaqachon mavjud)

5. **Git tarixidan olib tashlash (agar commit qilgan bo'lsangiz):**
   ```bash
   # Faylni tarixdan butunlay o'chirish
   git filter-branch --force --index-filter \
   "git rm --cached --ignore-unmatch src/ZiyoMarket.Api/firebase-service-account.json" \
   --prune-empty --tag-name-filter cat -- --all

   # Force push (ehtiyot bo'ling!)
   git push origin --force --all
   ```

### 🛡️ Xavfsizlik qoidalari:

1. ✅ `firebase-service-account.json` `.gitignore` da
2. ✅ API kalitlarni hech qachon kodda saqlamang
3. ✅ Environment variables yoki user secrets dan foydalaning
4. ✅ Kalitlarni 90 kunda bir marta yangilang
5. ✅ Access loglarini tekshiring (Firebase Console)

### 📊 Zarar baholash:

Agar kimdir sizning kalitingizni olgan bo'lsa, u quyidagilarni qilishi mumkin:
- ❌ Cheksiz push notification yuborish
- ❌ Firebase Realtime Database ga kirish
- ❌ Firebase Storage fayllarini o'chirish
- ❌ Sizning Firebase quota ni tugat ish

### ✅ Tekshirish ro'yxati:

- [ ] Eski kalitni o'chirdim
- [ ] Yangi kalit yaratdim
- [ ] Yangi kalitni loyihaga qo'shdim
- [ ] Git tarixini tozaladim (agar kerak bo'lsa)
- [ ] `.gitignore` da mavjudligini tekshirdim
- [ ] Loyihani qayta build qildim va test qildim
- [ ] Firebase Console da faoliyat loglarini ko'rib chiqdim

---

**Eslatma:** Kelajakda hech qachon API kalitlari, parollar, yoki xavfsiz ma'lumotlarni chatga yubormang!
