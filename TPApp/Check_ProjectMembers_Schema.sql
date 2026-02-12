-- =============================================
-- Script: Check and Fix ProjectMembers Schema
-- Description: Verify and update ProjectMembers.UserId column type
-- =============================================

PRINT '==============================================';
PRINT 'Checking ProjectMembers table schema...';
PRINT '==============================================';

-- Check if ProjectMembers table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMembers]') AND type in (N'U'))
BEGIN
    PRINT 'ProjectMembers table exists.';
    
    -- Check UserId column type
    DECLARE @UserIdType NVARCHAR(128);
    DECLARE @MaxLength INT;
    
    SELECT 
        @UserIdType = t.name,
        @MaxLength = c.max_length
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID(N'[dbo].[ProjectMembers]') 
    AND c.name = 'UserId';
    
    IF @UserIdType IS NOT NULL
    BEGIN
        DECLARE @FullType NVARCHAR(200) = @UserIdType;
        
        IF @UserIdType IN ('nvarchar', 'varchar', 'char', 'nchar')
        BEGIN
            SET @FullType = @UserIdType + '(' + CAST(@MaxLength AS NVARCHAR) + ')';
        END
        
        PRINT 'Current UserId type: ' + @FullType;
        
        -- Check what it should be (nvarchar(450) to match Entity)
        IF @UserIdType = 'int'
        BEGIN
            PRINT '';
            PRINT 'WARNING: UserId is currently INT but Entity expects NVARCHAR(450)';
            PRINT 'This will cause InvalidCastException errors!';
            PRINT '';
            PRINT 'Converting UserId from INT to NVARCHAR(450)...';
            
            -- Convert existing int values to string
            -- First, we need to convert the data
            ALTER TABLE [dbo].[ProjectMembers] 
            ALTER COLUMN [UserId] NVARCHAR(450) NOT NULL;
            
            PRINT 'SUCCESS: UserId converted to NVARCHAR(450)';
        END
        ELSE IF @UserIdType = 'nvarchar' AND @MaxLength = 900 -- 900 bytes = 450 chars for nvarchar
        BEGIN
            PRINT 'SUCCESS: UserId is already NVARCHAR(450) - Schema is correct!';
        END
        ELSE
        BEGIN
            PRINT 'WARNING: UserId has unexpected type: ' + @FullType;
            PRINT 'Converting to NVARCHAR(450)...';
            
            ALTER TABLE [dbo].[ProjectMembers] 
            ALTER COLUMN [UserId] NVARCHAR(450) NOT NULL;
            
            PRINT 'SUCCESS: UserId converted to NVARCHAR(450)';
        END
    END
    ELSE
    BEGIN
        PRINT 'ERROR: UserId column not found in ProjectMembers table!';
    END
    
    PRINT '';
    PRINT 'Checking sample data...';
    
    -- Show sample data
    IF EXISTS (SELECT * FROM ProjectMembers)
    BEGIN
        SELECT TOP 5 
            Id, 
            ProjectId, 
            UserId, 
            Role,
            JoinedDate
        FROM ProjectMembers
        ORDER BY Id;
        
        PRINT 'Sample data shown above.';
    END
    ELSE
    BEGIN
        PRINT 'No data in ProjectMembers table yet.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: ProjectMembers table does not exist!';
    PRINT 'Please run Create_Dashboard_Tables.sql first.';
END

PRINT '';
PRINT '==============================================';
PRINT 'Schema check complete.';
PRINT '==============================================';
GO
