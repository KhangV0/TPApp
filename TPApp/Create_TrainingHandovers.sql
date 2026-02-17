-- =============================================
-- Create Table: TrainingHandovers (Step 11)
-- Database: TechMartNew
-- =============================================

USE [TechMartNew]
GO

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
 CONSTRAINT [PK_TrainingHandovers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TrainingHandovers] ADD  DEFAULT ((0)) FOR [DaHoanThanh]
GO

ALTER TABLE [dbo].[TrainingHandovers] ADD  DEFAULT ((1)) FOR [StatusId]
GO

ALTER TABLE [dbo].[TrainingHandovers] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO

PRINT 'Table TrainingHandovers created successfully!';
