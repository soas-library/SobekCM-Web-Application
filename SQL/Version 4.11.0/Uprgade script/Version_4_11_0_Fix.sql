
  -- Correct the solr codes for publication date
  update SobekCM_Metadata_Types 
  set SolrCode='date', SolrCode_Facets='date_facets', SolrCode_Display='date.display'
  where MetadataName='Publication Date';
  GO

  -- Add this to the results view fields as well
  if ( not exists ( select 1 from SobekCM_Item_Aggregation_Result_Fields where MetadataTypeID=24 ))
  begin
	update SobekCM_Item_Aggregation_Result_Fields
	set DisplayOrder = DisplayOrder + 1 
	where DisplayOrder > 2;
  
	insert into SobekCM_Item_Aggregation_Result_Fields ( ItemAggregationResultID, MetadataTypeID, DisplayOrder )
	select distinct ItemAggregationResultID, 24, 3
	from SobekCM_Item_Aggregation_Result_Fields;
  end;
  GO

