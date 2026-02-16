-- =============================================
-- Search System Rebuild - Phase 4
-- Search Analytics Tables & Procedures
-- =============================================

USE TechMartNew;
GO

-- =============================================
-- Step 1: Create SearchQueryLog Table
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SearchQueryLog')
BEGIN
    PRINT 'Creating SearchQueryLog table...';
    
    CREATE TABLE dbo.SearchQueryLog
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        Keyword NVARCHAR(500) NOT NULL,
        NormalizedKeyword NVARCHAR(500) NOT NULL,
        ResultCount INT NOT NULL,
        SearchMode NVARCHAR(20) NOT NULL, -- 'normal' or 'ai'
        LanguageId NVARCHAR(50) NULL,
        TypeName NVARCHAR(100) NULL,
        Created DATETIME2 NOT NULL DEFAULT GETDATE(),
        UserAgent NVARCHAR(500) NULL,
        IpAddress NVARCHAR(50) NULL,
        ExecutionTimeMs INT NULL
    );

    PRINT 'SearchQueryLog table created successfully.';
END
ELSE
BEGIN
    PRINT 'SearchQueryLog table already exists.';
END
GO

-- =============================================
-- Step 2: Create Indexes on SearchQueryLog
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SearchQueryLog_Created')
BEGIN
    PRINT 'Creating index IX_SearchQueryLog_Created...';
    CREATE NONCLUSTERED INDEX IX_SearchQueryLog_Created 
    ON dbo.SearchQueryLog(Created DESC)
    INCLUDE (NormalizedKeyword, ResultCount, SearchMode);
    PRINT 'Index created successfully.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SearchQueryLog_NormalizedKeyword')
BEGIN
    PRINT 'Creating index IX_SearchQueryLog_NormalizedKeyword...';
    CREATE NONCLUSTERED INDEX IX_SearchQueryLog_NormalizedKeyword 
    ON dbo.SearchQueryLog(NormalizedKeyword)
    INCLUDE (Created, ResultCount);
    PRINT 'Index created successfully.';
END
GO

-- =============================================
-- Step 3: Create uspSearchTrending
-- =============================================
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.uspSearchTrending
    @Days INT = 7,
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate inputs
    IF @Days < 1 SET @Days = 7;
    IF @Days > 90 SET @Days = 90; -- Max 90 days
    IF @TopN < 1 SET @TopN = 10;
    IF @TopN > 100 SET @TopN = 100; -- Max 100 results

    -- Get trending searches (SQL Server 2012 compatible - no STRING_AGG)
    SELECT TOP (@TopN)
        NormalizedKeyword,
        COUNT(*) AS SearchCount,
        AVG(ResultCount) AS AvgResults,
        MAX(Created) AS LastSearched
    FROM dbo.SearchQueryLog
    WHERE Created >= DATEADD(DAY, -@Days, GETDATE())
        AND ResultCount > 0 -- Only count searches with results
    GROUP BY NormalizedKeyword
    ORDER BY COUNT(*) DESC;
END
GO

PRINT 'uspSearchTrending created successfully.';
GO

-- =============================================
-- Step 4: Create uspLogSearchQuery
-- =============================================
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.uspLogSearchQuery
    @Keyword NVARCHAR(500),
    @NormalizedKeyword NVARCHAR(500),
    @ResultCount INT,
    @SearchMode NVARCHAR(20),
    @LanguageId NVARCHAR(50) = NULL,
    @TypeName NVARCHAR(100) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @IpAddress NVARCHAR(50) = NULL,
    @ExecutionTimeMs INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SearchQueryLog
    (
        Keyword,
        NormalizedKeyword,
        ResultCount,
        SearchMode,
        LanguageId,
        TypeName,
        UserAgent,
        IpAddress,
        ExecutionTimeMs,
        Created
    )
    VALUES
    (
        @Keyword,
        @NormalizedKeyword,
        @ResultCount,
        @SearchMode,
        @LanguageId,
        @TypeName,
        @UserAgent,
        @IpAddress,
        @ExecutionTimeMs,
        GETDATE()
    );
END
GO

PRINT 'uspLogSearchQuery created successfully.';
GO

-- =============================================
-- Test Queries
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'Testing Analytics Procedures';
PRINT '=============================================';
PRINT '';

-- Insert sample data
PRINT 'Inserting sample search logs...';
EXEC dbo.uspLogSearchQuery 
    @Keyword = N'máy dò kim loại',
    @NormalizedKeyword = N'may do kim loai',
    @ResultCount = 15,
    @SearchMode = 'normal';

EXEC dbo.uspLogSearchQuery 
    @Keyword = N'máy dò kim loại',
    @NormalizedKeyword = N'may do kim loai',
    @ResultCount = 15,
    @SearchMode = 'ai';

EXEC dbo.uspLogSearchQuery 
    @Keyword = N'thiết bị đo lường',
    @NormalizedKeyword = N'thiet bi do luong',
    @ResultCount = 25,
    @SearchMode = 'normal';

PRINT 'Sample data inserted.';
PRINT '';

-- Test trending
PRINT 'Test: Get trending searches (last 7 days)';
EXEC dbo.uspSearchTrending 
    @Days = 7,
    @TopN = 10;
PRINT '';

PRINT '=============================================';
PRINT 'Phase 4 Complete!';
PRINT 'Search Analytics tables and procedures ready.';
PRINT '=============================================';
GO
