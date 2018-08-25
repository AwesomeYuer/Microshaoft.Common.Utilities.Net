USE [master]
GO
/****** Object:  Database [Test]    Script Date: 8/25/2018 10:31:29 PM ******/
CREATE DATABASE [Test]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Test', FILENAME = N'd:\mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\Test.mdf' , SIZE = 139264KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Test_log', FILENAME = N'd:\mssql\MSSQL14.MSSQLSERVER\MSSQL\DATA\Test_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [Test] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Test].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Test] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Test] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Test] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Test] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Test] SET ARITHABORT OFF 
GO
ALTER DATABASE [Test] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Test] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Test] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Test] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Test] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Test] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Test] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Test] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Test] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Test] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Test] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Test] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Test] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Test] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Test] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Test] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Test] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Test] SET RECOVERY FULL 
GO
ALTER DATABASE [Test] SET  MULTI_USER 
GO
ALTER DATABASE [Test] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Test] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Test] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Test] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Test] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Test', N'ON'
GO
ALTER DATABASE [Test] SET QUERY_STORE = OFF
GO
USE [Test]
GO
ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [Test]
GO
/****** Object:  UserDefinedTableType [dbo].[udt_vcidt]    Script Date: 8/25/2018 10:31:29 PM ******/
CREATE TYPE [dbo].[udt_vcidt] AS TABLE(
	[varchar] [varchar](16) NULL,
	[int] [int] NULL,
	[date] [date] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[zTVF_SplitStringToTable]    Script Date: 8/25/2018 10:31:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[zTVF_SplitStringToTable]
(
	@Text   nvarchar(MAX),  --待分拆的字符串
	@Separator varchar(10) = ','     --数据分隔符
)
RETURNS
	@Table
TABLE
	(
		id int
		, F varchar(32)
	)
AS
/*
select *
from [zTVF_SplitStringToTable](',,4、4,,,5、5,7；7,7;7,',',')
*/

BEGIN
	--select
	--	@Text = replace(@Text, a.[Separator], ',')
	--from
	--	[Separators] a with(nolock)

	set @Text = replace(@Text,' ','')
	DECLARE @SeparatorLen int
	SET @SeparatorLen = LEN(@Separator + '$') - 2
	set @Text = replace(@Text,' ','')
	declare @i int
	set @i = 1
	WHILE
		(
			CHARINDEX(@Separator,@Text )>0
		)
	BEGIN
		declare @v varchar(1000)
		set @v = (LEFT(@Text ,CHARINDEX(@Separator,@Text )-1))
		INSERT
			@Table (id,F)
		select
			@i,@v
		where
			rtrim(ltrim(@v)) != '' 
			and
				not exists
					(
						select
							1
						from
							@Table
						where
							F = @v
					)
		if @@rowcount > 0
		begin
			set @i = @i + 1
		end
		SET @Text = STUFF(@Text ,1,CHARINDEX(@Separator,@Text )+@SeparatorLen,'')
	END
	INSERT @Table
		(
			id
			,F
		)
	select
		@i
		, @Text
	where
			rtrim(ltrim(@Text)) != ''
			and
			not exists
				(
					select
						1
					from
						@Table
					where
						F = @Text
				)
	return
end
GO
/****** Object:  Table [dbo].[zObjectsChangesLogsHistory]    Script Date: 8/25/2018 10:31:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[zObjectsChangesLogsHistory](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[DatabaseName] [sysname] NULL,
	[EventType] [varchar](50) NULL,
	[ObjectName] [varchar](256) NULL,
	[ObjectType] [varchar](25) NULL,
	[TSQLCommand] [nvarchar](max) NULL,
	[LoginName] [varchar](256) NULL,
	[HostName] [varchar](256) NULL,
	[PostTime] [datetime] NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_zObjectsChangesLogsHistory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[zObjectsChangesLogsHistory] ADD  CONSTRAINT [DF_zObjectsChangesLogsHistory_PostTime]  DEFAULT (getdate()) FOR [PostTime]
GO
/****** Object:  StoredProcedure [dbo].[zsp_Test]    Script Date: 8/25/2018 10:31:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create proc [dbo].[zsp_Test]
(	
	@image						image						= null
	,@text						text						= null
	,@uniqueidentifier			uniqueidentifier			= null
	,@date						date						= null
	,@time						time						= null
	,@datetime2					datetime2					= null
	,@datetimeoffset			datetimeoffset				= null
	,@tinyint					tinyint						= null
	,@smallint					smallint					= null
	,@int						int							= null
	,@smalldatetime				smalldatetime				= null
	,@real						real						= null
	,@money						money						= null
	,@datetime					datetime					--= null
	,@float						float						= null
	,@sql_variant				sql_variant					= null
	,@ntext						ntext						= null
	,@bit						bit							= null
	,@decimal					decimal				(38,16)	= null
	,@numeric					numeric				(38,16)	= null
	,@smallmoney				smallmoney					= null
	,@bigint					bigint						= null
	,@hierarchyid				hierarchyid					= null
	,@geometry					geometry					= null
	,@geography					geography					= null
	,@varbinary					varbinary			(16)	= null
	,@varchar					varchar				(16)	= null
	,@binary					binary				(16)	= null
	,@char						char				(16)	= null
	,@timestamp					timestamp					= null
	,@nvarchar					nvarchar			(16)	= null
	,@nchar						nchar				(16)	= null
	,@xml						xml							= null
	,@sysname					sysname						= null out

	,@udt_vcidt					udt_vcidt readonly
	,@udt_vcidt2					udt_vcidt readonly
)
as
begin
/*
	exec [zsp_GetDatesAfter] '2018-01-05'
		
*/
--return
if (@datetime is null)
begin
	set @datetime = getdate()
end

if (@date is null)
begin
	set @date = @datetime
end

if (@uniqueidentifier is null)
begin
	set @uniqueidentifier = newid()
end

select
	a.[varchar]					as a_udt_varchar
	, a.[int]					as a_udt_int
	, a.[date]					as a_udt_date
	--, b.[varchar]				as b_udt_varchar
	--, b.[int]					as b_udt_int
	--, b.[date]					as b_udt_date
	,@image						as image								
	,@text						as text					
	,@uniqueidentifier			as uniqueidentifier		
	,@date						as date					
	,@time						as time					
	,@datetime2					as datetime2				
	,@datetimeoffset			as datetimeoffset		
	,@tinyint					as tinyint				
	,@smallint					as smallint				
	,@int						as int					
	,@smalldatetime				as smalldatetime			
	,@real						as real					
	,@money						as money					
	,@datetime					as datetime				
	,@float						as float					
	,@sql_variant				as sql_variant			
	,@ntext						as ntext					
	,@bit						as bit					
	,@decimal					as decimal				
	,@numeric					as numeric				
	,@smallmoney				as smallmoney			
	,@bigint					as bigint				
	,@hierarchyid				as hierarchyid			
	,@geometry					as geometry				
	,@geography					as geography				
	,@varbinary					as varbinary				
	,@varchar					as varchar				
	,@binary					as binary				
	,@char						as char					
	,@timestamp					as timestamp				
	,@nvarchar					as nvarchar				
	,@nchar						as nchar					
	,@xml						as xml					
	,@sysname					as sysname				


from
	--sys.types a
	@udt_vcidt a
	--, @udt_vcidt2 b

set @sysname = 'aaaaaaaaaaa'

end
GO
/****** Object:  StoredProcedure [dbo].[zsp_zObjectsChangesLogsHistory_Get]    Script Date: 8/25/2018 10:31:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[zsp_zObjectsChangesLogsHistory_Get]
(
	@SearchObjectName		varchar(256) = null
	, @SearchBeforeTime		datetime = null
	, @SearchIdsList		varchar(64) = null
	, @Top					int = 100
)
as
/*

exec [zsp_zObjectsChangesLogsHistory_Get] @SearchIdsList='4,2'

*/

begin
if (@SearchObjectName is not null)
begin
	select
		Top (@Top)
		   a.[ID]
		  ,a.[DatabaseName]
		  ,a.[EventType]
		  ,a.[ObjectName]
		  ,a.[ObjectType]
		  --,a.[TSQLCommand]
		  ,a.[LoginName]
		  ,a.[HostName]
		  ,a.[PostTime]
		  ,a.[Version]
	from
		[dbo].[zObjectsChangesLogsHistory] a with(nolock)
	where
		a.ObjectName like '%' + @SearchObjectName + '%'
		and
		(
			@SearchBeforeTime is null
			or
			a.PostTime <= @SearchBeforeTime
		)
	order by
		a.ObjectName
		, a.ID desc
end
else if (@SearchIdsList is not null)
begin

	;with T
	as
	(
		select
			cast(a.F as int) as F
			, a.id
		from
			dbo.[zTVF_SplitStringToTable]
				(
					@SearchIdsList
					, ','
				) a
	)
	select
	   a.[ID]
      ,a.[DatabaseName]
      ,a.[EventType]
      ,a.[ObjectName]
      ,a.[ObjectType]
      ,a.[TSQLCommand]
      ,a.[LoginName]
      ,a.[HostName]
      ,a.[PostTime]
      ,a.[Version]
	from
		[zObjectsChangesLogsHistory] a with(nolock)
			inner join
				T b
					on
						a.ID = b.F
	order by
		b.id
end
	
end
GO
/****** Object:  DdlTrigger [ztrigger_ddl]    Script Date: 8/25/2018 10:31:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE TRIGGER [ztrigger_ddl]
	ON DATABASE
	FOR
		CREATE_PROCEDURE
		, ALTER_PROCEDURE
		, DROP_PROCEDURE
		, CREATE_TABLE
		, ALTER_TABLE
		, DROP_TABLE
		, CREATE_FUNCTION
		, ALTER_FUNCTION
		, DROP_FUNCTION
		, CREATE_TRIGGER
		, ALTER_TRIGGER
		, DROP_TRIGGER
		, CREATE_VIEW
		, ALTER_VIEW
		, DROP_VIEW
		, CREATE_INDEX
		, ALTER_INDEX
		, DROP_INDEX
		, CREATE_TYPE
		, DROP_TYPE
AS
begin
--SET NOCOUNT ON
DECLARE
		@DatabaseName varchar(256)
		, @EventType varchar(50)
		, @ObjectName varchar(256)
		, @ObjectType varchar(25)
		, @TSQLCommand nvarchar(max)
		, @LoginName varchar(256)
--		, @SPID int

DECLARE @data XML = EVENTDATA()
set @DatabaseName = @data.value('(/EVENT_INSTANCE/DatabaseName)[1]', 'varchar(256)')
			--,@EventType = @data.value('(/EVENT_INSTANCE/EventType)[1]', 'varchar(50)')
set @ObjectName = @data.value('(/EVENT_INSTANCE/ObjectName)[1]', 'varchar(256)')
			--,@ObjectType = @data.value('(/EVENT_INSTANCE/ObjectType)[1]', 'varchar(25)') 
			--,@TSQLCommand = @data.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'nvarchar(max)') 
			--,@LoginName = @data.value('(/EVENT_INSTANCE/LoginName)[1]', 'varchar(256)')
			--,@SPID = @data.value('(/EVENT_INSTANCE/SPID)[1]', 'int')
INSERT INTO
		[zObjectsChangesLogsHistory]
		(
			  [DatabaseName]
			, [EventType]
			, [ObjectName]
			, [ObjectType]
			, [TSQLCommand]
			, [LoginName]
			, [HostName]
			--, [PostTime]
			, [Version]
		)
--select
values
		(
			@DatabaseName		-- = @data.value('(/EVENT_INSTANCE/DatabaseName)[1]', 'varchar(256)')
			, @data.value('(/EVENT_INSTANCE/EventType)[1]', 'varchar(50)')
			, @ObjectName		--= @data.value('(/EVENT_INSTANCE/ObjectName)[1]', 'varchar(256)')
			, @data.value('(/EVENT_INSTANCE/ObjectType)[1]', 'varchar(25)') 
			, @data.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'nvarchar(max)') 
			, @data.value('(/EVENT_INSTANCE/LoginName)[1]', 'varchar(256)')
			--, @SPID	= @data.value('(/EVENT_INSTANCE/SPID)[1]', 'int')
			, HOST_NAME()
			--, GETDATE()
			,
				(
					select
						isnull(max([version]), 0) + 1
					from
						[zObjectsChangesLogsHistory] a
					where
						a.[DatabaseName] = @DatabaseName
						and
						a.[ObjectName] = @ObjectName
				)
		)
		
END







GO
ENABLE TRIGGER [ztrigger_ddl] ON DATABASE
GO
USE [master]
GO
ALTER DATABASE [Test] SET  READ_WRITE 
GO
