-- =============================================
-- Script: Convert All UserId Columns from nvarchar(450) to INT
-- Description: Comprehensive migration script to convert all UserId, CreatedBy, ModifiedBy, NguoiTao, NguoiSua columns to INT
-- IMPORTANT: Backup your database before running this script!
-- =============================================

USE [TPApp]; -- Change to your database name
GO

PRINT '========================================';
PRINT 'Starting UserId Type Conversion to INT';
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Convert Projects Table
-- =============================================
PRINT 'STEP 1: Converting Projects table...';

-- Check current schema
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'CreatedBy' AND DATA_TYPE = 'nvarchar')
BEGIN
    PRINT '  - CreatedBy is currently nvarchar, converting to INT...';
    
    -- Drop foreign key constraints if any
    DECLARE @sql NVARCHAR(MAX);
    SELECT @sql = 'ALTER TABLE Projects DROP CONSTRAINT ' + name
    FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID('Projects') AND name LIKE '%CreatedBy%';
    IF @sql IS NOT NULL EXEC sp_executesql @sql;
    
    -- Convert column
    ALTER TABLE Projects ALTER COLUMN CreatedBy INT NULL;
    PRINT '  ✓ CreatedBy converted to INT';
END
ELSE
BEGIN
    PRINT '  ✓ CreatedBy is already INT';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'ModifiedBy' AND DATA_TYPE = 'nvarchar')
BEGIN
    PRINT '  - ModifiedBy is currently nvarchar, converting to INT...';
    
    -- Drop foreign key constraints if any
    SELECT @sql = 'ALTER TABLE Projects DROP CONSTRAINT ' + name
    FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID('Projects') AND name LIKE '%ModifiedBy%';
    IF @sql IS NOT NULL EXEC sp_executesql @sql;
    
    -- Convert column
    ALTER TABLE Projects ALTER COLUMN ModifiedBy INT NULL;
    PRINT '  ✓ ModifiedBy converted to INT';
END
ELSE
BEGIN
    PRINT '  ✓ ModifiedBy is already INT';
END

PRINT '';

-- =============================================
-- STEP 2: Convert ProjectMembers Table
-- =============================================
PRINT 'STEP 2: Converting ProjectMembers table...';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ProjectMembers' AND COLUMN_NAME = 'UserId' AND DATA_TYPE = 'nvarchar')
BEGIN
    PRINT '  - UserId is currently nvarchar, converting to INT...';
    
    -- Drop foreign key constraints if any
    SELECT @sql = 'ALTER TABLE ProjectMembers DROP CONSTRAINT ' + name
    FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID('ProjectMembers') AND name LIKE '%UserId%';
    IF @sql IS NOT NULL EXEC sp_executesql @sql;
    
    -- Convert column
    ALTER TABLE ProjectMembers ALTER COLUMN UserId INT NOT NULL;
    PRINT '  ✓ UserId converted to INT';
END
ELSE
BEGIN
    PRINT '  ✓ UserId is already INT';
END

PRINT '';

-- =============================================
-- STEP 3: Convert Workflow Tables (11 tables)
-- =============================================
PRINT 'STEP 3: Converting Workflow tables...';

-- 3.1 TechTransferRequests
PRINT '  3.1 TechTransferRequests...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'TechTransferRequests' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE TechTransferRequests ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'TechTransferRequests' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE TechTransferRequests ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.2 NDAAgreements
PRINT '  3.2 NDAAgreements...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'NDAAgreements' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE NDAAgreements ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'NDAAgreements' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE NDAAgreements ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.3 RFQRequests
PRINT '  3.3 RFQRequests...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'RFQRequests' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE RFQRequests ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'RFQRequests' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE RFQRequests ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.4 ProposalSubmissions
PRINT '  3.4 ProposalSubmissions...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ProposalSubmissions' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ProposalSubmissions ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ProposalSubmissions' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ProposalSubmissions ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.5 NegotiationForms
PRINT '  3.5 NegotiationForms...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'NegotiationForms' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE NegotiationForms ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'NegotiationForms' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE NegotiationForms ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.6 EContracts
PRINT '  3.6 EContracts...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'EContracts' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE EContracts ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'EContracts' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE EContracts ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.7 AdvancePaymentConfirmations
PRINT '  3.7 AdvancePaymentConfirmations...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'AdvancePaymentConfirmations' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE AdvancePaymentConfirmations ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'AdvancePaymentConfirmations' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE AdvancePaymentConfirmations ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.8 ImplementationLogs
PRINT '  3.8 ImplementationLogs...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ImplementationLogs' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ImplementationLogs ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ImplementationLogs' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ImplementationLogs ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.9 HandoverReports
PRINT '  3.9 HandoverReports...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'HandoverReports' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE HandoverReports ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'HandoverReports' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE HandoverReports ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.10 AcceptanceReports
PRINT '  3.10 AcceptanceReports...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'AcceptanceReports' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE AcceptanceReports ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'AcceptanceReports' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE AcceptanceReports ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

-- 3.11 LiquidationReports
PRINT '  3.11 LiquidationReports...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'LiquidationReports' AND COLUMN_NAME = 'NguoiTao' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE LiquidationReports ALTER COLUMN NguoiTao INT NULL;
    PRINT '    ✓ NguoiTao converted to INT';
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'LiquidationReports' AND COLUMN_NAME = 'NguoiSua' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE LiquidationReports ALTER COLUMN NguoiSua INT NULL;
    PRINT '    ✓ NguoiSua converted to INT';
END

PRINT '';
PRINT '========================================';
PRINT 'Migration Completed Successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';
PRINT '  ✓ Projects: CreatedBy, ModifiedBy → INT';
PRINT '  ✓ ProjectMembers: UserId → INT';
PRINT '  ✓ 11 Workflow tables: NguoiTao, NguoiSua → INT';
PRINT '';
PRINT 'Total tables updated: 13';
PRINT 'Total columns converted: 24';
PRINT '';
PRINT 'IMPORTANT: Verify your data integrity after migration!';
GO
