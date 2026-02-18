-- =============================================
-- Permission System Migration
-- Created: 2026-02-18
-- Description: Create StepPermissions and ProjectConsultants tables
-- =============================================

USE TechMartNew;
GO

-- =============================================
-- 1. Create StepPermissions Table
-- =============================================
IF OBJECT_ID('dbo.StepPermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StepPermissions (
        Id INT IDENTITY(1,1) NOT NULL,
        StepNumber INT NOT NULL,
        RoleType INT NOT NULL, -- 1=Buyer, 2=Seller, 3=Consultant, 4=Admin
        CanView BIT NOT NULL DEFAULT 0,
        CanEdit BIT NOT NULL DEFAULT 0,
        CanSubmit BIT NOT NULL DEFAULT 0,
        CanApprove BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_StepPermissions PRIMARY KEY (Id),
        CONSTRAINT UX_StepPermissions UNIQUE (StepNumber, RoleType),
        CONSTRAINT CK_StepPermissions_StepNumber CHECK (StepNumber BETWEEN 1 AND 14),
        CONSTRAINT CK_StepPermissions_RoleType CHECK (RoleType BETWEEN 1 AND 4)
    );
    
    PRINT 'Created table: StepPermissions';
END
ELSE
BEGIN
    PRINT 'Table StepPermissions already exists';
END
GO

-- =============================================
-- 2. Create ProjectConsultants Table
-- =============================================
IF OBJECT_ID('dbo.ProjectConsultants', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectConsultants (
        Id INT IDENTITY(1,1) NOT NULL,
        ProjectId INT NOT NULL,
        ConsultantId INT NOT NULL,
        AssignedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        IsActive BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(500) NULL,
        CONSTRAINT PK_ProjectConsultants PRIMARY KEY (Id),
        CONSTRAINT FK_ProjectConsultants_Projects FOREIGN KEY (ProjectId) 
            REFERENCES Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectConsultants_Users FOREIGN KEY (ConsultantId) 
            REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT UX_ProjectConsultants UNIQUE (ProjectId, ConsultantId)
    );
    
    CREATE INDEX IX_ProjectConsultants_ProjectId ON dbo.ProjectConsultants(ProjectId);
    CREATE INDEX IX_ProjectConsultants_ConsultantId ON dbo.ProjectConsultants(ConsultantId);
    
    PRINT 'Created table: ProjectConsultants';
END
ELSE
BEGIN
    PRINT 'Table ProjectConsultants already exists';
END
GO

-- =============================================
-- 3. Seed Permission Matrix Data
-- =============================================
-- Clear existing data if re-running
DELETE FROM dbo.StepPermissions;
PRINT 'Cleared existing StepPermissions data';
GO

-- Legend:
-- RoleType: 1=Buyer, 2=Seller, 3=Consultant, 4=Admin
-- Permissions: CanView, CanEdit, CanSubmit, CanApprove

-- =============================================
-- Step 1: Technology Transfer Request
-- Buyer: Full CRUD, Consultant: View only, Admin: Full
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (1, 1, 1, 1, 1, 0), -- Buyer: View, Edit, Submit
    (1, 2, 0, 0, 0, 0), -- Seller: No access
    (1, 3, 1, 0, 0, 0), -- Consultant: View only
    (1, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 2: NDA Signature
-- Buyer: View + Submit (sign), Seller: View + Submit if invited
-- Consultant: View only, Admin: Full
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (2, 1, 1, 0, 1, 0), -- Buyer: View, Submit (sign NDA)
    (2, 2, 1, 0, 1, 0), -- Seller: View, Submit (requires invitation guard)
    (2, 3, 1, 0, 0, 0), -- Consultant: View only
    (2, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 3: RFQ Creation
-- Buyer: Full CRUD, Consultant: View + Edit (suggest), Admin: Full
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (3, 1, 1, 1, 1, 0), -- Buyer: View, Edit, Submit RFQ
    (3, 2, 0, 0, 0, 0), -- Seller: No access (will see via invitation)
    (3, 3, 1, 1, 0, 0), -- Consultant: View, Edit (add notes/suggestions)
    (3, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 4: Proposal Submission
-- Seller: CRUD own proposal, Buyer: View + Approve (select winner)
-- Consultant: View + Approve (technical evaluation), Admin: Full
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (4, 1, 1, 0, 0, 1), -- Buyer: View proposals, Approve (select)
    (4, 2, 1, 1, 1, 0), -- Seller: View, Edit, Submit own proposal
    (4, 3, 1, 0, 0, 1), -- Consultant: View, Approve (technical eval)
    (4, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 5: Negotiation
-- Buyer/Seller: Full CRUD, Consultant: View + Edit (comments)
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (5, 1, 1, 1, 1, 0), -- Buyer: View, Edit, Submit negotiation
    (5, 2, 1, 1, 1, 0), -- Seller: View, Edit, Submit negotiation
    (5, 3, 1, 1, 0, 0), -- Consultant: View, Edit (add comments)
    (5, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 6: Legal Review
-- Buyer/Seller: View + Approve, Consultant: View + Approve (technical)
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (6, 1, 1, 0, 0, 1), -- Buyer: View, Approve legal docs
    (6, 2, 1, 0, 0, 1), -- Seller: View, Approve legal docs
    (6, 3, 1, 0, 0, 1), -- Consultant: View, Approve (technical aspects)
    (6, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 7: Contract Signing
-- Buyer/Seller: View + Submit (e-sign), Consultant/Admin: View only
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (7, 1, 1, 0, 1, 0), -- Buyer: View, Submit (sign contract)
    (7, 2, 1, 0, 1, 0), -- Seller: View, Submit (sign contract)
    (7, 3, 1, 0, 0, 0), -- Consultant: View only
    (7, 4, 1, 0, 0, 0); -- Admin: View only (no override on legal docs)

-- =============================================
-- Step 8: Advance Payment
-- Buyer: View + Submit (confirm payment), Seller: View
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (8, 1, 1, 0, 1, 0), -- Buyer: View, Submit payment confirmation
    (8, 2, 1, 0, 0, 0), -- Seller: View only
    (8, 3, 1, 0, 0, 0), -- Consultant: View only
    (8, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 9: Pilot Test
-- Seller: View + Submit (test report), Buyer: View + Approve
-- Consultant: View + Approve (technical validation)
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (9, 1, 1, 0, 0, 1), -- Buyer: View, Approve test results
    (9, 2, 1, 1, 1, 0), -- Seller: View, Edit, Submit test report
    (9, 3, 1, 0, 0, 1), -- Consultant: View, Approve (technical validation)
    (9, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 10: Technology Handover
-- Seller: View + Submit (handover docs), Buyer: View + Approve
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (10, 1, 1, 0, 0, 1), -- Buyer: View, Approve handover
    (10, 2, 1, 1, 1, 0), -- Seller: View, Edit, Submit handover
    (10, 3, 1, 0, 0, 1), -- Consultant: View, Approve (verify completeness)
    (10, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 11: Training
-- Seller: View + Submit (training completion), Buyer: View + Approve
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (11, 1, 1, 0, 0, 1), -- Buyer: View, Approve training completion
    (11, 2, 1, 1, 1, 0), -- Seller: View, Edit, Submit training docs
    (11, 3, 1, 0, 0, 1), -- Consultant: View, Approve (quality check)
    (11, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 12: Technical Documentation
-- Seller: View + Submit (docs), Buyer: View + Approve
-- Consultant: View + Approve (technical review)
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (12, 1, 1, 0, 0, 1), -- Buyer: View, Approve documentation
    (12, 2, 1, 1, 1, 0), -- Seller: View, Edit, Submit docs
    (12, 3, 1, 0, 0, 1), -- Consultant: View, Approve (technical review)
    (12, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 13: Acceptance Report
-- Buyer: View + Submit (acceptance), Seller: View
-- Consultant: View + Approve (final verification)
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (13, 1, 1, 1, 1, 0), -- Buyer: View, Edit, Submit acceptance
    (13, 2, 1, 0, 0, 0), -- Seller: View only
    (13, 3, 1, 0, 0, 1), -- Consultant: View, Approve (final verification)
    (13, 4, 1, 1, 1, 1); -- Admin: Full access

-- =============================================
-- Step 14: Project Liquidation
-- Buyer: View + Submit (liquidation), Seller: View
-- Consultant: View + Approve, Admin: Full
-- =============================================
INSERT INTO dbo.StepPermissions (StepNumber, RoleType, CanView, CanEdit, CanSubmit, CanApprove)
VALUES
    (14, 1, 1, 1, 1, 0), -- Buyer: View, Edit, Submit liquidation
    (14, 2, 1, 0, 0, 0), -- Seller: View only
    (14, 3, 1, 0, 0, 1), -- Consultant: View, Approve (final sign-off)
    (14, 4, 1, 1, 1, 1); -- Admin: Full access

GO

-- =============================================
-- 4. Verify Seed Data
-- =============================================
PRINT '';
PRINT '==============================================';
PRINT 'Permission Matrix Summary';
PRINT '==============================================';

SELECT 
    StepNumber,
    CASE RoleType 
        WHEN 1 THEN 'Buyer'
        WHEN 2 THEN 'Seller'
        WHEN 3 THEN 'Consultant'
        WHEN 4 THEN 'Admin'
    END AS Role,
    CanView,
    CanEdit,
    CanSubmit,
    CanApprove
FROM dbo.StepPermissions
ORDER BY StepNumber, RoleType;

PRINT '';
PRINT 'Total permission records: ' + CAST((SELECT COUNT(*) FROM dbo.StepPermissions) AS VARCHAR(10));
PRINT 'Expected: 56 records (14 steps × 4 roles)';
PRINT '';
PRINT 'Migration completed successfully!';
GO
