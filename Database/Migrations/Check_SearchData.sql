-- =============================================
-- Check SearchIndexContents Data
-- =============================================

USE TechMartNew;
GO

PRINT 'Checking SearchIndexContents table...';
PRINT '';

-- Check total records
SELECT COUNT(*) AS TotalRecords FROM dbo.SearchIndexContents;

-- Check records with "may" keyword
SELECT COUNT(*) AS RecordsWithMay 
FROM dbo.SearchIndexContents 
WHERE Title LIKE N'%máy%' 
   OR Title LIKE N'%may%' 
   OR RemovedUnicode LIKE '%may%'
   OR Contents LIKE N'%máy%'
   OR Contents LIKE N'%may%';

-- Show sample records
SELECT TOP 10 
    Id, 
    Title, 
    TypeName, 
    RefId,
    Created
FROM dbo.SearchIndexContents 
WHERE Title IS NOT NULL AND Title <> ''
ORDER BY Created DESC;

PRINT '';
PRINT 'Testing FullText search for "máy dò kim loại"...';
PRINT '';

-- Test FullText search
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'máy dò kim loại', 
    @PageNumber = 1, 
    @PageSize = 10;

PRINT '';
PRINT 'Testing FullText search for "may do kim loai" (no accents)...';
PRINT '';

-- Test without accents
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'may do kim loai', 
    @PageNumber = 1, 
    @PageSize = 10;
