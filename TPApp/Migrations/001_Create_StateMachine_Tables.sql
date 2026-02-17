-- =============================================
-- CREATE STATE MACHINE TABLES
-- Database: TechMartNew
-- Purpose: Enterprise State Machine for 14-step workflow
-- =============================================

USE [TechMartNew]
GO

PRINT '================================================';
PRINT 'Creating State Machine Tables for Workflow';
PRINT '================================================';
PRINT '';

-- =============================================
-- TABLE 1: ProjectWorkflowStates
-- Single source of truth for workflow per project
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectWorkflowStates]'))
BEGIN
    CREATE TABLE [dbo].[ProjectWorkflowStates] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [ProjectId] INT NOT NULL,
        [CurrentStep] INT NOT NULL,
        [OverallStatus] INT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        -- Constraints
        CONSTRAINT [UQ_ProjectWorkflowStates_ProjectId] UNIQUE ([ProjectId]),
        CONSTRAINT [FK_ProjectWorkflowStates_Projects] FOREIGN KEY ([ProjectId]) 
            REFERENCES [dbo].[Projects]([Id]) ON DELETE CASCADE,
        CONSTRAINT [CK_ProjectWorkflowStates_CurrentStep] CHECK ([CurrentStep] BETWEEN 1 AND 14),
        CONSTRAINT [CK_ProjectWorkflowStates_OverallStatus] CHECK ([OverallStatus] BETWEEN 1 AND 4)
    );
    
    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_ProjectWorkflowStates_ProjectId] 
        ON [dbo].[ProjectWorkflowStates]([ProjectId]);
    
    CREATE NONCLUSTERED INDEX [IX_ProjectWorkflowStates_CurrentStep] 
        ON [dbo].[ProjectWorkflowStates]([CurrentStep]);
    
    PRINT '✓ Created table: ProjectWorkflowStates';
END
ELSE
BEGIN
    PRINT '⚠ Table already exists: ProjectWorkflowStates';
END

PRINT '';

-- =============================================
-- TABLE 2: ProjectStepStates
-- Track each step status and evidence
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectStepStates]'))
BEGIN
    CREATE TABLE [dbo].[ProjectStepStates] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [ProjectId] INT NOT NULL,
        [StepNumber] INT NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [StartedAt] DATETIME2 NULL,
        [SubmittedAt] DATETIME2 NULL,
        [CompletedAt] DATETIME2 NULL,
        [BlockedReason] NVARCHAR(500) NULL,
        [DataRefTable] NVARCHAR(100) NULL,
        [DataRefId] NVARCHAR(50) NULL,
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        -- Constraints
        CONSTRAINT [UQ_ProjectStepStates_ProjectId_StepNumber] UNIQUE ([ProjectId], [StepNumber]),
        CONSTRAINT [FK_ProjectStepStates_Projects] FOREIGN KEY ([ProjectId]) 
            REFERENCES [dbo].[Projects]([Id]) ON DELETE CASCADE,
        CONSTRAINT [CK_ProjectStepStates_StepNumber] CHECK ([StepNumber] BETWEEN 1 AND 14),
        CONSTRAINT [CK_ProjectStepStates_Status] CHECK ([Status] BETWEEN 0 AND 6)
    );
    
    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_ProjectStepStates_ProjectId] 
        ON [dbo].[ProjectStepStates]([ProjectId]);
    
    CREATE NONCLUSTERED INDEX [IX_ProjectStepStates_ProjectId_StepNumber] 
        ON [dbo].[ProjectStepStates]([ProjectId], [StepNumber]);
    
    CREATE NONCLUSTERED INDEX [IX_ProjectStepStates_Status] 
        ON [dbo].[ProjectStepStates]([Status]);
    
    PRINT '✓ Created table: ProjectStepStates';
END
ELSE
BEGIN
    PRINT '⚠ Table already exists: ProjectStepStates';
END

PRINT '';

-- =============================================
-- TABLE 3: WorkflowTransitionLogs
-- Immutable audit trail
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowTransitionLogs]'))
BEGIN
    CREATE TABLE [dbo].[WorkflowTransitionLogs] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [ProjectId] INT NOT NULL,
        [FromStep] INT NULL,
        [ToStep] INT NOT NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [ActorUserId] INT NOT NULL,
        [IpAddress] NVARCHAR(64) NULL,
        [UserAgent] NVARCHAR(300) NULL,
        [Message] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        -- Constraints
        CONSTRAINT [FK_WorkflowTransitionLogs_Projects] FOREIGN KEY ([ProjectId]) 
            REFERENCES [dbo].[Projects]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkflowTransitionLogs_Users] FOREIGN KEY ([ActorUserId]) 
            REFERENCES [dbo].[Users]([UserId]),
        CONSTRAINT [CK_WorkflowTransitionLogs_FromStep] CHECK ([FromStep] IS NULL OR [FromStep] BETWEEN 1 AND 14),
        CONSTRAINT [CK_WorkflowTransitionLogs_ToStep] CHECK ([ToStep] BETWEEN 1 AND 14)
    );
    
    -- Indexes
    CREATE NONCLUSTERED INDEX [IX_WorkflowTransitionLogs_ProjectId] 
        ON [dbo].[WorkflowTransitionLogs]([ProjectId]);
    
    CREATE NONCLUSTERED INDEX [IX_WorkflowTransitionLogs_ActorUserId] 
        ON [dbo].[WorkflowTransitionLogs]([ActorUserId]);
    
    CREATE NONCLUSTERED INDEX [IX_WorkflowTransitionLogs_CreatedAt] 
        ON [dbo].[WorkflowTransitionLogs]([CreatedAt] DESC);
    
    CREATE NONCLUSTERED INDEX [IX_WorkflowTransitionLogs_ProjectId_CreatedAt] 
        ON [dbo].[WorkflowTransitionLogs]([ProjectId], [CreatedAt] DESC);
    
    PRINT '✓ Created table: WorkflowTransitionLogs';
END
ELSE
BEGIN
    PRINT '⚠ Table already exists: WorkflowTransitionLogs';
END

PRINT '';
PRINT '================================================';
PRINT 'State Machine Tables Created Successfully!';
PRINT '================================================';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Register entities in AppDbContext.cs';
PRINT '2. Run this script in SQL Server';
PRINT '3. Verify tables created';
PRINT '4. Proceed to Phase 2 (E-Sign System)';
PRINT '';
GO
