CREATE TABLE [dbo].[EContracts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RFQId] [int] NULL,
	[DuAnId] [int] NULL,
	[SoHopDong] [nvarchar](50) NOT NULL,
	[FileHopDong] [nvarchar](max) NULL,
	[NguoiKyBenA] [nvarchar](200) NULL,
	[NguoiKyBenB] [nvarchar](200) NULL,
	[TrangThaiKy] [nvarchar](50) NOT NULL DEFAULT (N'Chưa ký'),
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_EContracts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
