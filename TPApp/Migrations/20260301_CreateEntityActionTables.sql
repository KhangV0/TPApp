-- ============================================================
-- Generic Entity Rating + Action Log + View Counter System
-- Run on TechMartNew database
-- ============================================================

-- 1) EntityRatings
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityRatings')
BEGIN
    CREATE TABLE dbo.EntityRatings (
        Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
        EntityId    INT NOT NULL,
        EntityType  NVARCHAR(100) NOT NULL,
        UserId      NVARCHAR(450) NOT NULL,
        Stars       INT NOT NULL CHECK (Stars BETWEEN 1 AND 5),
        Title       NVARCHAR(200) NULL,
        Comment     NVARCHAR(MAX) NULL,
        StatusId    INT NOT NULL DEFAULT 1,
        Created     DATETIME NOT NULL DEFAULT GETUTCDATE(),
        Modified    DATETIME NULL
    );

    CREATE UNIQUE INDEX UX_EntityRatings_User_Entity
        ON dbo.EntityRatings(UserId, EntityType, EntityId);

    CREATE INDEX IX_EntityRatings_Entity
        ON dbo.EntityRatings(EntityType, EntityId, StatusId);

    PRINT 'Created table EntityRatings';
END
GO

-- 2) EntityActionLogs
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityActionLogs')
BEGIN
    CREATE TABLE dbo.EntityActionLogs (
        Id           BIGINT IDENTITY(1,1) PRIMARY KEY,
        EntityId     INT NOT NULL,
        EntityType   NVARCHAR(100) NOT NULL,
        UserId       NVARCHAR(450) NULL,
        ActionType   NVARCHAR(50) NOT NULL,
        MetadataJson NVARCHAR(MAX) NULL,
        Created      DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_EntityActionLogs_Entity
        ON dbo.EntityActionLogs(EntityType, EntityId, ActionType);

    PRINT 'Created table EntityActionLogs';
END
GO

-- 3) EntityViewCounters (fallback for entities without Viewed column)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityViewCounters')
BEGIN
    CREATE TABLE dbo.EntityViewCounters (
        EntityId    INT NOT NULL,
        EntityType  NVARCHAR(100) NOT NULL,
        ViewCount   INT NOT NULL DEFAULT 0,
        PRIMARY KEY (EntityType, EntityId)
    );

    PRINT 'Created table EntityViewCounters';
END
GO
