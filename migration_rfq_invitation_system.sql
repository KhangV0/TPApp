-- Create RFQInvitations table
CREATE TABLE [dbo].[RFQInvitations] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ProjectId] INT NOT NULL,
    [RFQId] INT NOT NULL,
    [SellerId] INT NOT NULL,
    [InvitedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [StatusId] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [ViewedDate] DATETIME2 NULL,
    [ResponseDate] DATETIME2 NULL,
    [Notes] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_RFQInvitations_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]),
    CONSTRAINT [FK_RFQInvitations_RFQRequests] FOREIGN KEY ([RFQId]) REFERENCES [RFQRequests]([Id]),
    CONSTRAINT [FK_RFQInvitations_Users] FOREIGN KEY ([SellerId]) REFERENCES [Users]([UserId])
);

-- Create ProjectAccessLogs table
CREATE TABLE [dbo].[ProjectAccessLogs] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ProjectId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [Action] NVARCHAR(100) NOT NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [IpAddress] NVARCHAR(50) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [AdditionalData] NVARCHAR(1000) NULL,
    CONSTRAINT [FK_ProjectAccessLogs_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]),
    CONSTRAINT [FK_ProjectAccessLogs_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
);

-- Create indexes for performance
CREATE INDEX [IX_RFQInvitations_ProjectId] ON [RFQInvitations]([ProjectId]);
CREATE INDEX [IX_RFQInvitations_SellerId] ON [RFQInvitations]([SellerId]);
CREATE INDEX [IX_RFQInvitations_RFQId] ON [RFQInvitations]([RFQId]);
CREATE INDEX [IX_ProjectAccessLogs_ProjectId] ON [ProjectAccessLogs]([ProjectId]);
CREATE INDEX [IX_ProjectAccessLogs_UserId] ON [ProjectAccessLogs]([UserId]);

PRINT 'Migration completed successfully!';
PRINT 'Created tables: RFQInvitations, ProjectAccessLogs';
