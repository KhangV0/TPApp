CREATE TABLE [dbo].[NDAAgreements](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BenA] [nvarchar](200) NOT NULL,
	[BenB] [nvarchar](200) NOT NULL,
	[LoaiNDA] [nvarchar](100) NOT NULL,
	[ThoiHanBaoMat] [nvarchar](100) NOT NULL,
	[XacNhanKySo] [nvarchar](100) NULL,
	[DuAnId] [int] NULL,
	[DaDongY] [bit] NOT NULL,
	[StatusId] [int] NOT NULL DEFAULT ((1)),
	[NguoiTao] [nvarchar](450) NULL,
	[NgayTao] [datetime] NOT NULL DEFAULT (getdate()),
	[NguoiSua] [nvarchar](450) NULL,
	[NgaySua] [datetime] NULL,
 CONSTRAINT [PK_NDAAgreements] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
