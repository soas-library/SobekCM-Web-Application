#region Using directives

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.UI.WebControls;

#endregion

namespace SobekCM.Library.ItemViewer.Viewers
{
    /// <summary> Item viewer shows plain text view of any text file associated with this digital resource, including OCR'd texte </summary>
    /// <remarks> This class extends the abstract class <see cref="abstractItemViewer"/> and implements the 
    /// <see cref="iItemViewer" /> interface. </remarks>
    public class Text_ItemViewer : abstractItemViewer
    {
        /// <summary> Gets the type of item viewer this object represents </summary>
        /// <value> This property always returns the enumerational value <see cref="ItemViewer_Type_Enum.Text"/>. </value>
        public override ItemViewer_Type_Enum ItemViewer_Type
        {
            get { return ItemViewer_Type_Enum.Text; }
        }
        
        /// <summary> Flag indicates if this view should be overriden if the item is checked out by another user </summary>
        /// <remarks> This always returns the value TRUE for this viewer </remarks>
        public override bool Override_On_Checked_Out
        {
            get
            {
                return true;
            }
        }

        /// <summary> Stream to which to write the HTML for this subwriter  </summary>
        /// <param name="Output"> Response stream for the item viewer to write directly to </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        public override void Write_Main_Viewer_Section(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Text_ItemViewer.Write_Main_Viewer_Section", "");
            }

            // Build the value
            Output.WriteLine("\t\t<td align=\"left\" colspan=\"3\">"  );
            Output.WriteLine("\t\t\t<table class=\"SobekDocumentText\">" );
            Output.WriteLine("\t\t\t\t<tr>" );
            Output.WriteLine("\t\t\t\t\t<td width=\"15\"> </td>" );
            Output.WriteLine("\t\t\t\t\t<td>");
            Output.WriteLine("\t\t\t\t\t\t<pre>" );	
            
            if ( FileName.Length > 0 )
            {
                string filesource = CurrentItem.Web.Source_URL + "/" + FileName;
                string text = Get_Html_Page( filesource, Tracer );

                // If there was a term search here, highlight it
                if (CurrentMode.Text_Search.Length > 0)
                {
                    string upper_text = text.ToUpper();
                    string upper_search = CurrentMode.Text_Search.ToUpper();
                    StringBuilder text_builder = new StringBuilder(text);

                    int start_point = 0;
                    int adjust = 0;
                    int this_point = upper_text.IndexOf(upper_search, start_point);
                    while (this_point >= 0)
                    {
                        if ((this_point + adjust) < text_builder.Length)
                        {
                            text_builder.Insert(this_point + adjust, "<span style=\"background-color: #FFFF00\">");
                        }
                        if (this_point + 40 + upper_search.Length + adjust < text_builder.Length)
                        {
                            text_builder.Insert(this_point + 40 + upper_search.Length + adjust, "</span>");
                        }
                        else
                        {
                            text_builder.Append("</span>");
                        }

                        adjust += 47;
                        start_point = this_point + upper_search.Length;
                        if (start_point < upper_text.Length)
                        {
                            this_point = upper_text.IndexOf(upper_search, start_point);
                        }
                        else
                        {
                            this_point = -1;
                        }
                    }

                    Output.Write(text_builder.ToString());
                }
                else
                {
                    Output.Write(text);
                }
            }

            Output.WriteLine("\t\t\t\t\t\t</pre>" );
            Output.WriteLine("\t\t\t\t\t</td>");
            Output.WriteLine("\t\t\t\t\t<td width=\"15\"> </td>" );
            Output.WriteLine("\t\t\t\t</TR>" );
            Output.WriteLine("\t\t\t</TABLE>" );
            Output.WriteLine("\t\t</td>" );
        }

        private string Get_Html_Page(string strURL, Custom_Tracer tracer )
        {
            tracer.Add_Trace("Text_ItemViewer.Get_Html_Page", "Pull full text from related text file");

            try
            {
                // the html retrieved from the page
                string strResult;
                WebRequest objRequest = WebRequest.Create(strURL);
                WebResponse objResponse = objRequest.GetResponse();

                // the using keyword will automatically dispose the object once complete
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    strResult = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
                return strResult;
            }
            catch (Exception ee)
            {
                throw new ApplicationException("Error pulling html data '" + strURL + "'", ee);
            }
        }
    }
}
