-- ZiyoMarket Database Fix Script
-- Run this script in PG Admin or any PostgreSQL client connected to your database.

BEGIN;

-- 1. Drop the legacy CategoryId column from Products table if it exists
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns 
               WHERE table_name = 'Products' AND column_name = 'CategoryId') THEN
        ALTER TABLE "Products" DROP COLUMN "CategoryId";
    END IF;
END $$;

-- 2. Add SKU column to Products table if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'Products' AND column_name = 'SKU') THEN
        ALTER TABLE "Products" ADD COLUMN "SKU" text;
    END IF;
END $$;

-- 3. Add Barcode column to Products table if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'Products' AND column_name = 'Barcode') THEN
        ALTER TABLE "Products" ADD COLUMN "Barcode" text;
    END IF;
END $$;

COMMIT;
