--/****** Script for SelectTopNRows command from SSMS  ******/
--SELECT TOP 1000 [MetadataTypeID]
--      ,[MetadataName]
--      ,[SobekCode]
--      ,[SolrCode]
--      ,[DisplayTerm]
--      ,[FacetTerm]
--      ,[CustomField]
--      ,[canFacetBrowse]
--      ,[DefaultAdvancedSearch]
--  FROM [test].[dbo].[SobekCM_Metadata_Types]

--  select * from SobekCM_Metadata_Types where SolrCode='all'

  -- Add a setting to choose which search system to use
  if ( not exists ( select 1 from SobekCM_Settings where Setting_Key='Search System' ))
  begin
	insert into SobekCM_Settings ( Setting_Key, Setting_Value, TabPage, Heading, Hidden, Reserved, Help, Options )
	values ( 'Search System', 'Legacy', 'System / Server Settings', 'System Settings', 0, 0, 'Which search system to use, the legacy which uses database searching, or the new completely solr based searching for version 5 (currenty in beta testing). \n\nIMPORTANT!! Using the beta search system requires at least version 7.1.0 of solr, using the new schemas, and re-indexing all your resources.', 'Legacy|Beta');
  end;
  GO

  -- Add legacy solr column, if it doesn't exist
  if ( COL_LENGTH('dbo.SobekCM_Metadata_Types', 'LegacySolrCode') is null )
  begin
	-- Add column
	alter table dbo.SobekCM_Metadata_Types 
	add LegacySolrCode varchar(100) null;
  end;
  GO

  -- Add the new solr facets column, if it doesn't exist
  if ( COL_LENGTH('dbo.SobekCM_Metadata_Types', 'SolrCode_Facets') is null )
  begin
	-- Add column
	alter table dbo.SobekCM_Metadata_Types 
	add SolrCode_Facets varchar(100) null;
  end;
  GO

  -- Add the new display column, if it doesn't exist
  if ( COL_LENGTH('dbo.SobekCM_Metadata_Types', 'SolrCode_Display') is null )
  begin
	-- Add column
	alter table dbo.SobekCM_Metadata_Types 
	add SolrCode_Display varchar(100) null;
  end;
  GO

  -- Copy the data over to the legacy solr code, if not there
  if ( not exists ( select 1 from SobekCM_Metadata_Types where LegacySolrCode is not null ))
  begin
  	-- Copy over all the current data
	update SobekCM_Metadata_Types
	set LegacySolrCode = SolrCode;
  end;
  GO

  update SobekCM_Metadata_Types set SolrCode='subject' where MetadataTypeID=7;
  update SobekCM_Metadata_Types set SolrCode='audience' where MetadataTypeID=9;
  update SobekCM_Metadata_Types set SolrCode='spatial_standard' where MetadataTypeID=10;
  update SobekCM_Metadata_Types set SolrCode='source' where MetadataTypeID=15;
  update SobekCM_Metadata_Types set SolrCode='holding' where MetadataTypeID=16;
  update SobekCM_Metadata_Types set SolrCode='other' where MetadataTypeID=19;
  update SobekCM_Metadata_Types set SolrCode='name_as_subject' where MetadataTypeID=27;
  update SobekCM_Metadata_Types set SolrCode='title_as_subject' where MetadataTypeID=28;
  update SobekCM_Metadata_Types set SolrCode='mime_type' where MetadataTypeID=34;
  update SobekCM_Metadata_Types set SolrCode='fullcitation' where MetadataTypeID=35;
  update SobekCM_Metadata_Types set SolrCode='tracking_box' where MetadataTypeID=36;

  -- DO these first
  update SobekCM_Metadata_Types set MetadataName=replace(MetadataName, '_', ' ') where MetadataName like 'LOM_%';
  update SobekCM_Metadata_Types set MetadataName='LOM Age Range' where MetadataName='LOM AgeRange';
  GO

  insert into SobekCM_Metadata_Types ( MetadataName, SobekCode, SolrCode, DisplayTerm, FacetTerm, CustomField, canFacetBrowse, DefaultAdvancedSearch, SolrCode_Facets, SolrCode_Display )
  values ( 'Performance', 'PE', 'performance', 'Performance', 'Peformance', 'false', 'false', 'false', 'peformance_facets', null );
  GO

  insert into SobekCM_Metadata_Types ( MetadataName, SobekCode, SolrCode, DisplayTerm, FacetTerm, CustomField, canFacetBrowse, DefaultAdvancedSearch, SolrCode_Facets, SolrCode_Display )
  values ( 'Performance Date', 'PD', 'performance_date', 'Performance Date', 'Peformance Date', 'false', 'false', 'false', 'peformance_date_facets', null );
  GO

  insert into SobekCM_Metadata_Types ( MetadataName, SobekCode, SolrCode, DisplayTerm, FacetTerm, CustomField, canFacetBrowse, DefaultAdvancedSearch, SolrCode_Facets, SolrCode_Display )
  values ( 'Performer', 'PR', 'performer', 'Performer', 'Peformer', 'false', 'false', 'false', 'peformer_facets', null );
  GO


  -- Also, add:
  --   lom learning time 
  --   lom resource type
  --   zt hierarchical
  --   translated title
  --   
  -- check all display fields and suppress?




  -- Create the update SQL
  select 'update SobekCM_Metadata_Types set SolrCode=''' + SolrCode + ''', SolrCode_Facets=''' + SolrCode_Facets + ''',SolrCode_Display=''' + SolrCode_Display + ''' where MetadataName=''' + MetadataName + ''''
  from SobekCM_Metadata_Types
  where SolrCode_Facets is not null and SolrCode_Display is not null
  union
  select 'update SobekCM_Metadata_Types set SolrCode=''' + SolrCode + ''', SolrCode_Facets=NULL,SolrCode_Display=''' + SolrCode_Display + ''' where MetadataName=''' + MetadataName + ''''
  from SobekCM_Metadata_Types
  where SolrCode_Facets is null and SolrCode_Display is not null
  union
  select 'update SobekCM_Metadata_Types set SolrCode=''' + SolrCode + ''', SolrCode_Facets=''' + SolrCode_Facets + ''',SolrCode_Display=NULL where MetadataName=''' + MetadataName + ''''
  from SobekCM_Metadata_Types
  where SolrCode_Facets is not null and SolrCode_Display is null
  union
  select 'update SobekCM_Metadata_Types set SolrCode=''' + SolrCode + ''', SolrCode_Facets=NULL,SolrCode_Display=NULL where MetadataName=''' + MetadataName + ''''
  from SobekCM_Metadata_Types
  where SolrCode_Facets is null and SolrCode_Display is null
