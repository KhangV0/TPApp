-- =============================================
-- Step 6: Extend LegalReviewForms + Create ContractComments
-- Database: TechMartNew
-- Run this script once
-- =============================================

USE [TechMartNew]
GO

-- ── 1. Extend LegalReviewForms ────────────────────────────────────────────────
-- Add new columns (idempotent: use IF NOT EXISTS checks)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'Version')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [Version] INT NOT NULL DEFAULT 1;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'HtmlSnapshot')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [HtmlSnapshot] NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'ContractFilePath')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [ContractFilePath] NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'ReviewDeadline')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [ReviewDeadline] DATETIME2 NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'ReviewedBy')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [ReviewedBy] INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'RejectionReason')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [RejectionReason] NVARCHAR(MAX) NULL;

-- Fix NOT NULL columns that have no data yet
-- NguoiKiemTra and KetQuaKiemTra already exist as NOT NULL; patch existing rows
UPDATE [dbo].[LegalReviewForms]
SET [NguoiKiemTra] = 'Hệ thống', [KetQuaKiemTra] = 'Bản nháp tự động'
WHERE [NguoiKiemTra] IS NULL OR [NguoiKiemTra] = '';
GO

-- ── 2. Create ContractComments ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ContractComments]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[ContractComments] (
        [Id]                INT IDENTITY(1,1) NOT NULL,
        [ProjectId]         INT NOT NULL,
        [LegalReviewFormId] INT NOT NULL,
        [CommentText]       NVARCHAR(MAX) NOT NULL,
        [CommentType]       INT NOT NULL DEFAULT 1,  -- 1=General,2=LegalIssue,3=Suggestion
        [IsResolved]        BIT NOT NULL DEFAULT 0,
        [AuthorId]          INT NOT NULL,
        [AuthorName]        NVARCHAR(200) NULL,
        [NgayTao]           DATETIME2 NOT NULL DEFAULT GETDATE(),
        [NgaySua]           DATETIME2 NULL,
        CONSTRAINT [PK_ContractComments] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- FK to LegalReviewForms
    ALTER TABLE [dbo].[ContractComments]
        ADD CONSTRAINT [FK_ContractComments_LegalReviewForms]
        FOREIGN KEY ([LegalReviewFormId]) REFERENCES [dbo].[LegalReviewForms]([Id]);

    PRINT 'Table ContractComments created.';
END
ELSE
    PRINT 'Table ContractComments already exists, skipped.';
GO

PRINT 'Step 6 migration completed successfully!';
