-- Manual Payment Verification System - SystemSettings

-- 1. Admin Karta Ma'lumotlari (O'z ma'lumotlaringiz bilan almashtiring!)
INSERT INTO "SystemSettings" ("SettingKey", "SettingValue", "Description", "Category", "DataType", "IsEditable", "CreatedAt")
VALUES
  ('Payment.BankTransfer.Enabled', 'true', 'Bank o''tkazmasi yoqilganmi', 'Payment', 'Boolean', true, NOW()),
  ('Payment.BankTransfer.CardNumber', '8600 1234 5678 9012', 'Admin karta raqami (O''ZGARTIRING!)', 'Payment', 'String', true, NOW()),
  ('Payment.BankTransfer.CardHolderName', 'Abdulaziz Raximov', 'Karta egasi (O''ZGARTIRING!)', 'Payment', 'String', true, NOW()),
  ('Payment.BankTransfer.BankName', 'Kapitalbank', 'Bank nomi (O''ZGARTIRING!)', 'Payment', 'String', true, NOW())
ON CONFLICT ("SettingKey") DO UPDATE
  SET "SettingValue" = EXCLUDED."SettingValue",
      "Description" = EXCLUDED."Description",
      "UpdatedAt" = NOW();

-- 2. Permissions (Admin uchun to'lovlarni tasdiqlash huquqi)
INSERT INTO "Permissions" ("PermissionName", "Description", "Category", "CreatedAt")
VALUES ('VerifyPayments', 'To''lovlarni tasdiqlash/rad etish', 'Payment', NOW())
ON CONFLICT ("PermissionName") DO NOTHING;

-- 3. SuperAdmin rolliga permission berish
INSERT INTO "RolePermissions" ("RoleId", "PermissionId", "CreatedAt")
SELECT r."Id", p."Id", NOW()
FROM "Roles" r, "Permissions" p
WHERE r."RoleName" = 'SuperAdmin' AND p."PermissionName" = 'VerifyPayments'
ON CONFLICT DO NOTHING;

-- 4. Tekshirish
SELECT "SettingKey", "SettingValue", "Description"
FROM "SystemSettings"
WHERE "SettingKey" LIKE 'Payment.BankTransfer%';
