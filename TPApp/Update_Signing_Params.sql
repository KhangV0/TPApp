-- =============================================
-- Update SYS_PARAMETERS cho Signing Providers
-- Dùng MERGE để UPDATE nếu đã tồn tại, INSERT nếu chưa có
-- Date: 2026-03-08
-- =============================================
USE [TechMartNew]
GO

PRINT '── Cập nhật cấu hình Signing Providers ──';

-- ═══════════════════════════════════════════════════
-- 1. CHUNG — Signing Mode & Provider Default
-- ═══════════════════════════════════════════════════

-- SIGNING_MODE: TESTING (stub) / PRODUCTION (real API)
MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_MODE' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = 'TESTING', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_MODE', 'TESTING', N'Chế độ ký: TESTING (stub) hoặc PRODUCTION (API thật)', N'CONTRACT', 1);

-- SIGNING_PROVIDER_DEFAULT: VNPT / FPT / Viettel
MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_PROVIDER_DEFAULT' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = 'VNPT', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_PROVIDER_DEFAULT', 'VNPT', N'CA provider mặc định (VNPT/FPT/Viettel)', N'CONTRACT', 1);

-- ═══════════════════════════════════════════════════
-- 2. VNPT SmartCA v2
-- ═══════════════════════════════════════════════════

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_VNPT_API_BASE' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'VNPT SmartCA API base URL (vd: https://smartca.vnpt.vn)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_VNPT_API_BASE', '', N'VNPT SmartCA API base URL (vd: https://smartca.vnpt.vn)', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_VNPT_API_KEY' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'VNPT SmartCA API Key (X-API-KEY header)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_VNPT_API_KEY', '', N'VNPT SmartCA API Key (X-API-KEY header)', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_VNPT_CREDENTIAL' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'VNPT Credential ID (cert serial từ SmartCA portal)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_VNPT_CREDENTIAL', '', N'VNPT Credential ID (cert serial từ SmartCA portal)', N'CONTRACT', 1);

-- ═══════════════════════════════════════════════════
-- 3. FPT Signing Hub
-- ═══════════════════════════════════════════════════

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_FPT_API_BASE' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'FPT Signing Hub API base URL (vd: https://signinghub.fpt.com.vn)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_FPT_API_BASE', '', N'FPT Signing Hub API base URL (vd: https://signinghub.fpt.com.vn)', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_FPT_API_KEY' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'FPT Signing Hub API Key (X-API-KEY header)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_FPT_API_KEY', '', N'FPT Signing Hub API Key (X-API-KEY header)', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_FPT_CLIENT_ID' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'FPT OAuth2 Client ID (cho xác thực token)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_FPT_CLIENT_ID', '', N'FPT OAuth2 Client ID (cho xác thực token)', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_FPT_SECRET' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'FPT OAuth2 Client Secret', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_FPT_SECRET', '', N'FPT OAuth2 Client Secret', N'CONTRACT', 1);

-- ═══════════════════════════════════════════════════
-- 4. Viettel MySign (Cloud-CA)
-- ═══════════════════════════════════════════════════

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_VIETTEL_API_BASE' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'Viettel MySign API base URL (vd: https://mysign.viettel-ca.vn)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_VIETTEL_API_BASE', '', N'Viettel MySign API base URL (vd: https://mysign.viettel-ca.vn)', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_VIETTEL_CLIENT_ID' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'Viettel MySign OAuth2 Client ID', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_VIETTEL_CLIENT_ID', '', N'Viettel MySign OAuth2 Client ID', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_VIETTEL_SECRET' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'Viettel MySign OAuth2 Client Secret', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_VIETTEL_SECRET', '', N'Viettel MySign OAuth2 Client Secret', N'CONTRACT', 1);

MERGE INTO [dbo].[SYS_PARAMETERS] AS target
USING (SELECT 'SIGNING_VIETTEL_USER_ID' AS Name) AS source ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Val = '', [Description] = N'Viettel MySign User ID (server-to-server, tùy chọn)', Domain = N'CONTRACT', Activated = 1
WHEN NOT MATCHED THEN
    INSERT (Name, Val, [Description], Domain, Activated)
    VALUES ('SIGNING_VIETTEL_USER_ID', '', N'Viettel MySign User ID (server-to-server, tùy chọn)', N'CONTRACT', 1);

-- Xóa key cũ không còn dùng (SIGNING_VIETTEL_API_KEY thay bằng CLIENT_ID + SECRET)
-- Uncomment nếu muốn dọn:
-- DELETE FROM [dbo].[SYS_PARAMETERS] WHERE Name = 'SIGNING_VIETTEL_API_KEY';
-- DELETE FROM [dbo].[SYS_PARAMETERS] WHERE Name = 'SIGNING_FPT_API_KEY';  -- FPT cũng đổi sang CLIENT_ID + SECRET

GO

-- ═══════════════════════════════════════════════════
-- KIỂM TRA KẾT QUẢ
-- ═══════════════════════════════════════════════════
SELECT Name, Val, [Description], Domain, Activated
FROM   [dbo].[SYS_PARAMETERS]
WHERE  Name LIKE 'SIGNING_%'
ORDER  BY Name;

PRINT '══ Update Signing Params hoàn tất ══';
GO

-- ═══════════════════════════════════════════════════
-- HƯỚNG DẪN SỬ DỤNG NHANH
-- ═══════════════════════════════════════════════════
-- 
-- ▶ Chuyển sang PRODUCTION:
--   UPDATE SYS_PARAMETERS SET Val = 'PRODUCTION' WHERE Name = 'SIGNING_MODE';
--
-- ▶ Chuyển provider sang FPT:
--   UPDATE SYS_PARAMETERS SET Val = 'FPT' WHERE Name = 'SIGNING_PROVIDER_DEFAULT';
--
-- ▶ Chuyển provider sang Viettel:
--   UPDATE SYS_PARAMETERS SET Val = 'Viettel' WHERE Name = 'SIGNING_PROVIDER_DEFAULT';
--
-- ▶ Điền API key VNPT:
--   UPDATE SYS_PARAMETERS SET Val = 'https://smartca.vnpt.vn' WHERE Name = 'SIGNING_VNPT_API_BASE';
--   UPDATE SYS_PARAMETERS SET Val = 'your-api-key-here'       WHERE Name = 'SIGNING_VNPT_API_KEY';
--   UPDATE SYS_PARAMETERS SET Val = 'your-credential-id'      WHERE Name = 'SIGNING_VNPT_CREDENTIAL';
--
-- ▶ Điền API key FPT:
--   UPDATE SYS_PARAMETERS SET Val = 'https://signinghub.fpt.com.vn' WHERE Name = 'SIGNING_FPT_API_BASE';
--   UPDATE SYS_PARAMETERS SET Val = 'your-fpt-api-key'              WHERE Name = 'SIGNING_FPT_API_KEY';
--   UPDATE SYS_PARAMETERS SET Val = 'your-fpt-client-id'            WHERE Name = 'SIGNING_FPT_CLIENT_ID';
--   UPDATE SYS_PARAMETERS SET Val = 'your-fpt-secret'               WHERE Name = 'SIGNING_FPT_SECRET';
--
-- ▶ Điền API key Viettel:
--   UPDATE SYS_PARAMETERS SET Val = 'https://mysign.viettel-ca.vn' WHERE Name = 'SIGNING_VIETTEL_API_BASE';
--   UPDATE SYS_PARAMETERS SET Val = 'your-viettel-client-id'       WHERE Name = 'SIGNING_VIETTEL_CLIENT_ID';
--   UPDATE SYS_PARAMETERS SET Val = 'your-viettel-secret'          WHERE Name = 'SIGNING_VIETTEL_SECRET';
--   UPDATE SYS_PARAMETERS SET Val = 'your-viettel-user-id'         WHERE Name = 'SIGNING_VIETTEL_USER_ID';
