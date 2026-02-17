-- =============================================
-- Create Table: LegalReviewForms (Step 6)
-- Database: TechMartNew
-- =============================================

USE [TechMartNew]
GO

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
 CONSTRAINT [PK_LegalReviewForms] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LegalReviewForms] ADD  DEFAULT ((0)) FOR [DaDuyet]
GO

ALTER TABLE [dbo].[LegalReviewForms] ADD  DEFAULT ((1)) FOR [StatusId]
GO

ALTER TABLE [dbo].[LegalReviewForms] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO

PRINT 'Table LegalReviewForms created successfully!';
