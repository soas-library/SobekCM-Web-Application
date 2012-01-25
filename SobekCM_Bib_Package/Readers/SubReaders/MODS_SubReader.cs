﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SobekCM.Bib_Package;
using SobekCM.Bib_Package.Bib_Info;
using SobekCM.Bib_Package.SobekCM_Info;
using SobekCM.Bib_Package.Divisions;

namespace SobekCM.Bib_Package.Readers.SubReaders
{
    /// <summary> Subreader reads a MODS-compliant section of XML and stores the data in the provided digital resource object </summary>
    public class MODS_SubReader
    {
        /// <summary> Reads the MODS-compliant section of XML and stores the data in the provided digital resource </summary>
        /// <param name="r"> XmlTextReader from which to read the MODS data </param>
        /// <param name="package"> Digital resource object to save the data to </param>
        public static void Read_MODS_Info(XmlTextReader r, SobekCM_Item package)
        {
            while (r.Read())
            {
                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "METS:mdWrap") || (r.Name == "mdWrap") || ( r.Name == "mods")))
                    return;

                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "mods:abstract":
                        case "abstract":
                            Abstract_Info thisAbstract = new Abstract_Info();
                            if (r.MoveToAttribute("ID"))
                                thisAbstract.ID = r.Value;
                            if (r.MoveToAttribute("type"))
                                thisAbstract.Type = r.Value;
                            if (r.MoveToAttribute("displayLabel"))
                                thisAbstract.Display_Label = r.Value;
                            if (r.MoveToAttribute("lang"))
                                thisAbstract.Language = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                thisAbstract.Abstract_Text = r.Value;
                                package.Bib_Info.Add_Abstract(thisAbstract);
                            }
                            break;

                        case "mods:accessCondition":
                        case "accessCondition":
                            if (r.MoveToAttribute("ID"))
                                package.Bib_Info.Access_Condition.ID = r.Value;
                            if (r.MoveToAttribute("type"))
                                package.Bib_Info.Access_Condition.Type = r.Value;
                            if (r.MoveToAttribute("displayLabel"))
                                package.Bib_Info.Access_Condition.Display_Label = r.Value;
                            if (r.MoveToAttribute("lang"))
                                package.Bib_Info.Access_Condition.Language = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                package.Bib_Info.Access_Condition.Text = r.Value;
                            }
                            break;

                        case "mods:classification":
                        case "classification":
                            Classification_Info thisClassification = new Classification_Info();
                            if (r.MoveToAttribute("edition"))
                                thisClassification.Edition = r.Value;
                            if (r.MoveToAttribute("authority"))
                                thisClassification.Authority = r.Value;
                            if (r.MoveToAttribute("displayLabel"))
                                thisClassification.Display_Label = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                thisClassification.Classification = r.Value;
                                package.Bib_Info.Add_Classification(thisClassification);
                            }
                            break;

                        case "mods:identifier":
                        case "identifier":
                            Identifier_Info thisIdentifier = new Identifier_Info();
                            if (r.MoveToAttribute("type"))
                                thisIdentifier.Type = r.Value;
                            if (r.MoveToAttribute("displayLabel"))
                                thisIdentifier.Display_Label = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                thisIdentifier.Identifier = r.Value;
                                package.Bib_Info.Add_Identifier(thisIdentifier);
                            }
                            break;

                        case "mods:genre":
                        case "genre":
                            Genre_Info thisGenre = new Genre_Info();
                            if (r.MoveToAttribute("ID"))
                                thisGenre.ID = r.Value;
                            if (r.MoveToAttribute("authority"))
                                thisGenre.Authority = r.Value;
                            if (r.MoveToAttribute("lang"))
                                thisGenre.Language = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                thisGenre.Genre_Term = r.Value;
                                package.Bib_Info.Add_Genre(thisGenre);
                            }
                            break;

                        case "mods:language":
                        case "language":
                            string language_text = String.Empty;
                            string language_rfc_code = String.Empty;
                            string language_iso_code = String.Empty;
                            string language_id = String.Empty;
                            if (r.MoveToAttribute("ID"))
                                language_id = r.Value;
                            while (r.Read())
                            {
                                if ((r.NodeType == XmlNodeType.Element) && ((r.Name == "mods:languageTerm") || ( r.Name == "languageTerm")))
                                {
                                    if (r.MoveToAttribute("type"))
                                    {
                                        switch (r.Value)
                                        {
                                            case "code":
                                                if (r.MoveToAttribute("authority"))
                                                {
                                                    if (r.Value == "rfc3066")
                                                    {
                                                        r.Read();
                                                        if (r.NodeType == XmlNodeType.Text)
                                                        {
                                                            language_rfc_code = r.Value;
                                                        }
                                                    }
                                                    else if (r.Value == "iso639-2b")
                                                    {
                                                        r.Read();
                                                        if (r.NodeType == XmlNodeType.Text)
                                                        {
                                                            language_iso_code = r.Value;
                                                        }
                                                    }
                                                }
                                                break;

                                            case "text":
                                                r.Read();
                                                if (r.NodeType == XmlNodeType.Text)
                                                {
                                                    language_text = r.Value;
                                                }
                                                // Quick check for a change we started in 2010
                                                if (language_text == "governmental publication")
                                                    language_text = "government publication";
                                                break;

                                            default:
                                                r.Read();
                                                if (r.NodeType == XmlNodeType.Text)
                                                {
                                                    language_text = r.Value;
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        r.Read();
                                        if (r.NodeType == XmlNodeType.Text)
                                        {
                                            language_text = r.Value;
                                        }
                                    }
                                }

                                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:language") || ( r.Name == "language")))
                                {
                                    break;
                                }
                            }

                            if ((language_text.Length > 0) || (language_rfc_code.Length > 0) || (language_iso_code.Length > 0))
                            {
                                package.Bib_Info.Add_Language(language_text, language_iso_code, language_rfc_code);
                            }
                            break;

                        case "mods:location":
                        case "location":
                            while (r.Read())
                            {
                                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:location") || (r.Name == "location")))
                                    break;

                                if (r.NodeType == XmlNodeType.Element)
                                {
                                    if ((r.Name == "mods:physicalLocation") || (r.Name == "physicalLocation"))
                                    {
                                        if (r.MoveToAttribute("type"))
                                        {
                                            if (r.Value == "code")
                                            {
                                                r.Read();
                                                if (r.NodeType == XmlNodeType.Text)
                                                {
                                                    package.Bib_Info.Location.Holding_Code = r.Value;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                package.Bib_Info.Location.Holding_Name = r.Value;
                                            }
                                        }
                                    }
                                    if ((r.Name == "mods:url") || (r.Name == "url"))
                                    {
                                        // TEST
                                        if (r.MoveToAttribute("access"))
                                        {
                                            if (r.Value == "object in context")
                                            {
                                                r.Read();
                                                if (r.NodeType == XmlNodeType.Text)
                                                {
                                                    package.Bib_Info.Location.PURL = r.Value;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string url_displayLabel = r.GetAttribute("displayLabel");
                                            string url_note = r.GetAttribute("note");
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                if ((url_displayLabel != null) && (url_displayLabel == "Finding Guide"))
                                                {
                                                    if (url_note != null)
                                                    {
                                                        package.Bib_Info.Location.EAD_Name = url_note;
                                                    }
                                                    package.Bib_Info.Location.EAD_URL = r.Value;
                                                }
                                                else
                                                {
                                                    if (url_displayLabel != null)
                                                    {
                                                        package.Bib_Info.Location.Other_URL_Display_Label = url_displayLabel;
                                                    }

                                                    if (url_note != null)
                                                    {
                                                        package.Bib_Info.Location.Other_URL_Note = url_note;
                                                    }
                                                    package.Bib_Info.Location.Other_URL = r.Value;
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            break;

                        case "mods:name":
                        case "name":
                            Name_Info tempNewName = read_name_object(r);
                            if (tempNewName.Main_Entity)
                                package.Bib_Info.Main_Entity_Name = tempNewName;
                            else
                            {
                                bool donor = false;
                                foreach (Name_Info_Role role in tempNewName.Roles)
                                {
                                    if ((role.Role == "donor") || (role.Role == "honoree") || (role.Role == "endowment") || (role.Role == "collection"))
                                    {
                                        donor = true;
                                        break;
                                    }
                                }
                                if (donor)
                                {
                                    package.Bib_Info.Donor = tempNewName;
                                }
                                else
                                {
                                    package.Bib_Info.Add_Named_Entity(tempNewName);
                                }
                            }
                            break;

                        case "mods:note":
                        case "note":
                            Note_Info newNote = new Note_Info();
                            if (r.MoveToAttribute("ID"))
                                newNote.ID = r.Value;
                            if (r.MoveToAttribute("type"))
                                newNote.Note_Type_String = r.Value;
                            if (r.MoveToAttribute("displayLabel"))
                                newNote.Display_Label = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                newNote.Note = r.Value;
                                package.Bib_Info.Add_Note(newNote);
                            }
                            break;

                        case "mods:originInfo":
                        case "originInfo":
                            while (r.Read())
                            {
                                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:originInfo") || (r.Name == "originInfo")))
                                    break;

                                if (r.NodeType == XmlNodeType.Element)
                                {
                                    switch (r.Name)
                                    {
                                        case "mods:publisher":
                                        case "publisher":
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                package.Bib_Info.Origin_Info.Add_Publisher(r.Value);
                                                package.Bib_Info.Add_Publisher(r.Value);
                                            }
                                            break;

                                        case "mods:place":
                                        case "place":
                                            string place_text = String.Empty;
                                            string place_marc = String.Empty;
                                            string place_iso = String.Empty;
                                            while ((r.Read()) && (!(((r.Name == "mods:place") || (r.Name == "place")) && (r.NodeType == XmlNodeType.EndElement))))
                                            {
                                                if ((r.NodeType == XmlNodeType.Element) && ((r.Name == "mods:placeTerm") || (r.Name == "placeTerm")))
                                                {
                                                    if ((r.MoveToAttribute("type")) && (r.Value == "code"))
                                                    {
                                                        if (r.MoveToAttribute("authority"))
                                                        {
                                                            switch (r.Value)
                                                            {
                                                                case "marccountry":
                                                                    r.Read();
                                                                    if (r.NodeType == XmlNodeType.Text)
                                                                    {
                                                                        place_marc = r.Value;
                                                                    }
                                                                    break;

                                                                case "iso3166":
                                                                    r.Read();
                                                                    if (r.NodeType == XmlNodeType.Text)
                                                                    {
                                                                        place_iso = r.Value;
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        r.Read();
                                                        if (r.NodeType == XmlNodeType.Text)
                                                        {
                                                            place_text = r.Value;
                                                        }
                                                    }
                                                }
                                            }
                                            if ((place_text.Length > 0) || (place_marc.Length > 0) || (place_iso.Length > 0))
                                            {
                                                package.Bib_Info.Origin_Info.Add_Place(place_text, place_marc, place_iso);
                                            }
                                            break;

                                        case "mods:dateIssued":
                                        case "dateIssued":
                                            if ((r.MoveToAttribute("encoding")) && (r.Value == "marc"))
                                            {
                                                if (r.MoveToAttribute("point"))
                                                {
                                                    if (r.Value == "start")
                                                    {
                                                        r.Read();
                                                        if (r.NodeType == XmlNodeType.Text)
                                                        {
                                                            package.Bib_Info.Origin_Info.MARC_DateIssued_Start = r.Value;
                                                        }
                                                    }
                                                    else if (r.Value == "end")
                                                    {
                                                        r.Read();
                                                        if (r.NodeType == XmlNodeType.Text)
                                                        {
                                                            package.Bib_Info.Origin_Info.MARC_DateIssued_End = r.Value;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    r.Read();
                                                    if (r.NodeType == XmlNodeType.Text)
                                                    {
                                                        package.Bib_Info.Origin_Info.MARC_DateIssued = r.Value;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                r.Read();
                                                if (r.NodeType == XmlNodeType.Text)
                                                {
                                                    package.Bib_Info.Origin_Info.Date_Issued = r.Value;
                                                }
                                            }
                                            break;

                                        case "mods:dateCreated":
                                        case "dateCreated":
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                package.Bib_Info.Origin_Info.Date_Created = r.Value;
                                            }
                                            break;

                                        case "mods:copyrightDate":
                                        case "copyrightDate":
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                package.Bib_Info.Origin_Info.Date_Copyrighted = r.Value;
                                            }
                                            break;

                                        case "mods:dateOther":
                                        case "dateOther":
                                            if ((r.MoveToAttribute("type")) && (r.Value == "reprint"))
                                            {
                                                r.Read();
                                                if (r.NodeType == XmlNodeType.Text)
                                                {
                                                    package.Bib_Info.Origin_Info.Date_Reprinted = r.Value;
                                                }
                                            }
                                            break;

                                        case "mods:edition":
                                        case "edition":
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                package.Bib_Info.Origin_Info.Edition = r.Value;
                                            }
                                            break;

                                        case "mods:frequency":
                                        case "frequency":
                                            string freq_authority = String.Empty;
                                            if (r.MoveToAttribute("authority"))
                                                freq_authority = r.Value;
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                package.Bib_Info.Origin_Info.Add_Frequency(r.Value, freq_authority);
                                            }
                                            break;

                                        case "mods:issuance":
                                        case "issuance":
                                            r.Read();
                                            if (r.NodeType == XmlNodeType.Text)
                                            {
                                                package.Bib_Info.Origin_Info.Add_Issuance(r.Value);
                                            }
                                            break;
                                    }
                                }
                            }

                            break;

                        case "mods:physicalDescription":
                        case "physicalDescription":
                            read_physical_description(r, package.Bib_Info.Original_Description );
                            break;

                        case "mods:recordInfo":
                        case "recordInfo":
                            while (r.Read())
                            {
                                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:recordInfo") || (r.Name == "recordInfo")))
                                {
                                    break;
                                }

                                if (r.NodeType == XmlNodeType.Element)
                                {
                                    switch (r.Name)
                                    {
                                        case "mods:recordCreationDate":
                                        case "recordCreationDate":
                                            if ((r.MoveToAttribute("encoding")) && (r.Value == "marc"))
                                            {
                                                r.Read();
                                                package.Bib_Info.Record.MARC_Creation_Date = r.Value;
                                            }
                                            break;

                                        case "mods:recordIdentifier":
                                        case "recordIdentifier":
                                            string source = String.Empty;
                                            if (r.MoveToAttribute("source"))
                                            {
                                                package.Bib_Info.Record.Main_Record_Identifier.Type = r.Value;
                                            }
                                            r.Read();
                                            package.Bib_Info.Record.Main_Record_Identifier.Identifier = r.Value;
                                            break;

                                        case "mods:recordOrigin":
                                        case "recordOrigin":
                                            r.Read();
                                            package.Bib_Info.Record.Record_Origin = r.Value;
                                            break;

                                        case "mods:descriptionStandard":
                                        case "descriptionStandard":
                                            r.Read();
                                            package.Bib_Info.Record.Description_Standard = r.Value;
                                            break;

                                        case "mods:recordContentSource":
                                        case "recordContentSource":
                                            if (r.MoveToAttribute("authority"))
                                            {
                                                if (r.Value == "marcorg")
                                                {
                                                    r.Read();
                                                    package.Bib_Info.Record.Add_MARC_Record_Content_Sources(r.Value);
                                                }
                                            }
                                            else
                                            {
                                                r.Read();
                                                package.Bib_Info.Source.Statement = r.Value;
                                            }
                                            break;

                                        case "mods:languageOfCataloging":
                                        case "languageOfCataloging":
                                            string cat_language_text = String.Empty;
                                            string cat_language_rfc_code = String.Empty;
                                            string cat_language_iso_code = String.Empty;
                                            string cat_language_id = String.Empty;
                                            while (r.Read())
                                            {
                                                if ((r.NodeType == XmlNodeType.Element) && ((r.Name == "mods:languageTerm") || (r.Name == "languageTerm")))
                                                {
                                                    if (r.MoveToAttribute("ID"))
                                                        cat_language_id = r.Value;

                                                    if (r.MoveToAttribute("type"))
                                                    {
                                                        switch (r.Value)
                                                        {
                                                            case "code":
                                                                if (r.MoveToAttribute("authority"))
                                                                {
                                                                    if (r.Value == "rfc3066")
                                                                    {
                                                                        r.Read();
                                                                        if (r.NodeType == XmlNodeType.Text)
                                                                        {
                                                                            cat_language_rfc_code = r.Value;
                                                                        }
                                                                    }
                                                                    else if (r.Value == "iso639-2b")
                                                                    {
                                                                        r.Read();
                                                                        if (r.NodeType == XmlNodeType.Text)
                                                                        {
                                                                            cat_language_iso_code = r.Value;
                                                                        }
                                                                    }
                                                                }
                                                                break;

                                                            case "text":
                                                                r.Read();
                                                                if (r.NodeType == XmlNodeType.Text)
                                                                {
                                                                    cat_language_text = r.Value;
                                                                }
                                                                break;

                                                            default:
                                                                r.Read();
                                                                if (r.NodeType == XmlNodeType.Text)
                                                                {
                                                                    cat_language_text = r.Value;
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        r.Read();
                                                        if (r.NodeType == XmlNodeType.Text)
                                                        {
                                                            cat_language_text = r.Value;
                                                        }
                                                    }
                                                }

                                                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:languageOfCataloging") || (r.Name == "languageOfCataloging")))
                                                {
                                                    break;
                                                }
                                            }

                                            if ((cat_language_text.Length > 0) || (cat_language_rfc_code.Length > 0) || (cat_language_iso_code.Length > 0))
                                            {
                                                Language_Info newCatLanguage = new Language_Info(cat_language_text, cat_language_iso_code, cat_language_rfc_code);
                                                package.Bib_Info.Record.Add_Catalog_Language(newCatLanguage);
                                            }
                                            break;
                                    }
                                }
                            }
                            break;


                        case "mods:relatedItem":
                        case "relatedItem":
                            string relatedItemType = String.Empty;
                            if (r.MoveToAttribute("type"))
                                relatedItemType = r.Value.ToLower();

                            switch (relatedItemType)
                            {
                                case "original":
                                    while (r.Read())
                                    {
                                        if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:relatedItem") || (r.Name == "relatedItem")))
                                            break;

                                        if (r.NodeType == XmlNodeType.Element)
                                        {
                                            if ((r.Name == "mods:physicalDescription") || (r.Name == "physicalDescription"))
                                            {
                                                read_physical_description(r, package.Bib_Info.Original_Description);
                                            }
                                        }
                                    }
                                    break;

                                case "series":
                                    string part_type = String.Empty;
                                    string part_caption = String.Empty;
                                    string part_number = String.Empty;

                                    while (r.Read())
                                    {
                                        if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:relatedItem") || (r.Name == "mods:relatedItem")))
                                        {
                                            break;
                                        }

                                        if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:detail") || (r.Name == "detail")))
                                        {
                                            try
                                            {
                                                switch (part_type)
                                                {
                                                    case "Enum1":
                                                        package.Bib_Info.Series_Part_Info.Enum1 = part_caption;
                                                        package.Bib_Info.Series_Part_Info.Enum1_Index = Convert.ToInt32(part_number);
                                                        break;

                                                    case "Enum2":
                                                        package.Bib_Info.Series_Part_Info.Enum2 = part_caption;
                                                        package.Bib_Info.Series_Part_Info.Enum2_Index = Convert.ToInt32(part_number);
                                                        break;

                                                    case "Enum3":
                                                        package.Bib_Info.Series_Part_Info.Enum3 = part_caption;
                                                        package.Bib_Info.Series_Part_Info.Enum3_Index = Convert.ToInt32(part_number);
                                                        break;

                                                    case "Enum4":
                                                        package.Bib_Info.Series_Part_Info.Enum4 = part_caption;
                                                        package.Bib_Info.Series_Part_Info.Enum4_Index = Convert.ToInt32(part_number);
                                                        break;

                                                    case "Year":
                                                        package.Bib_Info.Series_Part_Info.Year = part_caption;
                                                        package.Bib_Info.Series_Part_Info.Year_Index = Convert.ToInt32(part_number);
                                                        break;

                                                    case "Month":
                                                        package.Bib_Info.Series_Part_Info.Month = part_caption;
                                                        package.Bib_Info.Series_Part_Info.Month_Index = Convert.ToInt32(part_number);
                                                        break;

                                                    case "Day":
                                                        package.Bib_Info.Series_Part_Info.Day = part_caption;
                                                        package.Bib_Info.Series_Part_Info.Day_Index = Convert.ToInt32(part_number);
                                                        break;
                                                }
                                            }
                                            catch
                                            {

                                            }

                                            part_type = String.Empty;
                                            part_caption = String.Empty;
                                            part_number = String.Empty;
                                        }

                                        if (r.NodeType == XmlNodeType.Element)
                                        {
                                            switch (r.Name)
                                            {
                                                case "mods:titleInfo":
                                                case "titleInfo":
                                                    package.Bib_Info.SeriesTitle = read_title_object(r);
                                                    break;

                                                case "mods:detail":
                                                case "detail":
                                                    if (r.MoveToAttribute("type"))
                                                    {
                                                        part_type = r.Value;
                                                    }
                                                    break;

                                                case "mods:caption":
                                                case "caption":
                                                    r.Read();
                                                    if (r.NodeType == XmlNodeType.Text)
                                                    {
                                                        part_caption = r.Value;
                                                    }
                                                    break;

                                                case "mods:number":
                                                case "number":
                                                    r.Read();
                                                    if (r.NodeType == XmlNodeType.Text)
                                                    {
                                                        part_number = r.Value;
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    break;

                                default:
                                    Related_Item_Info newRelated = new Related_Item_Info();
                                    package.Bib_Info.Add_Related_Item(newRelated);
                                    switch (relatedItemType)
                                    {
                                        case "preceding":
                                            newRelated.Relationship = Related_Item_Type_Enum.preceding;
                                            break;

                                        case "succeeding":
                                            newRelated.Relationship = Related_Item_Type_Enum.succeeding;
                                            break;

                                        case "otherVersion":
                                            newRelated.Relationship = Related_Item_Type_Enum.otherVersion;
                                            break;

                                        case "otherFormat":
                                            newRelated.Relationship = Related_Item_Type_Enum.otherFormat;
                                            break;

                                        case "host":
                                            newRelated.Relationship = Related_Item_Type_Enum.host;
                                            break;
                                    }
                                    if (r.MoveToAttribute("ID"))
                                        newRelated.ID = r.Value;

                                    while (r.Read())
                                    {
                                        if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:relatedItem") || (r.Name == "relatedItem")))
                                        {
                                            break;
                                        }

                                        if (r.NodeType == XmlNodeType.Element)
                                        {
                                            switch (r.Name)
                                            {
                                                case "mods:titleInfo":
                                                case "titleInfo":
                                                    newRelated.Set_Main_Title(read_title_object(r));
                                                    break;

                                                case "mods:identifier":
                                                case "identifier":
                                                    Identifier_Info thisRIdentifier = new Identifier_Info();
                                                    if (r.MoveToAttribute("type"))
                                                        thisRIdentifier.Type = r.Value;
                                                    if (r.MoveToAttribute("displayLabel"))
                                                        thisRIdentifier.Display_Label = r.Value;
                                                    r.Read();
                                                    if (r.NodeType == XmlNodeType.Text)
                                                    {
                                                        thisRIdentifier.Identifier = r.Value;
                                                        newRelated.Add_Identifier(thisRIdentifier);
                                                    }
                                                    break;

                                                case "mods:name":
                                                case "name":
                                                    newRelated.Add_Name(read_name_object(r));
                                                    break;

                                                case "mods:note":
                                                case "note":
                                                    Note_Info newRNote = new Note_Info();
                                                    if (r.MoveToAttribute("ID"))
                                                        newRNote.ID = r.Value;
                                                    if (r.MoveToAttribute("type"))
                                                        newRNote.Note_Type_String = r.Value;
                                                    if (r.MoveToAttribute("displayLabel"))
                                                        newRNote.Display_Label = r.Value;
                                                    r.Read();
                                                    if (r.NodeType == XmlNodeType.Text)
                                                    {
                                                        newRNote.Note = r.Value;
                                                        newRelated.Add_Note(newRNote);
                                                    }
                                                    break;

                                                case "mods:url":
                                                case "url":
                                                    if (r.MoveToAttribute("displayLabel"))
                                                        newRelated.URL_Display_Label = r.Value;
                                                    r.Read();
                                                    if (r.NodeType == XmlNodeType.Text)
                                                    {
                                                        newRelated.URL = r.Value;
                                                    }
                                                    break;

                                                case "mods:publisher":
                                                case "publisher":
                                                    r.Read();
                                                    if (r.NodeType == XmlNodeType.Text)
                                                    {
                                                        newRelated.Publisher = r.Value;
                                                    }
                                                    break;

                                                case "mods:recordIdentifier":
                                                case "recordIdentifier":
                                                    if (r.MoveToAttribute("source"))
                                                    {
                                                        if ((r.Value == "ufdc") || (r.Value == "dloc"))
                                                        {
                                                            r.Read();
                                                            if (r.NodeType == XmlNodeType.Text)
                                                            {
                                                                newRelated.UFDC_ID = r.Value;
                                                            }
                                                        }
                                                    }
                                                    break;

                                                case "mods:dateIssued":
                                                case "dateIssued":
                                                    if (r.MoveToAttribute("point"))
                                                    {
                                                        if (r.Value == "start")
                                                        {
                                                            r.Read();
                                                            if (r.NodeType == XmlNodeType.Text)
                                                            {
                                                                newRelated.Start_Date = r.Value;
                                                            }
                                                        }
                                                        else if (r.Value == "end")
                                                        {
                                                            r.Read();
                                                            if (r.NodeType == XmlNodeType.Text)
                                                            {
                                                                newRelated.End_Date = r.Value;
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    break;

                            }
                            break;

                        case "mods:subject":
                        case "subject":
                            read_subject_object(r, package);
                            break;

                        case "mods:targetAudience":
                        case "targetAudience":
                            TargetAudience_Info newTarget = new TargetAudience_Info();
                            if (r.MoveToAttribute("ID"))
                                newTarget.ID = r.Value;
                            if (r.MoveToAttribute("authority"))
                                newTarget.Authority = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                newTarget.Audience = r.Value;
                                package.Bib_Info.Add_Target_Audience(newTarget);
                            }
                            break;

                        case "mods:tableOfContents":
                        case "tableOfContents":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                package.Bib_Info.TableOfContents = r.Value;
                            }
                            break;

                        case "mods:titleInfo":
                        case "titleInfo":
                            Title_Info thisTitle = read_title_object(r);
                            if (thisTitle.Title_Type == Title_Type_Enum.UNSPECIFIED)
                                package.Bib_Info.Main_Title = thisTitle;
                            else
                                package.Bib_Info.Add_Other_Title(thisTitle);
                            break;

                        case "mods:typeOfResource":
                        case "typeOfResource":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                package.Bib_Info.Type.Add_Uncontrolled_Type(r.Value);
                            }
                            break;

                        case "mods:extension":
                        case "extension":
                            string schema = String.Empty;
                            string alias = String.Empty;
                            if (r.HasAttributes)
                            {
                                for (int i = 0; i < r.AttributeCount; i++)
                                {
                                    r.MoveToAttribute(i);
                                    if (r.Name.IndexOf("xmlns") == 0)
                                    {
                                        alias = r.Name.Replace("xmlns:", "");
                                        schema = r.Value;
                                        break;
                                    }
                                }
                            }
                            if (schema.IndexOf("vra.xsd") > 0)
                            {
                                read_vra_core_extensions(r, package, alias);
                            }
                            break;
                    }
                }

            }
        }

        private static void read_physical_description(XmlTextReader r, PhysicalDescription_Info description)
        {
            while (r.Read())
            {
                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:physicalDescription") || (r.Name == "physicalDescription")))
                    break;

                if (r.NodeType == XmlNodeType.Element)
                {
                    if ((r.Name == "mods:extent") || (r.Name == "extent"))
                    {
                        r.Read();
                        if (r.NodeType == XmlNodeType.Text)
                        {
                            description.Extent = r.Value;
                        }
                    }
                    else if ((r.Name == "mods:note") || (r.Name == "note"))
                    {
                        r.Read();
                        if (r.NodeType == XmlNodeType.Text)
                        {
                            description.Add_Note(r.Value);
                        }
                    }
                    else if ((r.Name == "mods:form") || (r.Name == "form"))
                    {
                        string type = String.Empty;
                        string authority = String.Empty;
                        if (r.MoveToAttribute("type"))
                            type = r.Value;
                        if (r.MoveToAttribute("authority"))
                            authority = r.Value;
                        r.Read();
                        if (r.NodeType == XmlNodeType.Text)
                        {
                            description.Form_Info.Form = r.Value;
                            if (type.Length > 0)
                                description.Form_Info.Type = type;
                            if (authority.Length > 0)
                                description.Form_Info.Authority = authority;
                        }
                    }
                }
            }
        }

        private static void read_vra_core_extensions(XmlTextReader r, SobekCM_Item package, string alias)
        {
            while (r.Read())
            {
                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:extension") || (r.Name == "extension")))
                {
                    return;
                }

                if (r.NodeType == XmlNodeType.Element)
                {
                    string nodename = r.Name;
                    if (alias.Length > 0)
                    {
                        nodename = nodename.Replace(alias + ":", "");
                    }
                    switch (nodename)
                    {
                        case "culturalContext":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                if (r.Value.Length > 0)
                                    package.Bib_Info.VRACore.Add_Cultural_Context(r.Value);
                            }
                            break;

                        case "inscription":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                if (r.Value.Length > 0)
                                    package.Bib_Info.VRACore.Add_Inscription(r.Value);
                            }
                            break;

                        case "material":
                            string type = String.Empty;
                            if (r.HasAttributes)
                            {
                                if (r.MoveToAttribute("type"))
                                    type = r.Value;
                            }
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                if (r.Value.Length > 0)
                                    package.Bib_Info.VRACore.Add_Material(r.Value, type);
                            }
                            break;

                        case "measurements":
                            string units = String.Empty;
                            if (r.HasAttributes)
                            {
                                if (r.MoveToAttribute("unit"))
                                    units = r.Value;
                            }
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                if (r.Value.Length > 0)
                                    package.Bib_Info.VRACore.Add_Measurement(r.Value, units);
                            }
                            break;

                        case "stateEdition":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                if (r.Value.Length > 0)
                                    package.Bib_Info.VRACore.Add_State_Edition(r.Value);
                            }
                            break;

                        case "stylePeriod":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                if (r.Value.Length > 0)
                                    package.Bib_Info.VRACore.Add_Style_Period(r.Value);
                            }
                            break;

                        case "technique":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                if (r.Value.Length > 0)
                                    package.Bib_Info.VRACore.Add_Technique(r.Value);
                            }
                            break;
                    }
                }
            }
        }

        private static void read_subject_object(XmlTextReader r, SobekCM_Item package)
        {
            string language = String.Empty;
            string authority = String.Empty;
            string id = String.Empty;

            if (r.MoveToAttribute("ID"))
                id = r.Value;
            if (r.MoveToAttribute("lang"))
                language = r.Value;
            if (r.MoveToAttribute("authority"))
                authority = r.Value;

            // Move to the next element
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Element)
                    break;
            }

            // Determine the subject type
            Subject_Info_Type type = Subject_Info_Type.UNKNOWN;

            // What is the name of this node?
            switch (r.Name)
            {
                case "mods:topic":
                case "mods:geographic":
                case "mods:genre":
                case "mods:temporal":
                case "mods:occupation":
                case "topic":
                case "geographic":
                case "genre":
                case "temporal":
                case "occupation":
                    type = Subject_Info_Type.Standard;
                    break;

                case "mods:hierarchicalGeographic":
                case "hierarchicalGeographic":
                    type = Subject_Info_Type.Hierarchical_Spatial;
                    break;

                case "mods:cartographics":
                case "cartographics":
                    type = Subject_Info_Type.Cartographics;
                    break;

                case "mods:name":
                case "name":
                    type = Subject_Info_Type.Name;
                    break;

                case "mods:titleInfo":
                case "titleInfo":
                    type = Subject_Info_Type.TitleInfo;
                    break;
            }

            // If no type was determined, return null
            if (type == Subject_Info_Type.UNKNOWN)
                return;

            // Was this the standard subject object?
            if (type == Subject_Info_Type.Standard)
            {
                Subject_Info_Standard standardSubject = new Subject_Info_Standard();
                standardSubject.Language = language;
                standardSubject.Authority = authority;
                standardSubject.ID = id;

                do
                {
                    if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:subject") || (r.Name == "subject")))
                    {
                        if ((standardSubject.Topics_Count > 0) || (standardSubject.Geographics_Count > 0) || (standardSubject.Genres_Count > 0) ||
                            (standardSubject.Temporals_Count > 0) || (standardSubject.Occupations_Count > 0))
                        {
                            package.Bib_Info.Add_Subject(standardSubject);
                        }
                        return;
                    }

                    if (r.NodeType == XmlNodeType.Element)
                    {
                        switch (r.Name)
                        {
                            case "mods:topic":
                            case "topic":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    standardSubject.Add_Topic(r.Value);
                                break;

                            case "mods:geographic":
                            case "geographic":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    standardSubject.Add_Geographic(r.Value);
                                break;

                            case "mods:genre":
                            case "genre":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    standardSubject.Add_Genre(r.Value);
                                break;

                            case "mods:temporal":
                            case "temporal":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    standardSubject.Add_Temporal(r.Value);
                                break;

                            case "mods:occupation":
                            case "occupation":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    standardSubject.Add_Occupation(r.Value);
                                break;
                        }
                    }
                } while (r.Read());
            }

            // Was this the hierarchical geography subject?
            if (type == Subject_Info_Type.Hierarchical_Spatial)
            {
                Subject_Info_HierarchicalGeographic geoSubject = new Subject_Info_HierarchicalGeographic();
                geoSubject.Language = language;
                geoSubject.Authority = authority;
                geoSubject.ID = id;

                while (r.Read())
                {
                    if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:subject") || (r.Name == "subject")))
                    {
                        package.Bib_Info.Add_Subject(geoSubject);
                        return;
                    }

                    if (r.NodeType == XmlNodeType.Element)
                    {
                        switch (r.Name)
                        {
                            case "mods:continent":
                            case "continent":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.Continent = r.Value;
                                break;

                            case "mods:country":
                            case "country":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.Country = r.Value;
                                break;

                            case "mods:province":
                            case "province":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.Province = r.Value;
                                break;

                            case "mods:region":
                            case "region":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.Region = r.Value;
                                break;

                            case "mods:state":
                            case "state":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.State = r.Value;
                                break;

                            case "mods:territory":
                            case "territory":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.Territory = r.Value;
                                break;

                            case "mods:county":
                            case "county":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.County = r.Value;
                                break;

                            case "mods:city":
                            case "city":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.City = r.Value;
                                break;

                            case "mods:citySection":
                            case "citySection":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.CitySection = r.Value;
                                break;

                            case "mods:island":
                            case "island":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.Island = r.Value;
                                break;

                            case "mods:area":
                            case "area":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    geoSubject.Area = r.Value;
                                break;

                        }
                    }
                }
            }

            // Was this the cartographics subject?
            if (type == Subject_Info_Type.Cartographics)
            {
                Subject_Info_Cartographics mapSubject = new Subject_Info_Cartographics();
                mapSubject.Language = language;
                mapSubject.Authority = authority;
                mapSubject.ID = id;

                while (r.Read())
                {
                    if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:subject") || ( r.Name == "subject")))
                    {
                        if ((mapSubject.Projection.Length > 0) || (mapSubject.Coordinates.Length > 0) || (mapSubject.Scale.Length > 0))
                        {
                            package.Bib_Info.Add_Subject(mapSubject);
                        }
                        return;
                    }

                    if (r.NodeType == XmlNodeType.Element)
                    {
                        switch (r.Name)
                        {
                            case "mods:coordinates":
                            case "coordinates":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    mapSubject.Coordinates = r.Value;
                                break;

                            case "mods:scale":
                            case "scale":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    mapSubject.Scale = r.Value;
                                break;

                            case "mods:projection":
                            case "projection":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    mapSubject.Projection = r.Value;
                                break;
                        }
                    }
                }
            }

            // Was this the name subject?
            if (type == Subject_Info_Type.Name)
            {
                Subject_Info_Name nameSubject = new Subject_Info_Name();
                nameSubject.Language = language;
                nameSubject.Authority = authority;
                nameSubject.ID = id;

                do
                {
                    if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:subject") || (r.Name == "subject")))
                    {
                        package.Bib_Info.Add_Subject(nameSubject);
                        return;
                    }

                    if (r.NodeType == XmlNodeType.Element)
                    {
                        switch (r.Name)
                        {
                            case "mods:name":
                            case "name":
                                Name_Info nameInfo = read_name_object(r);
                                nameSubject.Set_Internal_Name(nameInfo);
                                break;

                            case "mods:topic":
                            case "topic":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    nameSubject.Add_Topic(r.Value);
                                break;

                            case "mods:geographic":
                            case "geographic":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    nameSubject.Add_Geographic(r.Value);
                                break;

                            case "mods:genre":
                            case "genre":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    nameSubject.Add_Genre(r.Value);
                                break;

                            case "mods:temporal":
                            case "temporal":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    nameSubject.Add_Temporal(r.Value);
                                break;
                        }
                    }
                } while (r.Read());
            }

            // Was this the title subject?
            if (type == Subject_Info_Type.TitleInfo)
            {
                Subject_Info_TitleInfo titleSubject = new Subject_Info_TitleInfo();
                titleSubject.Language = language;
                titleSubject.Authority = authority;
                titleSubject.ID = id;

                do
                {
                    if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:subject") || (r.Name == "subject")))
                    {
                        package.Bib_Info.Add_Subject(titleSubject);
                        return;
                    }

                    if (r.NodeType == XmlNodeType.Element)
                    {
                        switch (r.Name)
                        {
                            case "mods:titleInfo":
                            case "titleInfo":
                                Title_Info titleInfo = read_title_object(r);
                                titleSubject.Set_Internal_Title(titleInfo);
                                break;

                            case "mods:topic":
                            case "topic":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    titleSubject.Add_Topic(r.Value);
                                break;

                            case "mods:geographic":
                            case "geographic":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    titleSubject.Add_Geographic(r.Value);
                                break;

                            case "mods:genre":
                            case "genre":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    titleSubject.Add_Genre(r.Value);
                                break;

                            case "mods:temporal":
                            case "temporal":
                                r.Read();
                                if (r.NodeType == XmlNodeType.Text)
                                    titleSubject.Add_Temporal(r.Value);
                                break;
                        }
                    }
                } while (r.Read());
            }
        }

        private static Title_Info read_title_object(XmlTextReader r)
        {
            Title_Info returnVal = new Title_Info();

            if (r.MoveToAttribute("ID"))
                returnVal.ID = r.Value;

            if (r.MoveToAttribute("type"))
            {
                switch (r.Value)
                {
                    case "alternative":
                        returnVal.Title_Type = Title_Type_Enum.alternative;
                        break;

                    case "translated":
                        returnVal.Title_Type = Title_Type_Enum.translated;
                        break;

                    case "uniform":
                        returnVal.Title_Type = Title_Type_Enum.uniform;
                        break;

                    case "abbreviated":
                        returnVal.Title_Type = Title_Type_Enum.abbreviated;
                        break;
                }
            }

            if (r.MoveToAttribute("displayLabel"))
                returnVal.Display_Label = r.Value;

            if (r.MoveToAttribute("lang"))
                returnVal.Language = r.Value;

            if (r.MoveToAttribute("authority"))
                returnVal.Authority = r.Value;

            while (r.Read())
            {
                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:titleInfo") || ( r.Name == "titleInfo")))
                    return returnVal;

                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "mods:title":
                        case "title":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnVal.Title = r.Value;
                            }
                            break;

                        case "mods:nonSort":
                        case "nonSort":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnVal.NonSort = r.Value;
                            }
                            break;

                        case "mods:subTitle":
                        case "subTitle":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnVal.Subtitle = r.Value;
                            }
                            break;

                        case "mods:partNumber":
                        case "partNumber":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnVal.Add_Part_Number(r.Value);
                            }
                            break;

                        case "mods:partName":
                        case "partName":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnVal.Add_Part_Name(r.Value);
                            }
                            break;
                    }
                }
            }

            return returnVal;
        }

        private static Name_Info read_name_object(XmlTextReader r)
        {
            Name_Info returnValue = new Name_Info();

            if (r.MoveToAttribute("type"))
            {
                if (r.Value == "personal")
                    returnValue.Name_Type = Name_Info_Type_Enum.personal;
                if (r.Value == "corporate")
                    returnValue.Name_Type = Name_Info_Type_Enum.corporate;
                if (r.Value == "conference")
                    returnValue.Name_Type = Name_Info_Type_Enum.conference;
            }

            if (r.MoveToAttribute("ID"))
                returnValue.ID = r.Value;

            while (r.Read())
            {
                if ((r.NodeType == XmlNodeType.EndElement) && ((r.Name == "mods:name") || (r.Name == "name")))
                    return returnValue;

                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "mods:displayForm":
                        case "displayForm":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnValue.Display_Form = r.Value;
                            }
                            break;

                        case "mods:affiliation":
                        case "affiliation":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnValue.Affiliation = r.Value;
                            }
                            break;

                        case "mods:description":
                        case "description":
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                returnValue.Description = r.Value;
                            }
                            break;

                        case "mods:namePart":
                        case "namePart":
                            string type = String.Empty;
                            if (r.MoveToAttribute("type"))
                            {
                                type = r.Value;
                            }
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                switch (type)
                                {
                                    case "given":
                                        returnValue.Given_Name = r.Value;
                                        break;

                                    case "family":
                                        returnValue.Family_Name = r.Value;
                                        break;

                                    case "termsOfAddress":
                                        returnValue.Terms_Of_Address = r.Value;
                                        break;

                                    case "date":
                                        returnValue.Dates = r.Value;
                                        break;

                                    default:
                                        returnValue.Full_Name = r.Value;
                                        break;
                                }
                            }
                            break;

                        case "mods:roleTerm":
                        case "roleTerm":
                            string role_type = String.Empty;
                            string role_authority = String.Empty;
                            if (r.MoveToAttribute("type"))
                                role_type = r.Value;
                            if (r.MoveToAttribute("authority"))
                                role_authority = r.Value;
                            r.Read();
                            if (r.NodeType == XmlNodeType.Text)
                            {
                                switch (role_type)
                                {
                                    case "code":
                                        returnValue.Add_Role(r.Value, role_authority, Name_Info_Role_Type_Enum.code);
                                        break;

                                    case "text":
                                        returnValue.Add_Role(r.Value, role_authority, Name_Info_Role_Type_Enum.text);
                                        break;

                                    default:
                                        if (r.Value == "Main Entity")
                                            returnValue.Main_Entity = true;
                                        else
                                            returnValue.Add_Role(r.Value, role_authority, Name_Info_Role_Type_Enum.UNSPECIFIED);
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            return returnValue;
        }
    }
}
