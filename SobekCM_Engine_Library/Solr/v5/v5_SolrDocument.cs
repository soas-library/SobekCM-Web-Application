﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Divisions;
using SobekCM.Resource_Object.Metadata_Modules;
using SobekCM.Resource_Object.Metadata_Modules.GeoSpatial;
using SobekCM.Resource_Object.Solr;
using SolrNet.Attributes;

namespace SobekCM.Engine_Library.Solr.v5
{
    /// <summary> Represents a solr document in the version 5.0 and on solr/lucene indexes </summary>
    public class v5_SolrDocument
    {

        // IN DATABASE HANDLE NEW FIELDS:
        //   Translated Title
        //   ZT Hierarchical
        //   LOM fields (and remove underscore in db metadata type names and learning time and resource type)

        // Add VRA core Materials display
        // Add VRA Core Measurements display

        // Performance

        private readonly List<string> additional_text_files = new List<string>();
        private readonly string fileLocation;

        #region Constructors for this class 

        /// <summary> Constructor for a new instance of the v5_SolrDocument class </summary>
        public v5_SolrDocument()
        {
            // Set default title and type
            Title = "Missing Title";
          //  Type = "Unkown";
        }


        /// <summary> Constructor for a new instance of the v5_SolrDocument class </summary>
        /// <param name="Digital_Object"> Digital object to create an easily indexable view object for </param>
        /// <param name="File_Location"> Location for all of the text files associated with this item </param>
        /// <remarks> Some work is done in the constructor; in particular, work that eliminates the number of times 
        /// iterations must be made through objects which may be indexed in a number of places.  
        /// This includes subject keywords, spatial information, genres, and information from the table of contents </remarks>
        public v5_SolrDocument(SobekCM_Item Digital_Object, string File_Location)
        {
            fileLocation = File_Location;

            // Set the unique key
            DID = Digital_Object.BibID + ":" + Digital_Object.VID;

            // Add the administrative fields
            Aggregations = new List<string>();
            Aggregations.AddRange(Digital_Object.Behaviors.Aggregation_Code_List);
            BibID = Digital_Object.BibID;
            VID = Digital_Object.VID;
            MainThumbnail = Digital_Object.Behaviors.Main_Thumbnail;

            // Add Serial hierarchy fields
            Level1_Text = String.Empty;
            Level1_Index = -1;
            Level2_Text = String.Empty;
            Level2_Index = -1;
            Level3_Text = String.Empty;
            Level3_Index = -1;
            if (Digital_Object.Behaviors != null)
            {
                if (Digital_Object.Behaviors.Serial_Info.Count > 0)
                {
                    Level1_Index = Digital_Object.Behaviors.Serial_Info[0].Order;
                    Level1_Text = Digital_Object.Behaviors.Serial_Info[0].Display;
                }
                if (Digital_Object.Behaviors.Serial_Info.Count > 1)
                {
                    Level1_Index = Digital_Object.Behaviors.Serial_Info[1].Order;
                    Level1_Text = Digital_Object.Behaviors.Serial_Info[1].Display;
                }
                if (Digital_Object.Behaviors.Serial_Info.Count > 2)
                {
                    Level1_Index = Digital_Object.Behaviors.Serial_Info[2].Order;
                    Level1_Text = Digital_Object.Behaviors.Serial_Info[2].Display;
                }

                Dark = Digital_Object.Behaviors.Dark_Flag;
            }

            // Some defaults
            Discover_Groups = new List<int> {-1};
            Discover_Users = new List<int> { -1 };
            Discover_IPs = new List<int> { -1 };
            RestrictedMsg = String.Empty;

            // Set the spatial KML
            GeoSpatial_Information geo = Digital_Object.Get_Metadata_Module(GlobalVar.GEOSPATIAL_METADATA_MODULE_KEY) as GeoSpatial_Information;
            if (geo != null)
            {
                if (SpatialFootprint == null) SpatialFootprint = new List<string>();
                SpatialFootprint.Add(geo.SobekCM_Main_Spatial_String);

                SpatialDistance = (int) geo.SobekCM_Main_Spatial_Distance;
            }

            // Get the rest of the metadata, from the item
            List<KeyValuePair<string, string>> searchTerms = Digital_Object.Search_Terms;

            foreach (KeyValuePair<string, string> searchTerm in searchTerms)
            {
                // Ensure there is a value here
                if (String.IsNullOrWhiteSpace(searchTerm.Value))
                    continue;

                // Assign based on the key term
                switch (searchTerm.Key.ToLower())
                {
                    case "title":
                        Title = searchTerm.Value;
                        break;

                    case "sort title":
                        SortTitle = searchTerm.Value;
                        break;

                    case "other title":
                        if (AltTitle == null) AltTitle = new List<string>();
                        AltTitle.Add(searchTerm.Value);
                        break;

                    case "translated title":
                        if (TranslatedTitle == null) TranslatedTitle = new List<string>();
                        TranslatedTitle.Add(searchTerm.Value);
                        break;

                    case "series title":
                        SeriesTitle = searchTerm.Value;
                        break;

                    case "other citation":
                        if (OtherCitation == null) OtherCitation = new List<string>();
                        OtherCitation.Add(searchTerm.Value);
                        break;

                    case "tickler":
                        if (Tickler == null) Tickler = new List<string>();
                        Tickler.Add(searchTerm.Value);
                        break;

                    case "abstract":
                        if (Abstract == null) Abstract = new List<string>();
                        Abstract.Add(searchTerm.Value);
                        break;
                        
                    case "affililation":
                        if (Affiliation == null) Affiliation = new List<string>();
                        Affiliation.Add(searchTerm.Value);
                        break;

                    case "genre":
                        if (Genre == null) Genre = new List<string>();
                        Genre.Add(searchTerm.Value);
                        break;

                    case "genre display":
                        if (GenreDisplay == null) GenreDisplay = new List<string>();
                        GenreDisplay.Add(searchTerm.Value);
                        break;

                    case "donor":
                        Donor = searchTerm.Value;
                        break;

                    case "identifier":
                        if (Identifier == null) Identifier = new List<string>();
                        Identifier.Add(searchTerm.Value);
                        break;

                    case "identifier display":
                        if (IdentifierDisplay == null) IdentifierDisplay = new List<string>();
                        IdentifierDisplay.Add(searchTerm.Value);
                        break;

                    case "accession number":
                        AccessionNumber = searchTerm.Value;
                        break;

                    case "language":
                        if (Language == null) Language = new List<string>();
                        Language.Add(searchTerm.Value);
                        break;

                    case "creator":
                        if (Creator == null) Creator = new List<string>();
                        Creator.Add(searchTerm.Value);
                        break;

                    case "creator.display":
                        if (Creator_Display == null) Creator_Display = new List<string>();
                        Creator_Display.Add(searchTerm.Value);
                        break;

                    case "publisher":
                        if (Publisher == null) Publisher = new List<string>();
                        Publisher.Add(searchTerm.Value);
                        break;

                    case "publisher.display":
                        if (Publisher_Display == null) Publisher_Display = new List<string>();
                        Publisher_Display.Add(searchTerm.Value);
                        break;

                    case "holding location":
                        Holding = searchTerm.Value;
                        break;

                    case "notes":
                        if (Notes == null) Notes = new List<string>();
                        Notes.Add(searchTerm.Value);
                        break;

                    case "frequency":
                        if (Frequency == null) Frequency = new List<string>();
                        Frequency.Add(searchTerm.Value);
                        break;

                    case "edition":
                        Edition = searchTerm.Value;
                        break;

                    case "publication place":
                        if (PubPlace == null) PubPlace = new List<string>();
                        PubPlace.Add(searchTerm.Value);
                        break;

                    case "format":
                        Format = searchTerm.Value;
                        break;

                    case "source institution":
                        Source = searchTerm.Value;
                        break;

                    case "target audience":
                        if (Audience == null) Audience = new List<string>();
                        Audience.Add(searchTerm.Value);
                        break;

                    case "type":
                        Type = searchTerm.Value;
                        break;

                    case "name as subject":
                        if (NameAsSubject == null) NameAsSubject = new List<string>();
                        NameAsSubject.Add(searchTerm.Value);
                        break;

                    case "name as subject dispay":
                        if (NameAsSubjectDisplay == null) NameAsSubjectDisplay = new List<string>();
                        NameAsSubjectDisplay.Add(searchTerm.Value);
                        break;

                    case "title as subject":
                        if (TitleAsSubject == null) TitleAsSubject = new List<string>();
                        TitleAsSubject.Add(searchTerm.Value);
                        break;

                    case "title as subject display":
                        if (TitleAsSubjectDisplay == null) TitleAsSubjectDisplay = new List<string>();
                        TitleAsSubjectDisplay.Add(searchTerm.Value);
                        break;
                        
                    case "spatial coverage":
                        if (Spatial == null) Spatial = new List<string>();
                        Spatial.Add(searchTerm.Value);
                        break;

                    case "spatial coverage.dispay":
                        if (SpatialDisplay == null) SpatialDisplay = new List<string>();
                        SpatialDisplay.Add(searchTerm.Value);
                        break;

                    case "country":
                        if (Country == null) Country = new List<string>();
                        Country.Add(searchTerm.Value);
                        break;

                    case "state":
                        if (State == null) State = new List<string>();
                        State.Add(searchTerm.Value);
                        break;

                    case "county":
                        if (County == null) County = new List<string>();
                        County.Add(searchTerm.Value);
                        break;

                    case "city":
                        if (City == null) City = new List<string>();
                        City.Add(searchTerm.Value);
                        break;

                    case "subject keyword":
                        if (Subject == null) Subject = new List<string>();
                        Subject.Add(searchTerm.Value.Trim());
                        break;

                    case "subjects.display":
                        if (SubjectDisplay == null) SubjectDisplay = new List<string>();
                        SubjectDisplay.Add(searchTerm.Value.Trim());
                        break;
                    
                    case "publication date":
                        Date = searchTerm.Value;
                        break;

                    case "toc":
                        if (TableOfContents == null) TableOfContents = new List<string>();
                        TableOfContents.Add(searchTerm.Value.Trim());
                        break;

                    case "mime type":
                        if (MimeType == null) MimeType = new List<string>();
                        MimeType.Add(searchTerm.Value.Trim());
                        break;

                    case "cultural context":
                        if ( CulturalContext == null ) CulturalContext = new List<string>();
                        CulturalContext.Add(searchTerm.Value.Trim());
                        break;

                    case "inscription":
                        if (Inscription == null) Inscription = new List<string>();
                        Inscription.Add(searchTerm.Value.Trim());
                        break;

                    case "materials":
                        if (Material == null) Material = new List<string>();
                        Material.Add(searchTerm.Value.Trim());
                        break;

                    case "measurements":
                        if (Measurements == null) Measurements = new List<string>();
                        Measurements.Add(searchTerm.Value.Trim());
                        break;

                    case "style period":
                        if (StylePeriod == null) StylePeriod = new List<string>();
                        StylePeriod.Add(searchTerm.Value.Trim());
                        break;

                    case "technique":
                        if (Technique == null) Technique = new List<string>();
                        Technique.Add(searchTerm.Value.Trim());
                        break;

                    case "interviewee":
                        if (Interviewee == null) Interviewee = new List<string>();
                        Interviewee.Add(searchTerm.Value.Trim());
                        break;

                    case "interviewer":
                        if (Interviewer == null) Interviewer = new List<string>();
                        Interviewer.Add(searchTerm.Value.Trim());
                        break;

                    case "performance":
                        Performance = searchTerm.Value.Trim();
                        break;
                        
                    case "performance date":
                        PerformanceDate = searchTerm.Value.Trim();
                        break;

                    case "performer":
                        if (Performer == null) Performer = new List<string>();
                        Performer.Add(searchTerm.Value.Trim());
                        break;

                    case "etd committee":
                        if (EtdCommittee == null) EtdCommittee = new List<string>();
                        EtdCommittee.Add(searchTerm.Value.Trim());
                        break;

                    case "etd degree":
                        EtdDegree = searchTerm.Value.Trim();
                        break;

                    case "etd degree discipline":
                        EtdDegreeDiscipline = searchTerm.Value.Trim();
                        break;

                    case "etd degree division":
                        EtdDegreeDivision = searchTerm.Value.Trim();
                        break;

                    case "etd degree grantor":
                        EtdDegreeGrantor = searchTerm.Value.Trim();
                        break;

                    case "etd degree level":
                        EtdDegreeLevel = searchTerm.Value.Trim();
                        break;

                    case "zt kingdom":
                        if (ZoologicalKingdom == null) ZoologicalKingdom = new List<string>();
                        ZoologicalKingdom.Add(searchTerm.Value.Trim());
                        break;

                    case "zt phylum":
                        if (ZoologicalPhylum == null) ZoologicalPhylum = new List<string>();
                        ZoologicalPhylum.Add(searchTerm.Value.Trim());
                        break;

                    case "zt class":
                        if (ZoologicalClass == null) ZoologicalClass = new List<string>();
                        ZoologicalClass.Add(searchTerm.Value.Trim());
                        break;

                    case "zt order":
                        if (ZoologicalOrder == null) ZoologicalOrder = new List<string>();
                        ZoologicalOrder.Add(searchTerm.Value.Trim());
                        break;

                    case "zt family":
                        if (ZoologicalFamily == null) ZoologicalFamily = new List<string>();
                        ZoologicalFamily.Add(searchTerm.Value.Trim());
                        break;

                    case "zt genus":
                        if (ZoologicalGenus == null) ZoologicalGenus = new List<string>();
                        ZoologicalGenus.Add(searchTerm.Value.Trim());
                        break;

                    case "zt species":
                        if (ZoologicalSpecies == null) ZoologicalSpecies = new List<string>();
                        ZoologicalSpecies.Add(searchTerm.Value.Trim());
                        break;

                    case "zt common name":
                        if (ZoologicalCommonName == null) ZoologicalCommonName = new List<string>();
                        ZoologicalCommonName.Add(searchTerm.Value.Trim());
                        break;

                    case "zt scientific name":
                        if (ZoologicalScientificName == null) ZoologicalScientificName = new List<string>();
                        ZoologicalScientificName.Add(searchTerm.Value.Trim());
                        break;

                    case "zt hierarchical":
                        if (ZoologicalHierarchical == null) ZoologicalHierarchical = new List<string>();
                        ZoologicalHierarchical.Add(searchTerm.Value.Trim());
                        break;

                    // Solr already rolls up to a zt_all field, so ignore this
                    case "zt all taxonomy":
                        break;

                    case "lom aggregation":
                        LomAggregation = searchTerm.Value.Trim();
                        break;

                    case "lom context":
                        if (LomContext == null) LomContext = new List<string>();
                        LomContext.Add(searchTerm.Value.Trim());
                        break;

                    case "lom context display":
                        if (LomContextDisplay == null) LomContextDisplay = new List<string>();
                        LomContextDisplay.Add(searchTerm.Value.Trim());
                        break;

                    case "lom difficulty":
                        LomDifficulty = searchTerm.Value.Trim();
                        break;

                    case "lom intended end user":
                        if (LomIntendedEndUser == null) LomIntendedEndUser = new List<string>();
                        LomIntendedEndUser.Add(searchTerm.Value.Trim());
                        break;

                    case "lom intended end user display":
                        LomIntendedEndUserDisplay = searchTerm.Value.Trim();
                        break;

                    case "lom interactivity level":
                        LomInteractivityLevel = searchTerm.Value.Trim();
                        break;

                    case "lom interactivity type":
                        LomInteractivityType = searchTerm.Value.Trim();
                        break;

                    case "lom status":
                        LomStatus = searchTerm.Value.Trim();
                        break;

                    case "lom requirement":
                        if (LomRequirement == null) LomRequirement = new List<string>();
                        LomRequirement.Add(searchTerm.Value.Trim());
                        break;

                    case "lom requirement display":
                        if (LomRequirementDisplay == null) LomRequirementDisplay = new List<string>();
                        LomRequirementDisplay.Add(searchTerm.Value.Trim());
                        break;

                    case "lom age range":
                        if ( LomAgeRange == null ) LomAgeRange = new List<string>();
                        LomAgeRange.Add(searchTerm.Value.Trim());
                        break;

                    case "lom resource type":
                        if (LomResourceType == null) LomResourceType = new List<string>();
                        LomResourceType.Add(searchTerm.Value.Trim());
                        break;

                    case "lom resource type display":
                        if (LomResourceTypeDisplay == null) LomResourceTypeDisplay = new List<string>();
                        LomResourceTypeDisplay.Add(searchTerm.Value.Trim());
                        break;

                    case "lom learning time":
                        LomLearningTime = searchTerm.Value.Trim();
                        break;

                    // Not handled yet
                    case "temporal year":
                    case "ead name":
                        break;
                        

                    // Ignore these
                    case "bibid":
                    case "vid":
                        break;
                        


                }
            }

 
            //// Subject metadata fields ( and also same spatial information )
            //List<string> spatials = new List<string>();
            //List<Subject_Info_HierarchicalGeographic> hierarhicals = new List<Subject_Info_HierarchicalGeographic>();
            //if ( Digital_Object.Bib_Info.Subjects_Count > 0 )
            //{
            //    List<string> subjects = new List<string>();
            //    List<string> name_as_subject = new List<string>();
            //    List<string> title_as_subject = new List<string>();

            //    // Collect the types of subjects
            //    foreach (Subject_Info thisSubject in Digital_Object.Bib_Info.Subjects)
            //    {
            //        switch (thisSubject.Class_Type)
            //        {
            //            case Subject_Info_Type.Name:
            //                name_as_subject.Add(thisSubject.ToString());
            //                break;

            //             case Subject_Info_Type.TitleInfo:
            //                title_as_subject.Add(thisSubject.ToString());
            //                break;

            //             case Subject_Info_Type.Standard:
            //                subjects.Add(thisSubject.ToString());
            //                Subject_Info_Standard standardSubj = thisSubject as Subject_Info_Standard;
            //                if (standardSubj.Geographics_Count > 0)
            //                {
            //                    spatials.AddRange(standardSubj.Geographics);
            //                }
            //                break;

            //            case Subject_Info_Type.Hierarchical_Spatial:
            //                hierarhicals.Add( thisSubject as Subject_Info_HierarchicalGeographic);
            //                break;
            //        }
            //    }

            //    // Now add to this document, if present
            //    if (name_as_subject.Count > 0)
            //    {
            //        NameAsSubject = new List<string>();
            //        NameAsSubject.AddRange(name_as_subject);
            //    }
            //    if (title_as_subject.Count > 0)
            //    {
            //        TitleAsSubject = new List<string>();
            //        TitleAsSubject.AddRange(title_as_subject);
            //    }
            //    if (subjects.Count > 0)
            //    {
            //        Subject = new List<string>();
            //        Subject.AddRange(subjects);
            //    }
            //}




            // Add the empty solr pages for now
            Solr_Pages = new List<Legacy_SolrPage>();

            // Prepare to step through all the divisions/pages in this item
            int pageorder = 1;
            List<abstract_TreeNode> divsAndPages = Digital_Object.Divisions.Physical_Tree.Divisions_PreOrder;

            // Get the list of all TXT files in this division
            string[] text_files = Directory.GetFiles(File_Location, "*.txt");
            Dictionary<string, string> text_files_existing = new Dictionary<string, string>();
            foreach (string thisTextFile in text_files)
            {
                string filename = (new FileInfo(thisTextFile)).Name.ToUpper();
                text_files_existing[filename] = filename;
            }

            // Get the list of all THM.JPG files in this division
            string[] thumbnail_files = Directory.GetFiles(File_Location, "*thm.jpg");
            Dictionary<string, string> thumbnail_files_existing = new Dictionary<string, string>();
            foreach (string thisTextFile in thumbnail_files)
            {
                string filename = (new FileInfo(thisTextFile)).Name;
                thumbnail_files_existing[filename.ToUpper().Replace("THM.JPG", "")] = filename;
            }

            // Step through all division nodes from the physical tree here
            List<string> text_files_included = new List<string>();
            foreach (abstract_TreeNode thisNode in divsAndPages)
            {
                if (thisNode.Page)
                {
                    // Cast to a page to continnue
                    Page_TreeNode pageNode = (Page_TreeNode)thisNode;

                    // Look for the root filename and then look for a matching TEXT file
                    if (pageNode.Files.Count > 0)
                    {
                        string root = pageNode.Files[0].File_Name_Sans_Extension;
                        if (text_files_existing.ContainsKey(root.ToUpper() + ".TXT"))
                        {
                            try
                            {
                                // SInce this is marked to be included, save this name
                                text_files_included.Add(root.ToUpper() + ".TXT");

                                // Read the page text
                                StreamReader reader = new StreamReader(File_Location + "\\" + root + ".txt");
                                string pageText = reader.ReadToEnd().Trim();
                                reader.Close();

                                // Look for a matching thumbnail
                                string thumbnail = String.Empty;
                                if (thumbnail_files_existing.ContainsKey(root.ToUpper()))
                                    thumbnail = thumbnail_files_existing[root.ToUpper()];

                                Legacy_SolrPage newPage = new Legacy_SolrPage(Digital_Object.BibID, Digital_Object.VID, pageorder, pageNode.Label, pageText, thumbnail);
                                Solr_Pages.Add(newPage);
                            }
                            catch
                            {
                            }
                        }
                    }

                    // Increment the page order for the next page irregardless
                    pageorder++;
                }
            }

            // Now, check for any other valid text files 
            additional_text_files = new List<string>();
            foreach (string thisTextFile in text_files_existing.Keys)
            {
                if ((!text_files_included.Contains(thisTextFile.ToUpper())) && (thisTextFile.ToUpper() != "AGREEMENT.TXT") && (thisTextFile.ToUpper().IndexOf("REQUEST") != 0))
                {
                    additional_text_files.Add(thisTextFile);
                }
            }
        }

        #endregion


        /// <summary> Gets the collection of page objects for Solr indexing </summary>
        public List<Legacy_SolrPage> Solr_Pages { get; set; }

        /// <summary> Returns the full text for all the pages within this document for the Solr engine to index for this document </summary>
        [SolrField("fulltext")]
        public string FullText
        {
            get
            {
                if (((Solr_Pages == null) || (Solr_Pages.Count == 0)) && (additional_text_files.Count == 0))
                    return null;

                StringBuilder builder = new StringBuilder(10000);

                // Add the text for each page 
                if (Solr_Pages != null)
                {
                    foreach (Legacy_SolrPage thisPage in Solr_Pages)
                    {
                        builder.Append(thisPage.PageText);
                        builder.Append(" ");
                    }
                }

                // Also add the text from any other text files
                if (additional_text_files != null)
                {
                    foreach (string textFile in additional_text_files)
                    {
                        try
                        {
                            StreamReader reader = new StreamReader(fileLocation + "\\" + textFile);
                            builder.Append(reader.ReadToEnd() + " ");
                            reader.Close();
                        }
                        catch
                        {
                            // do nothing
                        }
                    }
                }

                return builder.ToString();
            }
        }

        /// <summary> Unique key for this document</summary>
        [SolrUniqueKey("did")]
        public string DID { get; set; }

        #region Administrative Fields

        /// <summary> List of aggregations to which this item is linked </summary>
        [SolrField("aggregations")]
        public List<string> Aggregations { get; set; }

        /// <summary> Bibliographic identifier for this title (multiple volumes potentially) </summary>
        [SolrField("bibid")]
        public string BibID { get; set; }

        /// <summary> Volume identifier </summary>
        [SolrField("vid")]
        public string VID { get; set; }

        /// <summary> Main thumbnail for this item </summary>
        [SolrField("mainthumb")]
        public string MainThumbnail { get; set; }

        #endregion

        #region Serial hierarchy fields

        /// <summary> Text for the serial hierarchy level 1 for this item </summary>
        [SolrField("level1text")]
        public string Level1_Text { get; set; }

        /// <summary> Index for the serial hierarchy level 1 for this item </summary>
        [SolrField("level1index")]
        public int Level1_Index { get; set; }

        /// <summary> Text for the serial hierarchy level 2 for this item </summary>
        [SolrField("level2text")]
        public string Level2_Text { get; set; }

        /// <summary> Index for the serial hierarchy level 2 for this item </summary>
        [SolrField("level2index")]
        public int Level2_Index { get; set; }

        /// <summary> Text for the serial hierarchy level 3 for this item </summary>
        [SolrField("level3text")]
        public string Level3_Text { get; set; }

        /// <summary> Index for the serial hierarchy level 3 for this item </summary>
        [SolrField("level3index")]
        public int Level3_Index { get; set; }

        #endregion

        #region Authority system ID's for use in the local authority system

        /// <summary> Creator authority ids related to this digital resource </summary>
        [SolrField("creator.authid")]
        public List<int> Creator_AuthID { get; set; }

        /// <summary> Publisher authority ids related to this digital resource </summary>
        [SolrField("publisher.authid")]
        public List<int> Publisher_AuthID { get; set; }

        /// <summary> Subject authority ids related to this digital resource </summary>
        [SolrField("subject.authid")]
        public List<int> Subject_AuthID { get; set; }

        /// <summary> Spatial authority ids related to this digital resource </summary>
        [SolrField("spatial.authid")]
        public List<int> Spatial_AuthID { get; set; }

        #endregion

        #region Some basic behavior fields related to discoverability and access

        /// <summary> UserIDs which can discover this digital resource ( or -1 otherwise ) </summary>
        [SolrField("discover_users")]
        public List<int> Discover_Users { get; set; }

        /// <summary> GroupIDs which can discover this digital resource ( or -1 otherwise ) </summary>
        [SolrField("discover_groups")]
        public List<int> Discover_Groups { get; set; }

        /// <summary> Primary keys to the IP address ranges which can discover this digital resource ( or -1 otherwise ) </summary>
        [SolrField("discover_ips")]
        public List<int> Discover_IPs { get; set; }

        /// <summary> Dark flag indicates if this digital resource is considered "dark" or largely inaccessible </summary>
        [SolrField("dark")]
        public bool Dark { get; set; }

        /// <summary> Restricted message to display with this item - generally describes accessibility permissions </summary>
        [SolrField("restricted_msg")]
        public string RestrictedMsg { get; set; }

        #endregion

        #region Main metadata fields

        /// <summary> Main title for this document </summary>
        [SolrField("title")]
        public string Title { get; set; }

        /// <summary> Sort title for this document </summary>
        [SolrField("sorttitle")]
        public string SortTitle { get; set; }

        /// <summary> Alternate titles for this document </summary>
        [SolrField("alttitle")]
        public List<string> AltTitle { get; set; }

        /// <summary> Translated titles for this document (mostly used for display purposes)</summary>
        [SolrField("transtitle")]
        public List<string> TranslatedTitle { get; set; }

        /// <summary> Series titles for this document </summary>
        [SolrField("seriestitle")]
        public string SeriesTitle { get; set; }

        /// <summary> Overall resource type for this document </summary>
        [SolrField("type")]
        public string Type { get; set; }

        /// <summary> Languages for this document </summary>
        [SolrField("language")]
        public List<string> Language { get; set; }

        /// <summary> Creators (and contributors) for this document </summary>
        [SolrField("creator")]
        public List<string> Creator { get; set; }

        /// <summary> Creators (and contributors) for this document for display purposes, which includes the role </summary>
        [SolrField("creator.display")]
        public List<string> Creator_Display { get; set; }

        /// <summary> Affiliations for this document </summary>
        [SolrField("affiliation")]
        public List<string> Affiliation { get; set; }

        /// <summary> Publishers for this document </summary>
        [SolrField("publisher")]
        public List<string> Publisher { get; set; }

        /// <summary> Publisher (display format which includes publication place, etc..) for this document </summary>
        [SolrField("publisher.display")]
        public List<string> Publisher_Display { get; set; }

        /// <summary> Publication places for this document </summary>
        [SolrField("publication_place")]
        public List<string> PubPlace { get; set; }

        /// <summary> Audiences for this document </summary>
        [SolrField("audience")]
        public List<string> Audience { get; set; }

        /// <summary> Source institution (and location) for this document </summary>
        [SolrField("source")]
        public string Source { get; set; }

        /// <summary> Holding location for this document </summary>
        [SolrField("holding")]
        public string Holding { get; set; }

        /// <summary> Identifiers for this document </summary>
        [SolrField("identifier")]
        public List<string> Identifier { get; set; }

        /// <summary> Display version of the identifiers for this document, including identifier type </summary>
        [SolrField("identifier.display")]
        public List<string> IdentifierDisplay { get; set; }

        /// <summary> Notes for this document </summary>
        [SolrField("notes")]
        public List<string> Notes { get; set; }

        /// <summary> Ticklers for this document </summary>
        [SolrField("tickler")]
        public List<string> Tickler { get; set; }

        /// <summary> Donor for this document </summary>
        [SolrField("donor")]
        public string Donor { get; set; }

        /// <summary> Format for this document </summary>
        [SolrField("format")]
        public string Format { get; set; }

        /// <summary> Frequencies for this document </summary>
        [SolrField("frequency")]
        public List<string> Frequency { get; set; }

        /// <summary> Genres for this document </summary>
        [SolrField("genre")]
        public List<string> Genre { get; set; }

        /// <summary> Display version of the genres for this document including the authority </summary>
        [SolrField("genre.display")]
        public List<string> GenreDisplay { get; set; }

        /// <summary> Other citation fields </summary>
        [SolrField("other")]
        public List<string> OtherCitation { get; set; }

        #endregion

        #region Date metadata fields - STILL NEED TO REVIEW THIS!!!

        /// <summary> Date this material was published </summary>
        [SolrField("date.display")]
        public string Date { get; set; }

        #endregion

        #region Subject metadata fields

        /// <summary> Subjects and subject keywords for this document </summary>
        [SolrField("subject")]
        public List<string> Subject { get; set; }

        /// <summary> DIsplay version of subjects and subject keywords for this document </summary>
        [SolrField("subject.display")]
        public List<string> SubjectDisplay { get; set; }

        /// <summary> Name (corporate or personal) as subject for this document </summary>
        [SolrField("name_as_subject")]
        public List<string> NameAsSubject { get; set; }

        /// <summary> Display version of name (corporate or personal) as subject for this document </summary>
        [SolrField("name_as_subject.display")]
        public List<string> NameAsSubjectDisplay { get; set; }

        /// <summary> Title of a work as subject for this document </summary>
        [SolrField("title_as_subject")]
        public List<string> TitleAsSubject { get; set; }

        /// <summary> Dispay version of title of a work as subject for this document </summary>
        [SolrField("title_as_subject.display")]
        public List<string> TitleAsSubjectDisplay { get; set; }

        #endregion

        #region Spatial metadata fields

        /// <summary> Standard spatial subjects for this document </summary>
        [SolrField("spatial_standard")]
        public List<string> Spatial { get; set; }

        /// <summary> Spatial footprint to display this item on a map </summary>
        [SolrField("spatial_footprint")]
        public List<string> SpatialFootprint { get; set; }

        /// <summary> Distance of the spatial footprint for this item on the map </summary>
        [SolrField("spatial_footprint_distance")]
        public int SpatialDistance { get; set; }

        /// <summary> Display version of the standard spatial subjects for this document </summary>
        [SolrField("spatial_standard.display")]
        public List<string> SpatialDisplay { get; set; }

        /// <summary> Hierarchical spatial subjects for this document </summary>
        /// <remarks> Some individual components are also broken out below </remarks>
        [SolrField("spatial_hierarchical")]
        public List<string> HierarchicalSpatial { get; set; }

        /// <summary> Country spatial keywords for this document </summary>
        [SolrField("country")]
        public List<string> Country { get; set; }

        /// <summary> State spatial keywords for this document </summary>
        [SolrField("state")]
        public List<string> State { get; set; }

        /// <summary> County spatial keywords for this document </summary>
        [SolrField("county")]
        public List<string> County { get; set; }

        /// <summary> City spatial keywords for this document </summary>
        [SolrField("city")]
        public List<string> City { get; set; }

        #endregion

        #region Temporal subject fields- STILL NEED TO REVIEW THIS!!!

        // [SolrField("temporal_subject" type = "text_general" indexed = "true" stored = "true"  multiValued = "true" />

        // [SolrField("temporal_decade" type = "text_general" indexed = "true" stored = "true"  multiValued = "true" />

        // [SolrField("temporal_year" type = "text_general" indexed = "true" stored = "true" />

        #endregion

        #region Other standard fields 

        /// <summary> Attribution for this document</summary>
        [SolrField("attribution")]
        public string Attribution { get; set; }

        /// <summary> MIME types associated with this document </summary>
        [SolrField("mime_type")]
        public List<string> MimeType { get; set; }

        /// <summary> Tracking box for this document </summary>
        [SolrField("tracking_box")]
        public List<string> TrackingBox { get; set; }

        /// <summary> Abstract for this document </summary>
        [SolrField("abstract")]
        public List<string> Abstract { get; set; }

        /// <summary> Edition for this document</summary>
        [SolrField("edition")]
        public string Edition { get; set; }

        /// <summary> Table of contents text for this document </summary>
        [SolrField("toc")]
        public List<string> TableOfContents { get; set; }

        /// <summary> Accession Number for this document</summary>
        [SolrField("accession_number")]
        public string AccessionNumber { get; set; }

        #endregion

        #region Performing Arts metadata fields

        /// <summary> Name of the performance </summary>
        [SolrField("performance")]
        public string Performance { get; set; }

        /// <summary> Date of the performance </summary>
        [SolrField("performance_date")]
        public string PerformanceDate { get; set; }

        /// <summary> Performer from a performance </summary>
        [SolrField("performer")]
        public List<string> Performer { get; set; }

        #endregion

        #region Oral history metadata fields 

        /// <summary> Interviewee with this oral history interview </summary>
        [SolrField("interviewee")]
        public List<string> Interviewee { get; set; }

        /// <summary> Interviewer with this oral history interview </summary>
        [SolrField("interviewer")]
        public List<string> Interviewer { get; set; }

        #endregion

        #region VRA Core(visual resource) metadata fields 
        
        /// <summary> Measurements VRACore information for this resource </summary>
        [SolrField("measurements")]
        public List<string> Measurements { get; set; }

        /// <summary> Cultural context VRACore information for this resource </summary>
        [SolrField("cultural_context")]
        public List<string> CulturalContext { get; set; }

        /// <summary> Inscription VRACore information for this resource </summary>
        [SolrField("inscription")]
        public List<string> Inscription { get; set; }

        /// <summary> Material VRACore information for this resource </summary>
        [SolrField("material")]
        public List<string> Material { get; set; }

        /// <summary> Style / Period VRACore information for this resource </summary>
        [SolrField("style_period")]
        public List<string> StylePeriod { get; set; }

        /// <summary> Technique VRACore information for this resource </summary>
        [SolrField("technique")]
        public List<string> Technique { get; set; }

        #endregion

        #region Zoological Taxonomy metadata fields 

        /// <summary> Complete hierarchical zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_hierarchical")]
        public List<string> ZoologicalHierarchical { get; set; }

        /// <summary> Kingdom zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_kingdom")]
        public List<string> ZoologicalKingdom { get; set; }

        /// <summary> Phylum zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_phylum")]
        public List<string> ZoologicalPhylum { get; set; }

        /// <summary> Class zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_class")]
        public List<string> ZoologicalClass { get; set; }

        /// <summary> Order zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_order")]
        public List<string> ZoologicalOrder { get; set; }

        /// <summary> Family zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_family")]
        public List<string> ZoologicalFamily { get; set; }

        /// <summary> Genus zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_genus")]
        public List<string> ZoologicalGenus { get; set; }

        /// <summary> Species zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_species")]
        public List<string> ZoologicalSpecies { get; set; }

        /// <summary> Common name zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_common_name")]
        public List<string> ZoologicalCommonName { get; set; }

        /// <summary> zt_scientific_name zoological taxonomic (DarwinCore) data for this resource </summary>
        [SolrField("zt_scientific_name")]
        public List<string> ZoologicalScientificName { get; set; }

        #endregion

        #region (Electronic) Thesis and Dissertation metadata fields 

        /// <summary> Committee members information for this thesis/dissertation resource </summary>
        [SolrField("etd_committee")]
        public List<string> EtdCommittee { get; set; }

        /// <summary> Degree information for this thesis/dissertation resource </summary>
        [SolrField("etd_degree")]
        public string EtdDegree { get; set; }

        /// <summary> Degree discipline information for this thesis/dissertation resource </summary>
        [SolrField("etd_degree_discipline")]
        public string EtdDegreeDiscipline { get; set; }

        /// <summary> Degree grantor information for this thesis/dissertation resource </summary>
        [SolrField("etd_degree_grantor")]
        public string EtdDegreeGrantor { get; set; }

        /// <summary> Degree level ( i.e., masters, doctorate, etc.. ) information for this thesis/dissertation resource </summary>
        [SolrField("etd_degree_level")]
        public string EtdDegreeLevel { get; set; }

        /// <summary> Degree division information for this thesis/dissertation resource </summary>
        [SolrField("etd_degree_division")]
        public string EtdDegreeDivision { get; set; }

        #endregion

        #region Learning Object metadata fields 

        /// <summary> Aggregation information for this learning object resource </summary>
        [SolrField("lom_aggregation")]
        public string LomAggregation { get; set; }

        /// <summary> Context information for finding this learning object resource </summary>
        [SolrField("lom_context")]
        public List<string> LomContext { get; set; }

        /// <summary> Context information for displaying this learning object resource </summary>
        [SolrField("lom_context.display")]
        public List<string> LomContextDisplay { get; set; }

        /// <summary> Classification information for this learning object resource </summary>
        [SolrField("lom_classification")]
        public List<string> LomClassification { get; set; }

        /// <summary> Difficulty information for this learning object resource </summary>
        [SolrField("lom_difficulty")]
        public string LomDifficulty { get; set; }

        /// <summary> Intended end user information for this learning object resource </summary>
        [SolrField("lom_intended_end_user")]
        public List<string> LomIntendedEndUser { get; set; }

        /// <summary> Display version of all the intended end user roles for this learning object resource </summary>
        [SolrField("lom_intended_end_user.display")]
        public string LomIntendedEndUserDisplay { get; set; }

        /// <summary> Interactivity level information for this learning object resource </summary>
        [SolrField("lom_interactivity_level")]
        public string LomInteractivityLevel { get; set; }

        /// <summary> Interactivity type information for this learning object resource </summary>
        [SolrField("lom_interactivity_type")]
        public string LomInteractivityType { get; set; }

        /// <summary> Status information for this learning object resource </summary>
        [SolrField("lom_status")]
        public string LomStatus { get; set; }

        /// <summary> System requirements information for this learning object resource </summary>
        [SolrField("lom_requirement")]
        public List<string> LomRequirement { get; set; }

        /// <summary> Display version of the system requirements information for this learning object resource </summary>
        [SolrField("lom_requirement.display")]
        public List<string> LomRequirementDisplay { get; set; }

        /// <summary> Age range information for this learning object resource </summary>
        [SolrField("lom_age_range")]
        public List<string> LomAgeRange { get; set; }

        /// <summary> Resource type information for this learning object resource </summary>
        [SolrField("lom_resource_type")]
        public List<string> LomResourceType { get; set; }

        /// <summary> Display version of the resource type information for this learning object resource </summary>
        [SolrField("lom_resource_type.display")]
        public List<string> LomResourceTypeDisplay { get; set; }

        /// <summary> Learning time information for this learning object resource </summary>
        [SolrField("lom_learning_time")]
        public string LomLearningTime { get; set; }

        #endregion

        #region User defined metadata fields, used by plug-ins, etc.. 

        /// <summary> User defined metadata field (#1) for this learning object resource </summary>
        [SolrField("user_defined_01")]
        public List<string> UserDefined01 { get; set; }

        /// <summary> User defined metadata field (#2) for this learning object resource </summary>
        [SolrField("user_defined_02")]
        public List<string> UserDefined02 { get; set; }

        /// <summary> User defined metadata field (#3) for this learning object resource </summary>
        [SolrField("user_defined_03")]
        public List<string> UserDefined03 { get; set; }

        /// <summary> User defined metadata field (#4) for this learning object resource </summary>
        [SolrField("user_defined_04")]
        public List<string> UserDefined04 { get; set; }

        /// <summary> User defined metadata field (#5) for this learning object resource </summary>
        [SolrField("user_defined_05")]
        public List<string> UserDefined05 { get; set; }

        /// <summary> User defined metadata field (#6) for this learning object resource </summary>
        [SolrField("user_defined_06")]
        public List<string> UserDefined06 { get; set; }

        /// <summary> User defined metadata field (#7) for this learning object resource </summary>
        [SolrField("user_defined_07")]
        public List<string> UserDefined07 { get; set; }

        /// <summary> User defined metadata field (#8) for this learning object resource </summary>
        [SolrField("user_defined_08")]
        public List<string> UserDefined08 { get; set; }

        /// <summary> User defined metadata field (#9) for this learning object resource </summary>
        [SolrField("user_defined_09")]
        public List<string> UserDefined09 { get; set; }

        /// <summary> User defined metadata field (#10) for this learning object resource </summary>
        [SolrField("user_defined_10")]
        public List<string> UserDefined10 { get; set; }

        /// <summary> User defined metadata field (#11) for this learning object resource </summary>
        [SolrField("user_defined_11")]
        public List<string> UserDefined11 { get; set; }

        /// <summary> User defined metadata field (#12) for this learning object resource </summary>
        [SolrField("user_defined_12")]
        public List<string> UserDefined12 { get; set; }

        /// <summary> User defined metadata field (#13) for this learning object resource </summary>
        [SolrField("user_defined_13")]
        public List<string> UserDefined13 { get; set; }

        /// <summary> User defined metadata field (#14) for this learning object resource </summary>
        [SolrField("user_defined_14")]
        public List<string> UserDefined14 { get; set; }

        /// <summary> User defined metadata field (#15) for this learning object resource </summary>
        [SolrField("user_defined_15")]
        public List<string> UserDefined15 { get; set; }

        /// <summary> User defined metadata field (#16) for this learning object resource </summary>
        [SolrField("user_defined_16")]
        public List<string> UserDefined16 { get; set; }

        /// <summary> User defined metadata field (#17) for this learning object resource </summary>
        [SolrField("user_defined_17")]
        public List<string> UserDefined17 { get; set; }

        /// <summary> User defined metadata field (#18) for this learning object resource </summary>
        [SolrField("user_defined_18")]
        public List<string> UserDefined18 { get; set; }

        /// <summary> User defined metadata field (#19) for this learning object resource </summary>
        [SolrField("user_defined_19")]
        public List<string> UserDefined19 { get; set; }

        /// <summary> User defined metadata field (#20) for this learning object resource </summary>
        [SolrField("user_defined_20")]
        public List<string> UserDefined20 { get; set; }

        /// <summary> User defined metadata field (#21) for this learning object resource </summary>
        [SolrField("user_defined_21")]
        public List<string> UserDefined21 { get; set; }

        /// <summary> User defined metadata field (#22) for this learning object resource </summary>
        [SolrField("user_defined_22")]
        public List<string> UserDefined22 { get; set; }

        /// <summary> User defined metadata field (#23) for this learning object resource </summary>
        [SolrField("user_defined_23")]
        public List<string> UserDefined23 { get; set; }

        /// <summary> User defined metadata field (#24) for this learning object resource </summary>
        [SolrField("user_defined_24")]
        public List<string> UserDefined24 { get; set; }

        /// <summary> User defined metadata field (#25) for this learning object resource </summary>
        [SolrField("user_defined_25")]
        public List<string> UserDefined25 { get; set; }

        /// <summary> User defined metadata field (#26) for this learning object resource </summary>
        [SolrField("user_defined_26")]
        public List<string> UserDefined26 { get; set; }

        /// <summary> User defined metadata field (#27) for this learning object resource </summary>
        [SolrField("user_defined_27")]
        public List<string> UserDefined27 { get; set; }

        /// <summary> User defined metadata field (#28) for this learning object resource </summary>
        [SolrField("user_defined_28")]
        public List<string> UserDefined28 { get; set; }

        /// <summary> User defined metadata field (#29) for this learning object resource </summary>
        [SolrField("user_defined_29")]
        public List<string> UserDefined29 { get; set; }

        /// <summary> User defined metadata field (#30) for this learning object resource </summary>
        [SolrField("user_defined_30")]
        public List<string> UserDefined30 { get; set; }

        /// <summary> User defined metadata field (#31) for this learning object resource </summary>
        [SolrField("user_defined_31")]
        public List<string> UserDefined31 { get; set; }

        /// <summary> User defined metadata field (#32) for this learning object resource </summary>
        [SolrField("user_defined_32")]
        public List<string> UserDefined32 { get; set; }

        /// <summary> User defined metadata field (#33) for this learning object resource </summary>
        [SolrField("user_defined_33")]
        public List<string> UserDefined33 { get; set; }

        /// <summary> User defined metadata field (#34) for this learning object resource </summary>
        [SolrField("user_defined_34")]
        public List<string> UserDefined34 { get; set; }

        /// <summary> User defined metadata field (#35) for this learning object resource </summary>
        [SolrField("user_defined_35")]
        public List<string> UserDefined35 { get; set; }

        /// <summary> User defined metadata field (#36) for this learning object resource </summary>
        [SolrField("user_defined_36")]
        public List<string> UserDefined36 { get; set; }

        /// <summary> User defined metadata field (#37) for this learning object resource </summary>
        [SolrField("user_defined_37")]
        public List<string> UserDefined37 { get; set; }

        /// <summary> User defined metadata field (#38) for this learning object resource </summary>
        [SolrField("user_defined_38")]
        public List<string> UserDefined38 { get; set; }

        /// <summary> User defined metadata field (#39) for this learning object resource </summary>
        [SolrField("user_defined_39")]
        public List<string> UserDefined39 { get; set; }

        /// <summary> User defined metadata field (#40) for this learning object resource </summary>
        [SolrField("user_defined_40")]
        public List<string> UserDefined40 { get; set; }

        /// <summary> User defined metadata field (#41) for this learning object resource </summary>
        [SolrField("user_defined_41")]
        public List<string> UserDefined41 { get; set; }

        /// <summary> User defined metadata field (#42) for this learning object resource </summary>
        [SolrField("user_defined_42")]
        public List<string> UserDefined42 { get; set; }

        /// <summary> User defined metadata field (#43) for this learning object resource </summary>
        [SolrField("user_defined_43")]
        public List<string> UserDefined43 { get; set; }

        /// <summary> User defined metadata field (#44) for this learning object resource </summary>
        [SolrField("user_defined_44")]
        public List<string> UserDefined44 { get; set; }

        /// <summary> User defined metadata field (#45) for this learning object resource </summary>
        [SolrField("user_defined_45")]
        public List<string> UserDefined45 { get; set; }

        /// <summary> User defined metadata field (#46) for this learning object resource </summary>
        [SolrField("user_defined_46")]
        public List<string> UserDefined46 { get; set; }

        /// <summary> User defined metadata field (#47) for this learning object resource </summary>
        [SolrField("user_defined_47")]
        public List<string> UserDefined47 { get; set; }

        /// <summary> User defined metadata field (#48) for this learning object resource </summary>
        [SolrField("user_defined_48")]
        public List<string> UserDefined48 { get; set; }

        /// <summary> User defined metadata field (#49) for this learning object resource </summary>
        [SolrField("user_defined_49")]
        public List<string> UserDefined49 { get; set; }

        /// <summary> User defined metadata field (#50) for this learning object resource </summary>
        [SolrField("user_defined_50")]
        public List<string> UserDefined50 { get; set; }

        /// <summary> User defined metadata field (#51) for this learning object resource </summary>
        [SolrField("user_defined_51")]
        public List<string> UserDefined51 { get; set; }

        /// <summary> User defined metadata field (#52) for this learning object resource </summary>
        [SolrField("user_defined_52")]
        public List<string> UserDefined52 { get; set; }

        #endregion

        /// <summary> Highlighted snippet of text from this document </summary>
        public string Snippet { get; set; }



    }
}