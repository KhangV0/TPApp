-- =============================================
-- Script: Create Dashboard Tables (INT UserId version)
-- Description: Create Projects, ProjectMembers, and ProjectSteps tables with INT UserId
-- =============================================

-- 1. Create Projects table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Projects](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectCode] [nvarchar](50) NOT NULL,
        [ProjectName] [nvarchar](255) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [StatusId] [int] NOT NULL DEFAULT 1, -- 1=Draft, 2=Active, 3=Completed
        [CreatedBy] [int] NULL, -- INT to match Users.UserId
        [CreatedDate] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [ModifiedBy] [int] NULL, -- INT to match Users.UserId
        [ModifiedDate] [datetime2](7) NULL,
     CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY];
    
    PRINT 'Table Projects created successfully';
END
ELSE
BEGIN
    PRINT 'Table Projects already exists';
END
GO

-- 2. Create ProjectMembers table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMembers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProjectMembers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectId] [int] NOT NULL,
        [UserId] [int] NOT NULL, -- INT to match Users.UserId
        [Role] [int] NOT NULL, -- 1=Buyer, 2=Seller, 3=Consultant
        [JoinedDate] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_ProjectMembers] PRIMARY KEY CLUSTERED ([Id] ASC),
     CONSTRAINT [FK_ProjectMembers_Projects] FOREIGN KEY([ProjectId])
        REFERENCES [dbo].[Projects] ([Id])
        ON DELETE CASCADE
    ) ON [PRIMARY];
    
    -- Create index for better query performance
    CREATE NONCLUSTERED INDEX [IX_ProjectMembers_UserId] ON [dbo].[ProjectMembers]
    (
        [UserId] ASC
    );
    
    CREATE NONCLUSTERED INDEX [IX_ProjectMembers_ProjectId] ON [dbo].[ProjectMembers]
    (
        [ProjectId] ASC
    );
    
    PRINT 'Table ProjectMembers created successfully';
END
ELSE
BEGIN
    PRINT 'Table ProjectMembers already exists';
END
GO

-- 3. Create ProjectSteps table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectSteps]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProjectSteps](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectId] [int] NOT NULL,
        [StepNumber] [int] NOT NULL, -- 1 to 11
        [StepName] [nvarchar](200) NOT NULL,
        [StatusId] [int] NOT NULL DEFAULT 0, -- 0=NotStarted, 1=InProgress, 2=Completed
        [CreatedDate] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [CompletedDate] [datetime2](7) NULL,
     CONSTRAINT [PK_ProjectSteps] PRIMARY KEY CLUSTERED ([Id] ASC),
     CONSTRAINT [FK_ProjectSteps_Projects] FOREIGN KEY([ProjectId])
        REFERENCES [dbo].[Projects] ([Id])
        ON DELETE CASCADE
    ) ON [PRIMARY];
    
    -- Create index for better query performance
    CREATE NONCLUSTERED INDEX [IX_ProjectSteps_ProjectId] ON [dbo].[ProjectSteps]
    (
        [ProjectId] ASC,
        [StepNumber] ASC
    );
    
    PRINT 'Table ProjectSteps created successfully';
END
ELSE
BEGIN
    PRINT 'Table ProjectSteps already exists';
END
GO

PRINT 'All dashboard tables created/verified successfully!';
PRINT 'UserId type: INT (matches Users.UserId)';
GO
