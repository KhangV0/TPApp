-- =============================================
-- Create Table: PilotTestReports (Step 9)
-- Database: TechMartNew
-- =============================================

USE [TechMartNew]
GO

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
 CONSTRAINT [PK_PilotTestReports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[PilotTestReports] ADD  DEFAULT ((0)) FOR [DatYeuCau]
GO

ALTER TABLE [dbo].[PilotTestReports] ADD  DEFAULT ((1)) FOR [StatusId]
GO

ALTER TABLE [dbo].[PilotTestReports] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO

PRINT 'Table PilotTestReports created successfully!';
