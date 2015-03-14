#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SobekCM.Core.Aggregations;
using SobekCM.Core.Configuration;
using SobekCM.Core.MemoryMgmt;
using SobekCM.Core.Navigation;
using SobekCM.Core.Results;
using SobekCM.Core.WebContent;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.Configuration;
using SobekCM.Engine_Library.Database;
using SobekCM.Tools;

#endregion

namespace SobekCM.Engine_Library.Aggregations
{
	/// <summary> Class is used to build the appropriate instance of the <see cref="Item_Aggregation"/> object.  This class
	/// pulls the data from the database, fills the object, and then performs final preparation for displaying the item 
	/// aggregation via the web.  </summary>
	public class Item_Aggregation_Utilities
	{
	    /// <summary> Gets a fully built item aggregation object for a particular aggregation code   </summary>
	    /// <param name="AggregationCode">Code for this aggregation object</param>
	    /// <param name="IsRobot">Flag tells if this request is from a robot (which will vary cacheing time)</param>
	    /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
	    /// <returns>Fully built item aggregation object for the particular aggregation code and language code</returns>
	    /// <remarks>Item aggregation object is also placed in the cache.<br /><br />
	    /// Building of an item aggregation always starts by pulling the item from the database ( either <see cref="Engine_Database.Get_Item_Aggregation"/> or <see cref="SobekCM_Database.Get_Main_Aggregation"/> ).<br /><br />
	    /// Then, either the Item Aggregation XML file is read (if present) or the entire folder hierarchy is analyzed to find the browses, infos, banners, etc..</remarks>
	    public static Complete_Item_Aggregation Get_Complete_Item_Aggregation(string AggregationCode, bool IsRobot, Custom_Tracer Tracer)
	    {
	        // Does this exist in the cache?
	        if (Tracer != null)
	        {
	            Tracer.Add_Trace("Item_Aggregation_Builder.Get_Item_Aggregation", "Creating '" + AggregationCode + "' item aggregation");
	        }

	        // Get the information about this collection and this entry point
	        Complete_Item_Aggregation hierarchyObject;
	        if ((AggregationCode.Length > 0) && (AggregationCode != "all"))
	            hierarchyObject = Engine_Database.Get_Item_Aggregation(AggregationCode, false, IsRobot, Tracer);
	        else
	            hierarchyObject = Engine_Database.Get_Main_Aggregation(Tracer);

	        // If no value was returned, don't do anything else here
	        if (hierarchyObject != null)
	        {
	            // Add all the values to this object
	            string xmlDataFile = Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + hierarchyObject.ObjDirectory + "\\" + hierarchyObject.Code + ".xml";
	            if (File.Exists(xmlDataFile))
	            {
	                if (Tracer != null)
	                {
	                    Tracer.Add_Trace("Item_Aggregation_Builder.Get_Item_Aggregation", "Reading XML Configuration File");
	                }

	                // Add the ALL and NEW browses
	                Add_All_New_Browses(hierarchyObject);

	                // Add all the other data from the XML file
	                Item_Aggregation_XML_Reader reader = new Item_Aggregation_XML_Reader();
	                reader.Add_Info_From_XML_File(hierarchyObject, xmlDataFile);
	            }
	            else
	            {
	                if (Tracer != null)
	                {
	                    Tracer.Add_Trace("Item_Aggregation_Builder.Get_Item_Aggregation", "Adding banner, home, and all/new browse information");
	                }

	                Add_HTML(hierarchyObject);
	                Add_All_New_Browses(hierarchyObject);
	                if (!IsRobot)
	                {
	                    if (Tracer != null)
	                    {
	                        Tracer.Add_Trace("Item_Aggregation_Builder.Get_Item_Aggregation", "Scanning Design Directory for browse and info files");
	                    }
	                    Add_Browse_Files(hierarchyObject, Tracer);
	                }

	                // Since there was no configuration file, save one
	                hierarchyObject.Write_Configuration_File(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + hierarchyObject.ObjDirectory);
	            }

	            // Now, look for any satellite configuration files
	            string contactFormFile = Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + hierarchyObject.ObjDirectory + "\\config\\sobekcm_contactform.config";
	            if (File.Exists(contactFormFile))
	            {
	                hierarchyObject.ContactForm = ContactForm_Configuration_Reader.Read_Config(contactFormFile);
	            }

	            // Return this built hierarchy object
	            return hierarchyObject;
	        }

	        if (Tracer != null)
	        {
	            Tracer.Add_Trace("Item_Aggregation_Builder.Get_Item_Aggregation", "NULL value returned from database");
	        }
	        return null;

	    }


	    /// <summary> Adds the ALL ITEMS and NEW ITEMS browses to the item aggregation, if the display options and last added
		/// item date call for it </summary>
		/// <param name="ThisObject"> Item aggregation to which to add the ALL ITEMS and NEW ITEMS browse</param>
		/// <remarks>This method is always called while building an item aggregation, irregardless of whether there is an
		/// item aggregation configuration XML file or not.</remarks>
        protected static void Add_All_New_Browses(Complete_Item_Aggregation ThisObject)
		{
			// If this is the main home page for this site, do not show ALL since we cannot browse ALL items
			if (!ThisObject.Can_Browse_Items )
				return;

			// If this is in the display options, and the item browses
			if ((ThisObject.Display_Options.Length == 0) || (ThisObject.Display_Options.IndexOf("I") >= 0))
			{
				// Add the ALL browse, if there should be one
                ThisObject.Add_Child_Page(Item_Aggregation_Child_Visibility_Enum.Main_Menu, "all", String.Empty, "All Items");

				// Add the NEW search, if the ALL search exists
				if ((ThisObject.Get_Browse_Info_Object("all") != null) && (ThisObject.Show_New_Item_Browse))
				{
                    ThisObject.Add_Child_Page(Item_Aggregation_Child_Visibility_Enum.Main_Menu, "new", String.Empty, "Recently Added Items");
				}
			}
			else
			{
				// Add the ALL browse as an info
                ThisObject.Add_Child_Page(Item_Aggregation_Child_Visibility_Enum.None, "all", String.Empty, "All Items");
			}
		}

		/// <summary> Checks the appropriate design folders to add any existing browse or info pages to the item aggregation </summary>
		/// <param name="ThisObject"> Item aggregation object to add the browse and info pages to</param>
		/// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <remarks>This method is only called if the item aggregation does not have an existing XML configuration file.</remarks>
        protected static void Add_Browse_Files(Complete_Item_Aggregation ThisObject, Custom_Tracer Tracer)
		{
			// Collect the list of items in the browse folder
			if (Directory.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/browse"))
			{
				string[] files = Directory.GetFiles(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/browse", "*.htm*");
				foreach (string thisFile in files)
				{
					// Get the new browse info object
                    Complete_Item_Aggregation_Child_Page newBrowse = Get_Item_Aggregation_Browse_Info(thisFile, Item_Aggregation_Child_Visibility_Enum.Main_Menu, Tracer);
					if (newBrowse != null)
					{
						ThisObject.Add_Child_Page(newBrowse);
					}
				}
			}

			// Collect the list of items in the info folder
			if (Directory.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/info"))
			{
				string[] files = Directory.GetFiles(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/info", "*.htm*");
				foreach (string thisFile in files)
				{
					// Get the title for this file
					// Get the new browse info object
                    Complete_Item_Aggregation_Child_Page newInfo = Get_Item_Aggregation_Browse_Info(thisFile, Item_Aggregation_Child_Visibility_Enum.None, Tracer);
					if (newInfo != null)
					{
						ThisObject.Add_Child_Page(newInfo);
					}
				}
			}
		}

		/// <summary>Reads the item aggregation browse or info file and returns a built <see cref="Item_Aggregation_Child_Page"/> object for
		/// inclusion in the item aggregation </summary>
		/// <param name="FileName"> Filename of the browse or info file</param>
		/// <param name="Browse_Type"> Flag indicates if this is a browse or info file</param>
		/// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
		/// <returns> Built object containing all of the pertinent details about this info or browse </returns>
        private static Complete_Item_Aggregation_Child_Page Get_Item_Aggregation_Browse_Info(string FileName, Item_Aggregation_Child_Visibility_Enum Browse_Type, Custom_Tracer Tracer)
		{
			HTML_Based_Content fileContent = HTML_Based_Content_Reader.Read_HTML_File(FileName, false, Tracer);
            Complete_Item_Aggregation_Child_Page returnObject = new Complete_Item_Aggregation_Child_Page(Browse_Type, Item_Aggregation_Child_Source_Data_Enum.Static_HTML, fileContent.Code, FileName, fileContent.Title ?? "Missing Title");
			return returnObject;
		}

		/// <summary> Finds the home page source file and banner images or html for this item aggregation </summary>
		/// <param name="ThisObject"> Item aggregation to add the home page link and banner html </param>
		/// <remarks>This method is only called if the item aggregation does not have an existing XML configuration file. </remarks>
        protected static void Add_HTML(Complete_Item_Aggregation ThisObject)
		{
			// Just use the standard home text
            if ( File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/home/text.html"))
    			ThisObject.Add_Home_Page_File(  "html/home/text.html", Engine_ApplicationCache_Gateway.Settings.Default_UI_Language );
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/home/text_en.html"))
                ThisObject.Add_Home_Page_File("html/home/text_en.html",  Web_Language_Enum.English );
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/home/text_fr.html"))
                ThisObject.Add_Home_Page_File("html/home/text_fr.html", Web_Language_Enum.French);
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/home/text_es.html"))
                ThisObject.Add_Home_Page_File("html/home/text_es.html", Web_Language_Enum.Spanish);
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "html/home/text_sp.html"))
                ThisObject.Add_Home_Page_File("html/home/text_sp.html", Web_Language_Enum.Spanish);

			// Just use the standard banner image
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "images/banners/coll.jpg"))
                ThisObject.Add_Banner_Image("images/banners/coll.jpg", Engine_ApplicationCache_Gateway.Settings.Default_UI_Language);
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "images/banners/coll_en.jpg"))
                ThisObject.Add_Banner_Image("images/banners/coll_en.jpg", Web_Language_Enum.English);
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "images/banners/coll_fr.jpg"))
                ThisObject.Add_Banner_Image("images/banners/coll_fr.jpg", Web_Language_Enum.French);
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "images/banners/coll_es.jpg"))
                ThisObject.Add_Banner_Image("images/banners/coll_es.jpg", Web_Language_Enum.Spanish);
            if (File.Exists(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location + ThisObject.ObjDirectory + "images/banners/coll_sp.jpg"))
                ThisObject.Add_Banner_Image("images/banners/coll_sp.jpg", Web_Language_Enum.Spanish);
		}


        /// <summary> Method returns the table of results for the browse indicated </summary>
        /// <param name = "ChildPageObject">Object with all the information about the browse</param>
        /// <param name = "Page"> Page of results requested for the indicated browse </param>
        /// <param name = "Sort"> Sort applied to the results before being returned </param>
        /// <param name="Potentially_Include_Facets"> Flag indicates if facets could be included in this browse results </param>
        /// <param name = "Need_Browse_Statistics"> Flag indicates if the browse statistics (facets and total counts) are required for this browse as well </param>
        /// <param name = "Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <param name="Results_Per_Page"> Number of results to retrieve per page</param>
        /// <returns> Resutls for the browse or info in table form </returns>
        public static Multiple_Paged_Results_Args Get_Browse_Results(Item_Aggregation ItemAggr, Item_Aggregation_Child_Page ChildPageObject,
                                                                      int Page, int Sort, int Results_Per_Page, bool Potentially_Include_Facets, bool Need_Browse_Statistics,
                                                                      Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Item_Aggregation.Get_Browse_Table", String.Empty);
            }

            // Get the list of facets first
            List<short> facetsList = ItemAggr.Facets;
            if (!Potentially_Include_Facets)
                facetsList = null;

            // Pull data from the database if necessary
            if ((ChildPageObject.Code == "all") || (ChildPageObject.Code == "new"))
            {
                // Get this browse from the database
                if ((ItemAggr.ID < 0) || (ItemAggr.Code.ToUpper() == "ALL"))
                {
                    if (ChildPageObject.Code == "new")
                        return Engine_Database.Get_All_Browse_Paged(true, false, Results_Per_Page, Page, Sort, Need_Browse_Statistics, facetsList, Need_Browse_Statistics, Tracer);
                    return Engine_Database.Get_All_Browse_Paged(false, false, Results_Per_Page, Page, Sort, Need_Browse_Statistics, facetsList, Need_Browse_Statistics, Tracer);
                }

                if (ChildPageObject.Code == "new")
                {
                    return Engine_Database.Get_Item_Aggregation_Browse_Paged(ItemAggr.Code, true, false, Results_Per_Page, Page, Sort, Need_Browse_Statistics, facetsList, Need_Browse_Statistics, Tracer);
                }
                return Engine_Database.Get_Item_Aggregation_Browse_Paged(ItemAggr.Code, false, false, Results_Per_Page, Page, Sort, Need_Browse_Statistics, facetsList, Need_Browse_Statistics, Tracer);
            }

            // Default return NULL
            return null;
        }

        #region Method to save the complete item aggregation to the database

        /// <summary> Saves the information about this item aggregation to the database </summary>
        /// <param name="Username"> Name of the user performing this save, for the item aggregation milestones</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <returns>TRUE if successful, otherwise FALSE </returns>
        public static bool Save_To_Database(Complete_Item_Aggregation ItemAggr, string Username, Custom_Tracer Tracer)
        {
            // Build the list of language variants
            List<string> languageVariants = new List<string>();
            languageVariants.Add(Web_Language_Enum_Converter.Enum_To_Code(Engine_ApplicationCache_Gateway.Settings.Default_UI_Language));
            if (ItemAggr.Home_Page_File_Dictionary != null)
            {
                foreach (Web_Language_Enum language in ItemAggr.Home_Page_File_Dictionary.Keys)
                {
                    string code = Web_Language_Enum_Converter.Enum_To_Code(language);
                    if (!languageVariants.Contains(code))
                        languageVariants.Add(code);
                }
            }
            if (ItemAggr.Banner_Dictionary != null)
            {
                foreach (Web_Language_Enum language in ItemAggr.Banner_Dictionary.Keys)
                {
                    string code = Web_Language_Enum_Converter.Enum_To_Code(language);
                    if (!languageVariants.Contains(code))
                        languageVariants.Add(code);
                } 
            }
            if (ItemAggr.Child_Pages != null)
            {
                foreach (Complete_Item_Aggregation_Child_Page childPage in ItemAggr.Child_Pages)
                {
                    if (childPage.Label_Dictionary != null)
                    {
                        foreach (Web_Language_Enum language in childPage.Label_Dictionary.Keys)
                        {
                            string code2 = Web_Language_Enum_Converter.Enum_To_Code(language);
                            if (!languageVariants.Contains(code2))
                                languageVariants.Add(code2);
                        }
                    }
                    if (childPage.Source_Dictionary != null)
                    {
                        foreach (Web_Language_Enum language in childPage.Source_Dictionary.Keys)
                        {
                            string code2 = Web_Language_Enum_Converter.Enum_To_Code(language);
                            if (!languageVariants.Contains(code2))
                                languageVariants.Add(code2);
                        }
                    }
                }
            }
            StringBuilder languageVariantsBuilder = new StringBuilder();
            foreach (string language in languageVariants)
            {
                if (language.Length > 0)
                {
                    if (languageVariantsBuilder.Length > 0)
                        languageVariantsBuilder.Append("|" + language);
                    else
                        languageVariantsBuilder.Append(language);
                }
            }


            return Engine_Database.Save_Item_Aggregation(ItemAggr.ID, ItemAggr.Code, ItemAggr.Name, ItemAggr.ShortName,
                ItemAggr.Description, ItemAggr.Thematic_Heading, ItemAggr.Type, ItemAggr.Active, ItemAggr.Hidden,
                ItemAggr.Display_Options, ItemAggr.Map_Search, ItemAggr.Map_Search_Beta, ItemAggr.Map_Display, ItemAggr.Map_Display_Beta,
                ItemAggr.OAI_Enabled, ItemAggr.OAI_Metadata, ItemAggr.Contact_Email, String.Empty, ItemAggr.External_Link, -1, Username,
                languageVariantsBuilder.ToString(), Tracer);
        }

        #endregion

        #region Methods to get the language-specific item aggregation

	    public static Item_Aggregation Get_Item_Aggregation(Complete_Item_Aggregation CompAggr, Web_Language_Enum RequestedLanguage, Custom_Tracer Tracer)
	    {
            Item_Aggregation returnValue = new Item_Aggregation(RequestedLanguage, CompAggr.ID, CompAggr.Code)
            {
                Active = CompAggr.Active,
                BannerImage = CompAggr.Banner_Image(RequestedLanguage, null ),
                Child_Types = CompAggr.Child_Types,
                Contact_Email = CompAggr.Contact_Email,
                ContactForm = CompAggr.ContactForm,
                Default_BrowseBy = CompAggr.Default_BrowseBy,
                Default_Result_View = CompAggr.Default_Result_View,
                Default_Skin = CompAggr.Default_Skin,
                Description = CompAggr.Description,
                Display_Options = CompAggr.Display_Options,
                FrontBannerObj = CompAggr.Front_Banner_Image(RequestedLanguage),
                Hidden = CompAggr.Hidden,
                Last_Item_Added = CompAggr.Last_Item_Added,
                Map_Display = CompAggr.Map_Display,
                Map_Search = CompAggr.Map_Search,
                Name = CompAggr.Name,
                Rotating_Highlights = CompAggr.Rotating_Highlights,
                ShortName = CompAggr.ShortName,
                Statistics = CompAggr.Statistics,
                Type = CompAggr.Type
            };

            if (CompAggr.Children_Count > 0)
            {
                returnValue.Children = new List<Item_Aggregation_Related_Aggregations>();
                foreach (Item_Aggregation_Related_Aggregations thisAggr in CompAggr.Children)
                {
                    returnValue.Children.Add(thisAggr);
                }
            }
            if (CompAggr.Parent_Count > 0)
            {
                returnValue.Parents = new List<Item_Aggregation_Related_Aggregations>();
                foreach (Item_Aggregation_Related_Aggregations thisAggr in CompAggr.Parents)
                {
                    returnValue.Parents.Add(thisAggr);
                }
            }
            foreach (short thisFacet in CompAggr.Facets)
            {
                returnValue.Facets.Add(thisFacet);
            }
            foreach (Result_Display_Type_Enum display in CompAggr.Result_Views)
            {
                returnValue.Result_Views.Add(display);
            }
            if (CompAggr.Views_And_Searches != null)
            {
                foreach (Item_Aggregation_Views_Searches_Enum viewsSearches in CompAggr.Views_And_Searches)
                {
                    returnValue.Views_And_Searches.Add(viewsSearches);
                }
            }
            if ((CompAggr.Web_Skins != null) && (CompAggr.Web_Skins.Count > 0))
            {
                returnValue.Web_Skins = new List<string>();
                foreach (string thisSkin in CompAggr.Web_Skins)
                {
                    returnValue.Web_Skins.Add(thisSkin);
                }
            }

            // Language-specific (and simplified) metadata type info
            foreach (Complete_Item_Aggregation_Metadata_Type thisAdvSearchField in CompAggr.Search_Fields)
            {
                returnValue.Search_Fields.Add(new Item_Aggregation_Metadata_Type(thisAdvSearchField.DisplayTerm, thisAdvSearchField.SobekCode));
            }
            foreach (Complete_Item_Aggregation_Metadata_Type thisAdvSearchField in CompAggr.Browseable_Fields)
            {
                returnValue.Browseable_Fields.Add(new Item_Aggregation_Metadata_Type(thisAdvSearchField.DisplayTerm, thisAdvSearchField.SobekCode));
            }

            // Language-specific (and simplified) child pages information
            if ((CompAggr.Child_Pages != null) && (CompAggr.Child_Pages.Count > 0))
            {
                returnValue.Child_Pages = new List<Item_Aggregation_Child_Page>();
                foreach (Complete_Item_Aggregation_Child_Page fullPage in CompAggr.Child_Pages)
                {
                    Item_Aggregation_Child_Page newPage = new Item_Aggregation_Child_Page();
                    newPage.Browse_Type = fullPage.Browse_Type;
                    newPage.Code = fullPage.Code;
                    newPage.Parent_Code = fullPage.Parent_Code;
                    newPage.Source_Data_Type = fullPage.Source_Data_Type;

                    string label = fullPage.Get_Label(RequestedLanguage);
                    if (!String.IsNullOrEmpty(label))
                        newPage.Label = label;

                    string source = fullPage.Get_Static_HTML_Source(RequestedLanguage);
                    if (!String.IsNullOrEmpty(label))
                        newPage.Source = source;

                    returnValue.Child_Pages.Add(newPage);
                }
            }

            // Language-specific (and simplified) highlight information
            if ((CompAggr.Highlights != null) && (CompAggr.Highlights.Count > 0))
            {
                returnValue.Highlights = new List<Item_Aggregation_Highlights>();
                int day_integer = DateTime.Now.DayOfYear + 1;
                int highlight_to_use = day_integer % CompAggr.Highlights.Count;

                // If this is for rotating highlights, show up to eight
                if ((CompAggr.Rotating_Highlights.HasValue ) && ( CompAggr.Rotating_Highlights.Value ))
                {
                    // Copy over just the eight highlights we should use 
                    int number = Math.Min(8, CompAggr.Highlights.Count);
                    for (int i = 0; i < number; i++)
                    {
                        Complete_Item_Aggregation_Highlights thisHighlight = CompAggr.Highlights[highlight_to_use];

                        Item_Aggregation_Highlights newHighlight = new Item_Aggregation_Highlights
                        {
                            Image = thisHighlight.Image, 
                            Link = thisHighlight.Link
                        };

                        string text = thisHighlight.Get_Text(RequestedLanguage);
                        if (!String.IsNullOrEmpty(text))
                            newHighlight.Text = text;

                        string tooltip = thisHighlight.Get_Tooltip(RequestedLanguage);
                        if (!String.IsNullOrEmpty(tooltip))
                            newHighlight.Tooltip = tooltip;

                        returnValue.Highlights.Add(newHighlight);

                        highlight_to_use++;
                        if (highlight_to_use >= CompAggr.Highlights.Count)
                            highlight_to_use = 0;
                    }
                }
                else
                {
                    Complete_Item_Aggregation_Highlights thisHighlight = CompAggr.Highlights[highlight_to_use];

                    Item_Aggregation_Highlights newHighlight = new Item_Aggregation_Highlights
                    {
                        Image = thisHighlight.Image,
                        Link = thisHighlight.Link
                    };

                    string text = thisHighlight.Get_Text(RequestedLanguage);
                    if (!String.IsNullOrEmpty(text))
                        newHighlight.Text = text;

                    string tooltip = thisHighlight.Get_Tooltip(RequestedLanguage);
                    if (!String.IsNullOrEmpty(tooltip))
                        newHighlight.Tooltip = tooltip;

                    returnValue.Highlights.Add(newHighlight);
                }
            }

            // Language-specific source page
            returnValue.HomePageSource = String.Empty;
            if (!String.IsNullOrEmpty(CompAggr.Custom_Home_Page_Source_File))
            {
                returnValue.Custom_Home_Page = true;
                returnValue.HomePageSource = CompAggr.Custom_Home_Page_Source_File;
            }
            else
            {
                HTML_Based_Content homeHtml = Get_Home_HTML(CompAggr, RequestedLanguage, null);


                returnValue.HomePageHtml = homeHtml;
            }

            return returnValue;
	    }

        /// <summary>
        ///   Method gets the HOME PAGE html for the appropriate UI settings
        /// </summary>
        /// <param name = "Language"> Current language of the user interface </param>
        /// <param name = "Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <returns>Home page HTML</returns>
        private static HTML_Based_Content Get_Home_HTML(Complete_Item_Aggregation CompAggr, Web_Language_Enum Language, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Item_Aggregation.Get_Home_HTML", "Reading home text source file");
            }

            // Get the home file source
            string homeFileSource = Path.Combine(Engine_ApplicationCache_Gateway.Settings.Base_Design_Location, CompAggr.ObjDirectory, CompAggr.Home_Page_File(Language));

            // If no home file source even found, return a message to that affect
            if (homeFileSource.Length == 0)
            {
                return new HTML_Based_Content("<div class=\"error_div\">NO HOME PAGE SOURCE FILE FOUND</div>", null, homeFileSource);
            }

            // Do the rest in a try/catch
            try
            {
                // Does the file exist?
                if (!File.Exists(homeFileSource))
                {
                    return new HTML_Based_Content("<div class=\"error_div\">HOME PAGE SOURCE FILE '" + homeFileSource + "' DOES NOT EXIST.</div>", null, homeFileSource);
                }

                HTML_Based_Content content = HTML_Based_Content_Reader.Read_HTML_File(homeFileSource, true, Tracer);
                content.TEMP_Source = homeFileSource;

                return content;
            }
            catch (Exception ee)
            {
                return new HTML_Based_Content("<div class=\"error_div\">EXCEPTION CAUGHT WHILE TRYING TO READ THE HOME PAGE SOURCE FILE '" + homeFileSource + "'.<br /><br />ERROR: " + ee.Message + "</div>", null, homeFileSource);
            }
        }

        #endregion
    }
}