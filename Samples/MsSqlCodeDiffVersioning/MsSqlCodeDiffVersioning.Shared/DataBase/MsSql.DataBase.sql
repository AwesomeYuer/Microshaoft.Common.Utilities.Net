USE [Test]
GO
/****** Object:  UserDefinedTableType [dbo].[udt_int]    Script Date: 8/8/2019 1:21:52 PM ******/
CREATE TYPE [dbo].[udt_int] AS TABLE(
	[F1] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[udt_varchar]    Script Date: 8/8/2019 1:21:52 PM ******/
CREATE TYPE [dbo].[udt_varchar] AS TABLE(
	[F1] [varchar](16) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[udt_vcidt]    Script Date: 8/8/2019 1:21:52 PM ******/
CREATE TYPE [dbo].[udt_vcidt] AS TABLE(
	[varchar] [varchar](16) NULL,
	[int] [int] NULL,
	[date] [date] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[zTVF_SplitStringToTable]    Script Date: 8/8/2019 1:21:52 PM ******/
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
/****** Object:  View [dbo].[zv_all_PARAMETERS]    Script Date: 8/8/2019 1:21:52 PM ******/
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
/****** Object:  Table [dbo].[zObjectsChangesLogsHistory]    Script Date: 8/8/2019 1:21:52 PM ******/
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
/****** Object:  StoredProcedure [dbo].[usp_executesql]    Script Date: 8/8/2019 1:21:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[usp_executesql]
@sql nvarchar(max)
, @rowcount int = 100
, @A int
, @B bit
, @C varchar(100)
as
begin
select @A,@B,@C

set rowcount @rowcount
exec sp_executesql @sql
end
GO
/****** Object:  StoredProcedure [dbo].[zsp_zObjectsChangesLogsHistory_Get]    Script Date: 8/8/2019 1:21:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[zsp_zObjectsChangesLogsHistory_Get]
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
			'{a:1}' as Json_F1
			   ,a.[ID]
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
			a.ObjectName
			, a.ID desc
	end
	else
	begin
	select
			Top (@Top)
			'{a:1}' as Json_F1
			   ,a.[ID]
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
			a.ObjectName
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
	 '{a:1}' as Json_F1
		,a.[ID]
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
/****** Object:  DdlTrigger [ztrigger_ddl]    Script Date: 8/8/2019 1:21:52 PM ******/
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
