CREATE TABLE [dbo].[ImplementationLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DuAnId] [int] NULL,
	[EContractId] [int] NULL,
	[GiaiDoan] [nvarchar](100) NOT NULL,
	[KetQuaThucHien] [nvarchar](max) NULL,
	[HinhAnhVideoFile] [nvarchar](max) NULL,
	[BienBanXacNhanFile] [nvarchar](max) NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_ImplementationLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
