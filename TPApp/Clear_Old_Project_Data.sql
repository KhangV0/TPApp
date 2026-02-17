-- =============================================
-- CLEAR OLD PROJECT DATA
-- Database: TechMartNew
-- Mục đích: Xóa dữ liệu cũ để chuẩn bị cho workflow 14 bước
-- =============================================

USE [TechMartNew]
GO

PRINT '================================================';
PRINT 'CẢNH BÁO: Script này sẽ XÓA TẤT CẢ dữ liệu dự án!';
PRINT 'Vui lòng backup database trước khi chạy!';
PRINT '================================================';
PRINT '';

-- Uncomment dòng dưới để thực sự chạy script (bảo vệ tránh chạy nhầm)
-- DECLARE @ConfirmDelete BIT = 1;

DECLARE @ConfirmDelete BIT = 0; -- Mặc định = 0 để an toàn

IF @ConfirmDelete = 0
BEGIN
    PRINT '❌ Script bị hủy để an toàn!';
    PRINT 'Để chạy script, hãy set @ConfirmDelete = 1 ở dòng 15';
    RETURN;
END

PRINT 'Bắt đầu xóa dữ liệu...';
PRINT '';

-- =============================================
-- XÓA DỮ LIỆU WORKFLOW (14 BẢNG)
-- Xóa theo thứ tự ngược để tránh lỗi foreign key
-- =============================================

-- Bước 14: Thanh lý hợp đồng
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LiquidationReports]'))
BEGIN
    DELETE FROM [dbo].[LiquidationReports];
    PRINT '✓ Đã xóa dữ liệu từ LiquidationReports';
END

-- Bước 13: Nghiệm thu
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcceptanceReports]'))
BEGIN
    DELETE FROM [dbo].[AcceptanceReports];
    PRINT '✓ Đã xóa dữ liệu từ AcceptanceReports';
END

-- Bước 12: Bàn giao hồ sơ kỹ thuật (MỚI)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TechnicalDocHandovers]'))
BEGIN
    DELETE FROM [dbo].[TechnicalDocHandovers];
    PRINT '✓ Đã xóa dữ liệu từ TechnicalDocHandovers';
END

-- Bước 11: Đào tạo (MỚI)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TrainingHandovers]'))
BEGIN
    DELETE FROM [dbo].[TrainingHandovers];
    PRINT '✓ Đã xóa dữ liệu từ TrainingHandovers';
END

-- Bước 10: Bàn giao
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HandoverReports]'))
BEGIN
    DELETE FROM [dbo].[HandoverReports];
    PRINT '✓ Đã xóa dữ liệu từ HandoverReports';
END

-- Bước 9: Thử nghiệm Pilot (MỚI)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PilotTestReports]'))
BEGIN
    DELETE FROM [dbo].[PilotTestReports];
    PRINT '✓ Đã xóa dữ liệu từ PilotTestReports';
END

-- Bước 8: Tạm ứng
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdvancePaymentConfirmations]'))
BEGIN
    DELETE FROM [dbo].[AdvancePaymentConfirmations];
    PRINT '✓ Đã xóa dữ liệu từ AdvancePaymentConfirmations';
END

-- Bước 7: Hợp đồng điện tử
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EContracts]'))
BEGIN
    DELETE FROM [dbo].[EContracts];
    PRINT '✓ Đã xóa dữ liệu từ EContracts';
END

-- Bước 6: Kiểm tra pháp lý (MỚI)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LegalReviewForms]'))
BEGIN
    DELETE FROM [dbo].[LegalReviewForms];
    PRINT '✓ Đã xóa dữ liệu từ LegalReviewForms';
END

-- Bước 5: Đàm phán
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NegotiationForms]'))
BEGIN
    DELETE FROM [dbo].[NegotiationForms];
    PRINT '✓ Đã xóa dữ liệu từ NegotiationForms';
END

-- Bước 4: Nộp hồ sơ
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProposalSubmissions]'))
BEGIN
    DELETE FROM [dbo].[ProposalSubmissions];
    PRINT '✓ Đã xóa dữ liệu từ ProposalSubmissions';
END

-- Bước 3: RFQ
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RFQRequests]'))
BEGIN
    DELETE FROM [dbo].[RFQRequests];
    PRINT '✓ Đã xóa dữ liệu từ RFQRequests';
END

-- Bước 2: NDA
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NDAAgreements]'))
BEGIN
    DELETE FROM [dbo].[NDAAgreements];
    PRINT '✓ Đã xóa dữ liệu từ NDAAgreements';
END

-- Bước 1: Yêu cầu chuyển giao
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TechTransferRequests]'))
BEGIN
    DELETE FROM [dbo].[TechTransferRequests];
    PRINT '✓ Đã xóa dữ liệu từ TechTransferRequests';
END

PRINT '';

-- =============================================
-- XÓA DỮ LIỆU PROJECT STEPS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectSteps]'))
BEGIN
    DELETE FROM [dbo].[ProjectSteps];
    PRINT '✓ Đã xóa dữ liệu từ ProjectSteps';
END

-- =============================================
-- XÓA DỮ LIỆU PROJECT MEMBERS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMembers]'))
BEGIN
    DELETE FROM [dbo].[ProjectMembers];
    PRINT '✓ Đã xóa dữ liệu từ ProjectMembers';
END

-- =============================================
-- XÓA DỮ LIỆU PROJECTS
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Projects]'))
BEGIN
    DELETE FROM [dbo].[Projects];
    PRINT '✓ Đã xóa dữ liệu từ Projects';
END

-- =============================================
-- RESET IDENTITY (Tùy chọn)
-- Uncomment nếu muốn reset ID về 1
-- =============================================

/*
DBCC CHECKIDENT ('Projects', RESEED, 0);
DBCC CHECKIDENT ('ProjectMembers', RESEED, 0);
DBCC CHECKIDENT ('ProjectSteps', RESEED, 0);
DBCC CHECKIDENT ('TechTransferRequests', RESEED, 0);
DBCC CHECKIDENT ('NDAAgreements', RESEED, 0);
DBCC CHECKIDENT ('RFQRequests', RESEED, 0);
DBCC CHECKIDENT ('ProposalSubmissions', RESEED, 0);
DBCC CHECKIDENT ('NegotiationForms', RESEED, 0);
DBCC CHECKIDENT ('LegalReviewForms', RESEED, 0);
DBCC CHECKIDENT ('EContracts', RESEED, 0);
DBCC CHECKIDENT ('AdvancePaymentConfirmations', RESEED, 0);
DBCC CHECKIDENT ('PilotTestReports', RESEED, 0);
DBCC CHECKIDENT ('HandoverReports', RESEED, 0);
DBCC CHECKIDENT ('TrainingHandovers', RESEED, 0);
DBCC CHECKIDENT ('TechnicalDocHandovers', RESEED, 0);
DBCC CHECKIDENT ('AcceptanceReports', RESEED, 0);
DBCC CHECKIDENT ('LiquidationReports', RESEED, 0);
PRINT '';
PRINT '✓ Đã reset tất cả IDENTITY về 0';
*/

PRINT '';
PRINT '================================================';
PRINT 'Hoàn thành! Tất cả dữ liệu dự án đã được xóa.';
PRINT 'Database đã sẵn sàng cho workflow 14 bước mới.';
PRINT '================================================';
GO
