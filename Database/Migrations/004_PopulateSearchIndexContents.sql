-- =============================================
-- Search System Rebuild - Phase 9
-- Update uspMappingDataIndex for SearchIndexContents
-- =============================================

USE TechMartNew;
GO

-- =============================================
-- Vietnamese Accent Removal Function
-- =============================================
IF OBJECT_ID('dbo.fnRemoveVietnameseAccents', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fnRemoveVietnameseAccents;
GO

CREATE FUNCTION dbo.fnRemoveVietnameseAccents(@input NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @output NVARCHAR(MAX) = @input;
    
    -- Convert to lowercase first
    SET @output = LOWER(@output);
    
    -- Remove Vietnamese accents
    SET @output = REPLACE(@output, N'á', 'a');
    SET @output = REPLACE(@output, N'à', 'a');
    SET @output = REPLACE(@output, N'ả', 'a');
    SET @output = REPLACE(@output, N'ã', 'a');
    SET @output = REPLACE(@output, N'ạ', 'a');
    SET @output = REPLACE(@output, N'ă', 'a');
    SET @output = REPLACE(@output, N'ắ', 'a');
    SET @output = REPLACE(@output, N'ằ', 'a');
    SET @output = REPLACE(@output, N'ẳ', 'a');
    SET @output = REPLACE(@output, N'ẵ', 'a');
    SET @output = REPLACE(@output, N'ặ', 'a');
    SET @output = REPLACE(@output, N'â', 'a');
    SET @output = REPLACE(@output, N'ấ', 'a');
    SET @output = REPLACE(@output, N'ầ', 'a');
    SET @output = REPLACE(@output, N'ẩ', 'a');
    SET @output = REPLACE(@output, N'ẫ', 'a');
    SET @output = REPLACE(@output, N'ậ', 'a');
    
    SET @output = REPLACE(@output, N'đ', 'd');
    
    SET @output = REPLACE(@output, N'é', 'e');
    SET @output = REPLACE(@output, N'è', 'e');
    SET @output = REPLACE(@output, N'ẻ', 'e');
    SET @output = REPLACE(@output, N'ẽ', 'e');
    SET @output = REPLACE(@output, N'ẹ', 'e');
    SET @output = REPLACE(@output, N'ê', 'e');
    SET @output = REPLACE(@output, N'ế', 'e');
    SET @output = REPLACE(@output, N'ề', 'e');
    SET @output = REPLACE(@output, N'ể', 'e');
    SET @output = REPLACE(@output, N'ễ', 'e');
    SET @output = REPLACE(@output, N'ệ', 'e');
    
    SET @output = REPLACE(@output, N'í', 'i');
    SET @output = REPLACE(@output, N'ì', 'i');
    SET @output = REPLACE(@output, N'ỉ', 'i');
    SET @output = REPLACE(@output, N'ĩ', 'i');
    SET @output = REPLACE(@output, N'ị', 'i');
    
    SET @output = REPLACE(@output, N'ó', 'o');
    SET @output = REPLACE(@output, N'ò', 'o');
    SET @output = REPLACE(@output, N'ỏ', 'o');
    SET @output = REPLACE(@output, N'õ', 'o');
    SET @output = REPLACE(@output, N'ọ', 'o');
    SET @output = REPLACE(@output, N'ô', 'o');
    SET @output = REPLACE(@output, N'ố', 'o');
    SET @output = REPLACE(@output, N'ồ', 'o');
    SET @output = REPLACE(@output, N'ổ', 'o');
    SET @output = REPLACE(@output, N'ỗ', 'o');
    SET @output = REPLACE(@output, N'ộ', 'o');
    SET @output = REPLACE(@output, N'ơ', 'o');
    SET @output = REPLACE(@output, N'ớ', 'o');
    SET @output = REPLACE(@output, N'ờ', 'o');
    SET @output = REPLACE(@output, N'ở', 'o');
    SET @output = REPLACE(@output, N'ỡ', 'o');
    SET @output = REPLACE(@output, N'ợ', 'o');
    
    SET @output = REPLACE(@output, N'ú', 'u');
    SET @output = REPLACE(@output, N'ù', 'u');
    SET @output = REPLACE(@output, N'ủ', 'u');
    SET @output = REPLACE(@output, N'ũ', 'u');
    SET @output = REPLACE(@output, N'ụ', 'u');
    SET @output = REPLACE(@output, N'ư', 'u');
    SET @output = REPLACE(@output, N'ứ', 'u');
    SET @output = REPLACE(@output, N'ừ', 'u');
    SET @output = REPLACE(@output, N'ử', 'u');
    SET @output = REPLACE(@output, N'ữ', 'u');
    SET @output = REPLACE(@output, N'ự', 'u');
    
    SET @output = REPLACE(@output, N'ý', 'y');
    SET @output = REPLACE(@output, N'ỳ', 'y');
    SET @output = REPLACE(@output, N'ỷ', 'y');
    SET @output = REPLACE(@output, N'ỹ', 'y');
    SET @output = REPLACE(@output, N'ỵ', 'y');
    
    RETURN @output;
END
GO

PRINT 'fnRemoveVietnameseAccents function created successfully.';
GO

-- =============================================
-- Updated uspMappingDataIndex
-- Populates SearchIndexContents table
-- =============================================
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[uspMappingDataIndex] 
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SPCNTP_URL NVARCHAR(100) = N'http://techport.vn/2-cong-nghe-thiet-bi/1/';
    DECLARE @DVTV_URL NVARCHAR(100) = N'http://techport.vn/8-dich-vu-tu-van/';
    DECLARE @TKDT_URL NVARCHAR(100) = N'http://techport.vn/11-tim-kiem-doi-tac/';
    DECLARE @Techport NVARCHAR(100) = N'http://techport.vn/';
    DECLARE @DVCU_URL NVARCHAR(100) = N'http://techport.vn/8-dich-vu-cung-ung/';

    -- Aggregate all data sources
    ;WITH DataSource AS (
        -- Sản phẩm CNTB
        SELECT  
            s.ID AS RefId,
            REPLACE(S.Name, '-', ' ') AS Title,
            S.MoTaNgan AS [Description],
            (ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') + ' ' + ISNULL(S.UuDiem,'')) AS Contents,
            s.QuyTrinhHinhAnh AS ImgPreview,
            @SPCNTP_URL + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50)) + '.html' AS URL,
            N'Product' AS TypeName,
            s.Created,
            '1' AS LanguageId
        FROM dbo.SanPhamCNTB S
        WHERE s.StatusId = 3 AND s.LanguageId = 1
        
        UNION ALL
        
        -- Nhà tư vấn
        SELECT  
            s.TuVanId AS RefId,
            S.FullName AS Title,
            s.HocHam AS [Description],
            '' AS Contents,
            s.HinhDaiDien AS ImgPreview,
            @DVTV_URL + s.QueryString + '-' + CAST(s.TuVanId AS NVARCHAR(50)) + '.html' AS URL,
            N'Consultant' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.NhaTuVan S
        WHERE s.StatusId = 3 AND s.LanguageId = 1
        
        UNION ALL
        
        -- Tìm kiếm đối tác
        SELECT 
            s.TimDoiTacId AS RefId,
            S.TenSanPham AS Title,
            s.MoTa AS [Description],
            '' AS Contents,
            s.HinhDaiDien AS ImgPreview,
            @TKDT_URL + s.QueryString + '-' + CAST(s.TimDoiTacId AS NVARCHAR(50)) + '.html' AS URL,
            N'Partner' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.TimKiemDoiTac S
        WHERE s.StatusId = 3 AND s.LanguageId = 1
        
        UNION ALL
        
        -- Nhà cung ứng
        SELECT 
            s.CungUngId AS RefId,
            S.FullName AS Title,
            s.DiaChi AS [Description],
            (ISNULL(Phone,'') + ' ' + ISNULL(Email,'') + ' ' + ISNULL(Fax,'') + ' ' + ISNULL(Website,'') + ' ' + ISNULL(ChucNangChinh,'')) AS Contents,
            '' AS ImgPreview,
            @DVCU_URL + s.QueryString + '-' + CAST(s.CungUngId AS NVARCHAR(50)) + '.html' AS URL,
            N'Supplier' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.NhaCungUng S
        WHERE s.LanguageId = 1 AND s.StatusId = 3
        
        UNION ALL
        
        -- Tin tức
        SELECT 
            s.Id AS RefId,
            S.Title,
            s.[Description],
            s.Contents,
            s.[Image] AS ImgPreview,
            @Techport + CAST(s.MenuId AS NVARCHAR(50)) + '/' + s.QueryString + '-' + CAST(s.Id AS NVARCHAR(50)) + '.html' AS URL,
            N'News' AS TypeName,
            s.Created,
            '1' AS LanguageId
        FROM dbo.Contents S
        WHERE s.LanguageId = 1 AND s.StatusId = 3
        
        UNION ALL
        
        -- Chuyên gia
        SELECT 
            s.id_chuyengia AS RefId,
            S.[ho_ten] AS Title,
            [ten_donvi] AS [Description],
            (ISNULL(S.[linhvuc],'') + ' ' + ISNULL(S.[chuyennganh],'') + ' ' + ISNULL(S.[quatrinhcongtac],'') + ' ' + ISNULL(S.[quatrinhdaotao],'') + ' ' + ISNULL(S.[congtrinhnghiencuu],'') + ' ' + ISNULL(S.[detai],'') + ' ' + ISNULL(S.[ngoaingu],'')) AS Contents,
            '' AS ImgPreview,
            @Techport AS URL,
            N'Expert' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.ChuyenGia S
        
        UNION ALL
        
        -- Doanh nghiệp
        SELECT 
            s.id_doanhnghiepkhcn AS RefId,
            S.ten_doanhnghiep AS Title,
            (ISNULL(dia_chi,'') + ' Phone: ' + ISNULL(dien_thoai,'') + ' Fax: ' + ISNULL([fax],'')) AS [Description],
            ISNULL(S.sanpham_chungnhan,'') AS Contents,
            '' AS ImgPreview,
            @Techport AS URL,
            N'Enterprise' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.LienKetDoanhNghiep S
        
        UNION ALL
        
        -- Phòng thí nghiệm
        SELECT 
            s.id_ptn AS RefId,
            S.ten_tv AS Title,
            (ISNULL([ten_ta],'') + N'<br> Cơ quan: ' + ISNULL(coquan_chuquan,'') + N'<br> Phụ trách: ' + ISNULL(phu_trach,'') + N'<br> Đại diện: ' + ISNULL(dai_dien,'')) AS [Description],
            (ISNULL(S.dactrung_hoatdong,'') + ' ' + ISNULL(S.linhvucthunghiem,'') + ' ' + ISNULL(S.phuongphapthuchuyeu,'') + ' ' + ISNULL(S.vatlieusanpham,'') + ' ' + ISNULL(S.thietbithunghiem,'')) AS Contents,
            '' AS ImgPreview,
            @Techport AS URL,
            N'Laboratory' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.LienKetPhongThiNghiem S
        
        UNION ALL
        
        -- Tổ chức
        SELECT 
            s.id_tochuckhcn AS RefId,
            ten_tv AS Title,
            (ISNULL(dia_chi,'') + ' Phone: ' + ISNULL(dien_thoai,'') + ' Fax: ' + ISNULL([fax],'')) AS [Description],
            ISNULL(S.linhvuc_hoatdong,'') AS Contents,
            '' AS ImgPreview,
            @Techport AS URL,
            N'Organization' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.LienKetToChuc S
        
        UNION ALL
        
        -- Tài sản trí tuệ
        SELECT 
            s.id AS RefId,
            ten AS Title,
            ISNULL(mo_ta,'') AS [Description],
            (ISNULL(dang,'') + ' ' + ISNULL(loai,'') + ' ' + ISNULL(tinhtrangkhaithac,'') + ' ' + ISNULL(tinhtrangungdung,'') + ' ' + ISNULL(noidungungdung,'') + ' ' + ISNULL(nam,'') + ' ' + ISNULL(nhiemvu,'') + ' ' + ISNULL(coquanchutri,'') + ' ' + ISNULL(chunhiem,'') + ' ' + ISNULL(thongtinlienhe,'') + ' ' + ISNULL(nganh,'') + ' ' + ISNULL(chuongtrinh,'') + ' ' + ISNULL(linhvuc,'')) AS Contents,
            '' AS ImgPreview,
            @Techport AS URL,
            N'IntellectualProperty' AS TypeName,
            GETDATE() AS Created,
            '1' AS LanguageId
        FROM dbo.LienKetTaiSanTriTue S
    )
    
    -- Insert new records into SearchIndexContents
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
    SELECT 
        DS.ImgPreview,
        DS.Title,
        dbo.fnRemoveVietnameseAccents(DS.Title + ' ' + ISNULL(DS.Contents, '')) AS RemovedUnicode,
        ISNULL(DS.[Description], '') AS [Description],
        ISNULL(DS.Contents, '') AS Contents,
        DS.RefId,
        DS.TypeName,
        DS.URL,
        DS.Created,
        'system' AS Creator,
        DS.LanguageId
    FROM DataSource DS
    LEFT JOIN dbo.SearchIndexContents SI 
        ON DS.TypeName = SI.TypeName 
        AND DS.RefId = SI.RefId
    WHERE SI.Id IS NULL; -- Only insert if not exists

    PRINT 'Data populated into SearchIndexContents successfully.';
    PRINT CAST(@@ROWCOUNT AS NVARCHAR(50)) + ' records inserted.';
END
GO

PRINT 'uspMappingDataIndex created successfully.';
GO

-- =============================================
-- Execute the procedure to populate data
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'Populating SearchIndexContents...';
PRINT '=============================================';
PRINT '';

EXEC dbo.uspMappingDataIndex;

PRINT '';
PRINT '=============================================';
PRINT 'Verification';
PRINT '=============================================';

-- Show statistics
SELECT 
    TypeName,
    COUNT(*) AS RecordCount
FROM dbo.SearchIndexContents
GROUP BY TypeName
ORDER BY RecordCount DESC;

PRINT '';
SELECT COUNT(*) AS TotalRecords FROM dbo.SearchIndexContents;

PRINT '';
PRINT 'Sample records:';
SELECT TOP 5 
    Id,
    Title,
    TypeName,
    RefId,
    Created
FROM dbo.SearchIndexContents
ORDER BY Created DESC;
