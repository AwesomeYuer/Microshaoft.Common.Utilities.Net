--USE [master]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [zTVF_SplitStringToTable]
(
	@Text			NVARCHAR(MAX) ,		--待分拆的字符串
	@Separator		VARCHAR(10) = ','   --数据分隔符
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
	set @Text = replace(@Text,' ','')
	DECLARE @SeparatorLen int
	SET @SeparatorLen = LEN(@Separator + '$') - 2
	set @Text = replace(@Text, ' ', '')
	declare @i int
	set @i = 1
	WHILE
		(
			CHARINDEX(@Separator,@Text )>0
		)
	BEGIN
		declare @v varchar(1024)
		set @v = (LEFT(@Text ,CHARINDEX(@Separator, @Text) - 1))
		INSERT INTO
			@Table 
				(
					id
					, F
				)
		select
			  @i
			, @v
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
		SET @Text = STUFF(@Text , 1, CHARINDEX(@Separator, @Text) + @SeparatorLen, '')
	END
	INSERT INTO
		@Table
			(
				  id
				, F
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
CREATE TABLE [zObjectsChangesLogsHistory]
(
	[ID]				[BIGINT] IDENTITY(1,1) NOT NULL,
	[DatabaseName]		[sysname] NULL,
	[EventType]			[VARCHAR](50) NULL,
	[ObjectName]		[VARCHAR](256) NULL,
	[ObjectType]		[VARCHAR](25) NULL,
	[TSQLCommand]		[NVARCHAR](max) NULL,
	[LoginName]			[VARCHAR](256) NULL,
	[HostName]			[VARCHAR](256) NULL,
	[PostTime]			[DATETIME] DEFAULT GETDATE(),
	[Version]			[INT] NOT NULL,
	CONSTRAINT
		[PK_zObjectsChangesLogsHistory]
	PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)
	WITH
	(
		PAD_INDEX = OFF
		, STATISTICS_NORECOMPUTE = OFF
		, IGNORE_DUP_KEY = OFF
		, ALLOW_ROW_LOCKS = ON
		, ALLOW_PAGE_LOCKS = ON
	)
	ON [PRIMARY]
)
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[zObjectsChangesLogsHistory]
		ADD  CONSTRAINT [DF_zObjectsChangesLogsHistory_PostTime]
			DEFAULT (getdate()) FOR [PostTime]
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
		TOP
			(@Top)
			  a.[ID]
			, a.[DatabaseName]
			, a.[EventType]
			, a.[ObjectName]
			, a.[ObjectType]
			--, a.[TSQLCommand]
			, a.[LoginName]
			, a.[HostName]
			, a.[PostTime]
			, a.[Version]
	from
		[zObjectsChangesLogsHistory] a with(nolock)
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
			  CAST(a.F as int) as F
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

DECLARE @data XML	= EVENTDATA()
set @DatabaseName	= @data.value('(/EVENT_INSTANCE/DatabaseName)[1]', 'varchar(256)')
set @ObjectName		= @data.value('(/EVENT_INSTANCE/ObjectName)[1]', 'varchar(256)')

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
			  @DatabaseName
			, @data.value('(/EVENT_INSTANCE/EventType)[1]'		, 'varchar(64)'		)
			, @ObjectName
			, @data.value('(/EVENT_INSTANCE/ObjectType)[1]'		, 'varchar(32)'		) 
			, @data.value('(/EVENT_INSTANCE/TSQLCommand)[1]'	, 'nvarchar(max)'	) 
			, @data.value('(/EVENT_INSTANCE/LoginName)[1]'		, 'varchar(32)'		)
			--, @SPID	= @data.value('(/EVENT_INSTANCE/SPID)[1]', 'int')
			, HOST_NAME()
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
