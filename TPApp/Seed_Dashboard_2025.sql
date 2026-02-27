-- =============================================
-- Script: Seed Dashboard Sample Data 2025
-- Run AFTER Create_Admin_Dashboard_Tables.sql
-- =============================================

-- ─── 1. DashboardSnapshot (singleton row) ─────────────────────
-- Upsert – safe to run multiple times
MERGE [dbo].[DashboardSnapshot] AS target
USING (SELECT 1 AS Id) AS source ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        TotalProducts     = 342,
        CongNgheCount     = 185,
        ThietBiCount      = 112,
        TriTueCount       = 45,
        TotalProjects     = 87,
        ActiveProjects    = 34,
        CompletedProjects = 41,
        TotalSuppliers    = 156,
        ActiveSuppliers   = 118,
        TotalConsultants  = 74,
        ActiveConsultants = 59,
        UpdatedAt         = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (Id, TotalProducts, CongNgheCount, ThietBiCount, TriTueCount,
            TotalProjects, ActiveProjects, CompletedProjects,
            TotalSuppliers, ActiveSuppliers,
            TotalConsultants, ActiveConsultants, UpdatedAt)
    VALUES (1, 342, 185, 112, 45,
            87, 34, 41,
            156, 118,
            74, 59, GETUTCDATE());

PRINT 'DashboardSnapshot seeded.';
GO

-- ─── 2. DashboardMonthlyStats – 12 months of 2025 ────────────
-- Delete & re-insert 2025 rows cleanly
DELETE FROM [dbo].[DashboardMonthlyStats] WHERE [Year] = 2025;

INSERT INTO [dbo].[DashboardMonthlyStats]
    ([Year], [Month], [NewProducts], [NewProjects], [NewSuppliers], [NewConsultants], [CreatedAt])
VALUES
--  Year  Month  Products  Projects  Suppliers  Consultants
    (2025,  1,      18,       4,        8,          3,        GETUTCDATE()),
    (2025,  2,      22,       5,        6,          4,        GETUTCDATE()),
    (2025,  3,      31,       7,        9,          5,        GETUTCDATE()),
    (2025,  4,      27,       6,       11,          4,        GETUTCDATE()),
    (2025,  5,      35,       9,       13,          6,        GETUTCDATE()),
    (2025,  6,      29,       8,       10,          5,        GETUTCDATE()),
    (2025,  7,      33,      10,       12,          7,        GETUTCDATE()),
    (2025,  8,      38,      11,       14,          6,        GETUTCDATE()),
    (2025,  9,      41,      12,       15,          8,        GETUTCDATE()),
    (2025, 10,      36,       9,       11,          5,        GETUTCDATE()),
    (2025, 11,      44,      13,       16,          9,        GETUTCDATE()),
    (2025, 12,      48,      14,       18,         10,        GETUTCDATE());

PRINT '12 months of 2025 DashboardMonthlyStats seeded.';
GO

-- ─── 3. DashboardMonthlyStats – Jan & Feb 2026 ───────────────
DELETE FROM [dbo].[DashboardMonthlyStats] WHERE [Year] = 2026;

INSERT INTO [dbo].[DashboardMonthlyStats]
    ([Year], [Month], [NewProducts], [NewProjects], [NewSuppliers], [NewConsultants], [CreatedAt])
VALUES
--  Year  Month  Products  Projects  Suppliers  Consultants
    (2026,  1,      52,       15,       19,         11,       GETUTCDATE()),
    (2026,  2,      47,       13,       17,          9,       GETUTCDATE());

PRINT '2 months of 2026 DashboardMonthlyStats seeded.';
GO

-- ─── Verify ───────────────────────────────────────────────────
SELECT 'DashboardSnapshot' AS [Table], * FROM [dbo].[DashboardSnapshot];

SELECT 'MonthlyStats' AS [Table],
       [Year], [Month], [NewProducts], [NewProjects], [NewSuppliers], [NewConsultants]
FROM   [dbo].[DashboardMonthlyStats]
WHERE  [Year] IN (2025, 2026)
ORDER  BY [Year], [Month];
GO
