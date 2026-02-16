-- =============================================
-- Ultra Quick Populate - 200 Products Only
-- =============================================

USE TechMartNew;
GO

SET NOCOUNT ON;

PRINT 'Ultra quick populate - 200 products only';
PRINT '';

-- Insert 200 Products
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
    REPLACE(S.Name, '-', ' '),
    dbo.fnRemoveVietnameseAccents(REPLACE(S.Name, '-', ' ')),
    ISNULL(S.MoTaNgan, ''),
    ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,''),
    s.ID,
    N'Product',
    N'http://techport.vn/2-cong-nghe-thiet-bi/1/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50)) + '.html',
    s.QuyTrinhHinhAnh,
    s.Created,
    'system',
    '1'
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

-- Quick verification
SELECT COUNT(*) AS TotalProducts FROM dbo.SearchIndexContents WHERE TypeName = N'Product';

PRINT '';
PRINT 'Sample products:';
SELECT TOP 3 Id, Title, TypeName FROM dbo.SearchIndexContents WHERE TypeName = N'Product';

PRINT '';
PRINT 'Testing search:';
EXEC dbo.uspSearchIndex_Final @Keyword = N'máy', @PageNumber = 1, @PageSize = 5;
