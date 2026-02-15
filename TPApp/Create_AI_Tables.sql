-- =============================================
-- AI Semantic Supplier Matching System
-- Database Schema Creation Script
-- =============================================

USE TechMartNew;
GO

-- =============================================
-- Table: SanPhamEmbeddings
-- Purpose: Store OpenAI embeddings for products
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SanPhamEmbeddings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SanPhamEmbeddings] (
        [SanPhamId] INT NOT NULL PRIMARY KEY,
        [NCUId] INT NOT NULL,
        [Embedding] NVARCHAR(MAX) NOT NULL,
        [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_SanPhamEmbeddings_SanPhamCNTB] FOREIGN KEY ([SanPhamId]) 
            REFERENCES [dbo].[SanPhamCNTB]([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_SanPhamEmbeddings_NhaCungUng] FOREIGN KEY ([NCUId]) 
            REFERENCES [dbo].[NhaCungUng]([CungUngId])
    );
    
    PRINT 'Table SanPhamEmbeddings created successfully.';
END
ELSE
BEGIN
    PRINT 'Table SanPhamEmbeddings already exists.';
END
GO

-- =============================================
-- Index: IX_SanPhamEmbeddings_NCUId
-- Purpose: Optimize grouping by supplier
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SanPhamEmbeddings_NCUId' AND object_id = OBJECT_ID('SanPhamEmbeddings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SanPhamEmbeddings_NCUId]
    ON [dbo].[SanPhamEmbeddings] ([NCUId])
    INCLUDE ([SanPhamId], [Embedding]);
    
    PRINT 'Index IX_SanPhamEmbeddings_NCUId created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_SanPhamEmbeddings_NCUId already exists.';
END
GO

-- =============================================
-- Table: AISearchLogs
-- Purpose: Log all AI search queries for analytics
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AISearchLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AISearchLogs] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [QueryText] NVARCHAR(MAX) NOT NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ResultCount] INT NOT NULL DEFAULT 0
    );
    
    PRINT 'Table AISearchLogs created successfully.';
END
ELSE
BEGIN
    PRINT 'Table AISearchLogs already exists.';
END
GO

-- =============================================
-- Index: IX_AISearchLogs_CreatedDate
-- Purpose: Optimize date-based queries for analytics
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AISearchLogs_CreatedDate' AND object_id = OBJECT_ID('AISearchLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AISearchLogs_CreatedDate]
    ON [dbo].[AISearchLogs] ([CreatedDate] DESC);
    
    PRINT 'Index IX_AISearchLogs_CreatedDate created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_AISearchLogs_CreatedDate already exists.';
END
GO

PRINT 'AI Tables schema creation completed successfully.';
GO
