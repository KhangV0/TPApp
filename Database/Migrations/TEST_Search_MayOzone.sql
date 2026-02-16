-- =============================================
-- Test Search for "MÁY OZONE"
-- Debug why search is not returning results
-- =============================================

USE TechMartNew;
GO

SET NOCOUNT ON;

PRINT '==============================================';
PRINT 'Testing Search for "MÁY OZONE"';
PRINT '==============================================';
PRINT '';

-- Step 1: Check if data exists in SearchIndexContents
PRINT 'Step 1: Check if "MÁY OZONE" exists in SearchIndexContents';
PRINT '---------------------------------------------';

SELECT 
    Id,
    Title,
    TypeName,
    RefId,
    Created
FROM dbo.SearchIndexContents
WHERE Title LIKE N'%OZONE%' 
   OR Title LIKE N'%ozone%'
   OR Contents LIKE N'%OZONE%'
   OR Contents LIKE N'%ozone%';

PRINT '';

-- Step 2: Check FullText Index Status
PRINT 'Step 2: Check FullText Index Status';
PRINT '---------------------------------------------';

SELECT 
    OBJECT_NAME(object_id) AS TableName,
    is_enabled,
    change_tracking_state_desc,
    has_crawl_completed,
    crawl_type_desc,
    crawl_start_date,
    crawl_end_date
FROM sys.fulltext_indexes
WHERE object_id = OBJECT_ID('dbo.SearchIndexContents');

PRINT '';

-- Step 3: Check FullText Catalog Population Status
PRINT 'Step 3: Check FullText Catalog Status';
PRINT '---------------------------------------------';

SELECT 
    name AS CatalogName,
    is_importing
FROM sys.fulltext_catalogs
WHERE name = 'FTC_SearchIndex';

PRINT '';

-- Step 4: Test CONTAINS query directly
PRINT 'Step 4: Test CONTAINS query directly';
PRINT '---------------------------------------------';

SELECT 
    Id,
    Title,
    TypeName,
    KEY_TBL.RANK
FROM dbo.SearchIndexContents
INNER JOIN CONTAINSTABLE(dbo.SearchIndexContents, (Title, Contents), N'"MÁY" OR "OZONE"') AS KEY_TBL
    ON SearchIndexContents.Id = KEY_TBL.[KEY]
ORDER BY KEY_TBL.RANK DESC;

PRINT '';

-- Step 5: Test stored procedure
PRINT 'Step 5: Test uspSearchIndex_Final with "MÁY OZONE"';
PRINT '---------------------------------------------';

EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'MÁY OZONE', 
    @PageNumber = 1, 
    @PageSize = 10;

PRINT '';

-- Step 6: Test with single keyword
PRINT 'Step 6: Test with single keyword "OZONE"';
PRINT '---------------------------------------------';

EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'OZONE', 
    @PageNumber = 1, 
    @PageSize = 10;

PRINT '';

-- Step 7: Test with lowercase
PRINT 'Step 7: Test with lowercase "máy ozone"';
PRINT '---------------------------------------------';

EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'máy ozone', 
    @PageNumber = 1, 
    @PageSize = 10;

PRINT '';
PRINT '==============================================';
PRINT 'Diagnostic Complete';
PRINT '==============================================';
