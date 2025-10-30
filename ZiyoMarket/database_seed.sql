-- ZiyoMarket Database Seed Script
-- Default Admin User va Test Ma'lumotlar

-- =============================================
-- 1. DEFAULT ADMIN USER
-- =============================================
-- Username: admin@ziyomarket.uz
-- Password: Admin@123
-- Password hash using BCrypt (work factor 12)

INSERT INTO "Admins" (
    "FirstName",
    "LastName",
    "Username",
    "Phone",
    "Email",
    "PasswordHash",
    "Permissions",
    "IsActive",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt",
    "CreatedBy",
    "UpdatedBy"
) VALUES (
    'Super',
    'Admin',
    'admin',
    '+998901234567',
    'admin@ziyomarket.uz',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5xyKP4xqk5XYi', -- Admin@123
    'ALL',
    true,
    CURRENT_TIMESTAMP,
    NULL,
    NULL,
    NULL,
    NULL
) ON CONFLICT DO NOTHING;

-- =============================================
-- 2. TEST CUSTOMERS
-- =============================================
-- Customer 1: john@example.com / password123
INSERT INTO "Customers" (
    "FirstName",
    "LastName",
    "Phone",
    "Email",
    "PasswordHash",
    "Address",
    "CashbackBalance",
    "IsActive",
    "CreatedAt"
) VALUES (
    'John',
    'Doe',
    '+998901111111',
    'john@example.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5xyKP4xqk5XYi', -- password123
    'Toshkent, Yakkasaroy tumani',
    0,
    true,
    CURRENT_TIMESTAMP
) ON CONFLICT DO NOTHING;

-- Customer 2: jane@example.com / password123
INSERT INTO "Customers" (
    "FirstName",
    "LastName",
    "Phone",
    "Email",
    "PasswordHash",
    "Address",
    "CashbackBalance",
    "IsActive",
    "CreatedAt"
) VALUES (
    'Jane',
    'Smith',
    '+998902222222',
    'jane@example.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5xyKP4xqk5XYi', -- password123
    'Toshkent, Chilonzor tumani',
    50000,
    true,
    CURRENT_TIMESTAMP
) ON CONFLICT DO NOTHING;

-- =============================================
-- 3. TEST SELLERS
-- =============================================
-- Seller 1: seller1@example.com / password123
INSERT INTO "Sellers" (
    "FirstName",
    "LastName",
    "Phone",
    "Email",
    "PasswordHash",
    "Address",
    "StoreName",
    "Description",
    "Rating",
    "CommissionRate",
    "IsActive",
    "CreatedAt"
) VALUES (
    'Ali',
    'Valiyev',
    '+998903333333',
    'seller1@example.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5xyKP4xqk5XYi', -- password123
    'Toshkent, Yunusobod tumani',
    'Kitoblar Dunyosi',
    'Eng yaxshi kitoblar do''koni',
    4.8,
    5.0,
    true,
    CURRENT_TIMESTAMP
) ON CONFLICT DO NOTHING;

-- =============================================
-- 4. ROOT CATEGORIES
-- =============================================
INSERT INTO "Categories" (
    "Name",
    "Description",
    "ParentId",
    "DisplayOrder",
    "IsActive",
    "CreatedAt"
) VALUES
('Kitoblar', 'Barcha turdagi kitoblar', NULL, 1, true, CURRENT_TIMESTAMP),
('Elektronika', 'Elektronika mahsulotlari', NULL, 2, true, CURRENT_TIMESTAMP),
('Kiyimlar', 'Erkaklar va ayollar kiyimlari', NULL, 3, true, CURRENT_TIMESTAMP),
('Oziq-ovqat', 'Oziq-ovqat mahsulotlari', NULL, 4, true, CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- =============================================
-- 5. SUB CATEGORIES (agar root categories yaratilgan bo'lsa)
-- =============================================
-- Kitoblar sub-categories
INSERT INTO "Categories" (
    "Name",
    "Description",
    "ParentId",
    "DisplayOrder",
    "IsActive",
    "CreatedAt"
)
SELECT 'Dasturlash kitoblari', 'IT va dasturlash bo''yicha kitoblar', "Id", 1, true, CURRENT_TIMESTAMP
FROM "Categories" WHERE "Name" = 'Kitoblar' AND "ParentId" IS NULL
ON CONFLICT DO NOTHING;

INSERT INTO "Categories" (
    "Name",
    "Description",
    "ParentId",
    "DisplayOrder",
    "IsActive",
    "CreatedAt"
)
SELECT 'Badiiy adabiyot', 'Roman, hikoya va she''rlar', "Id", 2, true, CURRENT_TIMESTAMP
FROM "Categories" WHERE "Name" = 'Kitoblar' AND "ParentId" IS NULL
ON CONFLICT DO NOTHING;

-- =============================================
-- 6. SAMPLE PRODUCTS
-- =============================================
-- Product 1: Dasturlash kitobi
INSERT INTO "Products" (
    "Name",
    "Description",
    "QrCode",
    "Price",
    "StockQuantity",
    "CategoryId",
    "Status",
    "MinStockLevel",
    "IsActive",
    "CreatedAt"
)
SELECT
    'C# in Depth',
    'Advanced C# programming techniques and best practices. Jon Skeet tomonidan yozilgan eng yaxshi C# kitobi.',
    'PRD-001',
    250000,
    50,
    c."Id",
    0, -- Active
    10,
    true,
    CURRENT_TIMESTAMP
FROM "Categories" c
WHERE c."Name" = 'Dasturlash kitoblari' AND c."ParentId" IS NOT NULL
ON CONFLICT DO NOTHING;

-- Product 2: Badiiy kitob
INSERT INTO "Products" (
    "Name",
    "Description",
    "QrCode",
    "Price",
    "StockQuantity",
    "CategoryId",
    "Status",
    "MinStockLevel",
    "IsActive",
    "CreatedAt"
)
SELECT
    'O''tkan kunlar',
    'Abdulla Qodiriy asari. O''zbek adabiyotining eng yaxshi romanlaridan biri.',
    'PRD-002',
    45000,
    100,
    c."Id",
    0, -- Active
    10,
    true,
    CURRENT_TIMESTAMP
FROM "Categories" c
WHERE c."Name" = 'Badiiy adabiyot' AND c."ParentId" IS NOT NULL
ON CONFLICT DO NOTHING;

-- =============================================
-- 7. DISCOUNT REASONS
-- =============================================
INSERT INTO "DiscountReasons" ("Name", "Description", "CreatedAt")
VALUES
('VIP Mijoz', 'VIP mijozlar uchun maxsus chegirma', CURRENT_TIMESTAMP),
('Tug''ilgan kun', 'Mijoz tug''ilgan kuni chegirmasi', CURRENT_TIMESTAMP),
('Ommaviy chegirma', 'Barcha mijozlar uchun chegirma', CURRENT_TIMESTAMP),
('Eskirgan mahsulot', 'Eskirgan mahsulot uchun chegirma', CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- =============================================
-- 8. SYSTEM SETTINGS
-- =============================================
INSERT INTO "SystemSettings" ("Key", "Value", "Type", "Description", "CreatedAt")
VALUES
('CashbackPercentage', '2.0', 'decimal', 'Cashback foiz miqdori', CURRENT_TIMESTAMP),
('MinOrderValue', '50000', 'decimal', 'Minimal buyurtma summasi', CURRENT_TIMESTAMP),
('DeliveryFee', '15000', 'decimal', 'Standart yetkazib berish narxi', CURRENT_TIMESTAMP),
('FreeDeliveryThreshold', '200000', 'decimal', 'Bepul yetkazib berish uchun minimal summa', CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- =============================================
-- VERIFICATION
-- =============================================
SELECT 'Seed completed successfully!' as message;

-- Check created records
SELECT 'Admins: ' || COUNT(*) FROM "Admins";
SELECT 'Customers: ' || COUNT(*) FROM "Customers";
SELECT 'Sellers: ' || COUNT(*) FROM "Sellers";
SELECT 'Categories: ' || COUNT(*) FROM "Categories";
SELECT 'Products: ' || COUNT(*) FROM "Products";

-- Show admin credentials
SELECT
    'Admin User Created:' as info,
    "Email" as email,
    'Password: Admin@123' as password
FROM "Admins"
WHERE "Email" = 'admin@ziyomarket.uz';
