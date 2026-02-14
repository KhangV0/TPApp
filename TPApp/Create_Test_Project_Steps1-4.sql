-- =============================================
-- Create Test Project for User 22329  
-- Database: TechMartNew
-- Steps 1-4 (Already Implemented with Inline Editing)
-- =============================================

USE [TechMartNew]
GO

-- Step 1: Create Project
INSERT INTO [dbo].[Projects] (ProjectCode, ProjectName, Description, StatusId, CreatedBy, CreatedDate)
VALUES (
    N'PRJ-2024-TEST-003',
    N'Dự án Test Inline Editing - AI System',
    N'Dự án test inline editing cho Steps 1-4',
    1,
    22329,
    GETDATE()
);

DECLARE @ProjectId INT = SCOPE_IDENTITY();

-- Step 2: Add Project Members (3 roles for user 22329)
INSERT INTO [dbo].[ProjectMembers] (ProjectId, UserId, Role, JoinedDate, IsActive)
VALUES 
    (@ProjectId, 22329, 1, GETDATE(), 1),  -- Buyer
    (@ProjectId, 22329, 2, GETDATE(), 1),  -- Seller
    (@ProjectId, 22329, 3, GETDATE(), 1);  -- Consultant

-- Step 3: Initialize Project Steps
INSERT INTO [dbo].[ProjectSteps] (ProjectId, StepNumber, StepName, StatusId, CreatedDate)
VALUES
    (@ProjectId, 1, N'Yêu cầu chuyển giao', 1, GETDATE()),
    (@ProjectId, 2, N'Thỏa thuận NDA', 1, GETDATE()),
    (@ProjectId, 3, N'Yêu cầu báo giá (RFQ)', 1, GETDATE()),
    (@ProjectId, 4, N'Nộp hồ sơ đề xuất', 1, GETDATE()),
    (@ProjectId, 5, N'Thương lượng', 1, GETDATE()),
    (@ProjectId, 6, N'Ký hợp đồng điện tử', 1, GETDATE()),
    (@ProjectId, 7, N'Xác nhận tạm ứng', 1, GETDATE()),
    (@ProjectId, 8, N'Triển khai', 1, GETDATE()),
    (@ProjectId, 9, N'Bàn giao', 1, GETDATE()),
    (@ProjectId, 10, N'Nghiệm thu', 1, GETDATE()),
    (@ProjectId, 11, N'Thanh lý', 1, GETDATE());

-- =============================================
-- STEP 1: TechTransfer Request
-- =============================================
INSERT INTO [dbo].[TechTransferRequests] (
    ProjectId, HoTen, ChucVu, DonVi, DiaChi, DienThoai, Email,
    TenCongNghe, LinhVuc, MoTaNhuCau, NganSachDuKien,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    N'Nguyễn Văn Test',
    N'Giám đốc Công nghệ',
    N'Công ty TNHH Công nghệ ABC',
    N'123 Đường Lê Lợi, Quận 1, TP.HCM',
    N'0901234567',
    N'test@abc.com.vn',
    N'Hệ thống AI Phân tích Dữ liệu Lớn',
    N'Trí tuệ nhân tạo và Machine Learning',
    N'Cần chuyển giao công nghệ AI để phân tích dữ liệu khách hàng, dự đoán xu hướng thị trường và tối ưu hóa quy trình kinh doanh. Hệ thống cần xử lý được 1 triệu bản ghi/ngày.',
    5000000000,
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 2: NDA Agreement
-- =============================================
INSERT INTO [dbo].[NDAAgreements] (
    ProjectId, BenA, BenB, LoaiNDA, ThoiHanBaoMat,
    XacNhanKySo, DaDongY, StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    N'Công ty TNHH Công nghệ ABC',
    N'Công ty CP Giải pháp AI XYZ',
    N'Hai chiều',
    N'3 năm kể từ ngày ký',
    N'Đã xác nhận bằng chữ ký điện tử vào ngày ' + CONVERT(VARCHAR, GETDATE(), 103),
    1,
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 3: RFQ Request
-- =============================================
INSERT INTO [dbo].[RFQRequests] (
    ProjectId, MaRFQ, YeuCauKyThuat, TieuChuanNghiemThu,
    HanChotNopHoSo, DaGuiNhaCungUng, StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    N'RFQ-2024-AI-TEST-003',
    N'1. Hệ thống AI phải xử lý được tối thiểu 1 triệu bản ghi/ngày
2. Độ chính xác dự đoán tối thiểu 85%
3. Thời gian phản hồi trung bình < 2 giây
4. Hỗ trợ tiếng Việt và tiếng Anh
5. Tích hợp với hệ thống CRM hiện tại
6. Dashboard báo cáo real-time
7. API RESTful cho tích hợp bên thứ ba',
    N'1. Kiểm tra hiệu năng với 1 triệu bản ghi mẫu
2. Đánh giá độ chính xác trên tập dữ liệu test
3. Kiểm tra tích hợp với CRM
4. Đánh giá giao diện người dùng
5. Kiểm tra bảo mật và quyền truy cập
6. Đào tạo người dùng cuối',
    DATEADD(DAY, 30, GETDATE()),
    1,
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 4: Proposal Submission
-- =============================================
INSERT INTO [dbo].[ProposalSubmissions] (
    ProjectId, RFQId, GiaiPhapKyThuat, BaoGiaSoBo,
    ThoiGianTrienKhai, HoSoNangLucDinhKem,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    NULL,
    N'/uploads/proposals/giai-phap-ky-thuat-ai-2024-test.pdf',
    4500000000,
    N'6 tháng (bao gồm phát triển, testing và đào tạo)',
    N'/uploads/proposals/ho-so-nang-luc-xyz-test.pdf',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- Display Project Information
-- =============================================
SELECT 
    @ProjectId AS ProjectId,
    N'Dự án đã được tạo thành công!' AS Message,
    N'User 22329 có 3 vai trò: Buyer (1), Seller (2), Consultant (3)' AS Roles,
    N'Steps 1-4 đã có dữ liệu đầy đủ để test inline editing' AS Steps;

PRINT '================================================';
PRINT 'Project ID: ' + CAST(@ProjectId AS VARCHAR);
PRINT 'Project Code: PRJ-2024-TEST-003';
PRINT 'User: 22329 (3 roles: Buyer, Seller, Consultant)';
PRINT 'Steps 1-4 populated with sample data';
PRINT 'Ready to test inline editing!';
PRINT '================================================';
