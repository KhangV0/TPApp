-- =============================================
-- Script: Create Admin Analytics Dashboard Tables
-- Tables: DashboardSnapshot, DashboardMonthlyStats
-- Run this once against your SQL Server database
-- =============================================

-- 1. DashboardSnapshot (singleton row, Id = 1)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DashboardSnapshot]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[DashboardSnapshot] (
        [Id]                INT           NOT NULL DEFAULT 1,
        [TotalProducts]     INT           NOT NULL DEFAULT 0,
        [CongNgheCount]     INT           NOT NULL DEFAULT 0,
        [ThietBiCount]      INT           NOT NULL DEFAULT 0,
        [TriTueCount]       INT           NOT NULL DEFAULT 0,
        [TotalProjects]     INT           NOT NULL DEFAULT 0,
        [ActiveProjects]    INT           NOT NULL DEFAULT 0,
        [CompletedProjects] INT           NOT NULL DEFAULT 0,
        [TotalSuppliers]    INT           NOT NULL DEFAULT 0,
        [ActiveSuppliers]   INT           NOT NULL DEFAULT 0,
        [TotalConsultants]  INT           NOT NULL DEFAULT 0,
        [ActiveConsultants] INT           NOT NULL DEFAULT 0,
        [UpdatedAt]         DATETIME2(7)  NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_DashboardSnapshot] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DashboardSnapshot_SingleRow] CHECK ([Id] = 1)
    );

    -- Seed the single row
    INSERT INTO [dbo].[DashboardSnapshot] ([Id]) VALUES (1);

    PRINT 'Table DashboardSnapshot created and seeded.';
END
ELSE
BEGIN
    PRINT 'Table DashboardSnapshot already exists, skipping.';
END
GO

-- 2. DashboardMonthlyStats (one row per month)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DashboardMonthlyStats]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[DashboardMonthlyStats] (
        [Id]              INT           IDENTITY(1,1) NOT NULL,
        [Year]            INT           NOT NULL,
        [Month]           INT           NOT NULL,
        [NewProducts]     INT           NOT NULL DEFAULT 0,
        [NewProjects]     INT           NOT NULL DEFAULT 0,
        [NewSuppliers]    INT           NOT NULL DEFAULT 0,
        [NewConsultants]  INT           NOT NULL DEFAULT 0,
        [CreatedAt]       DATETIME2(7)  NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_DashboardMonthlyStats] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_DashboardMonthlyStats_YearMonth] UNIQUE ([Year], [Month])
    );

    CREATE NONCLUSTERED INDEX [IX_DashboardMonthlyStats_YearMonth]
        ON [dbo].[DashboardMonthlyStats] ([Year] DESC, [Month] DESC);

    PRINT 'Table DashboardMonthlyStats created.';
END
ELSE
BEGIN
    PRINT 'Table DashboardMonthlyStats already exists, skipping.';
END
GO

PRINT 'Admin Analytics Dashboard tables ready.';
GO
