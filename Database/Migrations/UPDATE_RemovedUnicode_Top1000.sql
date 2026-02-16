-- =============================================
-- Update RemovedUnicode - TOP 1000 Records Only
-- Use fnRemoveVietnameseAccents function
-- =============================================

USE TechMartNew;
GO

PRINT 'Updating RemovedUnicode for TOP 1000 newest records...';
PRINT '';

-- Update TOP 1000 newest records
UPDATE TOP (1000) dbo.SearchIndexContents
SET RemovedUnicode = dbo.fnRemoveVietnameseAccents(Title)
WHERE RemovedUnicode IS NULL 
   OR RemovedUnicode = ''
   OR RemovedUnicode = LOWER(Title);

PRINT CAST(@@ROWCOUNT AS NVARCHAR(50)) + ' rows updated.';
PRINT '';

-- Verify
PRINT 'Sample data after update:';
SELECT TOP 10 
    Id,
    Title,
    RemovedUnicode,
    TypeName,
    Created
FROM dbo.SearchIndexContents
WHERE Title LIKE N'%ozone%' OR Title LIKE N'%OZONE%'
ORDER BY Id DESC;

PRINT '';
PRINT 'Testing search with "MAY OZONE" (no accents)...';

EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'MAY OZONE', 
    @PageNumber = 1, 
    @PageSize = 5;

PRINT '';
PRINT 'Done!';
