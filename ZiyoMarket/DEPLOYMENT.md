# Railway.app ga Deploy qilish qo'llanmasi

Bu qo'llanma ZiyoMarket loyihasini Railway.app platformasiga deploy qilish bo'yicha to'liq yo'riqnoma.

## =� Kerakli narsalar

1. **Railway Account** - [railway.app](https://railway.app) da ro'yxatdan o'ting
2. **GitHub Account** - Loyihangiz GitHub da bo'lishi kerak
3. **Git** - Kompyuteringizda o'rnatilgan bo'lishi kerak

## =� Deployment Bosqichlari

### 1-qadam: Loyihani GitHub ga yuklash

Agar loyihangiz hali GitHub da bo'lmasa:

```bash
# Git repository yaratish
git init
git add .
git commit -m "Initial commit for Railway deployment"

# GitHub da yangi repository yarating va quyidagilarni bajaring:
git remote add origin https://github.com/YOUR_USERNAME/ZiyoMarket.git
git branch -M main
git push -u origin main
```

Agar allaqachon GitHub da bo'lsa, yangi o'zgarishlarni push qiling:

```bash
git add .
git commit -m "Add Railway deployment configuration"
git push
```

### 2-qadam: Railway Project yaratish

1. [railway.app](https://railway.app) ga kiring
2. "New Project" tugmasini bosing
3. "Deploy from GitHub repo" ni tanlang
4. ZiyoMarket repository ni tanlang

**MUHIM: Service Settings sozlash**

Service yaratilgandan keyin, Build xatolarini oldini olish uchun:

1. Railway dashboardda yangi yaratilgan service ni tanlang
2. "Settings" tabiga o'ting
3. **"Source"** bo'limida quyidagilarni sozlang:
   - **Root Directory**: Bo'sh qoldiring yoki `/` kiriting (agar loyihangiz repository root da bo'lsa)
   - **Build Command**: Bo'sh qoldiring (Dockerfile ishlatiladi)

4. **"Deploy"** bo'limida:
   - **Builder**: `DOCKERFILE` tanlanganini tekshiring
   - **Dockerfile Path**: `Dockerfile` (default)

5. O'zgarishlarni saqlang va qayta deploy qiling

### 3-qadam: PostgreSQL Database qo'shish

1. Railway dashboardda "New" tugmasini bosing
2. "Database" -> "Add PostgreSQL" ni tanlang
3. PostgreSQL service yaratiladi va avtomatik ulanish ma'lumotlari yaratiladi

### 4-qadam: Environment Variables sozlash

Backend service uchun quyidagi environment variables ni qo'shing:

1. Railway dashboardda backend service ni tanlang
2. "Variables" tabiga o'ting
3. Quyidagi o'zgaruvchilarni qo'shing:

```bash
# Database Connection (PostgreSQL service dan olinadi)
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}

# JWT Settings
JwtSettings__SecretKey=YourSuperSecretKeyForJWT_MinLength32Characters!
JwtSettings__Issuer=ZiyoMarket
JwtSettings__Audience=ZiyoMarketUsers
JwtSettings__AccessTokenExpirationMinutes=1440

# ASP.NET Core Settings
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

**MUHIM:** `JwtSettings__SecretKey` ni xavfsiz va unikal qiymatga o'zgartiring!

### 5-qadam: Database Migration qo'llash

Railway automatik migration qo'llamaydi. Ikki yo'l bor:

#### Variant A: Railway CLI orqali (Tavsiya etiladi)

```bash
# Railway CLI ni o'rnatish
npm install -g @railway/cli

# Login qilish
railway login

# Loyihangizni ulash
railway link

# Migration qo'llash
railway run dotnet ef database update --project src/ZiyoMarket.Data --startup-project src/ZiyoMarket.Api
```

#### Variant B: Local kompyuterdan (Muqobil)

1. Railway PostgreSQL connection string ni oling
2. Local `appsettings.json` da connection string ni o'zgartiring
3. Migration qo'llang:

```bash
cd src/ZiyoMarket.Api
dotnet ef database update --project ../ZiyoMarket.Data
```

4. Keyin `appsettings.json` ni qaytadan tiklang

### 6-qadam: Deploy qilish

Railway avtomatik deploy qiladi har safar GitHub ga push qilganingizda.

Qo'lda deploy qilish uchun:
1. Railway dashboardda backend service ni tanlang
2. "Deployments" tabiga o'ting
3. "Deploy" tugmasini bosing

### 7-qadam: Domain sozlash (Ixtiyoriy)

1. Railway dashboardda backend service ni tanlang
2. "Settings" -> "Networking" ga o'ting
3. "Generate Domain" tugmasini bosing
4. Railway sizga `*.up.railway.app` domenini beradi

Custom domain qo'shish:
1. "Custom Domains" bo'limida "Add Domain" ni bosing
2. O'z domeningizni kiriting (masalan, `api.ziyomarket.uz`)
3. Railway bergan DNS recordlarini domen provayderingizda sozlang

## = Tekshirish

Deploy tugagandan keyin:

1. **Health Check**: `https://your-app.up.railway.app/health`
2. **API Root**: `https://your-app.up.railway.app/`
3. **Swagger UI**: `https://your-app.up.railway.app/swagger`

## =� Logs ko'rish

Railway dashboardda:
1. Backend service ni tanlang
2. "Deployments" tabidan oxirgi deployment ni tanlang
3. "View Logs" tugmasini bosing

## =� Muammolarni bartaraf etish

### Problem 1: Build xatosi - "skipping Dockerfile as it is not rooted at a valid path"

**Sabab:** Railway service settings noto'g'ri sozlangan.

**Yechim:**
1. Railway dashboardda service ni tanlang
2. "Settings" -> "Source" ga o'ting
3. **Root Directory** ni bo'sh qoldiring yoki `/` kiriting
4. "Settings" -> "Deploy" ga o'ting
5. **Builder** ni `DOCKERFILE` ga o'zgartiring
6. **Dockerfile Path** ni `Dockerfile` deb qoldiring
7. "Redeploy" tugmasini bosing

### Problem 2: Build xatosi - "Script start.sh not found"

**Sabab:** Railway Nixpacks ishlatmoqchi, lekin biz Dockerfile ishlatmoqchimiz.

**Yechim:**
1. Railway dashboardda service ni tanlang
2. "Settings" -> "Deploy" ga o'ting
3. **Builder** ni `NIXPACKS` dan `DOCKERFILE` ga o'zgartiring
4. O'zgarishlarni saqlang va qayta deploy qiling

### Problem 3: Database connection xatosi

**Yechim:**
- `ConnectionStrings__DefaultConnection` to'g'ri sozlanganini tekshiring
- PostgreSQL service ishlayotganini tekshiring
- Environment variable: `ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}`
- Migration qo'llanganini tekshiring

### Problem 4: 502 Bad Gateway

**Yechim:**
- Dockerfile to'g'ri build bo'layotganini tekshiring
- `ASPNETCORE_URLS=http://+:8080` sozlanganini tekshiring
- Railway logs dan xatolarni ko'rib chiqing
- Health check endpoint ishlayotganini tekshiring: `/health`

### Problem 5: Migration xatolari

**Yechim:**
```bash
# Barcha migrationlarni qayta qo'llash
railway run dotnet ef database drop --force --project src/ZiyoMarket.Data --startup-project src/ZiyoMarket.Api
railway run dotnet ef database update --project src/ZiyoMarket.Data --startup-project src/ZiyoMarket.Api
```

## = Xavfsizlik

1. **Secrets**: Hech qachon `appsettings.json` da production secrets ni commit qilmang
2. **JWT Key**: `JwtSettings__SecretKey` ni har safar yangi qiymatga o'zgartiring
3. **Database**: PostgreSQL parolini maxfiy saqlang
4. **HTTPS**: Railway avtomatik HTTPS ni ta'minlaydi

## =� Monitoring va Scaling

Railway automatically monitors your application:
- CPU va memory usage
- Request count
- Error rates

Scaling uchun:
1. "Settings" -> "Resources" ga o'ting
2. Kerakli plan ni tanlang

## =� Pricing

Railway **$5/month** credit beradi bepul. Undan keyin:
- **Hobby Plan**: $5/month
- **Pro Plan**: $20/month (production uchun tavsiya)

## = Avtomatik Deploy

Har safar `main` branchga push qilganingizda, Railway avtomatik:
1. Docker image build qiladi
2. Yangi versiyani deploy qiladi
3. Health check qiladi
4. Zero-downtime deployment

## =� Yordam

Muammolar yuzaga kelsa:
- Railway Documentation: https://docs.railway.app
- Railway Discord: https://discord.gg/railway
- Railway Support: support@railway.app

## =� Test qilish uchun endpoint

Deployment muvaffaqiyatli bo'lgandan keyin:

```bash
# Health check
curl https://your-app.up.railway.app/health

# API root
curl https://your-app.up.railway.app/

# Register yangi foydalanuvchi
curl -X POST https://your-app.up.railway.app/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "first_name": "Test",
    "last_name": "User",
    "email": "test@example.com",
    "password": "Test@123",
    "phone": "+998901234567",
    "user_type": "Customer"
  }'
```

---

**Muvaffaqiyatli deploy qiling!** =�
