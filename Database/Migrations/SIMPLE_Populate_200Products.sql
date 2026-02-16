-- =============================================
-- SIMPLE SCRIPT - No Accent Removal
-- Populate SearchIndexContents with 200 Products
-- Search theo tiếng Việt có dấu
-- =============================================

USE TechMartNew;
GO

SET NOCOUNT ON;

PRINT 'Inserting 200 products (Vietnamese with accents only)...';
PRINT '';

-- Insert 200 Products WITHOUT RemovedUnicode processing
INSERT INTO dbo.SearchIndexContents (
    Title,
    RemovedUnicode,
    [Description],
    Contents,
    RefId,
    TypeName,
    URL,
    ImgPreview,
    Created,
    Creator,
    LanguageId
)
SELECT TOP 200
    REPLACE(S.Name, '-', ' ') AS Title,
    '' AS RemovedUnicode,  -- Empty for now, will populate later if needed
    ISNULL(S.MoTaNgan, '') AS [Description],
    ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') AS Contents,
    s.ID AS RefId,
    N'Product' AS TypeName,
    N'http://techport.vn/2-cong-nghe-thiet-bi/1/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50)) + '.html' AS URL,
    s.QuyTrinhHinhAnh AS ImgPreview,
    s.Created,
    'system' AS Creator,
    '1' AS LanguageId
FROM dbo.SanPhamCNTB s
LEFT JOIN dbo.SearchIndexContents si 
    ON si.TypeName = N'Product' AND si.RefId = s.ID
WHERE s.StatusId = 3 
    AND s.LanguageId = 1
    AND si.Id IS NULL
ORDER BY s.Created DESC;

DECLARE @inserted INT = @@ROWCOUNT;
PRINT CAST(@inserted AS NVARCHAR(50)) + ' products inserted.';
PRINT '';

-- Verify
PRINT '==============================================';
PRINT 'Verification';
PRINT '==============================================';

SELECT COUNT(*) AS TotalProducts 
FROM dbo.SearchIndexContents 
WHERE TypeName = N'Product';

PRINT '';
PRINT 'Sample products:';
SELECT TOP 5 
    Id,
    Title,
    TypeName,
    RefId
FROM dbo.SearchIndexContents 
WHERE TypeName = N'Product'
ORDER BY Created DESC;

-- Test FullText Search (Vietnamese with accents)
PRINT '';
PRINT '==============================================';
PRINT 'Testing FullText Search (Vietnamese accents)';
PRINT '==============================================';
PRINT '';

PRINT 'Test 1: Search for "máy dò"';
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'máy dò', 
    @PageNumber = 1, 
    @PageSize = 5;

PRINT '';
PRINT 'Test 2: Search for "thiết bị"';
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'thiết bị', 
    @PageNumber = 1, 
    @PageSize = 5;

PRINT '';
PRINT 'Test 3: Search for "cảm biến"';
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'cảm biến', 
    @PageNumber = 1, 
    @PageSize = 5;

PRINT '';
PRINT '==============================================';
PRINT 'DONE! Data populated successfully.';
PRINT 'Note: Search chỉ hoạt động với tiếng Việt có dấu';
PRINT '==============================================';
