-- =============================================
-- Create Table: TechnicalDocHandovers (Step 12)
-- Database: TechMartNew
-- =============================================

USE [TechMartNew]
GO

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
 CONSTRAINT [PK_TechnicalDocHandovers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TechnicalDocHandovers] ADD  DEFAULT ((0)) FOR [DaBanGiaoDayDu]
GO

ALTER TABLE [dbo].[TechnicalDocHandovers] ADD  DEFAULT ((1)) FOR [StatusId]
GO

ALTER TABLE [dbo].[TechnicalDocHandovers] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO

PRINT 'Table TechnicalDocHandovers created successfully!';
