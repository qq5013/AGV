USE [master]
GO
/****** Object:  Database [AgvWarehoueDb]    Script Date: 11/26/2016 01:47:37 ******/
CREATE DATABASE [AgvWarehoueDb] ON  PRIMARY 
( NAME = N'AgvWarehoueDb', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER20082\MSSQL\DATA\AgvWarehoueDb.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'AgvWarehoueDb_log', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER20082\MSSQL\DATA\AgvWarehoueDb_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [AgvWarehoueDb] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [AgvWarehoueDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [AgvWarehoueDb] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET ANSI_NULLS OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET ANSI_PADDING OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET ARITHABORT OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [AgvWarehoueDb] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [AgvWarehoueDb] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [AgvWarehoueDb] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET  DISABLE_BROKER
GO
ALTER DATABASE [AgvWarehoueDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [AgvWarehoueDb] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [AgvWarehoueDb] SET  READ_WRITE
GO
ALTER DATABASE [AgvWarehoueDb] SET RECOVERY SIMPLE
GO
ALTER DATABASE [AgvWarehoueDb] SET  MULTI_USER
GO
ALTER DATABASE [AgvWarehoueDb] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [AgvWarehoueDb] SET DB_CHAINING OFF
GO
USE [AgvWarehoueDb]
GO
/****** Object:  Table [dbo].[Warehouse]    Script Date: 11/26/2016 01:47:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Warehouse](
	[AgvId] [int] NOT NULL,
	[WHNr] [varchar](50) NOT NULL,
 CONSTRAINT [PK_WareHouse] PRIMARY KEY CLUSTERED 
(
	[WHNr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_WareHouse] UNIQUE NONCLUSTERED 
(
	[AgvId] ASC,
	[WHNr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StockTask]    Script Date: 11/26/2016 01:47:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StockTask](
	[id] [int] NOT NULL,
	[BoxType] [int] NULL,
	[PositionFloor] [int] NULL,
	[PositionColumn] [int] NULL,
	[PositionRow] [int] NULL,
	[AgvPassFlag] [int] NULL,
	[RestPositionFlag] [int] NULL,
	[BarCode] [varchar](50) NULL,
	[State] [int] NULL,
	[Type] [int] NULL,
	[CreatedAt] [datetime] NULL,
 CONSTRAINT [PK_StockTask] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Part]    Script Date: 11/26/2016 01:47:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Part](
	[PartNr] [varchar](50) NOT NULL,
	[BoxType] [int] NULL,
 CONSTRAINT [PK_Part] PRIMARY KEY CLUSTERED 
(
	[PartNr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Movement]    Script Date: 11/26/2016 01:47:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Movement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SourcePosition] [varchar](50) NOT NULL,
	[AimedPosition] [varchar](50) NOT NULL,
	[OperationType] [int] NOT NULL,
	[Operator] [varchar](50) NOT NULL,
	[Time] [datetime] NOT NULL,
	[CreatedAt] [datetime] NULL,
 CONSTRAINT [PK_Movement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UniqueItem]    Script Date: 11/26/2016 01:47:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UniqueItem](
	[UniqNr] [varchar](50) NOT NULL,
	[PartNr] [varchar](50) NOT NULL,
	[State] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Item] PRIMARY KEY CLUSTERED 
(
	[UniqNr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Position]    Script Date: 11/26/2016 01:47:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Position](
	[PositionNr] [varchar](50) NOT NULL,
	[WHNr] [varchar](50) NOT NULL,
	[Floor] [int] NOT NULL,
	[Column] [int] NOT NULL,
	[Row] [int] NOT NULL,
	[State] [int] NULL,
 CONSTRAINT [PK_Posation] PRIMARY KEY CLUSTERED 
(
	[PositionNr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[UniqueItemView]    Script Date: 11/26/2016 01:47:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[UniqueItemView]
AS
SELECT     dbo.UniqueItem.UniqNr, dbo.UniqueItem.PartNr, dbo.UniqueItem.State, dbo.UniqueItem.CreatedAt, dbo.Part.BoxType
FROM         dbo.UniqueItem INNER JOIN
                      dbo.Part ON dbo.UniqueItem.PartNr = dbo.Part.PartNr
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "UniqueItem"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 167
               Right = 180
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Part"
            Begin Extent = 
               Top = 6
               Left = 218
               Bottom = 95
               Right = 360
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'UniqueItemView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'UniqueItemView'
GO
/****** Object:  Table [dbo].[Storage]    Script Date: 11/26/2016 01:47:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Storage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PositionNr] [varchar](50) NOT NULL,
	[PartNr] [varchar](50) NOT NULL,
	[FIFO] [datetime] NOT NULL,
	[UniqItemNr] [varchar](50) NULL,
	[CreatedAt] [datetime] NULL,
 CONSTRAINT [PK_Storage_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  ForeignKey [FK_Item_Part]    Script Date: 11/26/2016 01:47:39 ******/
ALTER TABLE [dbo].[UniqueItem]  WITH CHECK ADD  CONSTRAINT [FK_Item_Part] FOREIGN KEY([PartNr])
REFERENCES [dbo].[Part] ([PartNr])
GO
ALTER TABLE [dbo].[UniqueItem] CHECK CONSTRAINT [FK_Item_Part]
GO
/****** Object:  ForeignKey [FK_Posation_WareHouse]    Script Date: 11/26/2016 01:47:39 ******/
ALTER TABLE [dbo].[Position]  WITH CHECK ADD  CONSTRAINT [FK_Posation_WareHouse] FOREIGN KEY([WHNr])
REFERENCES [dbo].[Warehouse] ([WHNr])
GO
ALTER TABLE [dbo].[Position] CHECK CONSTRAINT [FK_Posation_WareHouse]
GO
/****** Object:  ForeignKey [FK_Storage_Part]    Script Date: 11/26/2016 01:47:41 ******/
ALTER TABLE [dbo].[Storage]  WITH CHECK ADD  CONSTRAINT [FK_Storage_Part] FOREIGN KEY([PartNr])
REFERENCES [dbo].[Part] ([PartNr])
GO
ALTER TABLE [dbo].[Storage] CHECK CONSTRAINT [FK_Storage_Part]
GO
/****** Object:  ForeignKey [FK_Storage_Posation]    Script Date: 11/26/2016 01:47:41 ******/
ALTER TABLE [dbo].[Storage]  WITH CHECK ADD  CONSTRAINT [FK_Storage_Posation] FOREIGN KEY([PositionNr])
REFERENCES [dbo].[Position] ([PositionNr])
GO
ALTER TABLE [dbo].[Storage] CHECK CONSTRAINT [FK_Storage_Posation]
GO
/****** Object:  ForeignKey [FK_Storage_UniqueItem]    Script Date: 11/26/2016 01:47:41 ******/
ALTER TABLE [dbo].[Storage]  WITH CHECK ADD  CONSTRAINT [FK_Storage_UniqueItem] FOREIGN KEY([UniqItemNr])
REFERENCES [dbo].[UniqueItem] ([UniqNr])
GO
ALTER TABLE [dbo].[Storage] CHECK CONSTRAINT [FK_Storage_UniqueItem]
GO
