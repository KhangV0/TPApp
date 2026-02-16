-- =============================================
-- Update RemovedUnicode with Accent Removal
-- Use fnRemoveVietnameseAccents function
-- =============================================

USE TechMartNew;
GO

PRINT 'Updating RemovedUnicode column with accent removal...';
PRINT '';

-- Update in batches to avoid long-running transaction
DECLARE @BatchSize INT = 100;
DECLARE @RowsAffected INT = 1;
DECLARE @TotalUpdated INT = 0;

WHILE @RowsAffected > 0
BEGIN
    UPDATE TOP (@BatchSize) dbo.SearchIndexContents
    SET RemovedUnicode = dbo.fnRemoveVietnameseAccents(Title)
    WHERE RemovedUnicode IS NULL 
       OR RemovedUnicode = ''
       OR RemovedUnicode = LOWER(Title); -- Also update rows that only have lowercase but still have accents

    SET @RowsAffected = @@ROWCOUNT;
    SET @TotalUpdated = @TotalUpdated + @RowsAffected;
    
    PRINT 'Updated ' + CAST(@RowsAffected AS NVARCHAR(10)) + ' rows...';
END

PRINT '';
PRINT 'Total updated: ' + CAST(@TotalUpdated AS NVARCHAR(50)) + ' rows.';
PRINT '';

-- Verify
PRINT 'Sample data after update:';
SELECT TOP 10 
    Id,
    Title,
    RemovedUnicode,
    TypeName
FROM dbo.SearchIndexContents
WHERE Title LIKE N'%ozone%' OR Title LIKE N'%OZONE%'
ORDER BY Id;

PRINT '';
PRINT 'Testing search with "MAY OZONE" (no accents)...';

EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'MAY OZONE', 
    @PageNumber = 1, 
    @PageSize = 5;

PRINT '';
PRINT 'Testing search with "may ozone" (lowercase, no accents)...';

EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'may ozone', 
    @PageNumber = 1, 
    @PageSize = 5;

PRINT '';
PRINT 'Done!';
