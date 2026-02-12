CREATE TABLE [dbo].[TechTransferRequests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[ChucVu] [nvarchar](100) NULL,
	[DonVi] [nvarchar](200) NULL,
	[DiaChi] [nvarchar](200) NULL,
	[DienThoai] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[TenCongNghe] [nvarchar](200) NOT NULL,
	[MoTaNhuCau] [nvarchar](max) NOT NULL,
	[LinhVuc] [nvarchar](100) NULL,
	[NganSachDuKien] [decimal](18, 2) NULL,
	[DuAnId] [int] NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_TechTransferRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
