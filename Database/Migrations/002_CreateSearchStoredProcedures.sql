-- =============================================
-- Search System Rebuild - Phase 3
-- Stored Procedures
-- =============================================

USE TechMartNew;
GO

-- =============================================
-- uspSearchIndex_Final
-- Main search stored procedure using FullText
-- =============================================
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.uspSearchIndex_Final
    @Keyword NVARCHAR(500),
    @LanguageId NVARCHAR(50) = NULL,
    @TypeName NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate inputs
    IF @Keyword IS NULL OR LTRIM(RTRIM(@Keyword)) = ''
    BEGIN
        RAISERROR('Keyword cannot be empty', 16, 1);
        RETURN;
    END

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 100 SET @PageSize = 100; -- Max 100 results per page

    -- Parse keyword into AND logic (SQL Server 2012 compatible)
    -- "word1 word2 word3" → '"word1" AND "word2" AND "word3"'
    DECLARE @SearchTerm NVARCHAR(1000);
    DECLARE @XML XML;
    
    -- Convert space-separated words to XML for splitting
    SET @XML = CAST('<r><w>' + REPLACE(@Keyword, ' ', '</w><w>') + '</w></r>' AS XML);
    
    -- Build AND search term
    SELECT @SearchTerm = STUFF((
        SELECT ' AND "' + LTRIM(RTRIM(T.c.value('.', 'NVARCHAR(500)'))) + '"'
        FROM @XML.nodes('/r/w') T(c)
        WHERE LTRIM(RTRIM(T.c.value('.', 'NVARCHAR(500)'))) <> ''
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 5, ''); -- Remove leading ' AND '

    -- If parsing failed, use original keyword
    IF @SearchTerm IS NULL OR @SearchTerm = ''
        SET @SearchTerm = '"' + @Keyword + '"';

    -- Calculate offset for pagination
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Main search query with FullText ranking
    SELECT 
        s.Id,
        s.Title,
        s.RemovedUnicode,
        s.Description,
        s.Contents,
        s.FutherIndex,
        s.RefId,
        s.ImgPreview,
        s.TypeName,
        s.MimeType,
        s.URL,
        s.AbsPath,
        s.Created,
        s.Modified,
        s.IndexTime,
        s.Noted,
        s.Creator,
        s.LanguageId,
        s.SiteId,
        KEY_TBL.RANK AS FullTextRank
    FROM dbo.SearchIndexContents s
    INNER JOIN CONTAINSTABLE(
        dbo.SearchIndexContents, 
        (Title, RemovedUnicode, Contents), 
        @SearchTerm
    ) AS KEY_TBL ON s.Id = KEY_TBL.[KEY]
    WHERE 
        (@LanguageId IS NULL OR s.LanguageId = @LanguageId)
        AND (@TypeName IS NULL OR s.TypeName = @TypeName)
    ORDER BY KEY_TBL.RANK DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Return total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM dbo.SearchIndexContents s
    INNER JOIN CONTAINSTABLE(
        dbo.SearchIndexContents, 
        (Title, RemovedUnicode, Contents), 
        @SearchTerm
    ) AS KEY_TBL ON s.Id = KEY_TBL.[KEY]
    WHERE 
        (@LanguageId IS NULL OR s.LanguageId = @LanguageId)
        AND (@TypeName IS NULL OR s.TypeName = @TypeName);
END
GO

PRINT 'uspSearchIndex_Final created successfully.';
GO

-- =============================================
-- uspSearchSuggest
-- Autocomplete suggestions using prefix search
-- =============================================
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.uspSearchSuggest
    @Prefix NVARCHAR(100),
    @LanguageId NVARCHAR(50) = NULL,
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate inputs
    IF @Prefix IS NULL OR LTRIM(RTRIM(@Prefix)) = ''
    BEGIN
        RAISERROR('Prefix cannot be empty', 16, 1);
        RETURN;
    END

    IF @TopN < 1 SET @TopN = 10;
    IF @TopN > 50 SET @TopN = 50; -- Max 50 suggestions

    -- Create prefix search term
    DECLARE @SearchTerm NVARCHAR(200) = '"' + @Prefix + '*"';

    -- Get suggestions from Title only (faster than searching all fields)
    SELECT TOP (@TopN)
        s.Id,
        s.Title,
        s.TypeName,
        s.URL,
        s.ImgPreview,
        KEY_TBL.RANK
    FROM dbo.SearchIndexContents s
    INNER JOIN CONTAINSTABLE(
        dbo.SearchIndexContents, 
        Title, 
        @SearchTerm
    ) AS KEY_TBL ON s.Id = KEY_TBL.[KEY]
    WHERE 
        (@LanguageId IS NULL OR s.LanguageId = @LanguageId)
        AND s.Title IS NOT NULL
        AND s.Title <> ''
    ORDER BY KEY_TBL.RANK DESC;
END
GO

PRINT 'uspSearchSuggest created successfully.';
GO

-- =============================================
-- Test Queries
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'Testing Stored Procedures';
PRINT '=============================================';
PRINT '';

-- Test uspSearchIndex_Final
PRINT 'Test 1: Search for "máy dò kim loại"';
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'máy dò kim loại',
    @PageNumber = 1,
    @PageSize = 10;
PRINT '';

-- Test uspSearchSuggest
PRINT 'Test 2: Autocomplete for "máy"';
EXEC dbo.uspSearchSuggest 
    @Prefix = N'máy',
    @TopN = 5;
PRINT '';

PRINT '=============================================';
PRINT 'Phase 3 Complete!';
PRINT 'Stored Procedures are ready.';
PRINT '=============================================';
GO
