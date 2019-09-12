USE [MessagesPush]
GO
/****** Object:  StoredProcedure [dbo].[zsp_GetAppGroupUsers]    Script Date: 2014/4/27 13:46:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[zsp_GetAppGroupUsers]
@AppID varchar(50)
, @GroupID varchar(50)
, @UserID varchar(50) = '*'
, @OffsetRows int
, @FetchRows int
, @TotalRows int = -1 out
, @IsLast bit = 0 out

as
begin
/*
	declare @OffsetRows int = 0
	declare @FetchRows int = 1
	declare @islast bit = 0
	declare @TotalRows int = -1
	exec zsp_GetAppGroupUsers
				'cos'
				, 'audit'
				, '*'
				, @OffsetRows
				, @FetchRows
				, @TotalRows out
				, @islast out
	select @TotalRows, @OffsetRows, @FetchRows, @islast 
*/

	if (@TotalRows <= 0)
	begin
		select
			@TotalRows = count(1)
		from
			[AppsGroupsUsers]
		where
		(AppID = @AppID or @AppID ='*' )
		and (GroupID = @GroupID or @GroupID ='*' )
		and(@UserID = '*' or UserID = @UserID)
	end
	if (@TotalRows <= (@OffsetRows + @FetchRows))
	begin
		set @Islast = 1
	end
	
	SELECT
		*
	from
		[AppsGroupsUsers]
	where
		(AppID = @AppID or @AppID ='*' )
		and (GroupID = @GroupID or @GroupID ='*' )
		and(@UserID = '*' or UserID = @UserID)
	order by
		ID
	OFFSET
		@OffsetRows
					ROWS 
	FETCH NEXT
		@FetchRows
			ROWS ONLY
end 



USE [Test]
GO
/****** Object:  StoredProcedure [dbo].[sp_Pager_OnePage]    Script Date: 2015/7/1 16:05:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[sp_Pager_OnePage]
	@Any varchar(max)

	--===========================================================

	, @TabName					nvarchar(50)	out
	, @Version					nvarchar(50)	out
	, @SysID					nvarchar(50)	out
	, @InfCenterID				nvarchar(50)	out
	, @ProvinceOrgID			nvarchar(50)	out
	, @DataStartDate			date			out
	, @DataEndDate				date			out
	, @IncID					nvarchar(50)	out
	--, @RecNum					varchar(128)	out
	, @Sep						nvarchar(50)	out
	--, @TotalFiles				int				out
	--, @FileSeq					int				out
	, @GenTime					datetime		out

	--============================================================
	, @MinID					int = -1		out
	, @MaxID					int = -1		out
	, @LeftID					int = -1		out
	, @Top						int = 10
	, @IsLast					bit = 0			out 
	--====================================================================
	, @Result					int out
as
begin
	if
		(
			@MinID = -1
			and @MaxID = -1
		)
	begin
		;with TSummary
		as
		(
			--================================================
			select
				min(a.object_id) as MinID
				, max(a.Object_id) as MaxID
				, count(1) as RowsCount
			from
				sys.objects a
			--where
			--	a.object_id > 0
			--	and a.object_id  < 1001
			--================================================
		)
		select
			@MinID = MinID
			,@MaxID = MaxID
		from
			TSummary
	end
	set @IsLast = 0
	if 
		(
			@MinID > 0
			and
			@MaxID > 0
			and
			@MaxID >= @MinID
		)
	begin
		if (@LeftID = -1)
		begin
			set @LeftID = @MinID - 1	
		end
		
		--========================================================
		select
				Top
					(@Top)
			*
			, cast(9999999999.0000 as decimal(38,16)) as amount
		from
			sys.objects a
		where
			a.object_id > @LeftID
		--=========================================================
		order by
			a.object_id
		--=========================================================
		if ((@LeftID + @Top) >= @MaxID)
		begin
			set @IsLast = 1
		end
	end


	set @DataStartDate = getdate()
	set @version = 'sadsadsadasdsad'
end
