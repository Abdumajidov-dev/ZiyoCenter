-- Railway PostgreSQL bazasidagi barcha tablelarni ko'rish
SELECT tablename
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY tablename;

-- Agar tablelar bo'lsa, ularni o'chirish uchun SQL generate qilish
SELECT 'DROP TABLE IF EXISTS "' || tablename || '" CASCADE;' as drop_statement
FROM pg_tables
WHERE schemaname = 'public';
