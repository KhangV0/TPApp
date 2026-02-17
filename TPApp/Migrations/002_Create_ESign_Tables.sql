-- =============================================
-- CREATE E-SIGN SYSTEM TABLES
-- Database: TechMartNew
-- Purpose: Electronic signature system for NDA and contracts
-- =============================================

USE [TechMartNew]
GO

PRINT '================================================';
PRINT 'Creating E-Sign System Tables';
PRINT '================================================';
PRINT '';

-- =============================================
-- TABLE 1: ESignDocuments
-- Electronic documents requiring signatures
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ESignDocuments]'))
BEGIN
    CREATE TABLE [dbo].[ESignDocuments] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [ProjectId] INT NOT NULL,
        [DocType] INT NOT NULL,
        [DocumentName] NVARCHAR(200) NOT NULL,
        [FilePath] NVARCHAR(500) NULL,
        [FileHash] NVARCHAR(64) NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [SignedAt] DATETIME2 NULL,
        [CreatedBy] INT NULL,
        
        -- Constraints
        CONSTRAINT [FK_ESignDocuments_Projects] FOREIGN KEY ([ProjectId]) 
            REFERENCES [dbo].[Projects]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ESignDocuments_Users] FOREIGN KEY ([CreatedBy]) 
            REFERENCES [dbo].[Users]([UserId]),
        CONSTRAINT [CK_ESignDocuments_DocType] CHECK ([DocType] BETWEEN 1 AND 10),
        CONSTRAINT [CK_ESignDocuments_Status] CHECK ([Status] BETWEEN 0 AND 3)
    );
    
    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_ESignDocuments_ProjectId] 
        ON [dbo].[ESignDocuments]([ProjectId]);
    
    CREATE NONCLUSTERED INDEX [IX_ESignDocuments_ProjectId_DocType] 
        ON [dbo].[ESignDocuments]([ProjectId], [DocType]);
    
    CREATE NONCLUSTERED INDEX [IX_ESignDocuments_Status] 
        ON [dbo].[ESignDocuments]([Status]);
    
    PRINT '✓ Created table: ESignDocuments';
END
ELSE
BEGIN
    PRINT '⚠ Table already exists: ESignDocuments';
END

PRINT '';

-- =============================================
-- TABLE 2: ESignSignatures
-- Individual signatures on documents
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ESignSignatures]'))
BEGIN
    CREATE TABLE [dbo].[ESignSignatures] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [DocumentId] BIGINT NOT NULL,
        [SignerUserId] INT NOT NULL,
        [SignerRole] NVARCHAR(50) NOT NULL,
        [SignatureHash] NVARCHAR(64) NULL,
        [OtpCodeHash] NVARCHAR(100) NULL,
        [OtpSentAt] DATETIME2 NULL,
        [OtpVerifiedAt] DATETIME2 NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [SignedAt] DATETIME2 NULL,
        [IpAddress] NVARCHAR(64) NULL,
        [UserAgent] NVARCHAR(300) NULL,
        
        -- Constraints
        CONSTRAINT [FK_ESignSignatures_Documents] FOREIGN KEY ([DocumentId]) 
            REFERENCES [dbo].[ESignDocuments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ESignSignatures_Users] FOREIGN KEY ([SignerUserId]) 
            REFERENCES [dbo].[Users]([UserId]),
        CONSTRAINT [CK_ESignSignatures_Status] CHECK ([Status] BETWEEN 0 AND 2),
        CONSTRAINT [UQ_ESignSignatures_DocumentId_SignerUserId] UNIQUE ([DocumentId], [SignerUserId])
    );
    
    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_ESignSignatures_DocumentId] 
        ON [dbo].[ESignSignatures]([DocumentId]);
    
    CREATE NONCLUSTERED INDEX [IX_ESignSignatures_SignerUserId] 
        ON [dbo].[ESignSignatures]([SignerUserId]);
    
    CREATE NONCLUSTERED INDEX [IX_ESignSignatures_Status] 
        ON [dbo].[ESignSignatures]([Status]);
    
    PRINT '✓ Created table: ESignSignatures';
END
ELSE
BEGIN
    PRINT '⚠ Table already exists: ESignSignatures';
END

PRINT '';

-- =============================================
-- TABLE 3: ESignAuditLogs
-- Immutable audit trail for E-Sign activities
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ESignAuditLogs]'))
BEGIN
    CREATE TABLE [dbo].[ESignAuditLogs] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [DocumentId] BIGINT NOT NULL,
        [UserId] INT NOT NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [IpAddress] NVARCHAR(64) NULL,
        [UserAgent] NVARCHAR(300) NULL,
        [Details] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        -- Constraints
        CONSTRAINT [FK_ESignAuditLogs_Documents] FOREIGN KEY ([DocumentId]) 
            REFERENCES [dbo].[ESignDocuments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ESignAuditLogs_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([UserId])
    );
    
    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_ESignAuditLogs_DocumentId] 
        ON [dbo].[ESignAuditLogs]([DocumentId]);
    
    CREATE NONCLUSTERED INDEX [IX_ESignAuditLogs_UserId] 
        ON [dbo].[ESignAuditLogs]([UserId]);
    
    CREATE NONCLUSTERED INDEX [IX_ESignAuditLogs_CreatedAt] 
        ON [dbo].[ESignAuditLogs]([CreatedAt] DESC);
    
    CREATE NONCLUSTERED INDEX [IX_ESignAuditLogs_DocumentId_CreatedAt] 
        ON [dbo].[ESignAuditLogs]([DocumentId], [CreatedAt] DESC);
    
    PRINT '✓ Created table: ESignAuditLogs';
END
ELSE
BEGIN
    PRINT '⚠ Table already exists: ESignAuditLogs';
END

PRINT '';
PRINT '================================================';
PRINT 'E-Sign System Tables Created Successfully!';
PRINT '================================================';
PRINT '';
PRINT 'Tables created:';
PRINT '  1. ESignDocuments - Electronic documents';
PRINT '  2. ESignSignatures - Signature records with OTP';
PRINT '  3. ESignAuditLogs - Audit trail';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Register entities in AppDbContext.cs';
PRINT '2. Run this script in SQL Server';
PRINT '3. Verify tables created';
PRINT '4. Proceed to Phase 3 (Application Layer)';
PRINT '';
GO
