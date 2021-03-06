DROP TABLE [dbo].[TagPhotoes]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 06/10/2011 17:36:10 ******/
DROP TABLE [dbo].[Tags]
GO
/****** Object:  Table [dbo].[Photos]    Script Date: 06/10/2011 17:36:10 ******/
DROP TABLE [dbo].[Photos]
GO
/****** Object:  Table [dbo].[Photos]    Script Date: 06/10/2011 17:36:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Photos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[FileName] [nvarchar](max) NOT NULL,
	[ContentType] [nvarchar](max) NOT NULL,
	[Length] [int] NOT NULL,
	[MessageId] [nvarchar](max) NULL,
	[TakenAt] [datetime] NULL,
	[EntryAt] [datetime] NOT NULL,
	[IsUploaded] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[Sender] [nvarchar](max) NULL,
	[DeleteCode] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 06/10/2011 17:36:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Hash] [int] NOT NULL,
	[Text] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TagPhotoes]    Script Date: 06/10/2011 17:36:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TagPhotoes](
	[Tag_Id] [int] NOT NULL,
	[Photo_Id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Tag_Id] ASC,
	[Photo_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [Tag_Photos_Source]    Script Date: 06/10/2011 17:36:10 ******/
ALTER TABLE [dbo].[TagPhotoes]  WITH CHECK ADD  CONSTRAINT [Tag_Photos_Source] FOREIGN KEY([Tag_Id])
REFERENCES [dbo].[Tags] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TagPhotoes] CHECK CONSTRAINT [Tag_Photos_Source]
GO
/****** Object:  ForeignKey [Tag_Photos_Target]    Script Date: 06/10/2011 17:36:10 ******/
ALTER TABLE [dbo].[TagPhotoes]  WITH CHECK ADD  CONSTRAINT [Tag_Photos_Target] FOREIGN KEY([Photo_Id])
REFERENCES [dbo].[Photos] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TagPhotoes] CHECK CONSTRAINT [Tag_Photos_Target]
GO
