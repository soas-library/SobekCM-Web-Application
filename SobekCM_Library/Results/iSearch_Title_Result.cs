﻿namespace SobekCM.Library.Results
{
    /// <summary> Interface which any class that contains the required display and identification 
    /// information for a single title within a set of results. </summary>
    public interface iSearch_Title_Result
    {
        /// <summary> Gets the number of items contained within this title result </summary>
        int Item_Count { get; }

        /// <summary> Gets the item tree view used for showing all the items under this title in a tree type html display </summary>
        /// <remarks> This includes intermediary nodes, while the item list only includes the actual items </remarks>
        Search_Result_Item_Tree Item_Tree { get; }

        /// <summary> Bibliographic identifier (BibID) for this title result </summary>
        string BibID { get; }

        /// <summary> Group title for this title result </summary>
        string GroupTitle { get; }

        /// <summary> Publisher for this title result </summary>
        string Publisher { get; }

        /// <summary> Author for this title result </summary>
        string Author { get; }

        /// <summary> Format for this title result </summary>
        string Format { get; }

        /// <summary> Spatial coverage for this title result </summary>
        string Spatial_Coverage { get; }

        /// <summary> Spatial coverage for this title result in terms of coordinates for map display </summary>
        string Spatial_Coordinates { get; }

        /// <summary> Edition for this title result </summary>
        string Edition { get; }

        /// <summary> Institution for this title result </summary>
        string Institution { get; }

        /// <summary> Material information (not type, rather for museum objects) for this title result </summary>
        string Material { get; }

        /// <summary> Measurement for this title result </summary>
        string Measurement { get; }

        /// <summary> Style/Period for this title result </summary>
        string Style_Period { get; }

        /// <summary> Technique used for this title result </summary>
        string Technique { get; }

        /// <summary> Subjects associated with this title result </summary>
        string Subjects { get; }

        /// <summary> Donor for this title result </summary>
        string Donor { get; }

        /// <summary> ALEPH cataloging number for this title result </summary>
        long ALEPH_Number { get; }

        /// <summary> OCLC cataloging number for this title result </summary>
        long OCLC_Number { get; }

        /// <summary> Group-wide thumbnail for this title result </summary>
        string GroupThumbnail { get; }

        /// <summary> User notes for this title result, if it is in a bookshelf </summary>
        string UserNotes { get; }

        /// <summary> Material type for this title result </summary>
        string MaterialType { get; }

        /// <summary> Highlighted snippet of text from this document </summary>
        string Snippet { get; }

        /// <summary> Type of the primary alternate identifier for this resource ( i.e. 'Accession Number', etc.. )</summary>
        string Primary_Identifier_Type { get; }

        /// <summary> Primary alternate identifier for this resource</summary>
        string Primary_Identifier { get; }

        /// <summary> Gets the item indicated by the provided index </summary>
        /// <param name="Index"> Index of the item requested </param>
        /// <returns> Item result requested, or NULL </returns>
        iSearch_Item_Result Get_Item(int Index);

        /// <summary> Builds the tree of items under this title, for multiple item titles </summary>
        void Build_Item_Tree(string resultsIndex);
    }
}
