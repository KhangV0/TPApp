-- =============================================
-- Add Data for Steps 5-11 to Project ID 4
-- Database: TechMartNew
-- =============================================

USE [TechMartNew]
GO

DECLARE @ProjectId INT = 4;  -- Project created from previous script

-- =============================================
-- STEP 5: Negotiation Form
-- Columns: Id, RFQId, ProjectId, GiaChotCuoiCung, DieuKhoanThanhToan, 
--          BienBanThuongLuongFile, HinhThucKy, DaKySo, StatusId, NguoiTao, NgayTao, NguoiSua, NgaySua
-- =============================================
INSERT INTO [dbo].[NegotiationForms] (
    RFQId, ProjectId, GiaChotCuoiCung, DieuKhoanThanhToan,
    BienBanThuongLuongFile, HinhThucKy, DaKySo,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    NULL,
    @ProjectId,
    4200000000,  -- Giá cuối cùng 4.2 tỷ
    N'Thanh toán 3 đợt: 30% tạm ứng, 40% khi hoàn thành 50%, 30% khi nghiệm thu',
    N'/uploads/negotiations/bien-ban-thuong-luong-prj4.pdf',
    N'Chữ ký điện tử',
    1,  -- Đã ký số
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 6: E-Contract
-- Columns: Id, RFQId, ProjectId, SoHopDong, FileHopDong, 
--          NguoiKyBenA, NguoiKyBenB, TrangThaiKy, StatusId, NguoiTao, NgayTao, NguoiSua, NgaySua
-- =============================================
INSERT INTO [dbo].[EContracts] (
    RFQId, ProjectId, SoHopDong, FileHopDong,
    NguoiKyBenA, NguoiKyBenB, TrangThaiKy,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    NULL,
    @ProjectId,
    N'HĐ-AI-2024-TEST-003',
    N'/uploads/contracts/hop-dong-ai-prj4.pdf',
    N'Nguyễn Văn A - Giám đốc Công ty ABC',
    N'Trần Thị B - Giám đốc Công ty XYZ',
    N'Đã ký bởi cả hai bên',
    1,
    22329,
    GETDATE()
);

DECLARE @EContractId INT = SCOPE_IDENTITY();

-- =============================================
-- STEP 7: Advance Payment Confirmation
-- Columns: Id, EContractId, ProjectId, SoTienTamUng, ChungTuChuyenTienFile,
--          NgayChuyen, DaXacNhanNhanTien, StatusId, NguoiTao, NgayTao, NguoiSua, NgaySua
-- =============================================
INSERT INTO [dbo].[AdvancePaymentConfirmations] (
    EContractId, ProjectId, SoTienTamUng, ChungTuChuyenTienFile,
    NgayChuyen, DaXacNhanNhanTien,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @EContractId,
    @ProjectId,
    1260000000,  -- 30% của 4.2 tỷ
    N'/uploads/payments/chung-tu-tam-ung-prj4.pdf',
    GETDATE(),
    1,  -- Đã xác nhận nhận tiền
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 8: Implementation Log
-- Columns: Id, ProjectId, EContractId, GiaiDoan, KetQuaThucHien,
--          HinhAnhVideoFile, BienBanXacNhanFile, StatusId, NguoiTao, NgayTao, NguoiSua, NgaySua
-- =============================================
INSERT INTO [dbo].[ImplementationLogs] (
    ProjectId, EContractId, GiaiDoan, KetQuaThucHien,
    HinhAnhVideoFile, BienBanXacNhanFile,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    @EContractId,
    N'Giai đoạn 1: Phân tích và thiết kế hệ thống (Hoàn thành 100%)',
    N'Đã hoàn thành phân tích yêu cầu, thiết kế kiến trúc hệ thống AI, thiết kế database và API. Tiến độ đúng kế hoạch.',
    N'/uploads/implementation/progress-phase1-prj4.mp4',
    N'/uploads/implementation/bien-ban-phase1-prj4.pdf',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 9: Handover Report
-- Columns: Id, ProjectId, EContractId, DanhMucThietBiJson, DanhMucHoSoJson,
--          DaHoanThanhDaoTao, DanhGiaSao, NhanXet, StatusId, NguoiTao, NgayTao, NguoiSua, NgaySua
-- =============================================
INSERT INTO [dbo].[HandoverReports] (
    ProjectId, EContractId, 
    DanhMucThietBiJson, 
    DanhMucHoSoJson,
    DaHoanThanhDaoTao, DanhGiaSao, NhanXet,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    @EContractId,
    N'[{"ten":"Server AI GPU", "soLuong":2, "trangThai":"Đã bàn giao"}, {"ten":"Workstation", "soLuong":5, "trangThai":"Đã bàn giao"}]',
    N'[{"ten":"Source Code", "loai":"ZIP", "dungLuong":"2.5GB"}, {"ten":"Tài liệu kỹ thuật", "loai":"PDF", "dungLuong":"150MB"}, {"ten":"Video hướng dẫn", "loai":"MP4", "dungLuong":"800MB"}]',
    1,  -- Đã hoàn thành đào tạo
    5,  -- Đánh giá 5 sao
    N'Đội ngũ kỹ thuật rất chuyên nghiệp, hệ thống hoạt động ổn định. Đào tạo chi tiết và dễ hiểu.',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 10: Acceptance Report
-- Columns: Id, ProjectId, EContractId, NgayNghiemThu, ThanhPhanThamGia,
--          KetLuanNghiemThu, VanDeTonDong, ChuKyBenA, ChuKyBenB, TrangThaiKy,
--          StatusId, NguoiTao, NgayTao, NguoiSua, NgaySua
-- =============================================
INSERT INTO [dbo].[AcceptanceReports] (
    ProjectId, EContractId, NgayNghiemThu, ThanhPhanThamGia,
    KetLuanNghiemThu, VanDeTonDong, 
    ChuKyBenA, ChuKyBenB, TrangThaiKy,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    @EContractId,
    DATEADD(MONTH, 6, GETDATE()),
    N'Bên A: Nguyễn Văn A (GĐ), Phạm Văn C (Trưởng phòng IT). Bên B: Trần Thị B (GĐ), Lê Văn D (Trưởng dự án)',
    N'Đạt yêu cầu - Hệ thống đáp ứng đầy đủ các yêu cầu kỹ thuật đã cam kết',
    N'Không có vấn đề tồn đọng',
    N'Nguyễn Văn A - Chữ ký điện tử',
    N'Trần Thị B - Chữ ký điện tử',
    N'Đã ký đầy đủ',
    1,
    22329,
    GETDATE()
);

-- =============================================
-- STEP 11: Liquidation Report
-- Columns: Id, ProjectId, EContractId, GiaTriThanhToanConLai, SoHoaDon,
--          HoaDonFile, SanDaChuyenTien, HopDongClosed, StatusId, NguoiTao, NgayTao, NguoiSua, NgaySua
-- =============================================
INSERT INTO [dbo].[LiquidationReports] (
    ProjectId, EContractId, GiaTriThanhToanConLai, SoHoaDon,
    HoaDonFile, SanDaChuyenTien, HopDongClosed,
    StatusId, NguoiTao, NgayTao
)
VALUES (
    @ProjectId,
    @EContractId,
    0,  -- Đã thanh toán hết
    N'HD-2024-AI-003-FINAL',
    N'/uploads/invoices/hoa-don-thanh-ly-prj4.pdf',
    1,  -- Đã chuyển tiền
    1,  -- Hợp đồng đã đóng
    1,
    22329,
    GETDATE()
);

-- =============================================
-- Display Success Message
-- =============================================
SELECT 
    @ProjectId AS ProjectId,
    @EContractId AS EContractId,
    N'Đã thêm dữ liệu thành công cho Steps 5-11!' AS Message;

PRINT '================================================';
PRINT 'Project ID: ' + CAST(@ProjectId AS VARCHAR);
PRINT 'EContract ID: ' + CAST(@EContractId AS VARCHAR);
PRINT 'Steps 5-11 populated successfully!';
PRINT 'All 11 steps now have complete data';
PRINT '================================================';
