-- =============================================
-- Script: Insert Sample Dashboard Data
-- Description: Create sample projects, members, and steps for testing Dashboard
-- =============================================

-- Get a test user (replace with actual username)
DECLARE @UserId NVARCHAR(450); -- String to match Entity definition
DECLARE @UserIdInt INT;

-- Try to get the first user from Users table
SELECT TOP 1 @UserIdInt = UserId, @UserId = CAST(UserId AS NVARCHAR(450))
FROM Users
ORDER BY UserId;

IF @UserId IS NULL
BEGIN
    PRINT 'ERROR: No users found in Users table. Please create a user first.';
    RETURN;
END

PRINT 'Using UserId: ' + @UserId;

-- 1. Create sample projects
DECLARE @Project1Id INT, @Project2Id INT, @Project3Id INT;

-- Project 1: Completed project
IF NOT EXISTS (SELECT * FROM Projects WHERE ProjectCode = 'PRJ-2024-001')
BEGIN
    INSERT INTO Projects (ProjectCode, ProjectName, Description, StatusId, CreatedBy, CreatedDate)
    VALUES ('PRJ-2024-001', N'Chuyển giao công nghệ sản xuất linh kiện điện tử', 
            N'Dự án chuyển giao công nghệ sản xuất linh kiện điện tử từ Nhật Bản', 
            3, @UserId, DATEADD(DAY, -90, GETDATE()));
    
    SET @Project1Id = SCOPE_IDENTITY();
    PRINT 'Created Project 1: PRJ-2024-001';
END
ELSE
BEGIN
    SELECT @Project1Id = Id FROM Projects WHERE ProjectCode = 'PRJ-2024-001';
    PRINT 'Project 1 already exists';
END

-- Project 2: In Progress project
IF NOT EXISTS (SELECT * FROM Projects WHERE ProjectCode = 'PRJ-2024-002')
BEGIN
    INSERT INTO Projects (ProjectCode, ProjectName, Description, StatusId, CreatedBy, CreatedDate)
    VALUES ('PRJ-2024-002', N'Chuyển giao công nghệ xử lý nước thải', 
            N'Dự án chuyển giao công nghệ xử lý nước thải công nghiệp', 
            2, @UserId, DATEADD(DAY, -30, GETDATE()));
    
    SET @Project2Id = SCOPE_IDENTITY();
    PRINT 'Created Project 2: PRJ-2024-002';
END
ELSE
BEGIN
    SELECT @Project2Id = Id FROM Projects WHERE ProjectCode = 'PRJ-2024-002';
    PRINT 'Project 2 already exists';
END

-- Project 3: Just started project
IF NOT EXISTS (SELECT * FROM Projects WHERE ProjectCode = 'PRJ-2024-003')
BEGIN
    INSERT INTO Projects (ProjectCode, ProjectName, Description, StatusId, CreatedBy, CreatedDate)
    VALUES ('PRJ-2024-003', N'Chuyển giao công nghệ sản xuất thực phẩm hữu cơ', 
            N'Dự án chuyển giao công nghệ sản xuất thực phẩm hữu cơ từ Hàn Quốc', 
            1, @UserId, DATEADD(DAY, -5, GETDATE()));
    
    SET @Project3Id = SCOPE_IDENTITY();
    PRINT 'Created Project 3: PRJ-2024-003';
END
ELSE
BEGIN
    SELECT @Project3Id = Id FROM Projects WHERE ProjectCode = 'PRJ-2024-003';
    PRINT 'Project 3 already exists';
END

-- 2. Add user as member to all projects with different roles
-- Project 1: User is Buyer
IF NOT EXISTS (SELECT * FROM ProjectMembers WHERE ProjectId = @Project1Id AND UserId = @UserId)
BEGIN
    INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
    VALUES (@Project1Id, @UserId, 1, DATEADD(DAY, -90, GETDATE()), 1);
    PRINT 'Added user as Buyer to Project 1';
END

-- Project 2: User is Seller
IF NOT EXISTS (SELECT * FROM ProjectMembers WHERE ProjectId = @Project2Id AND UserId = @UserId)
BEGIN
    INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
    VALUES (@Project2Id, @UserId, 2, DATEADD(DAY, -30, GETDATE()), 1);
    PRINT 'Added user as Seller to Project 2';
END

-- Project 3: User is Consultant
IF NOT EXISTS (SELECT * FROM ProjectMembers WHERE ProjectId = @Project3Id AND UserId = @UserId)
BEGIN
    INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
    VALUES (@Project3Id, @UserId, 3, DATEADD(DAY, -5, GETDATE()), 1);
    PRINT 'Added user as Consultant to Project 3';
END

-- 3. Create 11 workflow steps for each project

-- Helper: Create steps for a project
DECLARE @StepNames TABLE (StepNumber INT, StepName NVARCHAR(200));
INSERT INTO @StepNames VALUES
(1, N'Yêu cầu chuyển giao công nghệ'),
(2, N'Thỏa thuận bảo mật (NDA)'),
(3, N'Yêu cầu báo giá (RFQ)'),
(4, N'Nộp hồ sơ đề xuất'),
(5, N'Thương thảo giá'),
(6, N'Ký hợp đồng điện tử'),
(7, N'Xác nhận tạm ứng'),
(8, N'Nhật ký triển khai'),
(9, N'Biên bản bàn giao'),
(10, N'Biên bản nghiệm thu'),
(11, N'Biên bản thanh lý');

-- Project 1: All steps completed
DECLARE @i INT = 1;
WHILE @i <= 11
BEGIN
    IF NOT EXISTS (SELECT * FROM ProjectSteps WHERE ProjectId = @Project1Id AND StepNumber = @i)
    BEGIN
        INSERT INTO ProjectSteps (ProjectId, StepNumber, StepName, StatusId, CreatedDate, CompletedDate)
        SELECT @Project1Id, StepNumber, StepName, 2, 
            DATEADD(DAY, -90 + (StepNumber * 7), GETDATE()),
            DATEADD(DAY, -90 + (StepNumber * 7) + 5, GETDATE())
        FROM @StepNames WHERE StepNumber = @i;
    END
    SET @i = @i + 1;
END
PRINT 'Created steps for Project 1 (all completed)';

-- Project 2: Steps 1-4 completed, step 5 in progress, rest not started
SET @i = 1;
WHILE @i <= 11
BEGIN
    IF NOT EXISTS (SELECT * FROM ProjectSteps WHERE ProjectId = @Project2Id AND StepNumber = @i)
    BEGIN
        DECLARE @status INT = CASE 
            WHEN @i <= 4 THEN 2  -- Completed
            WHEN @i = 5 THEN 1   -- InProgress
            ELSE 0               -- NotStarted
        END;
        
        DECLARE @completedDate DATETIME2 = CASE 
            WHEN @i <= 4 THEN DATEADD(DAY, -30 + (@i * 5), GETDATE())
            ELSE NULL
        END;
        
        INSERT INTO ProjectSteps (ProjectId, StepNumber, StepName, StatusId, CreatedDate, CompletedDate)
        SELECT @Project2Id, StepNumber, StepName, @status, 
            DATEADD(DAY, -30 + (@i * 3), GETDATE()),
            @completedDate
        FROM @StepNames WHERE StepNumber = @i;
    END
    SET @i = @i + 1;
END
PRINT 'Created steps for Project 2 (in progress at step 5)';

-- Project 3: Step 1 in progress, rest not started
SET @i = 1;
WHILE @i <= 11
BEGIN
    IF NOT EXISTS (SELECT * FROM ProjectSteps WHERE ProjectId = @Project3Id AND StepNumber = @i)
    BEGIN
        DECLARE @status3 INT = CASE WHEN @i = 1 THEN 1 ELSE 0 END;
        
        INSERT INTO ProjectSteps (ProjectId, StepNumber, StepName, StatusId, CreatedDate, CompletedDate)
        SELECT @Project3Id, StepNumber, StepName, @status3, 
            DATEADD(DAY, -5, GETDATE()),
            NULL
        FROM @StepNames WHERE StepNumber = @i;
    END
    SET @i = @i + 1;
END
PRINT 'Created steps for Project 3 (just started)';

-- Summary
PRINT '';
PRINT '==============================================';
PRINT 'Sample data created successfully!';
PRINT '==============================================';
PRINT 'Projects created: 3';
PRINT '  - PRJ-2024-001: Completed (all 11 steps done)';
PRINT '  - PRJ-2024-002: In Progress (at step 5)';
PRINT '  - PRJ-2024-003: Just Started (at step 1)';
PRINT '';
PRINT 'User roles:';
PRINT '  - Project 1: Buyer';
PRINT '  - Project 2: Seller';
PRINT '  - Project 3: Consultant';
PRINT '==============================================';
GO
