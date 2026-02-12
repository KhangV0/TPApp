-- =============================================
-- Script: Complete Schema Fix for Dashboard
-- Description: Fix all type mismatches in Projects and ProjectMembers tables
-- Run this script to resolve InvalidCastException errors
-- =============================================

PRINT '==============================================';
PRINT 'Starting Complete Schema Fix...';
PRINT '==============================================';
PRINT '';

-- 1. Fix Projects table
PRINT '1. Fixing Projects table...';
PRINT '-------------------------------------------';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND type in (N'U'))
BEGIN
    -- Fix CreatedBy column
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND name = 'CreatedBy')
    BEGIN
        DECLARE @CreatedByType NVARCHAR(128);
        SELECT @CreatedByType = t.name
        FROM sys.columns c
        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'[dbo].[Projects]') AND c.name = 'CreatedBy';
        
        IF @CreatedByType = 'int'
        BEGIN
            PRINT '  Converting Projects.CreatedBy from INT to NVARCHAR(450)...';
            ALTER TABLE [dbo].[Projects] ALTER COLUMN [CreatedBy] NVARCHAR(450) NULL;
            PRINT '  ✓ CreatedBy converted';
        END
        ELSE
        BEGIN
            PRINT '  ✓ CreatedBy is already NVARCHAR';
        END
    END
    ELSE
    BEGIN
        PRINT '  Adding CreatedBy column...';
        ALTER TABLE [dbo].[Projects] ADD [CreatedBy] NVARCHAR(450) NULL;
        PRINT '  ✓ CreatedBy added';
    END
    
    -- Fix ModifiedBy column
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND name = 'ModifiedBy')
    BEGIN
        DECLARE @ModifiedByType NVARCHAR(128);
        SELECT @ModifiedByType = t.name
        FROM sys.columns c
        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'[dbo].[Projects]') AND c.name = 'ModifiedBy';
        
        IF @ModifiedByType = 'int'
        BEGIN
            PRINT '  Converting Projects.ModifiedBy from INT to NVARCHAR(450)...';
            ALTER TABLE [dbo].[Projects] ALTER COLUMN [ModifiedBy] NVARCHAR(450) NULL;
            PRINT '  ✓ ModifiedBy converted';
        END
        ELSE
        BEGIN
            PRINT '  ✓ ModifiedBy is already NVARCHAR';
        END
    END
    ELSE
    BEGIN
        PRINT '  Adding ModifiedBy column...';
        ALTER TABLE [dbo].[Projects] ADD [ModifiedBy] NVARCHAR(450) NULL;
        PRINT '  ✓ ModifiedBy added';
    END
    
    PRINT '✓ Projects table fixed';
END
ELSE
BEGIN
    PRINT '✗ ERROR: Projects table does not exist!';
END

PRINT '';
PRINT '2. Fixing ProjectMembers table...';
PRINT '-------------------------------------------';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMembers]') AND type in (N'U'))
BEGIN
    -- Fix UserId column
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMembers]') AND name = 'UserId')
    BEGIN
        DECLARE @UserIdType NVARCHAR(128);
        SELECT @UserIdType = t.name
        FROM sys.columns c
        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'[dbo].[ProjectMembers]') AND c.name = 'UserId';
        
        IF @UserIdType = 'int'
        BEGIN
            PRINT '  Converting ProjectMembers.UserId from INT to NVARCHAR(450)...';
            
            -- Check if there's existing data
            DECLARE @RowCount INT;
            SELECT @RowCount = COUNT(*) FROM ProjectMembers;
            
            IF @RowCount > 0
            BEGIN
                PRINT '  Found ' + CAST(@RowCount AS NVARCHAR) + ' existing rows. Converting data...';
                
                -- Add temporary column
                ALTER TABLE [dbo].[ProjectMembers] ADD [UserId_Temp] NVARCHAR(450) NULL;
                
                -- Copy data with conversion
                UPDATE [dbo].[ProjectMembers] SET [UserId_Temp] = CAST([UserId] AS NVARCHAR(450));
                
                -- Drop old column
                ALTER TABLE [dbo].[ProjectMembers] DROP COLUMN [UserId];
                
                -- Rename temp column
                EXEC sp_rename 'ProjectMembers.UserId_Temp', 'UserId', 'COLUMN';
                
                -- Make it NOT NULL
                ALTER TABLE [dbo].[ProjectMembers] ALTER COLUMN [UserId] NVARCHAR(450) NOT NULL;
                
                PRINT '  ✓ Data converted successfully';
            END
            ELSE
            BEGIN
                ALTER TABLE [dbo].[ProjectMembers] ALTER COLUMN [UserId] NVARCHAR(450) NOT NULL;
                PRINT '  ✓ UserId converted (no data to migrate)';
            END
        END
        ELSE
        BEGIN
            PRINT '  ✓ UserId is already NVARCHAR';
        END
    END
    ELSE
    BEGIN
        PRINT '  ✗ ERROR: UserId column not found!';
    END
    
    PRINT '✓ ProjectMembers table fixed';
END
ELSE
BEGIN
    PRINT '✗ ERROR: ProjectMembers table does not exist!';
END

PRINT '';
PRINT '==============================================';
PRINT 'Schema Fix Complete!';
PRINT '==============================================';
PRINT '';
PRINT 'Summary:';
PRINT '  - Projects.CreatedBy: NVARCHAR(450)';
PRINT '  - Projects.ModifiedBy: NVARCHAR(450)';
PRINT '  - ProjectMembers.UserId: NVARCHAR(450)';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Restart your application (dotnet run)';
PRINT '  2. Test /Project/Details/1';
PRINT '  3. Test /Dashboard/Index';
PRINT '';
GO
