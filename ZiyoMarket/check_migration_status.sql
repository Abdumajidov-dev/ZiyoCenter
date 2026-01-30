-- Test script to check if ProductCategories table exists in local database
-- Run this to verify the migration status

-- Check if ProductCategories table exists
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = 'ProductCategories'
) as productcategories_exists;

-- Check if Products still has CategoryId column
SELECT EXISTS (
    SELECT FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND table_name = 'Products'
    AND column_name = 'CategoryId'
) as products_has_categoryid;

-- Count records in Products
SELECT COUNT(*) as total_products FROM "Products" WHERE "DeletedAt" IS NULL;

-- Count records in Categories
SELECT COUNT(*) as total_categories FROM "Categories" WHERE "DeletedAt" IS NULL;

-- If ProductCategories exists, count records
SELECT COUNT(*) as total_product_categories FROM "ProductCategories" WHERE "DeletedAt" IS NULL;
