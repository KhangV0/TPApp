-- =============================================
-- MANUAL SCRIPT - Run this in SSMS
-- Populate SearchIndexContents with 200 Products
-- =============================================

USE TechMartNew;
GO

-- Step 1: Check if function exists, if not create it
IF OBJECT_ID('dbo.fnRemoveVietnameseAccents', 'FN') IS NULL
BEGIN
    EXEC('
    CREATE FUNCTION dbo.fnRemoveVietnameseAccents(@input NVARCHAR(MAX))
    RETURNS NVARCHAR(MAX)
    AS
    BEGIN
        DECLARE @output NVARCHAR(MAX) = LOWER(@input);
        
        -- Remove Vietnamese accents
        SET @output = REPLACE(@output, N''á'', ''a'');
        SET @output = REPLACE(@output, N''à'', ''a'');
        SET @output = REPLACE(@output, N''ả'', ''a'');
        SET @output = REPLACE(@output, N''ã'', ''a'');
        SET @output = REPLACE(@output, N''ạ'', ''a'');
        SET @output = REPLACE(@output, N''ă'', ''a'');
        SET @output = REPLACE(@output, N''ắ'', ''a'');
        SET @output = REPLACE(@output, N''ằ'', ''a'');
        SET @output = REPLACE(@output, N''ẳ'', ''a'');
        SET @output = REPLACE(@output, N''ẵ'', ''a'');
        SET @output = REPLACE(@output, N''ặ'', ''a'');
        SET @output = REPLACE(@output, N''â'', ''a'');
        SET @output = REPLACE(@output, N''ấ'', ''a'');
        SET @output = REPLACE(@output, N''ầ'', ''a'');
        SET @output = REPLACE(@output, N''ẩ'', ''a'');
        SET @output = REPLACE(@output, N''ẫ'', ''a'');
        SET @output = REPLACE(@output, N''ậ'', ''a'');
        SET @output = REPLACE(@output, N''đ'', ''d'');
        SET @output = REPLACE(@output, N''é'', ''e'');
        SET @output = REPLACE(@output, N''è'', ''e'');
        SET @output = REPLACE(@output, N''ẻ'', ''e'');
        SET @output = REPLACE(@output, N''ẽ'', ''e'');
        SET @output = REPLACE(@output, N''ẹ'', ''e'');
        SET @output = REPLACE(@output, N''ê'', ''e'');
        SET @output = REPLACE(@output, N''ế'', ''e'');
        SET @output = REPLACE(@output, N''ề'', ''e'');
        SET @output = REPLACE(@output, N''ể'', ''e'');
        SET @output = REPLACE(@output, N''ễ'', ''e'');
        SET @output = REPLACE(@output, N''ệ'', ''e'');
        SET @output = REPLACE(@output, N''í'', ''i'');
        SET @output = REPLACE(@output, N''ì'', ''i'');
        SET @output = REPLACE(@output, N''ỉ'', ''i'');
        SET @output = REPLACE(@output, N''ĩ'', ''i'');
        SET @output = REPLACE(@output, N''ị'', ''i'');
        SET @output = REPLACE(@output, N''ó'', ''o'');
        SET @output = REPLACE(@output, N''ò'', ''o'');
        SET @output = REPLACE(@output, N''ỏ'', ''o'');
        SET @output = REPLACE(@output, N''õ'', ''o'');
        SET @output = REPLACE(@output, N''ọ'', ''o'');
        SET @output = REPLACE(@output, N''ô'', ''o'');
        SET @output = REPLACE(@output, N''ố'', ''o'');
        SET @output = REPLACE(@output, N''ồ'', ''o'');
        SET @output = REPLACE(@output, N''ổ'', ''o'');
        SET @output = REPLACE(@output, N''ỗ'', ''o'');
        SET @output = REPLACE(@output, N''ộ'', ''o'');
        SET @output = REPLACE(@output, N''ơ'', ''o'');
        SET @output = REPLACE(@output, N''ớ'', ''o'');
        SET @output = REPLACE(@output, N''ờ'', ''o'');
        SET @output = REPLACE(@output, N''ở'', ''o'');
        SET @output = REPLACE(@output, N''ỡ'', ''o'');
        SET @output = REPLACE(@output, N''ợ'', ''o'');
        SET @output = REPLACE(@output, N''ú'', ''u'');
        SET @output = REPLACE(@output, N''ù'', ''u'');
        SET @output = REPLACE(@output, N''ủ'', ''u'');
        SET @output = REPLACE(@output, N''ũ'', ''u'');
        SET @output = REPLACE(@output, N''ụ'', ''u'');
        SET @output = REPLACE(@output, N''ư'', ''u'');
        SET @output = REPLACE(@output, N''ứ'', ''u'');
        SET @output = REPLACE(@output, N''ừ'', ''u'');
        SET @output = REPLACE(@output, N''ử'', ''u'');
        SET @output = REPLACE(@output, N''ữ'', ''u'');
        SET @output = REPLACE(@output, N''ự'', ''u'');
        SET @output = REPLACE(@output, N''ý'', ''y'');
        SET @output = REPLACE(@output, N''ỳ'', ''y'');
        SET @output = REPLACE(@output, N''ỷ'', ''y'');
        SET @output = REPLACE(@output, N''ỹ'', ''y'');
        SET @output = REPLACE(@output, N''ỵ'', ''y'');
        
        RETURN @output;
    END
    ');
    PRINT 'Function fnRemoveVietnameseAccents created.';
END
ELSE
BEGIN
    PRINT 'Function fnRemoveVietnameseAccents already exists.';
END
GO

-- Step 2: Insert 200 Products
PRINT '';
PRINT 'Inserting 200 products...';

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
    dbo.fnRemoveVietnameseAccents(REPLACE(S.Name, '-', ' ')) AS RemovedUnicode,
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

PRINT CAST(@@ROWCOUNT AS NVARCHAR(50)) + ' products inserted.';
GO

-- Step 3: Verify
PRINT '';
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

-- Step 4: Test FullText Search
PRINT '';
PRINT '==============================================';
PRINT 'Testing FullText Search';
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
PRINT 'Test 3: Search for "may do" (no accents)';
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'may do', 
    @PageNumber = 1, 
    @PageSize = 5;

PRINT '';
PRINT '==============================================';
PRINT 'DONE! SearchIndexContents populated successfully.';
PRINT '==============================================';
