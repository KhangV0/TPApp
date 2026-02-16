-- =============================================
-- Fix SearchIndexContents Table
-- Set Id column as IDENTITY (auto-increment)
-- =============================================

USE TechMartNew;
GO

PRINT 'Fixing SearchIndexContents table...';
PRINT '';

-- Step 1: Check current table structure
PRINT 'Current table structure:';
EXEC sp_help 'SearchIndexContents';
PRINT '';

-- Step 2: Create new table with IDENTITY column
PRINT 'Creating new table with IDENTITY...';

-- Drop temp table if exists
IF OBJECT_ID('dbo.SearchIndexContents_New', 'U') IS NOT NULL
    DROP TABLE dbo.SearchIndexContents_New;

CREATE TABLE dbo.SearchIndexContents_New (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(500) NULL,
    RemovedUnicode NVARCHAR(500) NULL,
    [Description] NVARCHAR(MAX) NULL,
    Contents NVARCHAR(MAX) NULL,
    FutherIndex NVARCHAR(500) NULL,
    RefId BIGINT NULL,
    ImgPreview NVARCHAR(500) NULL,
    TypeName NVARCHAR(100) NULL,
    MimeType NVARCHAR(100) NULL,
    URL NVARCHAR(500) NULL,
    AbsPath NVARCHAR(500) NULL,
    Created DATETIME2 NULL,
    Modified DATETIME2 NULL,
    IndexTime DATETIME2 NULL,
    Noted NVARCHAR(MAX) NULL,
    Creator NVARCHAR(100) NULL,
    LanguageId NVARCHAR(50) NULL,
    SiteId NVARCHAR(50) NULL
);

PRINT 'New table created with IDENTITY column.';
PRINT '';

-- Step 3: Copy existing data (if any)
PRINT 'Copying existing data...';

DECLARE @maxId BIGINT = 0;

-- Get max Id from old table
SELECT @maxId = ISNULL(MAX(Id), 0) FROM dbo.SearchIndexContents;
PRINT 'Max Id in old table: ' + CAST(@maxId AS NVARCHAR(50));

IF @maxId > 0
BEGIN
    -- Copy data with existing Ids
    SET IDENTITY_INSERT dbo.SearchIndexContents_New ON;

    INSERT INTO dbo.SearchIndexContents_New (
        Id, Title, RemovedUnicode, [Description], Contents, FutherIndex,
        RefId, ImgPreview, TypeName, MimeType, URL, AbsPath,
        Created, Modified, IndexTime, Noted, Creator, LanguageId, SiteId
    )
    SELECT 
        Id, Title, RemovedUnicode, [Description], Contents, FutherIndex,
        RefId, ImgPreview, TypeName, MimeType, URL, AbsPath,
        Created, Modified, IndexTime, Noted, Creator, LanguageId, SiteId
    FROM dbo.SearchIndexContents;

    SET IDENTITY_INSERT dbo.SearchIndexContents_New OFF;
    
    PRINT CAST(@@ROWCOUNT AS NVARCHAR(50)) + ' rows copied.';
    
    -- Reseed IDENTITY to continue from max Id
    DBCC CHECKIDENT ('SearchIndexContents_New', RESEED, @maxId);
    PRINT 'IDENTITY reseeded to: ' + CAST(@maxId AS NVARCHAR(50));
END
ELSE
BEGIN
    PRINT 'No existing data to copy.';
END

PRINT '';

-- Step 4: Drop old table and rename new table
PRINT 'Replacing old table...';

-- Drop FullText index first if exists
IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.SearchIndexContents'))
BEGIN
    PRINT 'Dropping FullText index...';
    DROP FULLTEXT INDEX ON dbo.SearchIndexContents;
END

-- Drop old table
DROP TABLE dbo.SearchIndexContents;

-- Rename new table
EXEC sp_rename 'dbo.SearchIndexContents_New', 'SearchIndexContents';

PRINT 'Table replaced successfully.';
PRINT '';

-- Step 5: Recreate FullText index
PRINT 'Recreating FullText index...';

CREATE FULLTEXT INDEX ON dbo.SearchIndexContents (
    Title LANGUAGE 1066,
    RemovedUnicode LANGUAGE 1066,
    Contents LANGUAGE 1066
)
KEY INDEX PK__SearchIn__3214EC07 ON FTC_SearchIndex
WITH CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM;

PRINT 'FullText index recreated.';
PRINT '';

-- Step 6: Verify
PRINT '==============================================';
PRINT 'Verification';
PRINT '==============================================';

PRINT 'Table structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID('SearchIndexContents'), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SearchIndexContents'
    AND COLUMN_NAME = 'Id';

PRINT '';
PRINT 'Record count:';
SELECT COUNT(*) AS TotalRecords FROM dbo.SearchIndexContents;

PRINT '';
PRINT 'Current IDENTITY value:';
SELECT IDENT_CURRENT('SearchIndexContents') AS CurrentIdentity;

PRINT '';
PRINT 'Sample data:';
SELECT TOP 5 Id, Title, TypeName, RefId 
FROM dbo.SearchIndexContents 
ORDER BY Id DESC;

PRINT '';
PRINT '==============================================';
PRINT 'DONE! Table fixed with IDENTITY column.';
PRINT 'All existing data preserved.';
PRINT 'Now you can run the populate script.';
PRINT '==============================================';
