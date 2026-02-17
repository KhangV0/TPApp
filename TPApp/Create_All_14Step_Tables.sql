-- =============================================
-- CREATE ALL NEW TABLES FOR 14-STEP WORKFLOW
-- Database: TechMartNew
-- Run this script to create all 4 new tables at once
-- =============================================

USE [TechMartNew]
GO

PRINT '================================================';
PRINT 'Starting creation of 4 new tables for 14-step workflow';
PRINT '================================================';
PRINT '';

-- =============================================
-- TABLE 1: LegalReviewForms (Step 6)
-- =============================================
PRINT 'Creating table: LegalReviewForms...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LegalReviewForms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LegalReviewForms](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectId] [int] NULL,
        [NegotiationFormId] [int] NULL,
        [NguoiKiemTra] [nvarchar](200) NOT NULL,
        [KetQuaKiemTra] [nvarchar](max) NOT NULL,
        [VanDePhapLy] [nvarchar](max) NULL,
        [DeXuatChinhSua] [nvarchar](max) NULL,
        [FileKiemTra] [nvarchar](500) NULL,
        [DaDuyet] [bit] NOT NULL,
        [NgayKiemTra] [datetime2](7) NULL,
        [StatusId] [int] NOT NULL,
        [NguoiTao] [int] NULL,
        [NgayTao] [datetime2](7) NOT NULL,
        [NguoiSua] [int] NULL,
        [NgaySua] [datetime2](7) NULL,
     CONSTRAINT [PK_LegalReviewForms] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[LegalReviewForms] ADD DEFAULT ((0)) FOR [DaDuyet]
    ALTER TABLE [dbo].[LegalReviewForms] ADD DEFAULT ((1)) FOR [StatusId]
    ALTER TABLE [dbo].[LegalReviewForms] ADD DEFAULT (getdate()) FOR [NgayTao]
    
    PRINT '✓ Table LegalReviewForms created successfully!';
END
ELSE
    PRINT '⚠ Table LegalReviewForms already exists, skipping...';

PRINT '';

-- =============================================
-- TABLE 2: PilotTestReports (Step 9)
-- =============================================
PRINT 'Creating table: PilotTestReports...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PilotTestReports]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PilotTestReports](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectId] [int] NULL,
        [EContractId] [int] NULL,
        [MoTaThuNghiem] [nvarchar](max) NOT NULL,
        [KetQuaThuNghiem] [nvarchar](max) NOT NULL,
        [VanDePhatSinh] [nvarchar](max) NULL,
        [GiaiPhap] [nvarchar](max) NULL,
        [FileKetQua] [nvarchar](500) NULL,
        [FileBaoCao] [nvarchar](500) NULL,
        [DatYeuCau] [bit] NOT NULL,
        [NgayBatDau] [datetime2](7) NULL,
        [NgayKetThuc] [datetime2](7) NULL,
        [StatusId] [int] NOT NULL,
        [NguoiTao] [int] NULL,
        [NgayTao] [datetime2](7) NOT NULL,
        [NguoiSua] [int] NULL,
        [NgaySua] [datetime2](7) NULL,
     CONSTRAINT [PK_PilotTestReports] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[PilotTestReports] ADD DEFAULT ((0)) FOR [DatYeuCau]
    ALTER TABLE [dbo].[PilotTestReports] ADD DEFAULT ((1)) FOR [StatusId]
    ALTER TABLE [dbo].[PilotTestReports] ADD DEFAULT (getdate()) FOR [NgayTao]
    
    PRINT '✓ Table PilotTestReports created successfully!';
END
ELSE
    PRINT '⚠ Table PilotTestReports already exists, skipping...';

PRINT '';

-- =============================================
-- TABLE 3: TrainingHandovers (Step 11)
-- =============================================
PRINT 'Creating table: TrainingHandovers...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TrainingHandovers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TrainingHandovers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectId] [int] NULL,
        [EContractId] [int] NULL,
        [NoiDungDaoTao] [nvarchar](max) NOT NULL,
        [DanhSachNguoiThamGia] [nvarchar](max) NULL,
        [SoNguoiThamGia] [int] NULL,
        [SoGioDaoTao] [int] NULL,
        [TaiLieuDaoTao] [nvarchar](500) NULL,
        [VideoHuongDan] [nvarchar](500) NULL,
        [BienBanDaoTao] [nvarchar](500) NULL,
        [DaHoanThanh] [bit] NOT NULL,
        [NgayBatDau] [datetime2](7) NULL,
        [NgayKetThuc] [datetime2](7) NULL,
        [StatusId] [int] NOT NULL,
        [NguoiTao] [int] NULL,
        [NgayTao] [datetime2](7) NOT NULL,
        [NguoiSua] [int] NULL,
        [NgaySua] [datetime2](7) NULL,
     CONSTRAINT [PK_TrainingHandovers] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[TrainingHandovers] ADD DEFAULT ((0)) FOR [DaHoanThanh]
    ALTER TABLE [dbo].[TrainingHandovers] ADD DEFAULT ((1)) FOR [StatusId]
    ALTER TABLE [dbo].[TrainingHandovers] ADD DEFAULT (getdate()) FOR [NgayTao]
    
    PRINT '✓ Table TrainingHandovers created successfully!';
END
ELSE
    PRINT '⚠ Table TrainingHandovers already exists, skipping...';

PRINT '';

-- =============================================
-- TABLE 4: TechnicalDocHandovers (Step 12)
-- =============================================
PRINT 'Creating table: TechnicalDocHandovers...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TechnicalDocHandovers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TechnicalDocHandovers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProjectId] [int] NULL,
        [EContractId] [int] NULL,
        [DanhMucHoSo] [nvarchar](max) NOT NULL,
        [SourceCode] [nvarchar](500) NULL,
        [TaiLieuKyThuat] [nvarchar](500) NULL,
        [TaiLieuHuongDanSuDung] [nvarchar](500) NULL,
        [TaiLieuBaoTri] [nvarchar](500) NULL,
        [Database] [nvarchar](500) NULL,
        [GhiChu] [nvarchar](max) NULL,
        [DaBanGiaoDayDu] [bit] NOT NULL,
        [NgayBanGiao] [datetime2](7) NULL,
        [StatusId] [int] NOT NULL,
        [NguoiTao] [int] NULL,
        [NgayTao] [datetime2](7) NOT NULL,
        [NguoiSua] [int] NULL,
        [NgaySua] [datetime2](7) NULL,
     CONSTRAINT [PK_TechnicalDocHandovers] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[TechnicalDocHandovers] ADD DEFAULT ((0)) FOR [DaBanGiaoDayDu]
    ALTER TABLE [dbo].[TechnicalDocHandovers] ADD DEFAULT ((1)) FOR [StatusId]
    ALTER TABLE [dbo].[TechnicalDocHandovers] ADD DEFAULT (getdate()) FOR [NgayTao]
    
    PRINT '✓ Table TechnicalDocHandovers created successfully!';
END
ELSE
    PRINT '⚠ Table TechnicalDocHandovers already exists, skipping...';

PRINT '';
PRINT '================================================';
PRINT 'All tables created successfully!';
PRINT 'Your database now supports 14-step workflow.';
PRINT '================================================';
GO
