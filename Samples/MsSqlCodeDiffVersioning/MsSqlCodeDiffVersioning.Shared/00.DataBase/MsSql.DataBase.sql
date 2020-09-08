USE [master]
GO
/****** Object:  Database [Test]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE DATABASE [Test]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Test', FILENAME = N'D:\MSSQL\Data\Test\Test.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 8192KB )
 LOG ON 
( NAME = N'Test_log', FILENAME = N'D:\MSSQL\Data\Test\Test_log.ldf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 8192KB)
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
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
ALTER DATABASE [Test] SET  DISABLE_BROKER 
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
ALTER DATABASE [Test] SET RECOVERY SIMPLE 
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
/****** Object:  UserDefinedTableType [dbo].[udt_ErrorExceptionLoggingEntry]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE TYPE [dbo].[udt_ErrorExceptionLoggingEntry] AS TABLE(
	[ID] [bigint] NULL,
	[EnqueueTime] [datetime] NULL,
	[DequeueTime] [datetime] NULL,
	[QueueTimingInMilliseconds] [decimal](16, 6) NULL,
	[ErrorExceptionTime] [datetime] NULL,
	[ErrorExceptionSource] [varchar](2048) NULL,
	[ErrorExceptionTraceID] [uniqueidentifier] NULL,
	[ErrorException] [varchar](max) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[udt_int]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE TYPE [dbo].[udt_int] AS TABLE(
	[F1] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[udt_RequestResponseLoggingEntry]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE TYPE [dbo].[udt_RequestResponseLoggingEntry] AS TABLE(
	[ID] [bigint] NULL,
	[EnqueueTime] [datetime] NULL,
	[DequeueTime] [datetime] NULL,
	[QueueTimingInMilliseconds] [decimal](16, 6) NULL,
	[requestTraceID] [uniqueidentifier] NULL,
	[requestUrl] [varchar](4096) NULL,
	[requestPath] [varchar](128) NULL,
	[requestPathBase] [varchar](128) NULL,
	[requestActionRoutePath] [varchar](128) NULL,
	[requestHeaders] [varchar](max) NULL,
	[requestBody] [varchar](max) NULL,
	[requestMethod] [varchar](8) NULL,
	[requestBeginTime] [datetime] NULL,
	[requestContentLength] [bigint] NULL,
	[requestContentType] [varchar](64) NULL,
	[responseHeaders] [varchar](max) NULL,
	[responseBody] [varchar](max) NULL,
	[responseStatusCode] [int] NULL,
	[responseStartingTime] [datetime] NULL,
	[responseContentLength] [bigint] NULL,
	[responseContentType] [varchar](64) NULL,
	[requestResponseTimingInMilliseconds] [decimal](16, 6) NULL,
	[dbExecutingTimingInMilliseconds] [decimal](16, 6) NULL,
	[clientIP] [varchar](64) NULL,
	[locationLongitude] [decimal](24, 18) NULL,
	[locationLatitude] [decimal](24, 18) NULL,
	[userID] [varchar](32) NULL,
	[roleID] [varchar](32) NULL,
	[orgUnitID] [varchar](32) NULL,
	[deviceID] [varchar](64) NULL,
	[deviceInfo] [varchar](64) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[udt_varchar]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE TYPE [dbo].[udt_varchar] AS TABLE(
	[F1] [varchar](16) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[udt_vcidt]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE TYPE [dbo].[udt_vcidt] AS TABLE(
	[varchar] [varchar](16) NULL,
	[int] [int] NULL,
	[date] [date] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[zTVF_SplitStringToTable]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[zTVF_SplitStringToTable]
(
	@Text   nvarchar(MAX),  --���ֲ���ַ���
	@Separator varchar(10) = ','     --���ݷָ���
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
from [zTVF_SplitStringToTable](',,4��4,,,5��5,7��7,7;7,',',')
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
/****** Object:  View [dbo].[zv_all_PARAMETERS]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[zv_all_PARAMETERS]  
AS  
SELECT  
	 DB_NAME()									AS SPECIFIC_CATALOG
	 , SCHEMA_NAME(o.schema_id)					AS SPECIFIC_SCHEMA
	, o.name									AS SPECIFIC_NAME
	, c.parameter_id							AS ORDINAL_POSITION
	,
		convert
			(
				nvarchar(10)
				,
					CASE  
						WHEN
							c.parameter_id = 0
								THEN
									'OUT'  
						WHEN
							c.is_output = 1
								THEN
									'INOUT'  
						ELSE
							'IN'
					END
			)									AS PARAMETER_MODE
	,
		convert
			(
				nvarchar(10)
				,
					CASE
						WHEN  
							c.parameter_id = 0
								THEN
									'YES'
						ELSE
							'NO'
					END
			)											AS IS_RESULT
	, convert(nvarchar(10), 'NO')						AS AS_LOCATOR
	, c.name											AS PARAMETER_NAME
	, ISNULL(TYPE_NAME(c.system_type_id), u.name)		AS DATA_TYPE
	, COLUMNPROPERTY
		(c.object_id, c.name, 'charmaxlen')				AS CHARACTER_MAXIMUM_LENGTH
	, COLUMNPROPERTY
		(c.object_id, c.name, 'octetmaxlen')			AS CHARACTER_OCTET_LENGTH
	, convert(sysname, null)							AS COLLATION_CATALOG
	, convert(sysname, null) collate catalog_default	AS COLLATION_SCHEMA
	,
		convert
			(
				sysname
				,
					CASE  
						WHEN
							c.system_type_id IN (35, 99, 167, 175, 231, 239)
								THEN -- [n]char/[n]varchar/[n]text  
									SERVERPROPERTY('collation')
					END
			)												AS COLLATION_NAME
	, convert( sysname, null)								AS CHARACTER_SET_CATALOG
	, convert( sysname, null) collate catalog_default		AS CHARACTER_SET_SCHEMA
	,
		convert
			(
				sysname
				,
					CASE  
						WHEN
							c.system_type_id IN (35, 167, 175)
								THEN
									SERVERPROPERTY('sqlcharsetname') -- char/varchar/text  
						WHEN
							c.system_type_id IN (99, 231, 239)
								THEN
									N'UNICODE' -- nchar/nvarchar/ntext  
					END
			)												AS CHARACTER_SET_NAME
	, 
		convert
			(
				tinyint
				,
					CASE -- int/decimal/numeric/real/float/money  
						WHEN
							c.system_type_id IN (48, 52, 56, 59, 60, 62, 106, 108, 122, 127)
								THEN
									c.precision  
					END
			)												AS NUMERIC_PRECISION
	,
		convert
			(
				smallint
					,
						CASE -- int/money/decimal/numeric  
							WHEN
								c.system_type_id IN (48, 52, 56, 60, 106, 108, 122, 127)
									THEN
										10  
							WHEN
								c.system_type_id IN (59, 62)
									THEN
										2
						END
			)												AS NUMERIC_PRECISION_RADIX
	, -- real/float  
		convert
			(
				int
				,
					CASE -- datetime/smalldatetime  
						WHEN
							c.system_type_id IN (40, 41, 42, 43, 58, 61) 
								THEN
									NULL  
						ELSE
							ODBCSCALE(c.system_type_id, c.scale)
					END
			)												AS NUMERIC_SCALE
	,  
		convert
			(
				smallint
				,
					CASE -- datetime/smalldatetime  
						WHEN
							c.system_type_id IN (40, 41, 42, 43, 58, 61)
								THEN
									ODBCSCALE(c.system_type_id, c.scale)
					END
			)												AS DATETIME_PRECISION
	, convert(nvarchar(30), null)							AS INTERVAL_TYPE
	, convert(smallint, null)								AS INTERVAL_PRECISION
	, 
		convert
			(
				sysname
				,
					CASE
						WHEN
							u.schema_id <> 4  
								THEN
									DB_NAME()
					END
			)												AS USER_DEFINED_TYPE_CATALOG
	,
		convert
			(
				sysname
				,
					CASE
						WHEN
							u.schema_id <> 4  
								THEN
									SCHEMA_NAME(u.schema_id)
					END
			)												AS USER_DEFINED_TYPE_SCHEMA
	,
		convert
			(
				sysname
				,
					CASE
						WHEN
							u.schema_id <> 4  
								THEN
									u.name
						END
			)												AS USER_DEFINED_TYPE_NAME
	, convert(sysname, null)								AS SCOPE_CATALOG
	, convert(sysname, null) collate catalog_default		AS SCOPE_SCHEMA
	, convert(sysname, null) collate catalog_default		AS SCOPE_NAME  
FROM
	sys.all_objects o
		JOIN
			sys.all_parameters c
				ON
					c.object_id = o.object_id  
		JOIN
			sys.types u
				ON
					u.user_type_id = c.user_type_id  
WHERE
	o.type IN ('P','FN','TF', 'IF', 'IS', 'AF','PC', 'FS', 'FT')  
GO
/****** Object:  Table [dbo].[AppCenters]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppCenters](
	[Model] [varchar](256) NULL,
	[OS] [varchar](256) NULL,
	[Brand] [varchar](32) NULL,
	[Type] [varchar](32) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ErrorExceptionLogging]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorExceptionLogging](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[EnqueueTime] [datetime] NULL,
	[DequeueTime] [datetime] NULL,
	[QueueTimingInMilliseconds] [decimal](16, 6) NULL,
	[ErrorExceptionTime] [datetime] NULL,
	[ErrorExceptionSource] [varchar](2048) NULL,
	[ErrorExceptionTraceID] [uniqueidentifier] NULL,
	[ErrorException] [varchar](max) NULL,
	[serverHostOsPlatformName] [varchar](64) NULL,
	[serverHostOsVersion] [varchar](64) NOT NULL,
	[serverHostFrameworkDescription] [varchar](64) NULL,
	[serverHostProcessId] [int] NULL,
	[serverHostProcessName] [varchar](64) NULL,
	[serverHostProcessStartTime] [datetime] NULL,
	[serverHostProcessWorkingSet64] [bigint] NULL,
	[serverHostProcessPeakWorkingSet64] [bigint] NULL,
	[BatchDateTimeStamp] [datetime] NULL,
	[HostName] [varchar](32) NULL,
	[CreateTime] [datetime] NULL,
 CONSTRAINT [PK_ErrorExceptionLogging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestResponseLogging]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestResponseLogging](
	[AutoID] [bigint] IDENTITY(1,1) NOT NULL,
	[ID] [bigint] NULL,
	[EnqueueTime] [datetime] NULL,
	[DequeueTime] [datetime] NULL,
	[QueueTimingInMilliseconds] [decimal](16, 6) NULL,
	[requestTraceID] [uniqueidentifier] NULL,
	[requestUrl] [varchar](4096) NULL,
	[requestPath] [varchar](128) NULL,
	[requestPathBase] [varchar](128) NULL,
	[requestActionRoutePath] [varchar](128) NULL,
	[requestHeaders] [varchar](max) NULL,
	[requestBody] [varchar](max) NULL,
	[requestMethod] [varchar](8) NULL,
	[requestBeginTime] [datetime] NULL,
	[requestContentLength] [bigint] NULL,
	[requestContentType] [varchar](64) NULL,
	[responseHeaders] [varchar](max) NULL,
	[responseBody] [varchar](max) NULL,
	[responseStatusCode] [int] NULL,
	[responseStartingTime] [datetime] NULL,
	[responseContentLength] [bigint] NULL,
	[responseContentType] [varchar](64) NULL,
	[requestResponseTimingInMilliseconds] [decimal](16, 6) NULL,
	[dbExecutingTimingInMilliseconds] [decimal](16, 6) NULL,
	[serverHostOsPlatformName] [varchar](64) NULL,
	[serverHostOsVersion] [varchar](64) NOT NULL,
	[serverHostFrameworkDescription] [varchar](64) NULL,
	[clientIP] [varchar](64) NULL,
	[locationLongitude] [decimal](24, 18) NULL,
	[locationLatitude] [decimal](24, 18) NULL,
	[userID] [varchar](32) NULL,
	[roleID] [varchar](16) NULL,
	[orgUnitID] [varchar](16) NULL,
	[deviceID] [varchar](64) NULL,
	[deviceInfo] [varchar](128) NULL,
	[serverHostProcessId] [int] NULL,
	[serverHostProcessName] [varchar](64) NULL,
	[serverHostProcessStartTime] [datetime] NULL,
	[serverHostProcessWorkingSet64] [bigint] NULL,
	[serverHostProcessPeakWorkingSet64] [bigint] NULL,
	[BatchDateTimeStamp] [datetime] NULL,
	[HostName] [varchar](32) NULL,
	[CreateTime] [datetime] NULL,
 CONSTRAINT [PK_RequestResponseLogging] PRIMARY KEY CLUSTERED 
(
	[AutoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[zObjectsChangesLogsHistory]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[zObjectsChangesLogsHistory](
	[HistoryID] [bigint] IDENTITY(1,1) NOT NULL,
	[ID] [bigint] NOT NULL,
	[DatabaseServerName] [varchar](64) NULL,
	[DatabaseName] [sysname] NULL,
	[EventType] [varchar](64) NULL,
	[ObjectName] [varchar](256) NULL,
	[ObjectType] [varchar](25) NULL,
	[TSQLCommand] [nvarchar](max) NULL,
	[LoginName] [varchar](256) NULL,
	[UserName] [varchar](256) NULL,
	[PostTime] [datetime] NULL,
	[SPID] [int] NULL,
	[HostName] [varchar](256) NULL,
	[Version] [int] NOT NULL,
	[EventData] [xml] NULL,
 CONSTRAINT [PK_zObjectsChangesLogsHistory] PRIMARY KEY CLUSTERED 
(
	[HistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[zObjectsChangesLogsLatest]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[zObjectsChangesLogsLatest](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[DatabaseServerName] [varchar](64) NOT NULL,
	[DatabaseName] [sysname] NOT NULL,
	[EventType] [varchar](64) NULL,
	[ObjectName] [varchar](256) NULL,
	[ObjectType] [varchar](25) NULL,
	[TSQLCommand] [nvarchar](max) NULL,
	[LoginName] [varchar](256) NULL,
	[UserName] [varchar](256) NULL,
	[PostTime] [datetime] NULL,
	[SPID] [int] NULL,
	[HostName] [varchar](256) NULL,
	[Version] [int] NOT NULL,
	[EventData] [xml] NULL,
 CONSTRAINT [PK_zObjectsChangesLogs] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [Idx_stat]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE NONCLUSTERED INDEX [Idx_stat] ON [dbo].[RequestResponseLogging]
(
	[serverHostProcessStartTime] ASC,
	[serverHostOsPlatformName] ASC,
	[serverHostOsVersion] ASC,
	[AutoID] DESC
)
INCLUDE([requestBeginTime],[responseStartingTime],[requestResponseTimingInMilliseconds]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [nci_unique-objs_history]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [nci_unique-objs_history] ON [dbo].[zObjectsChangesLogsHistory]
(
	[DatabaseServerName] ASC,
	[DatabaseName] ASC,
	[ObjectName] ASC,
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [nci_unique-objs_latest]    Script Date: 9/6/2020 10:45:19 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [nci_unique-objs_latest] ON [dbo].[zObjectsChangesLogsLatest]
(
	[DatabaseServerName] ASC,
	[DatabaseName] ASC,
	[ObjectName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ErrorExceptionLogging] ADD  CONSTRAINT [DF_ErrorExceptionLogging_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
ALTER TABLE [dbo].[RequestResponseLogging] ADD  CONSTRAINT [DF_RequestResponseLogging_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
ALTER TABLE [dbo].[zObjectsChangesLogsHistory] ADD  CONSTRAINT [DF_zObjectsChangesLogsHistory_PostTime]  DEFAULT (getdate()) FOR [PostTime]
GO
ALTER TABLE [dbo].[zObjectsChangesLogsLatest] ADD  CONSTRAINT [DF_zObjectsChangesLogs_PostTime]  DEFAULT (getdate()) FOR [PostTime]
GO
/****** Object:  StoredProcedure [dbo].[cc]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[cc]
as
begin

set statistics io on
set statistics time on
set statistics profile on
select '"111"' as F,222 as F, *
from
sys.objects

select '"222"' as F, *
from
sys.objects
end
GO
/****** Object:  StoredProcedure [dbo].[usp_executesql]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[usp_executesql]
@sql nvarchar(max)
, @rowcount int = 100
, @A int = null
, @B bit = null
, @C varchar(100) = null


--==========================
	, @HttpResponseStatusCode	int				= 200 out
	, @HttpResponseMessage		nvarchar(512)	= N'aaaaaa' out
as
begin
--select 
--@A
--,@B
--,@C
--return


set rowcount @rowcount

exec sp_executesql
@sql
end
GO
/****** Object:  StoredProcedure [dbo].[usp_TestUdt]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[usp_TestUdt]
@a [dbo].[udt_vcidt] readonly
as
begin
select
	*
from
	@a
union all
select
	*
from
	@a
end
GO
/****** Object:  StoredProcedure [dbo].[zsp_DynamicSqlPrepareCompiledTest]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE proc [dbo].[zsp_DynamicSqlPrepareCompiledTest]
as
begin
declare @sql nvarchar(max)
declare @ int = 1
declare @P1 int = 1
set @sql = N'
--yxy �Ĳ���
select
	1
from
	sys.objects a
where
	@ is null
	or
	a.object_id = @
'

if (@ is null)
begin
	set @sql += N'
	--@ is null'
end
else
begin
	set @sql += N'
	--@ is not null'
end

if (@P1 is null)
begin
	set @sql += N'
	--@P1 is null'
end
else
begin
	set @sql += N'
	--@P1 is not null'
end


print @sql
exec sp_executesql @sql, N'@ int', @


SELECT
	--distinct a.plan_handle
	*
FROM
	sys.dm_exec_cached_plans a
		CROSS APPLY
			sys.dm_exec_sql_text(a.plan_handle) b
		CROSS APPLY
			sys.dm_exec_query_plan(a.plan_handle) c
		--cross APPLY
		--	sys.dm_exec_plan_attributes(a.plan_handle) d
where
	b.text like '%--yxy%' 
	and
	a.objtype = 'Prepared'
end
GO
/****** Object:  StoredProcedure [dbo].[zsp_ErrorExceptionLogging]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[zsp_ErrorExceptionLogging]
	  @data								[dbo].[udt_ErrorExceptionLoggingEntry] readonly
	, @serverHostOsPlatformName			varchar(64) = null
	, @serverHostOsVersion				varchar(64)	= null
	, @serverHostFrameworkDescription	varchar(64)	= null
	, @serverHostMachineName			varchar(64)	= null
	, @serverHostProcessId					int			= null
	, @serverHostProcessName				varchar(64)	= null
	, @serverHostProcessStartTime			datetime	= null
	, @serverHostProcessWorkingSet64		bigint			= null
	, @serverHostProcessPeakWorkingSet64	bigint			= null
as
begin
--select 1/0
declare @BaseTime datetime = '2020-01-01'
declare @BatchingTime datetime
set @BatchingTime = DATEADD(second, DATEDIFF(SECOND,@BaseTime, getdate()), @BaseTime)
--select 1/0
--select 1/0
INSERT INTO 
		ErrorExceptionLogging
           (
				  [EnqueueTime]					
				, [DequeueTime]					
				, [QueueTimingInMilliseconds]		
	
				, [ErrorExceptionTime]			
				, [ErrorExceptionSource]			
				, [ErrorExceptionTraceID]			
				, [ErrorException]	

				, [serverHostOsPlatformName]			
				, [serverHostOsVersion]				
				, [serverHostFrameworkDescription]	
				  
				, [serverHostProcessId]		
				, [serverHostProcessName]		
				, [serverHostProcessStartTime]

				, [serverHostProcessWorkingSet64]	
				, [serverHostProcessPeakWorkingSet64]
				 
				, [BatchDateTimeStamp]
				, [HostName]
				--,[CreateTime]
			)
select
	  a.[EnqueueTime]					
	, a.[DequeueTime]					
	, a.[QueueTimingInMilliseconds]		
	
	, a.[ErrorExceptionTime]			
	, a.[ErrorExceptionSource]			
	, a.[ErrorExceptionTraceID]			
	, a.[ErrorException]				
	
	, @serverHostOsPlatformName			
	, @serverHostOsVersion				
	, @serverHostFrameworkDescription	
				 
	, @serverHostProcessId		
	, @serverHostProcessName		
	, @serverHostProcessStartTime

	, @serverHostProcessWorkingSet64	
	, @serverHostProcessPeakWorkingSet64				 
	
	, @BatchingTime
	, HOST_NAME()
from
	@data a
     




end
GO
/****** Object:  StoredProcedure [dbo].[zsp_Logging_Query]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[zsp_Logging_Query]
	@ bigint = 0
	, @Top int = 100
as
begin

SELECT TOP (@top) 
	a.AutoID
	, a.BatchDateTimeStamp
	, a.responseStatusCode
	, a.requestResponseTimingInMilliseconds
	, a.dbExecutingTimingInMilliseconds
	, a.QueueTimingInMilliseconds
	,*
  FROM [Test].[dbo].[RequestResponseLogging] a
  where
	a.AutoID > @
  order by
	a.AutoID desc

	--truncate table [RequestResponseLogging]
end
GO
/****** Object:  StoredProcedure [dbo].[zsp_Logging_Stat1]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[zsp_Logging_Stat1]
	@ bigint = 0
as
begin
;with T0
as
(
	select
		a.*
		, 
		First_value(a.requestResponseTimingInMilliseconds)
				OVER 
					(
						PARTITION BY
							a.serverHostProcessStartTime
							, a.serverHostOsPlatformName  
							, a.serverHostOsVersion
						ORDER BY
							a.AutoID desc
					)	
				as [last Timing]
	FROM 
		[RequestResponseLogging] a with(nolock,FORCESEEK)

	where
		a.AutoID > @



)
, T
as
(
	SELECT
		a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
		, avg		(a.requestResponseTimingInMilliseconds)	as [avg Timing]
		, count		(1)										as [count]	
		, MAX		(a.[last Timing])						as [last Timing]
		, max		(a.requestResponseTimingInMilliseconds)	as [max Timing]
		, min		(a.requestResponseTimingInMilliseconds)	as [min Timing]	

		, stdev		(a.requestResponseTimingInMilliseconds)	as [stdev Timing]	
		, stdevp	(a.requestResponseTimingInMilliseconds)	as [stdevp Timing]

		, MIN		(a.requestBeginTime)					as [min requestBeginTime]
		, MAX		(a.responseStartingTime)				as [max responseStartingTime]

		, MAX		(a.AutoID)	as [max AutoID]
		, Min		(a.AutoID)	as [min AutoID]

	FROM 
		T0 a with(nolock)

	where
		a.AutoID > @
	group by
		a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
)
, T1
as
(
	select
		a.*
		,
			iif
				(
					b.requestResponseTimingInMilliseconds >= a.[avg Timing]
					, ceiling((b.requestResponseTimingInMilliseconds - a.[avg Timing])/a.[stdev Timing])
					, floor((b.requestResponseTimingInMilliseconds - a.[avg Timing])/a.[stdev Timing])
				)
			
		 as [diffs Stdevp]
		, b.requestResponseTimingInMilliseconds 
		
	from
		T a
			inner join
				RequestResponseLogging b with(nolock)
					on
						a.serverHostProcessStartTime = b.serverHostProcessStartTime
						and
						a.serverHostOsPlatformName = b.serverHostOsPlatformName
						and
						a.serverHostOsVersion = b.serverHostOsVersion
	where
		b.AutoID > @
	
)
, T2
as
(
	select
		a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
		, a.[diffs Stdevp]
		, count(1) as [count]

	from
		T1 a
	group by
		a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
		, a.[diffs Stdevp]
)
, T3
as
(
	select
		*
	from
		(
			VALUES
				  (0, 1)
				, (1, 2)
				, (2, 3)
				, (3, 4)
				, (4, 9999)
		)
		T (MinExclusive, MaxInclusive) 
)
, T4
as
(
	select
		a.*
		, b.*

	from
		T2 a
			inner join
				T3 b
					on
						abs(a.[diffs Stdevp]) > b.MinExclusive
						and
						(
							abs(a.[diffs Stdevp]) <= b.MaxInclusive
						)

)
, T5
as
(
	select
		a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
		, a.MaxInclusive
		, sum(a.[count]) as count
		, sum
			(
				sum(a.[count])
			) 
				over
					(
						partition by
							a.serverHostProcessStartTime
							, a.serverHostOsPlatformName  
							, a.serverHostOsVersion
					)	as [Sum Count]
	from
		T4 a
	group by
		a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
		, a.MaxInclusive
)
, T6
as
(
	select
		serverHostProcessStartTime
		,serverHostOsPlatformName
		, serverHostOsVersion
		, [Sum Count]		
		, [1]									as [diff 1 Stdevp count]
		, 100.0 * [1]/[Sum Count]				as [diff 1 Stdevp %]
		, [2]									as [diff 2 Stdevp count]
		, 100.0 * [2]/[Sum Count]				as [diff 2 Stdevp %]
		, [3]									as [diff 3 Stdevp count]
		, 100.0 * [3]/[Sum Count]				as [diff 3 Stdevp %]
		, [4]									as [diff 4 Stdevp count]
		, 100.0 * [4]/[Sum Count]				as [diff 4 Stdevp %]
		, [9999]								as [diff N Stdevp count]
		, 100.0 * [9999]/[Sum Count]			as [diff N Stdevp %]
	from
		T5
		PIVOT
			(
				max([count])
				FOR
					[MaxInclusive] IN
				(
					[1]
					,[2]
					,[3]
					,[4]
					,[9999]
				)
			) AS pvt
)
select
	a.serverHostProcessStartTime					as [��������ʱ��]
	, a.serverHostOsPlatformName		as [��������������ϵͳƽ̨]
	, a.serverHostOsVersion				as [��������������ϵͳ�汾��]
	, a.[avg Timing]					as [ÿ��������Ӧƽ��ʱ��(����/��)]
	, a.[last Timing]					as [���������Ӧʱ��(����/��)]
	, a.[stdevp Timing]					as [������Ӧʱ����׼��(����)]
	, a.[count]							as [������Ӧ����(��)]
	
	, b.[diff 1 Stdevp count] + b.[diff 2 Stdevp count] 
											--as [diff (1 + 2) Stdevp count]
											as [2����׼���ڵ�������Ӧ����(��)]
	, b.[diff 1 Stdevp %] + b.[diff 2 Stdevp %]
											--as [diff (1 + 2) Stdevp %]
											as [2����׼���ڵ�������Ӧ����ռ��(%)]
	, DATEDIFF(SECOND, a.[min requestBeginTime], a.[max responseStartingTime])
											--as [duration in seconds]
											as [����ʱ��(��)]

	, b.[diff 1 Stdevp %]					as [1����׼���ڵ�������Ӧ����ռ��(%)]
	, b.[diff 1 Stdevp count]				as [1����׼���ڵ�������Ӧ����(��)]
	, b.[diff 2 Stdevp %]					as [2-1����׼��֮���������Ӧ����ռ��(%)]
	, b.[diff 2 Stdevp count]				as [2-1����׼��֮���������Ӧ����(��)]

	, a.[min Timing]						as [���������Ӧʱ��(����)]
	, a.[max Timing]						as [�������Ӧʱ��(����)]
	, a.[max AutoID]						as [������]
from
	T a
		inner join
			T6 b
				on
					a.serverHostProcessStartTime = b.serverHostProcessStartTime
					and
					a.serverHostOsPlatformName = b.serverHostOsPlatformName
					and
					a.serverHostOsVersion = b.serverHostOsVersion
order by 
	a.serverHostProcessStartTime
	, a.serverHostOsPlatformName
	, a.serverHostOsVersion
--select

--	a.processStartTime
--	, a.serverHostOsPlatformName  
--	, a.serverHostOsVersion
--	, a.[avg Timing]
	
	
--		--(
--		--	100.0 *
--		--	(
--		--		select
--		--			count(1)
--		--		from
--		--			[RequestResponseLogging] aa
--		--		where
--		--			aa.requestResponseTimingInMilliseconds >= a.[avg Timing] - 2 * a.[stdevp Timing]
--		--			and
--		--			aa.requestResponseTimingInMilliseconds <= a.[avg Timing] + 2 * a.[stdevp Timing]
--		--	)
--		--	/
--		--	a.[count] 
--		--) as NormalDistPercent
--	, DATEDIFF(SECOND, a.[min requestBeginTime], a.[max responseStartingTime]) as [duration in seconds]



--	, a.[min requestBeginTime]
--	, a.[max responseStartingTime]
--	, a.[count]	 
--	, a.[max Timing]
--	, a.[min Timing]	
--	, a.[stdev Timing]	
--	, a.[stdevp Timing]
--from
--	T a
--		cross apply
--			(
--				select
--					*
--				from
--					RequestResponseLogging  aa
--				where
--					aa.requestResponseTimingInMilliseconds 
			
--			)

--order by
--	a.[avg Timing]
--		--desc
--	--truncate table [RequestResponseLogging]
end
GO
/****** Object:  StoredProcedure [dbo].[zsp_Logging_Stat2]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[zsp_Logging_Stat2]
	@source int = 0
	,@ bigint = 0
	
as
begin


;with T01
as
(
	select
		'RequestResponse'						as Timing
		, a.*
		, a.RequestResponseTimingInMilliseconds as TimingInMilliseconds
		, First_value(a.RequestResponseTimingInMilliseconds)
				OVER 
					(
						PARTITION BY
							a.serverHostProcessStartTime
							, a.serverHostOsPlatformName  
							, a.serverHostOsVersion
						ORDER BY
							a.AutoID desc
					)	
				as [last Timing]
	FROM 
		[RequestResponseLogging] a with(nolock, FORCESEEK)
	where
		a.AutoID > @
		and
		@source = 0
)
, T02
as
(
	select
		'DBExecuting'						as Timing
		, a.*
		, a.dbExecutingTimingInMilliseconds as TimingInMilliseconds
		, First_value(a.dbExecutingTimingInMilliseconds)
				OVER 
					(
						PARTITION BY
							a.serverHostProcessStartTime
							, a.serverHostOsPlatformName  
							, a.serverHostOsVersion
						ORDER BY
							a.AutoID desc
					)	
				as [last Timing]
	FROM 
		[RequestResponseLogging] a with(nolock, FORCESEEK)
	where
		a.AutoID > @
		and
		@source = 1
)
, T0
as
(
	select
		*
	from
		T01 a
	union all
	select
		*
	from
		T02 a
)
, T1
as
(
	SELECT
		max(a.Timing)							as [Timing]
		, a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
		, avg		(a.TimingInMilliseconds)	as [avg Timing]
		, count		(1)										as [count]	
		, MAX		(a.[last Timing])						as [last Timing]
		, max		(a.TimingInMilliseconds)	as [max Timing]
		, min		(a.TimingInMilliseconds)	as [min Timing]	

		, stdev		(a.TimingInMilliseconds)	as [stdev Timing]	
		, stdevp	(a.TimingInMilliseconds)	as [stdevp Timing]

		, MIN		(a.requestBeginTime)					as [min requestBeginTime]
		, MAX		(a.responseStartingTime)				as [max responseStartingTime]
		, Min		(a.AutoID)	as [min AutoID]
		, MAX		(a.AutoID)	as [max AutoID]


	FROM 
		T0 a with(nolock)

	where
		a.AutoID > @
	group by
		a.serverHostProcessStartTime
		, a.serverHostOsPlatformName  
		, a.serverHostOsVersion
)
, T2
as
(
	select
		*
	from
		(
			VALUES
				  (	-10000	, -4)
				, (	-4		, -3)
				, (	-3		, -2)
				, (	-2		, -1)
				, (	-1		, 0)
				, (	0		, 1)
				, (	1		, 2)
				, (	2		, 3)
				, (	3		, 4)
				, (	4		, 10000)
		)
		T (MinExclusive, MaxInclusive)
)
, T2T1
as
(
	select
		--a.*
		--  b.[avg Timing]
		a.*
		, '(' + cast(a.MinExclusive as varchar) + ',' + cast(a.MaxInclusive as varchar) + ']' as [Timing range in Stdevp]
		, b.[avg Timing] + a.MinExclusive * b.[stdevp Timing] as [MinExclusive Timing]
		, b.[avg Timing] + a.MaxInclusive * b.[stdevp Timing] as [MaxInclusive Timing]
		, b.*
		
	from
		T2 a
			,
		T1 b
			
)
, T3
as
(
		select
			max(b.Timing)									as [Timing]
			, b.serverHostProcessStartTime
			, b.serverHostOsPlatformName
			, b.serverHostOsVersion
			, avg(b.[avg Timing])							as [avg Timing]
			, b.[Timing range in Stdevp]
			, avg(a.TimingInMilliseconds)	as [avg TimingInMilliseconds]
			, min(a.TimingInMilliseconds)	as [min TimingInMilliseconds]
			, max(a.TimingInMilliseconds)	as [max TimingInMilliseconds]
			, min(b.[MinExclusive Timing])					as [MinExclusive Timing]
			, max(b.[MaxInclusive Timing])					as [MaxInclusive Timing]
			, min(b.MinExclusive)							as [MinExclusive]
			, max(b.MaxInclusive)							as [MaxInclusive]
			--, b.[MinExclusive Timing]
			--, b.[MaxInclusive Timing]
			, count(a.TimingInMilliseconds)	as [count]
			, sum(count(a.TimingInMilliseconds))
					over 
						(
							partition by
									b.serverHostProcessStartTime
									, b.serverHostOsPlatformName
									, b.serverHostOsVersion
						)	as [Total Count]
		from
			T0 a
				right join
					T2T1 b
						on
							a.TimingInMilliseconds > b.[MinExclusive Timing]
							and
							a.TimingInMilliseconds <= b.[MaxInclusive Timing]
							and
							a.serverHostProcessStartTime	= b.serverHostProcessStartTime		
							and
							a.serverHostOsPlatformName		= b.serverHostOsPlatformName
							and
							a.serverHostOsVersion			= b.serverHostOsVersion
		group by
			b.serverHostProcessStartTime
			, b.serverHostOsPlatformName
			, b.serverHostOsVersion
			, b.[Timing range in Stdevp]
)
, TP1
as
(
	
	select
		serverHostProcessStartTime
		, serverHostOsPlatformName
		, serverHostOsVersion
		, [avg Timing]

		, [Total Count]
		, [-10000]					as '(-10000, -4] %'
		, [-4]						as '(-4, -3] %'
		, [-3]						as '(-3, -2] %'
		, [-2]						as '(-2, -1] %'
		, [-1]						as '(-1, 0] %'
		, [0]						as '(0, +1] %'
		, [1]						as '(+1, +2] %'
		, [2]						as '(+2, +3] %'
		, [3]						as '(+3, +4] %'
		, [4]						as '(+4, +10000] %'
	from
		(
			select
				  aa.serverHostProcessStartTime
				, aa.serverHostOsPlatformName
				, aa.serverHostOsVersion
				, aa.[avg Timing]
				--, aa.[avg TimingInMilliseconds]
				--, aa.[max TimingInMilliseconds]
				--, aa.[min TimingInMilliseconds]
				, aa.MinExclusive
				, aa.[Total Count]
				, 
					100.0
					* aa.[count]
					/ iif(aa.[Total Count] = 0, 1, aa.[Total Count]) as [Timing Range %]
			from
				T3 aa
		) a
		PIVOT
		(
			max([Timing Range %])
			FOR
				[MinExclusive] IN
				(
				  [-10000]
				 ,[-4]
				 ,[-3]
				 ,[-2]
				 ,[-1]
				 ,[0]
				 ,[1]
				 ,[2]
				 ,[3]
				 ,[4]
				 )
			) AS pvt

)
, TP2
as
(
	select
		serverHostProcessStartTime
		, serverHostOsPlatformName
		, serverHostOsVersion
		, [Total Count]
		, [-10000]					as '(-10000, -4] count'
		, [-4]						as '(-4, -3] count'
		, [-3]						as '(-3, -2] count'
		, [-2]						as '(-2, -1] count'
		, [-1]						as '(-1, 0] count'
		, [0]						as '(0, +1] count'
		, [1]						as '(+1, +2] count'
		, [2]						as '(+2, +3] count'
		, [3]						as '(+3, +4] count'
		, [4]						as '(+4, +10000] %'
	from
		(
			select
				  aa.serverHostProcessStartTime
				, aa.serverHostOsPlatformName
				, aa.serverHostOsVersion
				, aa.MinExclusive
				, aa.[Total Count]
				, aa.[count]
			from
				T3 aa
		) a
		PIVOT
		(
			max([count])
			FOR
				[MinExclusive] IN
				(
				  [-10000]
				 ,[-4]
				 ,[-3]
				 ,[-2]
				 ,[-1]
				 ,[0]
				 ,[1]
				 ,[2]
				 ,[3]
				 ,[4]
				 )
			) AS pvt

)
, TT
as
(
	select
		  a.Timing
		, a.serverHostProcessStartTime
		, a.serverHostOsPlatformName
		, a.serverHostOsVersion
		, DATEDIFF(SECOND, a.[min requestBeginTime], a.[max responseStartingTime]) as [duration in seconds (s)]
		, a.[min Timing]							as [min Timing (ms)]
		, a.[avg Timing]							as [avg Timing (ms)]
		, a.[max Timing]							as [max Timing (ms)]
		, a.[last Timing]							as [last Timing (ms)]
		, a.[stdevp Timing]							as [stdevp Timing (ms)]
		, b.[Total Count]							as [Total Count]
		, 
			  c.[(-2, -1]] count]
			+ c.[(-1, 0]] count]
			+ c.[(0, +1]] count]
			+ c.[(+1, +2]] count]					as [(-2, +2]] stdevp count]

		,
			  b.[(-2, -1]] %]
			+ b.[(-1, 0]] %]
			+ b.[(0, +1]] %]
			+ b.[(+2, +3]] %]						as [(-2, +2]] stdevp (%)]


		, a.[avg Timing] - 2 * a.[stdevp Timing]	as [avg - 2 * stdevp (ms)] --����Ϊ����
		, a.[avg Timing] + 2 * a.[stdevp Timing]	as [avg + 2 * stdevp (ms)]
	
		, a.[min AutoID]
		, a.[max AutoID]

		, a.[min requestBeginTime]					as [min requestBeginTime]
		, a.[max responseStartingTime]				as [max responseStartingTime]

	from
		T1 a
			left join
				TP1 b
					on
						a.serverHostProcessStartTime	= b.serverHostProcessStartTime		
						and
						a.serverHostOsPlatformName		= b.serverHostOsPlatformName
						and
						a.serverHostOsVersion			= b.serverHostOsVersion
			left join
				TP2 c
					on
						a.serverHostProcessStartTime	= c.serverHostProcessStartTime		
						and								  
						a.serverHostOsPlatformName		= c.serverHostOsPlatformName
						and								  
						a.serverHostOsVersion			= c.serverHostOsVersion
)
select
	a.Timing
	, a.serverHostProcessStartTime
	, a.serverHostOsPlatformName
	, a.serverHostOsVersion
	, a.[duration in seconds (s)]
	, a.[min Timing (ms)]
	, a.[avg Timing (ms)]
	, a.[max Timing (ms)]
	, a.[last Timing (ms)]
	, a.[stdevp Timing (ms)]
	, a.[Total Count]
	, a.[(-2, +2]] stdevp count]
	, a.[(-2, +2]] stdevp (%)]
	, IIF(a.[avg - 2 * stdevp (ms)] >= 0, a.[avg - 2 * stdevp (ms)], 0) as [avg - 2 * stdevp (ms)] --����Ϊ����
	, a.[avg + 2 * stdevp (ms)]
	, a.[min AutoID]
	, a.[max AutoID]
	, a.[min requestBeginTime]
	, a.[max responseStartingTime]
from
	TT a
order by
	a.serverHostProcessStartTime desc
	, a.serverHostOsPlatformName	
	, a.serverHostOsVersion		
end

GO
/****** Object:  StoredProcedure [dbo].[zsp_Logging_Stats]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[zsp_Logging_Stats]
	@p1 bigint = 0
	
as
begin

exec [zsp_Logging_Stat2] 0, @p1

exec [zsp_Logging_Stat2] 1, @p1

end
GO
/****** Object:  StoredProcedure [dbo].[zsp_RequestResponseLogging]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[zsp_RequestResponseLogging]
	  @data								[dbo].udt_RequestResponseLoggingEntry readonly
	, @serverHostOsPlatformName			varchar(64) = null
	, @serverHostOsVersion				varchar(64)	= null
	, @serverHostFrameworkDescription	varchar(64)	= null
	, @serverHostMachineName			varchar(64)	= null
	, @serverHostProcessId					int			= null
	, @serverHostProcessName				varchar(64)	= null
	, @serverHostProcessStartTime			datetime	= null
	, @serverHostProcessWorkingSet64		bigint			= null
	, @serverHostProcessPeakWorkingSet64	bigint			= null
as
begin
--select 1/0
declare @BaseTime datetime = '2020-01-01'
declare @BatchingTime datetime
set @BatchingTime = DATEADD(second, DATEDIFF(SECOND,@BaseTime, getdate()), @BaseTime)
--select 1/0
--select 1/0
INSERT INTO 
		[RequestResponseLogging]
           (
				  [ID]
				, [EnqueueTime]
				, [DequeueTime]
				, [QueueTimingInMilliseconds]
				, [requestTraceID]
				, [requestUrl]
				, [requestPath]
				, [requestPathBase]
				, [requestActionRoutePath]
				, [requestHeaders]
				, [requestBody]
				, [requestMethod]
				, [requestBeginTime]
				, [requestContentLength]
				, [requestContentType]
				, [responseHeaders]
				, [responseBody]
				, [responseStatusCode]
				, [responseStartingTime]
				, [responseContentLength]
				, [responseContentType]
				, [requestResponseTimingInMilliseconds]
				, [dbExecutingTimingInMilliseconds]

				, [clientIP]				-- [varchar](16) NULL,
				, [locationLongitude]		-- [decimal](24, 18) NULL,
				, [locationLatitude]		-- [decimal](24, 18) NULL,
				, [userID]					-- [varchar](32) NULL,
				, [roleID]					-- [varchar](16) NULL,
				, [orgUnitID]				-- [varchar](16) NULL,
				, [deviceID]				-- [varchar](64) NULL,
				, [deviceInfo]

				, [serverHostOsPlatformName]			
				, [serverHostOsVersion]				
				, [serverHostFrameworkDescription]	
				  
				, [serverHostProcessId]		
				, [serverHostProcessName]		
				, [serverHostProcessStartTime]
				 
				, [serverHostProcessWorkingSet64]
				, [serverHostProcessPeakWorkingSet64]

				, [BatchDateTimeStamp]
				, [HostName]
				--,[CreateTime]
			)
select
	  a.[ID]
	, a.[EnqueueTime]
	, a.[DequeueTime]
	, a.[QueueTimingInMilliseconds]
	, a.requestTraceID
	, a.[requestUrl]
	, a.[requestPath]
	, a.[requestPathBase]
	, a.[requestActionRoutePath]
	, a.[requestHeaders]
	, a.[requestBody]
	, a.[requestMethod]
	, a.[requestBeginTime]
	, a.[requestContentLength]
	, a.[requestContentType]
	, a.[responseHeaders]
	, a.[responseBody]
	, a.[responseStatusCode]
	, a.[responseStartingTime]
	, a.[responseContentLength]
	, a.[responseContentType]
	, a.[requestResponseTimingInMilliseconds]
	, a.[dbExecutingTimingInMilliseconds]

	, a.[clientIP]					-- [varchar](16) NULL,
	, a.[locationLongitude]			-- [decimal](24, 18) NULL,
	, a.[locationLatitude]			-- [decimal](24, 18) NULL,
	, a.[userID]					-- [varchar](32) NULL,
	, a.[roleID]					-- [varchar](16) NULL,
	, a.[orgUnitID]					-- [varchar](16) NULL,

	, a.[deviceID]					-- [varchar](64) NULL,
	, a.[deviceInfo]
	
	, @serverHostOsPlatformName			
	, @serverHostOsVersion				
	, @serverHostFrameworkDescription	
				 
	, @serverHostProcessId		
	, @serverHostProcessName		
	, @serverHostProcessStartTime
				 
	, @serverHostProcessWorkingSet64
	, @serverHostProcessPeakWorkingSet64

	, @BatchingTime
	, HOST_NAME()
from
	@data a
     




end
GO
/****** Object:  StoredProcedure [dbo].[zsp_zObjectsChangesLogsHistory_Get]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[zsp_zObjectsChangesLogsHistory_Get]
(
	@SearchObjectName			varchar(256)	= null
	, @SearchHostName			varchar(64)		= null
	, @SearchDatabaseName			varchar(64)		= null

	, @SearchAfterTime			datetime		= null
	, @SearchIdsList			varchar(64)		= null
	--, @SearchIdsList			udt_int readonly
	, @Top						int				= 100
	, @HttpResponseStatusCode	int				= 200 out
	, @HttpResponseMessage		nvarchar(512)	= N'' out
)
as
/*


declare @p5 int
--set @p5=default
declare @p6 nvarchar(512)
--set @p6=default
exec zsp_zObjectsChangesLogsHistory_Get @SearchObjectName=NULL,@SearchHostName=NULL,@SearchDatabaseName=NULL,@SearchAfterTime='8/1/2000',@HttpResponseStatusCode=@p5 output,@HttpResponseMessage=@p6 output
select @p5, @p6

*/

begin

--select 1/0

if (@SearchIdsList is  null)
begin
	if (@SearchObjectName is null or LTRIM(rtrim(@SearchObjectName)) = '')
	begin
		select
			Top (@Top)
			--'{a:1}' as Json_F1
			  --,
			  a.[ID]
			  , a.[DatabaseServerName]
			  , a.[DatabaseName]
			  , a.[EventType]
			  , a.[ObjectName]
			  , a.[ObjectType]
			  --, a.[TSQLCommand]
			  , a.[LoginName]
			  , a.[HostName]
			  , a.[PostTime]
			  , a.[Version]
			  , a.PostTime as MaxPostTime

			  , 
				(
					select
						top (@Top)
						*
					from
						[zObjectsChangesLogsHistory] aa
					where
						aa.DatabaseServerName = a.DatabaseServerName
						and
						aa.DatabaseName = a.DatabaseName
						and
						aa.ObjectName = a.ObjectName
					order by
						aa.id desc
					for json PATH
				) as childItems
		from
			[dbo].[zObjectsChangesLogsLatest] a with(nolock)
		where
			
			(
				@SearchAfterTime is null
				or
				a.PostTime >= @SearchAfterTime
			)
			and
			(
				@SearchHostName is null
				or
				a.HostName like '%' + @SearchHostName + '%'
			)
			and
			(
				@SearchDataBaseName is null
				or
				a.DatabaseName like '%' + @SearchDataBaseName + '%'
			)

		order by
			a.ID desc
	end
	else
	begin
		select
			Top (@Top)
			--'{a:1}' as Json_F1
			  --,
			  a.[ID]
			  , a.[DatabaseServerName]
			  , a.[DatabaseName]
			  , a.[EventType]
			  , a.[ObjectName]
			  , a.[ObjectType]
			  --, a.[TSQLCommand]
			  , a.[LoginName]
			  , a.[HostName]
			  , a.[PostTime]
			  , a.[Version]
			  , a.PostTime as MaxPostTime

			  , 
				(
					select
						top (@Top)
						*
					from
						[zObjectsChangesLogsHistory] aa
					where
						aa.DatabaseServerName = a.DatabaseServerName
						and
						aa.DatabaseName = a.DatabaseName
						and
						aa.ObjectName = a.ObjectName
					order by
						aa.id desc
					for json PATH
				) as childItems
		from
			[dbo].[zObjectsChangesLogsLatest] a with(nolock)
		where
			a.ObjectName like '%' + @SearchObjectName + '%'
			and
			(
				@SearchAfterTime is null
				or
				a.PostTime >= @SearchAfterTime
			)
			and
			(
				@SearchHostName is null
				or
				a.HostName like '%' + @SearchHostName + '%'
			)
			and
			(
				@SearchDataBaseName is null
				or
				a.DatabaseName like '%' + @SearchDataBaseName + '%'
			)

		order by
			--MaxPostTime desc
			--, a.ObjectName
			--,
			a.ID desc

	end
	
end
else if (@SearchIdsList is not null and @SearchObjectName is null)
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
	 --'{a:1}' as Json_F1
	    a.[ID]
	  , a.[DatabaseServerName]
      , a.[DatabaseName]
      , a.[EventType]
      , a.[ObjectName]
      , a.[ObjectType]
      , a.[TSQLCommand]
      , a.[LoginName]
      , a.[HostName]
      , a.[PostTime]
      , a.[Version]
	 
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
/****** Object:  StoredProcedure [dbo].[zsp_zObjectsChangesLogsHistory_Get_bak]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[zsp_zObjectsChangesLogsHistory_Get_bak]
(
	@SearchObjectName			varchar(256)	= null
	, @SearchHostName			varchar(64)		= null
	, @SearchDatabaseName			varchar(64)		= null

	, @SearchBeforeTime			datetime		= null
	, @SearchIdsList			varchar(64)		= null
	--, @SearchIdsList			udt_int readonly
	, @Top						int				= 100
	, @HttpResponseStatusCode	int				= 200 out
	, @HttpResponseMessage		nvarchar(512)	= N'' out
)
as
/*

exec [zsp_zObjectsChangesLogsHistory_Get] @SearchObjectName='U'

*/

begin


if (@SearchIdsList is  null)
begin
	if (@SearchObjectName is null or LTRIM(rtrim(@SearchObjectName)) = '')
	begin
		select
			Top (@Top)
			--'{a:1}' as Json_F1
			  --,
			  a.[ID]
			  , a.[DatabaseServerName]
			  , a.[DatabaseName]
			  ,a.[EventType]
			  ,a.[ObjectName]
			  ,a.[ObjectType]
			  --,a.[TSQLCommand]
			  ,a.[LoginName]
			  ,a.[HostName]
			  ,a.[PostTime]
			  ,a.[Version]
			  , Max(a.PostTime) over(partition by a.[DatabaseName] , a.[ObjectName])	as MaxPostTime
		from
			[dbo].[zObjectsChangesLogsHistory] a with(nolock)
		where

			(
				@SearchBeforeTime is null
				or
				a.PostTime <= @SearchBeforeTime
			)
			and
			(
				@SearchHostName is null
				or
				a.HostName like '%' + @SearchHostName + '%'
			)
			and
			(
				@SearchDataBaseName is null
				or
				a.DatabaseName like '%' + @SearchDataBaseName + '%'
			)

		order by
			MaxPostTime desc
			, a.ObjectName
			, a.ID desc
	end
	else
	begin
	select
			Top (@Top)
			--'{a:1}' as Json_F1
			  a.[ID]

			  ,a.[DatabaseServerName]
			  ,a.[DatabaseName]
			  ,a.[EventType]
			  ,a.[ObjectName]
			  ,a.[ObjectType]
			  --,a.[TSQLCommand]
			  ,a.[LoginName]
			  ,a.[HostName]
			  ,a.[PostTime]
			  ,a.[Version]
			  , Max(a.PostTime) over(partition by a.[DatabaseServerName], a.[DatabaseName] , a.[ObjectName])	as MaxPostTime
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
			and
			(
				@SearchHostName is null
				or
				a.HostName like '%' + @SearchHostName + '%'
			)
			and
			(
				@SearchDataBaseName is null
				or
				a.DatabaseName like '%' + @SearchDataBaseName + '%'
			)

		order by
			MaxPostTime desc
			, a.ObjectName
			, a.ID desc

	end
	
end
else if (@SearchIdsList is not null and @SearchObjectName is null)
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
	 --'{a:1}' as Json_F1
	   a.[ID]
	  ,a.[DatabaseServerName]
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
/****** Object:  DdlTrigger [ztrigger_ddl]    Script Date: 9/6/2020 10:45:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/****** Object:  DdlTrigger [ztrigger_ddl]    Script Date: 9/4/2020 9:02:44 PM ******/




CREATE   TRIGGER [ztrigger_ddl]
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
set xact_abort on
begin transaction

DECLARE @data XML = EVENTDATA()

DECLARE
		  @DatabaseServerName	varchar(256)		= @@servername
		
		, @DatabaseName			varchar(256)		= @data.value('(/EVENT_INSTANCE/DatabaseName)[1]'	, 'varchar(256)'	)
		, @EventType			varchar(64)			= @data.value('(/EVENT_INSTANCE/EventType)[1]'		, 'varchar(64)'		)
		, @ObjectName			varchar(256)		= @data.value('(/EVENT_INSTANCE/ObjectName)[1]'		, 'varchar(256)'	)
		, @ObjectType			varchar(32)			= @data.value('(/EVENT_INSTANCE/ObjectType)[1]'		, 'varchar(32)'		) 
		, @TSQLCommand			nvarchar(max)		= @data.value('(/EVENT_INSTANCE/TSQLCommand)[1]'	, 'nvarchar(max)'	) 
		, @LoginName			varchar(256)		= @data.value('(/EVENT_INSTANCE/LoginName)[1]'		, 'varchar(256)'	)
		, @UserName				varchar(256)		= @data.value('(/EVENT_INSTANCE/UserName)[1]'		, 'varchar(256)'	)
		, @PostTime				datetime			= @data.value('(/EVENT_INSTANCE/PostTime)[1]'		, 'datetime'		)
		, @SPID					int					= @data.value('(/EVENT_INSTANCE/SPID)[1]'			, 'int'				)
				
		, @HostName				varchar(256)		= Host_Name()

delete
	[Test].dbo.[zObjectsChangesLogsLatest]
where
	[ObjectName] = @ObjectName
	and
	[DataBaseName] = @DatabaseName
	and
	DatabaseServerName = @@SERVERNAME

declare @id bigint

declare @version int
select
	@version = isnull(max([version]), 0) + 1
from
	[Test].dbo.[zObjectsChangesLogsHistory] a
where
	a.[DatabaseName] = @DatabaseName
	and
	a.[ObjectName] = @ObjectName


INSERT INTO
		[Test].dbo.[zObjectsChangesLogsLatest]
		(
			DatabaseServerName
			
			, [DatabaseName]
			, [EventType]
			, [ObjectName]
			, [ObjectType]
			, [TSQLCommand]
			, [LoginName]
			, [UserName]
			, [PostTime]
			, [SPID]

			, [HostName]

			, [Version]
			, [EventData]
		)
--select
values
		(
			  @DatabaseServerName	

			, @DatabaseName			
			, @EventType			
			, @ObjectName			
			, @ObjectType			
			, @TSQLCommand			
			, @LoginName			
			, @UserName				
			, @PostTime				
			, @SPID					
					
			, @HostName				
			
			, @version
			, @data
		)

set @id = SCOPE_IDENTITY()
insert into
	[Test].dbo.[zObjectsChangesLogsHistory]
	(
		[ID]
		, [DatabaseServerName]
		, [DatabaseName]
		, [EventType]
		, [ObjectName]
		, [ObjectType]
		, [TSQLCommand]
		, [LoginName]
		, [UserName]

		, [PostTime]
		, [SPID]
		, [HostName]
		, [Version]
		, [EventData]
	)
values
		(
			@ID
			, @DatabaseServerName	

			, @DatabaseName			
			, @EventType			
			, @ObjectName			
			, @ObjectType			
			, @TSQLCommand			
			, @LoginName			
			, @UserName				
			, @PostTime				
			, @SPID					
					
			, @HostName				
			
			, @version
			, @data
		)
commit transaction
END

GO
ENABLE TRIGGER [ztrigger_ddl] ON DATABASE
GO
USE [master]
GO
ALTER DATABASE [Test] SET  READ_WRITE 
GO
