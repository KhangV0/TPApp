-- Mock Data for Project Member Management Testing
-- This script creates test users and project members for testing the member management feature

-- =============================================
-- 1. CREATE MOCK USERS (if not exist)
-- =============================================

-- Check if users exist, if not create them
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'buyer@techport.vn')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName)
    VALUES 
    (1, 'buyer@techport.vn', 'BUYER@TECHPORT.VN', 'buyer@techport.vn', 'BUYER@TECHPORT.VN', 1, 'AQAAAAIAAYagAAAAEDummyHashForTesting', NEWID(), NEWID(), 0, 0, 1, 0, 'Nguyen Van Buyer');
END

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'seller1@techport.vn')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName)
    VALUES 
    (2, 'seller1@techport.vn', 'SELLER1@TECHPORT.VN', 'seller1@techport.vn', 'SELLER1@TECHPORT.VN', 1, 'AQAAAAIAAYagAAAAEDummyHashForTesting', NEWID(), NEWID(), 0, 0, 1, 0, 'Tran Thi Seller 1');
END

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'seller2@techport.vn')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName)
    VALUES 
    (3, 'seller2@techport.vn', 'SELLER2@TECHPORT.VN', 'seller2@techport.vn', 'SELLER2@TECHPORT.VN', 1, 'AQAAAAIAAYagAAAAEDummyHashForTesting', NEWID(), NEWID(), 0, 0, 1, 0, 'Le Van Seller 2');
END

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'consultant1@techport.vn')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName)
    VALUES 
    (4, 'consultant1@techport.vn', 'CONSULTANT1@TECHPORT.VN', 'consultant1@techport.vn', 'CONSULTANT1@TECHPORT.VN', 1, 'AQAAAAIAAYagAAAAEDummyHashForTesting', NEWID(), NEWID(), 0, 0, 1, 0, 'Pham Thi Consultant 1');
END

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'consultant2@techport.vn')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName)
    VALUES 
    (5, 'consultant2@techport.vn', 'CONSULTANT2@TECHPORT.VN', 'consultant2@techport.vn', 'CONSULTANT2@TECHPORT.VN', 1, 'AQAAAAIAAYagAAAAEDummyHashForTesting', NEWID(), NEWID(), 0, 0, 1, 0, 'Hoang Van Consultant 2');
END

PRINT 'Mock users created successfully!';

-- =============================================
-- 2. CREATE PROJECT MEMBERS FOR PROJECT 7
-- =============================================

-- Clear existing members for project 7 (optional, for clean testing)
-- DELETE FROM ProjectMembers WHERE ProjectId = 7;

-- Add Buyer
IF NOT EXISTS (SELECT 1 FROM ProjectMembers WHERE ProjectId = 7 AND UserId = 1 AND Role = 1)
BEGIN
    INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
    VALUES (7, 1, 1, GETDATE(), 1); -- Buyer
    PRINT 'Added Buyer to Project 7';
END

-- Add Seller 1
IF NOT EXISTS (SELECT 1 FROM ProjectMembers WHERE ProjectId = 7 AND UserId = 2 AND Role = 2)
BEGIN
    INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
    VALUES (7, 2, 2, GETDATE(), 1); -- Seller
    PRINT 'Added Seller 1 to Project 7';
END

-- Add Seller 2
IF NOT EXISTS (SELECT 1 FROM ProjectMembers WHERE ProjectId = 7 AND UserId = 3 AND Role = 2)
BEGIN
    INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
    VALUES (7, 3, 2, GETDATE(), 1); -- Seller
    PRINT 'Added Seller 2 to Project 7';
END

PRINT 'Project members created successfully!';
PRINT '';
PRINT '=============================================';
PRINT 'MOCK DATA SUMMARY';
PRINT '=============================================';
PRINT 'Users Created:';
PRINT '  - User 1: buyer@techport.vn (Nguyen Van Buyer)';
PRINT '  - User 2: seller1@techport.vn (Tran Thi Seller 1)';
PRINT '  - User 3: seller2@techport.vn (Le Van Seller 2)';
PRINT '  - User 4: consultant1@techport.vn (Pham Thi Consultant 1)';
PRINT '  - User 5: consultant2@techport.vn (Hoang Van Consultant 2)';
PRINT '';
PRINT 'Project 7 Members:';
PRINT '  - User 1 (Buyer)';
PRINT '  - User 2 (Seller)';
PRINT '  - User 3 (Seller)';
PRINT '';
PRINT 'TEST SCENARIO:';
PRINT '1. Login as buyer@techport.vn';
PRINT '2. Navigate to /ProjectMembers/Index?projectId=7';
PRINT '3. Add User 4 (consultant1@techport.vn) as Consultant';
PRINT '4. Add User 5 (consultant2@techport.vn) as Consultant';
PRINT '5. Try to add User 4 again (should fail - duplicate)';
PRINT '6. Remove User 4 (should succeed)';
PRINT '7. Try to remove User 1 (should fail - cannot remove buyer)';
PRINT '=============================================';

-- =============================================
-- 3. VERIFICATION QUERIES
-- =============================================

-- View all project members
SELECT 
    pm.Id,
    pm.ProjectId,
    u.FullName AS UserName,
    u.Email,
    pm.Role,
    CASE pm.Role 
        WHEN 1 THEN 'Buyer'
        WHEN 2 THEN 'Seller'
        WHEN 3 THEN 'Consultant'
        ELSE 'Unknown'
    END AS RoleName,
    pm.JoinedDate,
    pm.IsActive
FROM ProjectMembers pm
INNER JOIN AspNetUsers u ON pm.UserId = u.Id
WHERE pm.ProjectId = 7
ORDER BY pm.Role, u.FullName;

-- View available consultants (users not in project 7)
SELECT 
    u.Id AS UserId,
    u.FullName,
    u.Email
FROM AspNetUsers u
WHERE u.Id NOT IN (
    SELECT UserId FROM ProjectMembers WHERE ProjectId = 7 AND IsActive = 1
)
ORDER BY u.FullName;
