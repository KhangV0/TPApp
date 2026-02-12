CREATE TABLE [dbo].[RFQRequests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MaRFQ] [nvarchar](50) NOT NULL,
	[YeuCauKyThuat] [nvarchar](max) NOT NULL,
	[TieuChuanNghiemThu] [nvarchar](max) NULL,
	[HanChotNopHoSo] [datetime] NOT NULL,
	[DuAnId] [int] NULL,
	[DaGuiNhaCungUng] [bit] NOT NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_RFQRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
