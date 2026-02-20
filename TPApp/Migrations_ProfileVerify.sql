-- ============================================================
-- Profile Verification Migration
-- Run against: TechMartNew
-- ============================================================

-- 1. Add verification columns to Users table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PhoneVerified')
    ALTER TABLE Users ADD PhoneVerified BIT NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'EmailVerified')
    ALTER TABLE Users ADD EmailVerified BIT NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'AccountTypeId')
    ALTER TABLE Users ADD AccountTypeId INT NOT NULL DEFAULT 1;  -- 1=Cá nhân, 2=Doanh nghiệp
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'VerificationLevel')
    ALTER TABLE Users ADD VerificationLevel INT NOT NULL DEFAULT 0; -- 0=None,1=Phone,2=Email,3=Full

-- 2. UserOtps – temporary OTP codes
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserOtps')
BEGIN
    CREATE TABLE UserOtps (
        Id        INT IDENTITY PRIMARY KEY,
        UserId    INT NOT NULL,
        OtpType   INT NOT NULL,         -- 1=Email, 2=Phone
        OtpCode   NVARCHAR(10) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        IsUsed    BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_UserOtps_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
    PRINT 'Created UserOtps';
END
GO

-- 3. UserVerificationDocs – CCCD / Business License uploads
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserVerificationDocs')
BEGIN
    CREATE TABLE UserVerificationDocs (
        Id           INT IDENTITY PRIMARY KEY,
        UserId       INT NOT NULL,
        DocType      INT NOT NULL,       -- 1=CCCD_Front, 2=CCCD_Back, 3=BusinessLicense
        FilePath     NVARCHAR(500) NOT NULL,
        FileName     NVARCHAR(200) NOT NULL,
        FileSize     BIGINT NOT NULL DEFAULT 0,
        UploadedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ReviewStatus INT NOT NULL DEFAULT 0, -- 0=Pending,1=Approved,2=Rejected
        ReviewNote   NVARCHAR(500),
        CONSTRAINT FK_UserVerificationDocs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
    PRINT 'Created UserVerificationDocs';
END
GO

-- Verification
SELECT name, column_id FROM sys.columns WHERE object_id = OBJECT_ID('Users')
AND name IN ('PhoneVerified','EmailVerified','AccountTypeId','VerificationLevel');
SELECT COUNT(*) AS OtpTable FROM sys.tables WHERE name='UserOtps';
SELECT COUNT(*) AS DocTable FROM sys.tables WHERE name='UserVerificationDocs';
GO
