
-- Ensure the stored procedure exists
IF object_id('mySobek_Permissions_Report') IS NULL EXEC ('create procedure dbo.mySobek_Permissions_Report as select 1;');
GO

-- Get the list of users that have top-level permissions, such as editing all items,
-- being an admin, deleting all items, or a power user
ALTER PROCEDURE dbo.mySobek_Permissions_Report as
begin

	-- Return the top-level permissions (non-aggregation specific)
	select '' as GroupName, U.UserID, UserName, EmailAddress, FirstName, LastName, Nickname, DateCreated, LastActivity, isActive, 
		case when e.UserID is null then 'false' else 'true' end as Can_Edit_All_Items,
		Internal_User, Can_Delete_All_Items, IsPortalAdmin, IsSystemAdmin, IsHostAdmin
	from mySobek_User as U left outer join
		 mySobek_User_Editable_Link as E on E.UserID = U.UserID and E.EditableID = 1 
	where      ( IsSystemAdmin = 'true' )
			or ( IsPortalAdmin = 'true' )
			or ( Can_Delete_All_Items = 'true' )
			or ( IsHostAdmin = 'true' )
			or ( Internal_User = 'true' )
	union
	select G.GroupName, U.UserID, UserName, EmailAddress, FirstName, LastName, Nickname, DateCreated, LastActivity, isActive, 
		case when e.UserGroupID is null then 'false' else 'true' end as Can_Edit_All_Items,
		G.Internal_User, G.Can_Delete_All_Items, G.IsPortalAdmin, G.IsSystemAdmin, 'false'
	from mySobek_User as U inner join
		 mySobek_User_Group_Link as L on U.UserID = L.UserID inner join
		 mySobek_User_Group as G on G.UserGroupID = L.UserGroupID left outer join
		 mySobek_User_Group_Editable_Link as E on E.UserGroupID = G.UserGroupID and E.EditableID = 1 
	where      ( G.IsSystemAdmin = 'true' )
			or ( G.IsPortalAdmin = 'true' )
			or ( G.Can_Delete_All_Items = 'true' )
			or ( G.Internal_User = 'true' )
	order by LastName ASC, FirstName ASC, GroupName ASC;
end;
GO

-- Ensure the stored procedure exists
IF object_id('mySobek_Permissions_Report_Aggregation_Links') IS NULL EXEC ('create procedure dbo.mySobek_Permissions_Report_Aggregation_Links as select 1;');
GO

-- Get the list of users and for each user the list of aggregations they
-- have special rights over (wither by user or through user group )
ALTER PROCEDURE dbo.mySobek_Permissions_Report_Aggregation_Links as
begin
	-- Create a temporary table to hold all the user-aggregations links
	create table #tmpAggrPermissions (
		UserID int primary key,
		UserPermissioned varchar(2000),
		GroupPermissioned varchar(2000)
	);


	-- Return the aggregation-specific permissions (at user level unioned with group level)
	insert into #tmpAggrPermissions (UserID)
	select UserID
	from mySobek_User_Edit_Aggregation as P inner join
		 SobekCM_Item_Aggregation A on A.AggregationID=P.AggregationID
	where ( P.CanEditMetadata='true' ) 
	   or ( P.CanEditBehaviors='true' )
	   or ( P.CanPerformQc='true' )
	   or ( P.CanUploadFiles='true' )
	   or ( P.CanChangeVisibility='true' )
	   or ( P.IsCurator='true' )
	   or ( P.IsAdmin='true' )
	group by UserID
	union
	select UserID
	from mySobek_User_Group_Link as L inner join
		 mySobek_User_Group as G on G.UserGroupID = L.UserGroupID inner join
		 mySobek_User_Group_Edit_Aggregation as P on P.UserGroupID = P.UserGroupID inner join
		 SobekCM_Item_Aggregation A on A.AggregationID=P.AggregationID
	where ( P.CanEditMetadata='true' ) 
	   or ( P.CanEditBehaviors='true' )
	   or ( P.CanPerformQc='true' )
	   or ( P.CanUploadFiles='true' )
	   or ( P.CanChangeVisibility='true' )
	   or ( P.IsCurator='true' )
	   or ( P.IsAdmin='true' )
	group by UserID;

	-- Create the cursor to go through the users
	declare UserCursor CURSOR
	LOCAL STATIC FORWARD_ONLY READ_ONLY
	for select UserID from #tmpAggrPermissions;

	-- Open the user cursor
	open UserCursor;

	-- Variable for the cursor loops
	declare @UserID int;
	declare @Code varchar(20);
	declare @UserPermissions varchar(2000);
	declare @GroupPermissions varchar(2000);

	-- Fetch first userid
	fetch next from UserCursor into @UserId;

	-- Step through all users
	While ( @@FETCH_STATUS = 0 )
	begin
		-- Clear the permissions variables
		set @UserPermissions = '';
		set @GroupPermissions = '';

		-- Create the cursor aggregation permissions at the user level	
		declare UserPermissionedCursor CURSOR
		LOCAL STATIC FORWARD_ONLY READ_ONLY
		FOR
		select A.Code
		from mySobek_User_Edit_Aggregation as P inner join
			 SobekCM_Item_Aggregation A on A.AggregationID=P.AggregationID
		where ( P.UserID=@UserID )
		  and (    ( P.CanEditMetadata='true' ) 
		        or ( P.CanEditBehaviors='true' )
		        or ( P.CanPerformQc='true' )
		        or ( P.CanUploadFiles='true' )
		        or ( P.CanChangeVisibility='true' )
		        or ( P.IsCurator='true' )
		        or ( P.IsAdmin='true' ))
		order by A.Code;
	    
		-- Open the user-level aggregation permissions cursor
		open UserPermissionedCursor;

		-- Fetch first user-level aggregation permissions
		fetch next from UserPermissionedCursor into @Code;

		-- Step through each aggregation-level permissioned
		while ( @@FETCH_STATUS = 0 )
		begin
			set @UserPermissions = @UserPermissions + @Code + ', ';

			-- Fetch next user-level aggregation permissions
			fetch next from UserPermissionedCursor into @Code;
		end;

		CLOSE UserPermissionedCursor;
		DEALLOCATE UserPermissionedCursor;

		-- Create the cursor aggregation permissions at the group level	
		declare GroupPermissionedCursor CURSOR
		LOCAL STATIC FORWARD_ONLY READ_ONLY
		FOR
		select A.Code
		from mySobek_User_Group_Link as L inner join
			 mySobek_User_Group as G on G.UserGroupID = L.UserGroupID inner join
			 mySobek_User_Group_Edit_Aggregation as P on P.UserGroupID = P.UserGroupID inner join
			 SobekCM_Item_Aggregation A on A.AggregationID=P.AggregationID
		where ( L.UserID=@UserID )
		  and (    ( P.CanEditMetadata='true' ) 
		        or ( P.CanEditBehaviors='true' )
		        or ( P.CanPerformQc='true' )
		        or ( P.CanUploadFiles='true' )
		        or ( P.CanChangeVisibility='true' )
		        or ( P.IsCurator='true' )
		        or ( P.IsAdmin='true' ))
		group by A.Code
		order by A.Code;
	    
		-- Open the group-level aggregation permissions cursor
		open GroupPermissionedCursor;

		-- Fetch first group-level aggregation permissions
		fetch next from GroupPermissionedCursor into @Code;

		-- Step through each aggregation-level permissioned
		while ( @@FETCH_STATUS = 0 )
		begin
			set @GroupPermissions = @GroupPermissions + @Code + ', ';

			-- Fetch next group-level aggregation permissions
			fetch next from GroupPermissionedCursor into @Code;
		end;

		CLOSE GroupPermissionedCursor;
		DEALLOCATE GroupPermissionedCursor;

		-- Now, update this row
		update #tmpAggrPermissions
		set UserPermissioned=@UserPermissions, GroupPermissioned=@GroupPermissions
		where UserID=@UserId;

		-- Fetch next userid
		fetch next from UserCursor into @UserId;
	end;

	CLOSE UserCursor;
	DEALLOCATE UserCursor;

	-- Return the list of users linked to aggregations, either by group or individually
	select U.UserID, UserName, EmailAddress, FirstName, LastName, Nickname, DateCreated, LastActivity, isActive, T.UserPermissioned, T.GroupPermissioned
	from #tmpAggrPermissions T, mySobek_User U
	where T.UserID=U.UserID
	order by LastName ASC, FirstName ASC;
end;
GO

GRANT EXECUTE ON mySobek_Permissions_Report_Aggregation_Links to sobek_user;
GO

-- Ensure the stored procedure exists
IF object_id('mySobek_Permissions_Report_Linked_Aggregations') IS NULL EXEC ('create procedure dbo.mySobek_Permissions_Report_Linked_Aggregations as select 1;');
GO

-- Get the list of aggregations that have special rights given to some users
ALTER PROCEDURE mySobek_Permissions_Report_Linked_Aggregations AS
BEGIN


	-- Get the list of all aggregations that have special links
	with aggregations_permissioned as
	(
		select distinct AggregationID 
		from mySobek_User_Edit_Aggregation
		union
		select distinct AggregationID 
		from mySobek_User_Group_Edit_Aggregation
	)
	select A.Code, A.Name, A.Type
	from SobekCM_Item_Aggregation A, aggregations_permissioned P
	where A.AggregationID = P.AggregationID
	order by A.Code;

END;
GO

GRANT EXECUTE ON mySobek_Permissions_Report_Linked_Aggregations to sobek_user;
GO

-- Ensure the stored procedure exists
IF object_id('mySobek_Permissions_Report_Submission_Rights') IS NULL EXEC ('create procedure dbo.mySobek_Permissions_Report_Submission_Rights as select 1;');
GO

-- Get the list of users, with informaiton about the templates and default metadata
ALTER PROCEDURE dbo.mySobek_Permissions_Report_Submission_Rights as
BEGIN

	-- Create a temporary table to hold all the user-aggregations links
	create table #tmpSubmitPermissions (
		UserID int primary key,
		Templates varchar(2000),
		DefaultMetadatas varchar(2000)
	);

	-- Get the list of all users that can submit materials
	insert into #tmpSubmitPermissions (UserID)
	select U.UserID
	from mySobek_User as U 
	where  ( Can_Submit_Items = 'true' )
	union
	select U.UserID
	from mySobek_User as U inner join
		 mySobek_User_Group_Link as L on U.UserID = L.UserID inner join
		 mySobek_User_Group as G on G.UserGroupID = L.UserGroupID 
	where ( G.Can_Submit_Items = 'true' );



	-- Return the list
	select U.UserID, UserName, EmailAddress, FirstName, LastName, Nickname, DateCreated, LastActivity, isActive, 
	       coalesce(T.Templates,'') as Templates, coalesce(T.DefaultMetadatas,'') as DefaultMetadatas
	from #tmpSubmitPermissions T, mySobek_User U
	where T.UserID=U.UserID
	order by LastName ASC, FirstName ASC;

	-- Drop the temporary table
	drop table #tmpSubmitPermissions;

END;
GO

GRANT EXECUTE ON mySobek_Permissions_Report_Submission_Rights to sobek_user;
GO