-- ============================================================
-- TECHPORT — Dashboard Analytics Setup Script
-- Version: 2026-02-27
-- Run this once on the target server (SQL Server 2016+)
--
-- What this does:
--   STEP 1 — Create DashboardSnapshot table (singleton)
--   STEP 2 — Create DashboardMonthlyStats table
--   STEP 3 — Seed DashboardSnapshot (safe MERGE / upsert)
--   STEP 4 — Seed DashboardMonthlyStats (2025 full + 2026 Jan–Feb)
--   STEP 5 — Verify: show row counts + quick preview
--
-- Safe to re-run: all DDL is IF NOT EXISTS, MERGE is idempotent
-- ============================================================

USE [TechMartNew];   -- <<< đổi thành tên database thực tế của bạn
GO

-- ============================================================
-- STEP 1 — DashboardSnapshot
-- Singleton table (always Id = 1)
-- ============================================================
IF NOT EXISTS (
    SELECT * FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[DashboardSnapshot]') AND type = N'U'
)
BEGIN
    CREATE TABLE [dbo].[DashboardSnapshot] (
        [Id]               INT            NOT NULL CONSTRAINT [PK_DashboardSnapshot] PRIMARY KEY,
        [TotalProducts]    INT            NOT NULL DEFAULT 0,
        [CongNgheCount]    INT            NOT NULL DEFAULT 0,
        [ThietBiCount]     INT            NOT NULL DEFAULT 0,
        [TriTueCount]      INT            NOT NULL DEFAULT 0,
        [TotalProjects]    INT            NOT NULL DEFAULT 0,
        [ActiveProjects]   INT            NOT NULL DEFAULT 0,
        [CompletedProjects]INT            NOT NULL DEFAULT 0,
        [TotalSuppliers]   INT            NOT NULL DEFAULT 0,
        [ActiveSuppliers]  INT            NOT NULL DEFAULT 0,
        [TotalConsultants] INT            NOT NULL DEFAULT 0,
        [ActiveConsultants]INT            NOT NULL DEFAULT 0,
        [UpdatedAt]        DATETIME2(0)   NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT '[OK] DashboardSnapshot created.';
END
ELSE
    PRINT '[SKIP] DashboardSnapshot already exists.';
GO

-- ============================================================
-- STEP 2 — DashboardMonthlyStats
-- One row per (Year, Month) — unique constraint enforced
-- ============================================================
IF NOT EXISTS (
    SELECT * FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[DashboardMonthlyStats]') AND type = N'U'
)
BEGIN
    CREATE TABLE [dbo].[DashboardMonthlyStats] (
        [Id]             INT          NOT NULL IDENTITY(1,1) CONSTRAINT [PK_DashboardMonthlyStats] PRIMARY KEY,
        [Year]           INT          NOT NULL,
        [Month]          INT          NOT NULL,
        [NewProducts]    INT          NOT NULL DEFAULT 0,
        [NewProjects]    INT          NOT NULL DEFAULT 0,
        [NewSuppliers]   INT          NOT NULL DEFAULT 0,
        [NewConsultants] INT          NOT NULL DEFAULT 0,
        [CreatedAt]      DATETIME2(0) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [UQ_DashboardMonthlyStats_YearMonth] UNIQUE ([Year], [Month])
    );

    CREATE NONCLUSTERED INDEX [IX_DashboardMonthlyStats_YearMonth]
        ON [dbo].[DashboardMonthlyStats] ([Year] DESC, [Month] DESC);

    PRINT '[OK] DashboardMonthlyStats created.';
END
ELSE
    PRINT '[SKIP] DashboardMonthlyStats already exists.';
GO

-- ============================================================
-- STEP 3 — Seed DashboardSnapshot (MERGE = safe upsert)
-- Update values to match your real production data if known,
-- otherwise the background job will overwrite after first run.
-- ============================================================
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
    INSERT (Id,
            TotalProducts, CongNgheCount, ThietBiCount, TriTueCount,
            TotalProjects, ActiveProjects, CompletedProjects,
            TotalSuppliers, ActiveSuppliers,
            TotalConsultants, ActiveConsultants,
            UpdatedAt)
    VALUES (1,
            342, 185, 112, 45,
            87,  34,  41,
            156, 118,
            74,  59,
            GETUTCDATE());

PRINT '[OK] DashboardSnapshot seeded (Id=1).';
GO

-- ============================================================
-- STEP 4 — Seed DashboardMonthlyStats
-- Full 2025 (12 months) + 2026 Jan & Feb
-- DELETE + INSERT per year — clean & repeatable
-- ============================================================

-- ── 2025 (12 months) ────────────────────────────────────────
DELETE FROM [dbo].[DashboardMonthlyStats] WHERE [Year] = 2025;

INSERT INTO [dbo].[DashboardMonthlyStats]
    ([Year], [Month], [NewProducts], [NewProjects], [NewSuppliers], [NewConsultants], [CreatedAt])
VALUES
--   Year  Month   Prod   Proj   Supp   Cons
    (2025,  1,      18,    4,     8,     3,    GETUTCDATE()),
    (2025,  2,      22,    5,     6,     4,    GETUTCDATE()),
    (2025,  3,      31,    7,     9,     5,    GETUTCDATE()),
    (2025,  4,      27,    6,    11,     4,    GETUTCDATE()),
    (2025,  5,      35,    9,    13,     6,    GETUTCDATE()),
    (2025,  6,      29,    8,    10,     5,    GETUTCDATE()),
    (2025,  7,      33,   10,    12,     7,    GETUTCDATE()),
    (2025,  8,      38,   11,    14,     6,    GETUTCDATE()),
    (2025,  9,      41,   12,    15,     8,    GETUTCDATE()),
    (2025, 10,      36,    9,    11,     5,    GETUTCDATE()),
    (2025, 11,      44,   13,    16,     9,    GETUTCDATE()),
    (2025, 12,      48,   14,    18,    10,    GETUTCDATE());

PRINT '[OK] 2025 monthly stats seeded (12 rows).';
GO

-- ── 2026 (Jan + Feb) ────────────────────────────────────────
DELETE FROM [dbo].[DashboardMonthlyStats] WHERE [Year] = 2026;

INSERT INTO [dbo].[DashboardMonthlyStats]
    ([Year], [Month], [NewProducts], [NewProjects], [NewSuppliers], [NewConsultants], [CreatedAt])
VALUES
    (2026,  1,      52,   15,    19,    11,    GETUTCDATE()),
    (2026,  2,      47,   13,    17,     9,    GETUTCDATE());

PRINT '[OK] 2026 monthly stats seeded (Jan + Feb).';
GO

-- ============================================================
-- STEP 5 — Verify
-- ============================================================
PRINT ''; PRINT '=== VERIFICATION ===';

SELECT
    'DashboardSnapshot' AS [Table],
    TotalProducts, CongNgheCount, ThietBiCount, TriTueCount,
    TotalProjects, TotalSuppliers, TotalConsultants,
    FORMAT(UpdatedAt, 'dd/MM/yyyy HH:mm') AS UpdatedAt
FROM [dbo].[DashboardSnapshot];

SELECT
    [Year], [Month],
    [NewProducts] AS Prod,
    [NewProjects] AS Proj,
    [NewSuppliers] AS Supp,
    [NewConsultants] AS Cons
FROM [dbo].[DashboardMonthlyStats]
WHERE [Year] IN (2025, 2026)
ORDER BY [Year], [Month];

SELECT
    COUNT(*) AS TotalMonthlyRows,
    MIN([Year]) AS FromYear,
    MAX([Year]) AS ToYear
FROM [dbo].[DashboardMonthlyStats];

PRINT '[DONE] Setup complete. Run the app — background job will auto-refresh snapshot.';
GO
