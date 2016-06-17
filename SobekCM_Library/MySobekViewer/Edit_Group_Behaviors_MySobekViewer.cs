﻿#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using SobekCM.Core.Client;
using SobekCM.Core.MemoryMgmt;
using SobekCM.Core.Navigation;
using SobekCM.Core.UI_Configuration;
using SobekCM.Core.UI_Configuration.StaticResources;
using SobekCM.Engine_Library.Configuration;
using SobekCM.Library.AdminViewer;
using SobekCM.Library.Citation;
using SobekCM.Library.Citation.Template;
using SobekCM.Library.HTML;
using SobekCM.Library.MainWriters;
using SobekCM.Library.UI;
using SobekCM.Resource_Object;
using SobekCM.Resource_Object.Database;
using SobekCM.Tools;
using SobekCM_Resource_Database;

#endregion

namespace SobekCM.Library.MySobekViewer
{
   /// <summary> Class allows an authenticated RequestSpecificValues.Current_User to edit the group-level behaviors for a title within this digital library </summary>
    /// <remarks> This class extends the <see cref="abstract_MySobekViewer"/> class.<br /><br />
    /// MySobek Viewers are used for registration and authentication with mySobek, as well as performing any task which requires
    /// authentication, such as online submittal, metadata editing, and system administrative tasks.<br /><br />
    /// During a valid html request, the following steps occur:
    /// <ul>
    /// <li>Application state is built/verified by the Application_State_Builder </li>
    /// <li>Request is analyzed by the QueryString_Analyzer and output as a <see cref="Navigation_Object"/> </li>
    /// <li>Main writer is created for rendering the output, in his case the <see cref="Html_MainWriter"/> </li>
    /// <li>The HTML writer will create the necessary subwriter.  Since this action requires authentication, an instance of the  <see cref="MySobek_HtmlSubwriter"/> class is created. </li>
    /// <li>The mySobek subwriter creates an instance of this viewer to display the group's behaviors for editing</li>
    /// <li>This viewer uses the <see cref="CompleteTemplate"/> class to display the correct elements for editing </li>
    /// </ul></remarks>
    public class Edit_Group_Behaviors_MySobekViewer : abstract_MySobekViewer
    {
       private readonly CompleteTemplate completeTemplate;
       private readonly SobekCM_Item currentItem;

       #region Constructor

       /// <summary> Constructor for a new instance of the Edit_Group_Behaviors_MySobekViewer class </summary>
       /// <param name="RequestSpecificValues"> All the necessary, non-global data specific to the current request </param>
       public Edit_Group_Behaviors_MySobekViewer(RequestCache RequestSpecificValues) : base(RequestSpecificValues)
       {
           RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", String.Empty);

           // If no user then that is an error
           if ((RequestSpecificValues.Current_User == null) || (!RequestSpecificValues.Current_User.LoggedOn))
           {
               RequestSpecificValues.Current_Mode.Mode = Display_Mode_Enum.Aggregation;
               RequestSpecificValues.Current_Mode.Aggregation = String.Empty;
               UrlWriterHelper.Redirect(RequestSpecificValues.Current_Mode);
               return;
           }

           // Ensure BibID provided
           RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "Validate provided bibid");
           if (String.IsNullOrEmpty(RequestSpecificValues.Current_Mode.BibID)) 
           {
               RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "BibID was not provided!");
               RequestSpecificValues.Current_Mode.Mode = Display_Mode_Enum.Error;
               RequestSpecificValues.Current_Mode.Error_Message = "Invalid Request : BibID missing in item group metadata edit request";
               return;
           }

           // Ensure the item is valid
           RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "Validate bibid exists");
           if (!UI_ApplicationCache_Gateway.Items.Contains_BibID(RequestSpecificValues.Current_Mode.BibID))
           {
               RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "BibID indicated is not valid", Custom_Trace_Type_Enum.Error);
               RequestSpecificValues.Current_Mode.Mode = Display_Mode_Enum.Error;
               RequestSpecificValues.Current_Mode.Error_Message = "Invalid Request : BibID indicated is not valid";
               return;
           }

           RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "Try to pull this sobek complete item group");
           currentItem = SobekEngineClient.Items.Get_Sobek_Item_Group(RequestSpecificValues.Current_Mode.BibID, RequestSpecificValues.Tracer);
           if (currentItem == null)
           {
               RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "Unable to build complete item group");
               RequestSpecificValues.Current_Mode.Mode = Display_Mode_Enum.Error;
               RequestSpecificValues.Current_Mode.Error_Message = "Invalid Request : Unable to build complete item";
               return;
           }

           // If the RequestSpecificValues.Current_User cannot edit this currentItem, go back
           if (!RequestSpecificValues.Current_User.Can_Edit_This_Item(currentItem.BibID, currentItem.Bib_Info.SobekCM_Type_String, currentItem.Bib_Info.Source.Code, currentItem.Bib_Info.HoldingCode, currentItem.Behaviors.Aggregation_Code_List))
           {
               RequestSpecificValues.Current_Mode.My_Sobek_Type = My_Sobek_Type_Enum.Home;
               UrlWriterHelper.Redirect(RequestSpecificValues.Current_Mode);
               return;
           }

           const string TEMPLATE_CODE = "groupbehaviors";
           completeTemplate = Template_MemoryMgmt_Utility.Retrieve_Template(TEMPLATE_CODE, RequestSpecificValues.Tracer);
           if (completeTemplate != null)
           {
               RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "Found CompleteTemplate in cache");
           }
           else
           {
               RequestSpecificValues.Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Constructor", "Reading CompleteTemplate file");

               // Read this CompleteTemplate
               Template_XML_Reader reader = new Template_XML_Reader();
               completeTemplate = new CompleteTemplate();
               if (File.Exists(UI_ApplicationCache_Gateway.Settings.Servers.Base_MySobek_Directory + "templates\\user\\standard\\" + TEMPLATE_CODE + ".xml"))
               {
                   reader.Read_XML(UI_ApplicationCache_Gateway.Settings.Servers.Base_MySobek_Directory + "templates\\user\\standard\\" + TEMPLATE_CODE + ".xml", completeTemplate, true);
               }
               else
               {
                   reader.Read_XML(UI_ApplicationCache_Gateway.Settings.Servers.Base_MySobek_Directory + "templates\\default\\standard\\" + TEMPLATE_CODE + ".xml", completeTemplate, true);
               }

               // Save this into the cache
               Template_MemoryMgmt_Utility.Store_Template(TEMPLATE_CODE, completeTemplate, RequestSpecificValues.Tracer);
           }

           // See if there was a hidden request
           string hidden_request = HttpContext.Current.Request.Form["behaviors_request"] ?? String.Empty;

           // If this was a cancel request do that
           if (hidden_request == "cancel")
           {
               RequestSpecificValues.Current_Mode.Mode = Display_Mode_Enum.Item_Display;
               UrlWriterHelper.Redirect(RequestSpecificValues.Current_Mode);
           }
           else if (hidden_request == "save")
           {
               // Save these changes to bib
               completeTemplate.Save_To_Bib(currentItem, RequestSpecificValues.Current_User, 1);

               // Save the group title
               SobekCM_Item_Database.Update_Item_Group(currentItem.BibID, currentItem.Behaviors.GroupTitle, currentItem.Bib_Info.SortSafeTitle(currentItem.Behaviors.GroupTitle, true), String.Empty, currentItem.Behaviors.Primary_Identifier.Type, currentItem.Behaviors.Primary_Identifier.Identifier );

               // Save the interfaces to the group currentItem as well
               SobekCM_Item_Database.Save_Item_Group_Web_Skins(currentItem.Web.GroupID, currentItem );

               // Store on the caches (to replace the other)
               CachedDataManager.Items.Remove_Digital_Resource_Objects(currentItem.BibID, RequestSpecificValues.Tracer);

               // Also remove the list of volumes, since this may have changed
               CachedDataManager.Items.Remove_Items_In_Title(currentItem.BibID, RequestSpecificValues.Tracer);

               // Also clear the engine
               SobekEngineClient.Items.Clear_Item_Group_Cache(currentItem.BibID, RequestSpecificValues.Tracer);


               // Forward
               RequestSpecificValues.Current_Mode.Mode = Display_Mode_Enum.Item_Display;
               UrlWriterHelper.Redirect(RequestSpecificValues.Current_Mode);
           }
       }

       #endregion

       /// <summary> Navigation type to be displayed (mostly used by the mySobek viewers) </summary>
       /// <value> This returns none since this viewer writes all the necessary navigational elements </value>
       /// <remarks> This is set to NONE if the viewer will write its own navigation and ADMIN if the standard
       /// administrative tabs should be included as well.  </remarks>
       public override MySobek_Admin_Included_Navigation_Enum Standard_Navigation_Type { get { return MySobek_Admin_Included_Navigation_Enum.NONE; } }

        /// <summary> Title for the page that displays this viewer, this is shown in the search box at the top of the page, just below the banner </summary>
        /// <value> This returns the value 'Edit Group Behaviors' </value>
        public override string Web_Title
        {
            get
            {
                return "Edit Group Behaviors";
            }
        }

        /// <summary> Add the HTML to be displayed in the main SobekCM viewer area (outside of the forms)</summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This class does nothing, since the interface list is added as controls, not HTML </remarks>
        public override void Write_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Write_HTML", "Do nothing");
        }

        /// <summary> Add the HTML to be displayed in the main SobekCM viewer area </summary>
        /// <param name="Output"> Textwriter to write the HTML for this viewer</param>
        /// <param name="Tracer">Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This class does nothing, since the individual metadata elements are added as controls, not HTML </remarks>
        public override void Write_ItemNavForm_Closing(TextWriter Output, Custom_Tracer Tracer)
        {
	        const string BEHAVIORS = "BEHAVIORS";

            Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Write_ItemNavForm_Closing", "");

            Output.WriteLine("<!-- Hidden field is used for postbacks to add new form elements (i.e., new name, new other titles, etc..) -->");
            Output.WriteLine("<input type=\"hidden\" id=\"behaviors_request\" name=\"behaviors_request\" value=\"\" />");

			Output.WriteLine("<div id=\"sbkIsw_Titlebar\">");

			string grouptitle = currentItem.Behaviors.GroupTitle;
			if (grouptitle.Length > 125)
			{
				Output.WriteLine("\t<h1 itemprop=\"name\"><abbr title=\"" + grouptitle + "\">" + grouptitle.Substring(0, 120) + "...</abbr></h1>");
			}
			else
			{
				Output.WriteLine("\t<h1 itemprop=\"name\">" + grouptitle + "</h1>");
			}

			Output.WriteLine("</div>");
			Output.WriteLine("<div class=\"sbkMenu_Bar\" id=\"sbkIsw_MenuBar\" style=\"height:20px\">&nbsp;</div>");

			Output.WriteLine("<div id=\"container-inner1000\">");
			Output.WriteLine("<div id=\"pagecontainer\">");

            Output.WriteLine("<!-- Edit_Group_Behaviors_MySobekViewer.Write_ItemNavForm_Closing -->");
			Output.WriteLine("<div class=\"sbkMySobek_HomeText\">");
            Output.WriteLine("  <br />");

            Output.WriteLine("  <h2>Edit the behaviors associated with this item group within this library</h2>");
            Output.WriteLine("    <ul>");
            Output.WriteLine("      <li>Enter the data for this item group below and press the SAVE button when all your edits are complete.</li>");
            Output.WriteLine("      <li>Clicking on the green plus button ( <img class=\"repeat_button\" src=\"" + Static_Resources_Gateway.New_Element_Demo_Jpg + "\" /> ) will add another instance of the element, if the element is repeatable.</li>");
            Output.WriteLine("      <li>Click <a href=\"" + UI_ApplicationCache_Gateway.Settings.System.Help_URL(RequestSpecificValues.Current_Mode.Base_URL) + "help/groupbehaviors\" target=\"_EDIT_INSTRUCTIONS\">here for detailed instructions</a> on editing behaviors online.</li>");


            Output.WriteLine("     </ul>");
            Output.WriteLine("</div>");
            Output.WriteLine();

            Output.WriteLine("<a name=\"CompleteTemplate\"> </a>");
			Output.WriteLine("<div id=\"tabContainer\" class=\"fulltabs\">");
			Output.WriteLine("  <div class=\"tabs\">");
			Output.WriteLine("    <ul>");
			Output.WriteLine("      <li id=\"tabHeader_1\" class=\"tabActiveHeader\">" + BEHAVIORS + "</li>");
			Output.WriteLine("    </ul>");
			Output.WriteLine("  </div>");
			Output.WriteLine("  <div class=\"graytabscontent\">");
			Output.WriteLine("    <div class=\"tabpage\" id=\"tabpage_1\">");

            Output.WriteLine("      <!-- Add SAVE and CANCEL buttons to top of form -->");
            Output.WriteLine("      <script src=\"" + Static_Resources_Gateway.Sobekcm_Metadata_Js + "\" type=\"text/javascript\"></script>");
			Output.WriteLine();

			Output.WriteLine("      <div class=\"sbkMySobek_RightButtons\">");
			Output.WriteLine("        <button onclick=\"behaviors_cancel_form(); return false;\" class=\"sbkMySobek_BigButton\"><img src=\"" + Static_Resources_Gateway.Button_Previous_Arrow_Png + "\" class=\"sbkMySobek_RoundButton_LeftImg\" alt=\"\" /> CANCEL </button> &nbsp; &nbsp; ");
			Output.WriteLine("        <button onclick=\"behaviors_save_form(); return false;\" class=\"sbkMySobek_BigButton\"> SAVE <img src=\"" + Static_Resources_Gateway.Button_Next_Arrow_Png + "\" class=\"sbkMySobek_RoundButton_RightImg\" alt=\"\" /></button>");
            Output.WriteLine("      </div>");
			Output.WriteLine("      <br /><br />");
			Output.WriteLine();

            bool isMozilla = ((!String.IsNullOrEmpty(RequestSpecificValues.Current_Mode.Browser_Type)) && (RequestSpecificValues.Current_Mode.Browser_Type.ToUpper().IndexOf("FIREFOX") >= 0));

	        completeTemplate.Render_Template_HTML(Output, currentItem, RequestSpecificValues.Current_Mode.Skin == RequestSpecificValues.Current_Mode.Default_Skin ? RequestSpecificValues.Current_Mode.Skin.ToUpper() : RequestSpecificValues.Current_Mode.Skin, isMozilla, RequestSpecificValues.Current_User, RequestSpecificValues.Current_Mode.Language, UI_ApplicationCache_Gateway.Translation, RequestSpecificValues.Current_Mode.Base_URL, 1);

            // Add the second buttons at the bottom of the form
			Output.WriteLine();
            Output.WriteLine("      <!-- Add SAVE and CANCEL buttons to bottom of form -->");
			Output.WriteLine("      <div class=\"sbkMySobek_RightButtons\">");
			Output.WriteLine("        <button onclick=\"behaviors_cancel_form(); return false;\" class=\"sbkMySobek_BigButton\"><img src=\"" + Static_Resources_Gateway.Button_Previous_Arrow_Png + "\" class=\"sbkMySobek_RoundButton_LeftImg\" alt=\"\" /> CANCEL </button> &nbsp; &nbsp; ");
			Output.WriteLine("        <button onclick=\"behaviors_save_form(); return false;\" class=\"sbkMySobek_BigButton\"> SAVE <img src=\"" + Static_Resources_Gateway.Button_Next_Arrow_Png + "\" class=\"sbkMySobek_RoundButton_RightImg\" alt=\"\" /></button>");
			Output.WriteLine("      </div>");
			Output.WriteLine("      <br />");
			Output.WriteLine("    </div>");
			Output.WriteLine("  </div>");
			Output.WriteLine("</div>");
			Output.WriteLine("</div>");
			Output.WriteLine("</div>");
        }

        /// <summary> Add the HTML to be added near the top of the page for those viewers that implement pop-up forms for data retrieval </summary>
        /// <param name="Output"> Textwriter to write the pop-up form HTML for this viewer </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering</param>
        /// <remarks> This adds any popup divisions for form metadata elements </remarks>
        public override void Add_Popup_HTML(TextWriter Output, Custom_Tracer Tracer)
        {
            Tracer.Add_Trace("Edit_Group_Behaviors_MySobekViewer.Add_Popup_HTML", "Add any popup divisions for form elements");

            // Add the hidden field
            Output.WriteLine();
        }

		/// <summary> Gets the collection of special behaviors which this admin or mySobek viewer
		/// requests from the main HTML subwriter. </summary>
		/// <value> This tells the HTML and mySobek writers to mimic the currentItem viewer </value>
		public override List<HtmlSubwriter_Behaviors_Enum> Viewer_Behaviors
		{
			get
			{
				return new List<HtmlSubwriter_Behaviors_Enum>
				{
					HtmlSubwriter_Behaviors_Enum.MySobek_Subwriter_Mimic_Item_Subwriter,
					HtmlSubwriter_Behaviors_Enum.Suppress_Banner
				};
			}
		}

        /// <summary> Write any additional values within the HTML Head of the final served page </summary>
        /// <param name="Output"> Output stream currently within the HTML head tags </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> TRUE if this should completely override the default added by the admin or mySobek viewer </returns>
        public override bool Write_Within_HTML_Head(TextWriter Output, Custom_Tracer Tracer)
        {
            Output.WriteLine("  <link href=\"" + Static_Resources_Gateway.Sobekcm_Metadata_Css + "\" rel=\"stylesheet\" type=\"text/css\" />");
            return false;
        }

        /// <summary> Gets the CSS class of the container that the page is wrapped within </summary>
        public override string Container_CssClass { get { return "container-inner1000"; } }
    }
}




