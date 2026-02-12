CREATE TABLE [dbo].[ProposalSubmissions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RFQId] [int] NULL,
	[DuAnId] [int] NULL,
	[GiaiPhapKyThuat] [nvarchar](max) NOT NULL,
	[BaoGiaSoBo] [decimal](18, 2) NULL,
	[ThoiGianTrienKhai] [nvarchar](200) NOT NULL,
	[HoSoNangLucDinhKem] [nvarchar](max) NOT NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_ProposalSubmissions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
