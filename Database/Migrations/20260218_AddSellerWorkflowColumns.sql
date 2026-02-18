-- =============================================
-- Seller Workflow Schema Migration
-- Created: 2026-02-18
-- Description: Add SelectedSellerId to Projects and StatusId to ProposalSubmissions
-- =============================================

USE TechMartNew;
GO

-- =============================================
-- 1. Update Projects Table
-- =============================================
PRINT 'Updating Projects table...';

-- Add SelectedSellerId column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'SelectedSellerId')
BEGIN
    ALTER TABLE Projects
    ADD SelectedSellerId INT NULL;
    
    PRINT 'Added SelectedSellerId column to Projects';
END
ELSE
BEGIN
    PRINT 'SelectedSellerId already exists in Projects';
END
GO

-- Add SelectedDate column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'SelectedDate')
BEGIN
    ALTER TABLE Projects
    ADD SelectedDate DATETIME2 NULL;
    
    PRINT 'Added SelectedDate column to Projects';
END
ELSE
BEGIN
    PRINT 'SelectedDate already exists in Projects';
END
GO

-- Add Foreign Key constraint for SelectedSellerId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_Projects_SelectedSeller')
BEGIN
    ALTER TABLE Projects
    ADD CONSTRAINT FK_Projects_SelectedSeller 
        FOREIGN KEY (SelectedSellerId) REFERENCES Users(UserId);
    
    PRINT 'Added FK constraint FK_Projects_SelectedSeller';
END
ELSE
BEGIN
    PRINT 'FK constraint FK_Projects_SelectedSeller already exists';
END
GO

-- =============================================
-- 2. Update ProposalSubmissions Table
-- =============================================
PRINT '';
PRINT 'Updating ProposalSubmissions table...';

-- Add StatusId column if missing
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ProposalSubmissions' AND COLUMN_NAME = 'StatusId')
BEGIN
    ALTER TABLE ProposalSubmissions
    ADD StatusId INT NOT NULL DEFAULT 0;
    
    PRINT 'Added StatusId column to ProposalSubmissions';
    PRINT 'StatusId values: 0=Draft, 1=Submitted, 2=Selected, 3=Rejected';
END
ELSE
BEGIN
    PRINT 'StatusId already exists in ProposalSubmissions';
END
GO

-- Add SubmittedDate if missing
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ProposalSubmissions' AND COLUMN_NAME = 'SubmittedDate')
BEGIN
    ALTER TABLE ProposalSubmissions
    ADD SubmittedDate DATETIME2 NULL;
    
    PRINT 'Added SubmittedDate column to ProposalSubmissions';
END
ELSE
BEGIN
    PRINT 'SubmittedDate already exists in ProposalSubmissions';
END
GO

-- =============================================
-- 3. Create Index for Performance
-- =============================================
PRINT '';
PRINT 'Creating indexes...';

-- Index on SelectedSellerId for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Projects_SelectedSellerId' AND object_id = OBJECT_ID('Projects'))
BEGIN
    CREATE INDEX IX_Projects_SelectedSellerId ON Projects(SelectedSellerId);
    PRINT 'Created index IX_Projects_SelectedSellerId';
END
ELSE
BEGIN
    PRINT 'Index IX_Projects_SelectedSellerId already exists';
END
GO

-- Index on ProposalSubmissions StatusId
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_ProposalSubmissions_StatusId' AND object_id = OBJECT_ID('ProposalSubmissions'))
BEGIN
    CREATE INDEX IX_ProposalSubmissions_StatusId ON ProposalSubmissions(StatusId);
    PRINT 'Created index IX_ProposalSubmissions_StatusId';
END
ELSE
BEGIN
    PRINT 'Index IX_ProposalSubmissions_StatusId already exists';
END
GO

-- =============================================
-- 4. Verify Schema
-- =============================================
PRINT '';
PRINT '==============================================';
PRINT 'Schema Verification';
PRINT '==============================================';

-- Verify Projects columns
SELECT 'Projects' as TableName, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Projects' 
  AND COLUMN_NAME IN ('SelectedSellerId', 'SelectedDate')
ORDER BY COLUMN_NAME;

-- Verify ProposalSubmissions columns
SELECT 'ProposalSubmissions' as TableName, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProposalSubmissions' 
  AND COLUMN_NAME IN ('StatusId', 'SubmittedDate')
ORDER BY COLUMN_NAME;

PRINT '';
PRINT 'Migration completed successfully!';
GO
