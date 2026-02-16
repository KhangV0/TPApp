-- =============================================
-- Quick Test: Search for "ozone"
-- =============================================

USE TechMartNew;
GO

PRINT 'Testing search for "ozone"...';
PRINT '';

-- Test stored procedure
EXEC dbo.uspSearchIndex_Final 
    @Keyword = N'ozone', 
    @PageNumber = 1, 
    @PageSize = 10;

PRINT '';
PRINT 'Total count:';

-- Get total count
DECLARE @TotalCount INT;

;WITH SearchKeywords AS (
    SELECT CAST('<x>' + REPLACE(N'ozone', ' ', '</x><x>') + '</x>' AS XML) AS KeywordXml
),
KeywordList AS (
    SELECT LTRIM(RTRIM(T.c.value('.', 'NVARCHAR(100)'))) AS Keyword
    FROM SearchKeywords
    CROSS APPLY KeywordXml.nodes('/x') T(c)
    WHERE LEN(LTRIM(RTRIM(T.c.value('.', 'NVARCHAR(100)')))) > 0
),
SearchQuery AS (
    SELECT STRING_AGG('"' + Keyword + '"', ' AND ') AS FTSQuery
    FROM KeywordList
)
SELECT @TotalCount = COUNT(DISTINCT si.Id)
FROM dbo.SearchIndexContents si
CROSS JOIN SearchQuery sq
WHERE CONTAINS((si.Title, si.Contents), sq.FTSQuery);

SELECT @TotalCount AS TotalResults;
