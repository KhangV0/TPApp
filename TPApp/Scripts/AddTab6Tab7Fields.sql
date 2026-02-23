-- =============================================
-- Add 4 new columns for Tab 6 & Tab 7
-- GiaBanDuKien, ChiPhiPhatSinh, BaoHanhHoTro, BrochureUrl
-- =============================================

ALTER TABLE SanPhamCNTBs
ADD GiaBanDuKien NVARCHAR(MAX) NULL;
GO

ALTER TABLE SanPhamCNTBs
ADD ChiPhiPhatSinh NVARCHAR(MAX) NULL;
GO

ALTER TABLE SanPhamCNTBs
ADD BaoHanhHoTro NVARCHAR(MAX) NULL;
GO

ALTER TABLE SanPhamCNTBs
ADD BrochureUrl NVARCHAR(MAX) NULL;
GO
