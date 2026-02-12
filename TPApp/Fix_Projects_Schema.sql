-- =============================================
-- Script: Fix Projects Table Schema
-- Description: Update Projects table to match Entity definition
-- =============================================

-- Check if Projects table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND type in (N'U'))
BEGIN
    PRINT 'Projects table exists. Checking schema...';
    
    -- Check if CreatedBy column exists and its type
    IF EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') 
               AND name = 'CreatedBy')
    BEGIN
        -- Get current data type
        DECLARE @CurrentType NVARCHAR(128);
        SELECT @CurrentType = t.name + 
            CASE 
                WHEN t.name IN ('nvarchar', 'varchar', 'char', 'nchar') 
                THEN '(' + CAST(c.max_length AS NVARCHAR) + ')'
                ELSE ''
            END
        FROM sys.columns c
        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'[dbo].[Projects]') 
        AND c.name = 'CreatedBy';
        
        PRINT 'Current CreatedBy type: ' + @CurrentType;
        
        -- If it's int, we need to convert to nvarchar(450)
        IF @CurrentType LIKE 'int%'
        BEGIN
            PRINT 'Converting CreatedBy from int to nvarchar(450)...';
            
            -- First, convert existing int values to string
            ALTER TABLE [dbo].[Projects] 
            ALTER COLUMN [CreatedBy] NVARCHAR(450) NULL;
            
            PRINT 'CreatedBy column converted to nvarchar(450)';
        END
        ELSE IF @CurrentType = 'nvarchar(450)'
        BEGIN
            PRINT 'CreatedBy is already nvarchar(450) - OK';
        END
        ELSE
        BEGIN
            PRINT 'CreatedBy has unexpected type: ' + @CurrentType;
            PRINT 'Converting to nvarchar(450)...';
            
            ALTER TABLE [dbo].[Projects] 
            ALTER COLUMN [CreatedBy] NVARCHAR(450) NULL;
            
            PRINT 'CreatedBy column converted to nvarchar(450)';
        END
    END
    ELSE
    BEGIN
        PRINT 'CreatedBy column does not exist. Adding...';
        
        ALTER TABLE [dbo].[Projects] 
        ADD [CreatedBy] NVARCHAR(450) NULL;
        
        PRINT 'CreatedBy column added';
    END
    
    -- Check ModifiedBy column
    IF EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') 
               AND name = 'ModifiedBy')
    BEGIN
        DECLARE @ModifiedByType NVARCHAR(128);
        SELECT @ModifiedByType = t.name + 
            CASE 
                WHEN t.name IN ('nvarchar', 'varchar', 'char', 'nchar') 
                THEN '(' + CAST(c.max_length AS NVARCHAR) + ')'
                ELSE ''
            END
        FROM sys.columns c
        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'[dbo].[Projects]') 
        AND c.name = 'ModifiedBy';
        
        IF @ModifiedByType LIKE 'int%' OR @ModifiedByType != 'nvarchar(450)'
        BEGIN
            PRINT 'Converting ModifiedBy to nvarchar(450)...';
            
            ALTER TABLE [dbo].[Projects] 
            ALTER COLUMN [ModifiedBy] NVARCHAR(450) NULL;
            
            PRINT 'ModifiedBy column converted to nvarchar(450)';
        END
    END
    ELSE
    BEGIN
        PRINT 'ModifiedBy column does not exist. Adding...';
        
        ALTER TABLE [dbo].[Projects] 
        ADD [ModifiedBy] NVARCHAR(450) NULL;
        
        PRINT 'ModifiedBy column added';
    END
    
    PRINT 'Projects table schema updated successfully!';
END
ELSE
BEGIN
    PRINT 'ERROR: Projects table does not exist!';
    PRINT 'Please run Create_Dashboard_Tables.sql first.';
END
GO
