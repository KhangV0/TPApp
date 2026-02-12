CREATE TABLE [dbo].[HandoverReports](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DuAnId] [int] NULL,
	[EContractId] [int] NULL,
	[DanhMucThietBiJson] [nvarchar](max) NULL,
	[DanhMucHoSoJson] [nvarchar](max) NULL,
	[DaHoanThanhDaoTao] [bit] NOT NULL DEFAULT ((0)),
	[DanhGiaSao] [int] NULL,
	[NhanXet] [nvarchar](max) NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_HandoverReports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
