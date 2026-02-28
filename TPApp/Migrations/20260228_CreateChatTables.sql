-- ============================================================
-- Chat System Tables
-- Run this script on the Techport database
-- ============================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatConversations')
BEGIN
    CREATE TABLE dbo.ChatConversations (
        Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        ProductId       INT NULL,
        ProductType     INT NULL,
        BuyerUserId     NVARCHAR(450) NOT NULL,
        SupplierUserId  NVARCHAR(450) NOT NULL,
        ProductName     NVARCHAR(500) NULL,
        IsFromProductDetail BIT NOT NULL DEFAULT 1,
        Created         DATETIME NOT NULL DEFAULT GETUTCDATE(),
        LastMessageAt   DATETIME NULL
    );

    -- Unique: one conversation per buyer + supplier + product
    CREATE UNIQUE INDEX UX_ChatConversation_Product_Buyer_Supplier
        ON dbo.ChatConversations (ProductId, BuyerUserId, SupplierUserId);

    -- Fast lookup by user
    CREATE INDEX IX_ChatConversation_Buyer  ON dbo.ChatConversations (BuyerUserId);
    CREATE INDEX IX_ChatConversation_Supplier ON dbo.ChatConversations (SupplierUserId);

    PRINT 'Created table ChatConversations';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatMessages')
BEGIN
    CREATE TABLE dbo.ChatMessages (
        Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        ConversationId  BIGINT NOT NULL,
        SenderUserId    NVARCHAR(450) NOT NULL,
        Message         NVARCHAR(MAX) NOT NULL,
        IsRead          BIT NOT NULL DEFAULT 0,
        IsSystem        BIT NOT NULL DEFAULT 0,
        Created         DATETIME NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_ChatMessages_Conversation
            FOREIGN KEY (ConversationId)
            REFERENCES dbo.ChatConversations(Id)
            ON DELETE CASCADE
    );

    -- Fast lookup by conversation
    CREATE INDEX IX_ChatMessages_ConversationId ON dbo.ChatMessages (ConversationId);

    -- Unread count query
    CREATE INDEX IX_ChatMessages_Unread
        ON dbo.ChatMessages (ConversationId, SenderUserId, IsRead)
        WHERE IsRead = 0;

    PRINT 'Created table ChatMessages';
END
GO
