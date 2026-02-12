CREATE TABLE [dbo].[ProjectSteps](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProjectId] [int] NOT NULL,
	[StepNumber] [int] NOT NULL,
	[StepName] [nvarchar](200) NOT NULL,
	[StatusId] [int] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[CompletedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_ProjectSteps] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProjectSteps] ADD  DEFAULT ((0)) FOR [StatusId]
GO

ALTER TABLE [dbo].[ProjectSteps] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
