-- =============================================
-- Quick Populate - Products Only
-- Fast script to populate SearchIndexContents with Products
-- =============================================

USE TechMartNew;
GO

SET NOCOUNT ON;

PRINT 'Quick populate - Products only';
PRINT '';

-- Insert Products into SearchIndexContents
INSERT INTO dbo.SearchIndexContents (
    ImgPreview,
    Title,
    RemovedUnicode,
    [Description],
    Contents,
    RefId,
    TypeName,
    URL,
    Created,
    Creator,
    LanguageId
)
SELECT TOP 1000
    s.QuyTrinhHinhAnh AS ImgPreview,
    REPLACE(S.Name, '-', ' ') AS Title,
    dbo.fnRemoveVietnameseAccents(REPLACE(S.Name, '-', ' ') + ' ' + ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') + ' ' + ISNULL(S.UuDiem,'')) AS RemovedUnicode,
    ISNULL(S.MoTaNgan, '') AS [Description],
    ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') + ' ' + ISNULL(S.UuDiem,'') AS Contents,
    s.ID AS RefId,
    N'Product' AS TypeName,
    N'http://techport.vn/2-cong-nghe-thiet-bi/1/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50)) + '.html' AS URL,
    s.Created,
    'system' AS Creator,
    '1' AS LanguageId
FROM dbo.SanPhamCNTB s
LEFT JOIN dbo.SearchIndexContents si 
    ON si.TypeName = N'Product' 
    AND si.RefId = s.ID
WHERE s.StatusId = 3 
    AND s.LanguageId = 1
    AND si.Id IS NULL -- Only insert if not exists
ORDER BY s.Created DESC;

PRINT CAST(@@ROWCOUNT AS NVARCHAR(50)) + ' products inserted.';
PRINT '';

-- Verify
PRINT 'Verification:';
SELECT COUNT(*) AS TotalProducts FROM dbo.SearchIndexContents WHERE TypeName = N'Product';

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

PRINT '';
PRINT 'Testing search for "máy dò kim loại":';
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'máy dò kim loại', 
    @PageNumber = 1, 
    @PageSize = 5;
