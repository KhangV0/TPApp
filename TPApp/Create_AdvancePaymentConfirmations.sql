CREATE TABLE [dbo].[AdvancePaymentConfirmations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EContractId] [int] NULL,
	[DuAnId] [int] NULL,
	[SoTienTamUng] [decimal](18, 2) NOT NULL,
	[ChungTuChuyenTienFile] [nvarchar](max) NULL,
	[NgayChuyen] [datetime] NOT NULL,
	[DaXacNhanNhanTien] [bit] NOT NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_AdvancePaymentConfirmations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
