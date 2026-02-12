CREATE TABLE [dbo].[NegotiationForms](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RFQId] [int] NULL,
	[DuAnId] [int] NULL,
	[GiaChotCuoiCung] [decimal](18, 2) NULL,
	[DieuKhoanThanhToan] [nvarchar](max) NOT NULL,
	[BienBanThuongLuongFile] [nvarchar](max) NULL,
	[HinhThucKy] [nvarchar](50) NOT NULL,
	[DaKySo] [bit] NOT NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_NegotiationForms] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
