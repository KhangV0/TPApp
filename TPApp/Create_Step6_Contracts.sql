-- =============================================
-- Step 6 Contract Draft & Legal Review
-- Migration Script
-- Database: TechMartNew
-- Date: 2026-02-19
-- =============================================

USE [TechMartNew]
GO

-- ─── 1. Extend LegalReviewForms ───────────────────────────────────────────────
-- Add new columns needed for Step 6 (skip if already exist)

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'Version')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [Version] INT NOT NULL DEFAULT 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'HtmlSnapshot')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [HtmlSnapshot] NVARCHAR(MAX) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'ContractFilePath')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [ContractFilePath] NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'ReviewDeadline')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [ReviewDeadline] DATETIME NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'ReviewedBy')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [ReviewedBy] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'RejectionReason')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [RejectionReason] NVARCHAR(MAX) NULL;
GO

-- StatusId already exists in LegalReviewForms (was [DaDuyet] bit before).
-- Ensure StatusId column exists as INT for the new enum:
-- LegalReviewStatus: Draft=1, UnderReview=2, ChangesRequested=3, Approved=4, Completed=5

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'StatusId')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [StatusId] INT NOT NULL DEFAULT 1;
GO

-- NguoiSua / NgaySua (audit) - add if not exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'NguoiSua')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [NguoiSua] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LegalReviewForms') AND name = 'NgaySua')
    ALTER TABLE [dbo].[LegalReviewForms] ADD [NgaySua] DATETIME NULL;
GO

PRINT 'LegalReviewForms extended OK';
GO

-- ─── 2. Create ContractComments ───────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractComments')
BEGIN
    CREATE TABLE [dbo].[ContractComments] (
        [Id]                INT            IDENTITY(1,1) NOT NULL,
        [ProjectId]         INT            NULL,
        [LegalReviewFormId] INT            NULL,
        [CommentText]       NVARCHAR(MAX)  NOT NULL,
        -- CommentType: 1=General, 2=LegalIssue, 3=Suggestion
        [CommentType]       INT            NOT NULL DEFAULT 1,
        [AuthorId]          INT            NULL,
        [AuthorName]        NVARCHAR(200)  NULL,
        [IsResolved]        BIT            NOT NULL DEFAULT 0,
        [NgayTao]           DATETIME       NOT NULL DEFAULT GETDATE(),
        [NguoiSua]          INT            NULL,
        [NgaySua]           DATETIME       NULL,

        CONSTRAINT [PK_ContractComments] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- Index for fast lookup by project / form
    CREATE NONCLUSTERED INDEX [IX_ContractComments_ProjectId]
        ON [dbo].[ContractComments] ([ProjectId]);

    CREATE NONCLUSTERED INDEX [IX_ContractComments_LegalReviewFormId]
        ON [dbo].[ContractComments] ([LegalReviewFormId]);

    PRINT 'ContractComments table created OK';
END
ELSE
BEGIN
    PRINT 'ContractComments table already exists — skipped';
END
GO

-- ─── 3. Verification ─────────────────────────────────────────────────────────

SELECT
    'LegalReviewForms columns' AS TableName,
    name AS ColumnName
FROM sys.columns
WHERE object_id = OBJECT_ID('LegalReviewForms')
ORDER BY column_id;

SELECT
    'ContractComments columns' AS TableName,
    name AS ColumnName
FROM sys.columns
WHERE object_id = OBJECT_ID('ContractComments')
ORDER BY column_id;

PRINT '=== Step 6 Migration Complete ===';
GO
