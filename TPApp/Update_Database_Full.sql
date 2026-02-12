-- 1. Create ProjectSteps table if not exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectSteps]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProjectSteps](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectId] [int] NOT NULL,
        [StepNumber] [int] NOT NULL,
        [StepName] [nvarchar](200) NOT NULL,
        [StatusId] [int] NOT NULL DEFAULT 0,
        [CreatedDate] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [CompletedDate] [datetime2](7) NULL,
     CONSTRAINT [PK_ProjectSteps] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY];
END
GO

-- 2. Add StatusId to Projects table if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND name = 'StatusId')
BEGIN
    ALTER TABLE [dbo].[Projects] ADD [StatusId] [int] NOT NULL DEFAULT 1;
END
GO

-- 3. Add ProjectCode to Projects table if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND name = 'ProjectCode')
BEGIN
    ALTER TABLE [dbo].[Projects] ADD [ProjectCode] [nvarchar](50) NOT NULL DEFAULT '';
END
GO

-- 4. Rename DuAnId to ProjectId in TechTransferRequests
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TechTransferRequests]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'TechTransferRequests.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 5. Rename DuAnId to ProjectId in NDAAgreements
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NDAAgreements]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'NDAAgreements.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 6. Rename DuAnId to ProjectId in RFQRequests
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RFQRequests]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'RFQRequests.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 7. Rename DuAnId to ProjectId in ProposalSubmissions
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProposalSubmissions]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'ProposalSubmissions.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 8. Rename DuAnId to ProjectId in NegotiationForms
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[NegotiationForms]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'NegotiationForms.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 9. Rename DuAnId to ProjectId in EContracts
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EContracts]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'EContracts.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 10. Rename DuAnId to ProjectId in AdvancePaymentConfirmations
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AdvancePaymentConfirmations]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'AdvancePaymentConfirmations.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 11. Rename DuAnId to ProjectId in ImplementationLogs
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ImplementationLogs]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'ImplementationLogs.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 12. Rename DuAnId to ProjectId in HandoverReports
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HandoverReports]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'HandoverReports.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 13. Rename DuAnId to ProjectId in AcceptanceReports
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AcceptanceReports]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'AcceptanceReports.DuAnId', 'ProjectId', 'COLUMN';
END
GO

-- 14. Rename DuAnId to ProjectId in LiquidationReports
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LiquidationReports]') AND name = 'DuAnId')
BEGIN
    EXEC sp_rename 'LiquidationReports.DuAnId', 'ProjectId', 'COLUMN';
END
GO
