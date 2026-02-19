-- =============================================
-- Step 6+7 Full Migration: Contract & Digital Signing
-- Database: TechMartNew
-- Date: 2026-02-19
-- =============================================

USE [TechMartNew]
GO

-- ─── 1. ProjectContracts ──────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProjectContracts')
BEGIN
    CREATE TABLE [dbo].[ProjectContracts] (
        [Id]               INT            IDENTITY(1,1) NOT NULL,
        [ProjectId]        INT            NOT NULL,
        [VersionNumber]    INT            NOT NULL DEFAULT 1,
        [SourceType]       INT            NOT NULL DEFAULT 1,  -- 1=AutoGenerate, 2=Upload
        [TemplateCode]     NVARCHAR(100)  NULL,
        [Title]            NVARCHAR(300)  NOT NULL,
        [StatusId]         INT            NOT NULL DEFAULT 0,  -- ContractStatus enum
        [HtmlContent]      NVARCHAR(MAX)  NULL,
        [OriginalFilePath] NVARCHAR(500)  NULL,
        [OriginalFileName] NVARCHAR(260)  NULL,
        [SignedFilePath]   NVARCHAR(500)  NULL,
        [SignedFileName]   NVARCHAR(260)  NULL,
        [Sha256Original]   NVARCHAR(128)  NULL,
        [Sha256Signed]     NVARCHAR(128)  NULL,
        [ReadyToSignAt]    DATETIME2      NULL,
        [IsActive]         BIT            NOT NULL DEFAULT 0,
        [CreatedBy]        INT            NULL,
        [CreatedDate]      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedBy]       INT            NULL,
        [ModifiedDate]     DATETIME2      NULL,
        [ArchivedAt]       DATETIME2      NULL,
        [Note]             NVARCHAR(1000) NULL,
        CONSTRAINT [PK_ProjectContracts] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_ProjectContracts_ProjectId_IsActive]
        ON [dbo].[ProjectContracts] ([ProjectId], [IsActive])
        INCLUDE ([StatusId], [VersionNumber]);

    PRINT 'ProjectContracts created OK';
END
ELSE PRINT 'ProjectContracts already exists';
GO

-- ─── 2. ContractApprovals ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractApprovals')
BEGIN
    CREATE TABLE [dbo].[ContractApprovals] (
        [Id]          INT            IDENTITY(1,1) NOT NULL,
        [ContractId]  INT            NOT NULL,
        [UserId]      INT            NOT NULL,
        [Role]        INT            NOT NULL,  -- 1=Buyer, 2=Seller, 3=Consultant
        [StatusId]    INT            NOT NULL DEFAULT 0, -- 0=Pending, 1=Approved, 2=Rejected
        [Comment]     NVARCHAR(MAX)  NULL,
        [DecisionAt]  DATETIME2      NULL,
        [IPAddress]   NVARCHAR(100)  NULL,
        [UserAgent]   NVARCHAR(400)  NULL,
        [CreatedDate] DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ContractApprovals] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_ContractApprovals_ContractId_Role]
        ON [dbo].[ContractApprovals] ([ContractId], [Role]);

    PRINT 'ContractApprovals created OK';
END
ELSE PRINT 'ContractApprovals already exists';
GO

-- ─── 3. ContractSignatureRequests ───────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractSignatureRequests')
BEGIN
    CREATE TABLE [dbo].[ContractSignatureRequests] (
        [Id]             INT            IDENTITY(1,1) NOT NULL,
        [ContractId]     INT            NOT NULL,
        [UserId]         INT            NOT NULL,
        [Role]           INT            NOT NULL, -- 1=Buyer, 2=Seller
        [SignatureType]  INT            NOT NULL, -- ContractSignatureType enum
        [Provider]       NVARCHAR(50)   NULL,
        [StatusId]       INT            NOT NULL DEFAULT 0, -- ContractSignatureStatus enum
        [RequestRef]     NVARCHAR(200)  NULL,
        [ChallengeRef]   NVARCHAR(200)  NULL,
        [CallbackSecret] NVARCHAR(200)  NULL,
        [ErrorCode]      NVARCHAR(100)  NULL,
        [ErrorMessage]   NVARCHAR(1000) NULL,
        [CreatedDate]    DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate]    DATETIME2      NULL,
        CONSTRAINT [PK_ContractSignatureRequests] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_ContractSignatureRequests_ContractId_Role]
        ON [dbo].[ContractSignatureRequests] ([ContractId], [Role]);

    PRINT 'ContractSignatureRequests created OK';
END
ELSE PRINT 'ContractSignatureRequests already exists';
GO

-- ─── 4. ContractSignatures ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractSignatures')
BEGIN
    CREATE TABLE [dbo].[ContractSignatures] (
        [Id]                  INT            IDENTITY(1,1) NOT NULL,
        [ContractId]          INT            NOT NULL,
        [SignatureRequestId]  INT            NOT NULL,
        [UserId]              INT            NOT NULL,
        [Role]                INT            NOT NULL,
        [SignatureType]       INT            NOT NULL,
        [Provider]            NVARCHAR(50)   NULL,
        [CertificateSerial]   NVARCHAR(200)  NULL,
        [CertificateSubject]  NVARCHAR(500)  NULL,
        [CertificateIssuer]   NVARCHAR(500)  NULL,
        [SignedHash]          NVARCHAR(128)  NULL,
        [SignedAt]            DATETIME2      NULL,
        [TimeStampToken]      NVARCHAR(MAX)  NULL,
        [VerificationStatus]  INT            NOT NULL DEFAULT 0, -- 0=Unknown,1=Valid,2=Invalid
        [IPAddress]           NVARCHAR(100)  NULL,
        [UserAgent]           NVARCHAR(400)  NULL,
        [RawProviderPayload]  NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_ContractSignatures] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_ContractSignatures_ContractId_Role]
        ON [dbo].[ContractSignatures] ([ContractId], [Role]);

    PRINT 'ContractSignatures created OK';
END
ELSE PRINT 'ContractSignatures already exists';
GO

-- ─── 5. ContractAuditLogs ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractAuditLogs')
BEGIN
    CREATE TABLE [dbo].[ContractAuditLogs] (
        [Id]          BIGINT         IDENTITY(1,1) NOT NULL,
        [EntityName]  NVARCHAR(100)  NOT NULL,
        [EntityId]    NVARCHAR(50)   NOT NULL,
        [Action]      NVARCHAR(50)   NOT NULL,
        [DataJson]    NVARCHAR(MAX)  NULL,
        [ActorUserId] INT            NULL,
        [IPAddress]   NVARCHAR(100)  NULL,
        [CreatedDate] DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ContractAuditLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_ContractAuditLogs_EntityName_EntityId]
        ON [dbo].[ContractAuditLogs] ([EntityName], [EntityId]);

    PRINT 'ContractAuditLogs created OK';
END
ELSE PRINT 'ContractAuditLogs already exists';
GO

-- ─── 6. Seed SYS_PARAMETERS for signing providers ──────────────────────────
DECLARE @params TABLE (Name NVARCHAR(100), Val NVARCHAR(500), Description NVARCHAR(300))
INSERT INTO @params VALUES
    ('SIGNING_PROVIDER_DEFAULT',     'VNPT',                          N'CA provider mặc định'),
    ('SIGNING_VNPT_API_BASE',        '',                              N'VNPT API base URL'),
    ('SIGNING_VNPT_API_KEY',         '',                              N'VNPT API key (secret)'),
    ('SIGNING_FPT_API_BASE',         '',                              N'FPT eSign API base URL'),
    ('SIGNING_FPT_API_KEY',          '',                              N'FPT eSign API key'),
    ('SIGNING_VIETTEL_API_BASE',     '',                              N'Viettel CA API base URL'),
    ('SIGNING_VIETTEL_API_KEY',      '',                              N'Viettel CA API key'),
    ('ESIGN_OTP_PROVIDER',           'FPT',                           N'OTP eSigning provider'),
    ('ESIGN_OTP_TTL_SECONDS',        '300',                           N'OTP expiry (giây)'),
    ('CONTRACT_REVIEW_DEADLINE_HRS', '72',                            N'Thời hạn review hợp đồng (giờ)'),
    ('CONTRACT_ALLOWED_EXTS',        '.pdf,.docx',                    N'Phần mở rộng file hợp lệ'),
    ('CONTRACT_MAX_FILE_MB',         '25',                            N'Kích thước tối đa (MB)'),
    ('CONTRACT_STORAGE_ROOT',        'wwwroot/uploads/contracts',     N'Thư mục lưu hợp đồng'),
    ('CONTRACT_TEMPLATE_ROOT',       'Templates/Contracts',           N'Thư mục template')

INSERT INTO [dbo].[SYS_PARAMETERS] (Name, Val, Description, Domain, Activated)
SELECT p.Name, p.Val, p.Description, 'CONTRACT', 1
FROM   @params p
WHERE  NOT EXISTS (
    SELECT 1 FROM [dbo].[SYS_PARAMETERS] WHERE Name = p.Name
)

PRINT 'SYS_PARAMETERS seeded OK';
GO

-- ─── Verification ───────────────────────────────────────────────────────────
SELECT name, create_date FROM sys.tables
WHERE  name IN ('ProjectContracts','ContractApprovals','ContractSignatureRequests','ContractSignatures','ContractAuditLogs')
ORDER  BY create_date;

SELECT Name, Val
FROM   [dbo].[SYS_PARAMETERS]
WHERE  Name LIKE 'SIGNING_%' OR Name LIKE 'CONTRACT_%' OR Name LIKE 'ESIGN_%'
ORDER  BY Name;

PRINT '=== Step 6+7 Migration Complete ===';
GO
