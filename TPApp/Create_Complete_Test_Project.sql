-- =============================================
-- Create Complete Test Project for User 22329
-- Database: TechMartNew
-- Roles: Buyer (1), Seller (2), Consultant (3)
-- =============================================

USE [TechMartNew]
GO

-- Step 1: Create Project
INSERT INTO [dbo].[Projects] (ProjectCode, ProjectName, Description, StatusId, CreatedBy, CreatedDate)
VALUES (
    N'PRJ-2024-TEST-003',
    N'Dự án Test Inline Editing - AI System',
    N'Dự án test đầy đủ cho tất cả 11 bước với 3 vai trò để kiểm tra inline editing',
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
-- STEP 5: Negotiation Form
-- =============================================
INSERT INTO [dbo].[NegotiationForms] (
    ProjectId, NoiDungThongThuong, KetQuaThongThuong,
    GiaTriCuoiCung, DieuKhoanBoSung, StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    N'Đã thương lượng về giá, thời gian triển khai và điều khoản bảo hành. Bên mua đề xuất giảm 10% chi phí, bên bán đề xuất tăng thời gian triển khai thêm 1 tháng.',
    N'Thỏa thuận giá 4.2 tỷ VNĐ, thời gian 7 tháng, bảo hành 24 tháng',
    4200000000,
    N'1. Bảo hành 24 tháng kể từ ngày nghiệm thu
2. Hỗ trợ kỹ thuật 24/7 trong 6 tháng đầu
3. Đào tạo miễn phí cho 20 người dùng
4. Nâng cấp phần mềm miễn phí trong 12 tháng',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 6: E-Contract
-- =============================================
INSERT INTO [dbo].[EContracts] (
    ProjectId, SoHopDong, NgayKy, GiaTriHopDong,
    NoiDungHopDong, TrangThaiKy, FileHopDongDinhKem,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    N'HĐ-AI-2024-TEST-003',
    GETDATE(),
    4200000000,
    N'Hợp đồng chuyển giao công nghệ hệ thống AI phân tích dữ liệu. Giá trị: 4.2 tỷ VNĐ. Thời gian: 7 tháng. Bảo hành: 24 tháng.',
    N'Đã ký bởi cả hai bên',
    N'/uploads/contracts/hop-dong-ai-2024-test-003.pdf',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 7: Advance Payment Confirmation
-- =============================================
INSERT INTO [dbo].[AdvancePaymentConfirmations] (
    ProjectId, SoTienTamUng, PhanTramTamUng, NgayTamUng,
    PhuongThucThanhToan, MaGiaoDich, XacNhanNganHang,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    1260000000,  -- 30% of 4.2 billion
    30,
    GETDATE(),
    N'Chuyển khoản ngân hàng',
    N'TXN-2024-AI-TEST-003-ADV',
    N'Đã xác nhận bởi Ngân hàng Vietcombank - Chi nhánh TP.HCM',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 8: Implementation Log
-- =============================================
INSERT INTO [dbo].[ImplementationLogs] (
    ProjectId, NgayBatDau, NgayDuKienHoanThanh,
    TienDoHienTai, MoTaTienDo, VanDePhatsInh,
    GiaiPhapXuLy, StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    GETDATE(),
    DATEADD(MONTH, 7, GETDATE()),
    35,
    N'Đã hoàn thành phân tích yêu cầu và thiết kế hệ thống. Đang trong giai đoạn phát triển module AI core.',
    N'Gặp khó khăn trong việc tích hợp với hệ thống CRM cũ do API không đầy đủ',
    N'Đã làm việc với đội ngũ IT của khách hàng để bổ sung API cần thiết. Dự kiến hoàn thành trong 2 tuần.',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 9: Handover Report
-- =============================================
INSERT INTO [dbo].[HandoverReports] (
    ProjectId, NgayBanGiao, NoiDungBanGiao,
    TaiLieuBanGiao, DaoTaoNguoiDung, XacNhanTiepNhan,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    DATEADD(MONTH, 7, GETDATE()),
    N'Bàn giao hệ thống AI hoàn chỉnh bao gồm: Source code, Database, Tài liệu kỹ thuật, Tài liệu người dùng, Video hướng dẫn',
    N'/uploads/handover/tai-lieu-ban-giao-ai-system-test.zip',
    N'Đã đào tạo 20 người dùng trong 3 ngày. Nội dung: Sử dụng dashboard, Quản lý dữ liệu, Phân tích báo cáo, Xử lý sự cố cơ bản',
    N'Đã xác nhận tiếp nhận đầy đủ bởi Giám đốc Công nghệ - Công ty ABC',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 10: Acceptance Report
-- =============================================
INSERT INTO [dbo].[AcceptanceReports] (
    ProjectId, NgayNghiemThu, KetQuaNghiemThu,
    DanhGiaChiTiet, YKienKhachHang, TrangThaiNghiemThu,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    DATEADD(MONTH, 7, GETDATE()),
    N'Đạt yêu cầu',
    N'Hệ thống đáp ứng đầy đủ các yêu cầu kỹ thuật:
- Xử lý 1.2 triệu bản ghi/ngày (vượt yêu cầu)
- Độ chính xác 87% (vượt yêu cầu 85%)
- Thời gian phản hồi trung bình 1.5 giây
- Tích hợp CRM hoàn hảo
- Dashboard trực quan, dễ sử dụng',
    N'Rất hài lòng với chất lượng sản phẩm và dịch vụ hỗ trợ. Đội ngũ kỹ thuật chuyên nghiệp, nhiệt tình.',
    N'Nghiệm thu đạt',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 11: Liquidation Report
-- =============================================
INSERT INTO [dbo].[LiquidationReports] (
    ProjectId, NgayThanhLy, TongGiaTriHopDong,
    SoTienDaThanhToan, SoTienConLai, PhuongThucThanhToanCuoi,
    MaGiaoDichCuoi, XacNhanHoanTat, StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    DATEADD(MONTH, 8, GETDATE()),
    4200000000,
    4200000000,
    0,
    N'Chuyển khoản ngân hàng',
    N'TXN-2024-AI-TEST-003-FINAL',
    N'Đã xác nhận hoàn tất thanh lý hợp đồng. Cả hai bên không còn nghĩa vụ tài chính.',
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
    N'Tất cả 11 bước đã có dữ liệu đầy đủ' AS Steps;

PRINT '================================================';
PRINT 'Project ID: ' + CAST(@ProjectId AS VARCHAR);
PRINT 'Project Code: PRJ-2024-TEST-003';
PRINT 'User: 22329 (3 roles: Buyer, Seller, Consultant)';
PRINT 'All 11 steps populated with sample data';
PRINT '================================================';
