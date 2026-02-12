-- =============================================
-- Insert Sample Data for TechTransferRequests
-- =============================================

USE [TPApp];
GO

-- First, check if we have any projects
IF NOT EXISTS (SELECT 1 FROM Projects WHERE Id = 1)
BEGIN
    PRINT 'Creating sample project first...';
    
    -- Insert a sample project
    SET IDENTITY_INSERT Projects ON;
    INSERT INTO Projects (Id, ProjectCode, ProjectName, Description, StatusId, CreatedBy, CreatedDate)
    VALUES (1, 'PJ-20260212001', 'Dự án Chuyển giao Công nghệ AI', 'Dự án chuyển giao công nghệ trí tuệ nhân tạo', 1, 22329, GETDATE());
    SET IDENTITY_INSERT Projects OFF;
    
    -- Insert project member
    IF NOT EXISTS (SELECT 1 FROM ProjectMembers WHERE ProjectId = 1 AND UserId = 22329)
    BEGIN
        INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
        VALUES (1, 22329, 1, GETDATE(), 1); -- Role 1 = Buyer
    END
    
    PRINT '✓ Sample project created';
END
ELSE
BEGIN
    PRINT '✓ Project already exists';
END

-- Now insert TechTransferRequests
PRINT '';
PRINT 'Inserting TechTransferRequests...';

-- Sample 1: AI Technology Transfer
IF NOT EXISTS (SELECT 1 FROM TechTransferRequests WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT TechTransferRequests ON;
    INSERT INTO TechTransferRequests 
    (Id, HoTen, ChucVu, DonVi, DiaChi, DienThoai, Email, TenCongNghe, MoTaNhuCau, LinhVuc, NganSachDuKien, ProjectId, StatusId, NguoiTao, NgayTao)
    VALUES 
    (1, 
     N'Nguyễn Văn A', 
     N'Giám đốc Công nghệ', 
     N'Công ty TNHH Công nghệ ABC', 
     N'123 Đường Lê Lợi, Quận 1, TP.HCM', 
     '0901234567', 
     'nguyenvana@abc.com',
     N'Hệ thống AI nhận diện khuôn mặt',
     N'Cần chuyển giao công nghệ AI để xây dựng hệ thống nhận diện khuôn mặt cho hệ thống an ninh. Yêu cầu độ chính xác cao, xử lý real-time, hỗ trợ camera IP.',
     N'Trí tuệ nhân tạo',
     500000000, -- 500 triệu VNĐ
     1,
     1, -- Draft
     22329,
     GETDATE());
    SET IDENTITY_INSERT TechTransferRequests OFF;
    PRINT '  ✓ Inserted TechTransferRequest #1: AI Face Recognition';
END

-- Sample 2: IoT Technology
IF NOT EXISTS (SELECT 1 FROM TechTransferRequests WHERE Id = 2)
BEGIN
    SET IDENTITY_INSERT TechTransferRequests ON;
    INSERT INTO TechTransferRequests 
    (Id, HoTen, ChucVu, DonVi, DiaChi, DienThoai, Email, TenCongNghe, MoTaNhuCau, LinhVuc, NganSachDuKien, ProjectId, StatusId, NguoiTao, NgayTao)
    VALUES 
    (2, 
     N'Trần Thị B', 
     N'Trưởng phòng R&D', 
     N'Công ty Cổ phần Nông nghiệp XYZ', 
     N'456 Đường Nguyễn Huệ, Quận 3, TP.HCM', 
     '0912345678', 
     'tranthib@xyz.com',
     N'Hệ thống IoT giám sát nông nghiệp thông minh',
     N'Yêu cầu chuyển giao công nghệ IoT để giám sát độ ẩm đất, nhiệt độ, ánh sáng tự động. Cần tích hợp với hệ thống tưới tiêu tự động.',
     N'IoT & Nông nghiệp',
     300000000, -- 300 triệu VNĐ
     NULL, -- Chưa có project
     1, -- Draft
     22329,
     GETDATE());
    SET IDENTITY_INSERT TechTransferRequests OFF;
    PRINT '  ✓ Inserted TechTransferRequest #2: IoT Agriculture';
END

-- Sample 3: Blockchain Technology
IF NOT EXISTS (SELECT 1 FROM TechTransferRequests WHERE Id = 3)
BEGIN
    SET IDENTITY_INSERT TechTransferRequests ON;
    INSERT INTO TechTransferRequests 
    (Id, HoTen, ChucVu, DonVi, DiaChi, DienThoai, Email, TenCongNghe, MoTaNhuCau, LinhVuc, NganSachDuKien, ProjectId, StatusId, NguoiTao, NgayTao)
    VALUES 
    (3, 
     N'Lê Văn C', 
     N'CEO', 
     N'Startup Fintech DEF', 
     N'789 Đường Võ Văn Tần, Quận 3, TP.HCM', 
     '0923456789', 
     'levanc@def.com',
     N'Nền tảng Blockchain cho thanh toán điện tử',
     N'Cần chuyển giao công nghệ Blockchain để xây dựng nền tảng thanh toán an toàn, minh bạch. Yêu cầu hỗ trợ smart contract và tích hợp ví điện tử.',
     N'Blockchain & Fintech',
     800000000, -- 800 triệu VNĐ
     NULL,
     2, -- Processing
     22329,
     GETDATE());
    SET IDENTITY_INSERT TechTransferRequests OFF;
    PRINT '  ✓ Inserted TechTransferRequest #3: Blockchain Payment';
END

PRINT '';
PRINT '========================================';
PRINT 'Sample Data Insertion Completed!';
PRINT '========================================';
PRINT '';

-- Verify data
SELECT 
    Id,
    HoTen,
    TenCongNghe,
    LinhVuc,
    FORMAT(NganSachDuKien, 'N0') as NganSach,
    ProjectId,
    CASE StatusId 
        WHEN 1 THEN 'Draft'
        WHEN 2 THEN 'Processing'
        WHEN 3 THEN 'Completed'
    END as Status,
    NgayTao
FROM TechTransferRequests
ORDER BY Id;

PRINT '';
PRINT 'Total TechTransferRequests: ' + CAST((SELECT COUNT(*) FROM TechTransferRequests) AS VARCHAR);
GO
