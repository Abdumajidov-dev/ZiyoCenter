-- Manual Migration Script for ProductCategories
-- Run this in Railway Database Query interface

BEGIN;

-- Step 1: Create ProductCategories table
CREATE TABLE IF NOT EXISTS "ProductCategories" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" integer NOT NULL,
    "CategoryId" integer NOT NULL,
    "CreatedAt" text NOT NULL,
    "UpdatedAt" text,
    "DeletedAt" text,
    CONSTRAINT "FK_ProductCategories_Products_ProductId" 
        FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductCategories_Categories_CategoryId" 
        FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE CASCADE
);

-- Step 2: Create indexes
CREATE INDEX IF NOT EXISTS "IX_ProductCategories_ProductId" ON "ProductCategories" ("ProductId");
CREATE INDEX IF NOT EXISTS "IX_ProductCategories_CategoryId" ON "ProductCategories" ("CategoryId");

-- Step 3: Copy existing data (BEFORE dropping CategoryId)
INSERT INTO "ProductCategories" ("ProductId", "CategoryId", "CreatedAt")
SELECT 
    "Id" as "ProductId",
    "CategoryId",
    COALESCE("CreatedAt", NOW()::text) as "CreatedAt"
FROM "Products"
WHERE "CategoryId" IS NOT NULL AND "DeletedAt" IS NULL
ON CONFLICT DO NOTHING;

-- Step 4: Drop foreign key constraint
ALTER TABLE "Products" DROP CONSTRAINT IF EXISTS "FK_Products_Categories_CategoryId";

-- Step 5: Drop index
DROP INDEX IF EXISTS "IX_Products_CategoryId";

-- Step 6: Drop CategoryId column
ALTER TABLE "Products" DROP COLUMN IF EXISTS "CategoryId";

-- Step 7: Record migration in history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260130_AddProductCategoriesManual', '9.0.10')
ON CONFLICT DO NOTHING;

COMMIT;
