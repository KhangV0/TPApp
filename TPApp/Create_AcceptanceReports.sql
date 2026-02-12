CREATE TABLE [dbo].[AcceptanceReports](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DuAnId] [int] NULL,
	[EContractId] [int] NULL,
	[NgayNghiemThu] [datetime] NOT NULL DEFAULT (getdate()),
	[ThanhPhanThamGia] [nvarchar](max) NULL,
	[KetLuanNghiemThu] [nvarchar](max) NULL,
	[VanDeTonDong] [nvarchar](max) NULL,
	[ChuKyBenA] [nvarchar](450) NULL,
	[ChuKyBenB] [nvarchar](450) NULL,
	[TrangThaiKy] [nvarchar](50) NOT NULL DEFAULT (N'Chưa ký'),
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_AcceptanceReports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
