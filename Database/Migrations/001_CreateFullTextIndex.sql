-- =============================================
-- Search System Rebuild - Phase 1-2
-- FullText Catalog & Index Setup
-- =============================================

USE TechMartNew;
GO

-- =============================================
-- Step 1: Verify Primary Key
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_NAME = 'SearchIndexContents' 
    AND CONSTRAINT_TYPE = 'PRIMARY KEY'
)
BEGIN
    PRINT 'Creating Primary Key on SearchIndexContents...';
    ALTER TABLE dbo.SearchIndexContents
    ADD CONSTRAINT PK_SearchIndexContents PRIMARY KEY (Id);
    PRINT 'Primary Key created successfully.';
END
ELSE
BEGIN
    PRINT 'Primary Key already exists on SearchIndexContents.';
END
GO

-- =============================================
-- Step 2: Create FullText Catalog
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'FTC_SearchIndex')
BEGIN
    PRINT 'Creating FullText Catalog FTC_SearchIndex...';
    CREATE FULLTEXT CATALOG FTC_SearchIndex AS DEFAULT;
    PRINT 'FullText Catalog created successfully.';
END
ELSE
BEGIN
    PRINT 'FullText Catalog FTC_SearchIndex already exists.';
END
GO

-- =============================================
-- Step 3: Drop Existing FullText Index (if exists)
-- =============================================
IF EXISTS (
    SELECT 1 
    FROM sys.fulltext_indexes 
    WHERE object_id = OBJECT_ID('dbo.SearchIndexContents')
)
BEGIN
    PRINT 'Dropping existing FullText Index on SearchIndexContents...';
    DROP FULLTEXT INDEX ON dbo.SearchIndexContents;
    PRINT 'Existing FullText Index dropped.';
END
GO

-- =============================================
-- Step 4: Create FullText Index
-- =============================================
PRINT 'Creating FullText Index on SearchIndexContents...';
CREATE FULLTEXT INDEX ON dbo.SearchIndexContents
(
    Title LANGUAGE 1066,           -- Vietnamese
    RemovedUnicode LANGUAGE 1066,  -- Vietnamese (non-accented for better matching)
    Contents LANGUAGE 1066         -- Vietnamese
)
KEY INDEX PK_SearchIndexContents
ON FTC_SearchIndex
WITH (
    CHANGE_TRACKING = AUTO,
    STOPLIST = SYSTEM
);
GO

PRINT 'FullText Index created successfully.';
PRINT '';
PRINT '=============================================';
PRINT 'Phase 1-2 Complete!';
PRINT 'FullText Catalog and Index are ready.';
PRINT '=============================================';
GO

-- =============================================
-- Verification Queries
-- =============================================
PRINT '';
PRINT 'Verification:';
PRINT '-------------';

-- Check catalog
SELECT 
    name AS CatalogName,
    is_default AS IsDefault
FROM sys.fulltext_catalogs
WHERE name = 'FTC_SearchIndex';

-- Check index
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    is_enabled AS IsEnabled,
    change_tracking_state_desc AS ChangeTracking
FROM sys.fulltext_indexes
WHERE object_id = OBJECT_ID('dbo.SearchIndexContents');

-- Check indexed columns
SELECT 
    COL_NAME(fc.object_id, fc.column_id) AS ColumnName,
    l.name AS Language
FROM sys.fulltext_index_columns fc
INNER JOIN sys.fulltext_languages l ON fc.language_id = l.lcid
WHERE fc.object_id = OBJECT_ID('dbo.SearchIndexContents');
GO
