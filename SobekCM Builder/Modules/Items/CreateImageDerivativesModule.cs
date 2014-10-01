﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SobekCM.Library.Builder;
using SobekCM.Library.Settings;
using SobekCM.Resource_Object.Utilities;

namespace SobekCM.Builder.Modules.Items
{
    public class CreateImageDerivativesModule : abstractSubmissionPackageModule
    {
        public override void DoWork(Incoming_Digital_Resource Resource)
        {
            string resourceFolder = Resource.Resource_Folder;
            string bibID = Resource.BibID;
            string vid = Resource.VID;
            string imagemagick_executable = SobekCM_Library_Settings.ImageMagick_Executable;


            // Are there images that need to be processed here?
            if (!String.IsNullOrEmpty(imagemagick_executable))
            {
                // Get the list of jpeg and tiff files
                string[] jpeg_files = Directory.GetFiles(resourceFolder, "*.jpg");
                string[] tiff_files = Directory.GetFiles(resourceFolder, "*.tif");

                // Only continue if some exist
                if ((jpeg_files.Length > 0) || (tiff_files.Length > 0))
                {
                    // Create the image process object for creating 
                    Image_Derivative_Creation_Processor imageProcessor = new Image_Derivative_Creation_Processor(imagemagick_executable, Application.StartupPath + "\\Kakadu", true, true, SobekCM_Library_Settings.JPEG_Width, SobekCM_Library_Settings.JPEG_Height, false, SobekCM_Library_Settings.Thumbnail_Width, SobekCM_Library_Settings.Thumbnail_Height);
                    imageProcessor.New_Task_String += imageProcessor_New_Task_String;
                    imageProcessor.Error_Encountered += imageProcessor_Error_Encountered;


                    // Step through the JPEGS and ensure they have thumbnails (TIFF generation below makes them as well)
                    if (jpeg_files.Length > 0)
                    {
                        foreach (string jpegFile in jpeg_files)
                        {
                            FileInfo jpegFileInfo = new FileInfo(jpegFile);
                            string name = jpegFileInfo.Name.ToUpper();
                            if ((name.IndexOf("THM.JPG") < 0) && (name.IndexOf(".QC.JPG") < 0))
                            {
                                string name_sans_extension = jpegFileInfo.Name.Replace(jpegFileInfo.Extension, "");
                                if (!File.Exists(resourceFolder + "\\" + name_sans_extension + "thm.jpg"))
                                {
                                    imageProcessor.ImageMagick_Create_JPEG(jpegFile, resourceFolder + "\\" + name_sans_extension + "thm.jpg", SobekCM_Library_Settings.Thumbnail_Width, SobekCM_Library_Settings.Thumbnail_Height, Resource.BuilderLogId, Resource.BibID + ":" + Resource.VID);
                                }
                            }
                        }
                    }


                    // Step through any TIFFs as well
                    if (tiff_files.Length > 0)
                    {
                        // Do a complete image derivative creation process on these TIFF files
                        imageProcessor.Process(resourceFolder, bibID, vid, tiff_files, Resource.BuilderLogId);

                        // Since we are actually creating page images here (most likely) try to add
                        // them to the package as well
                        foreach (string thisTiffFile in tiff_files)
                        {
                            // Get the name of the tiff file
                            FileInfo thisTiffFileInfo = new FileInfo(thisTiffFile);
                            string tiffFileName = thisTiffFileInfo.Name.Replace(thisTiffFileInfo.Extension, "");

                            // Get matching files
                            string[] matching_files = Directory.GetFiles(resourceFolder, tiffFileName + ".*");

                            // Now, step through all these files
                            foreach (string derivativeFile in matching_files)
                            {
                                // If this is a page image type file, add it
                                FileInfo derivativeFileInfo = new FileInfo(derivativeFile);
                                if (SobekCM_Library_Settings.PAGE_IMAGE_EXTENSIONS.Contains(derivativeFileInfo.Extension.ToUpper().Replace(".", "")))
                                    Resource.NewImageFiles.Add(derivativeFileInfo.Name);
                            }
                        }
                    }
                }
            }
        }

        void imageProcessor_New_Task_String(string NewMessage, long ParentLogID, string BibID_VID)
        {
            OnProcess(NewMessage, "Image Processing", BibID_VID, String.Empty, ParentLogID);
        }

        void imageProcessor_Error_Encountered(string NewMessage, long ParentLogID, string BibID_VID)
        {
            if (NewMessage.IndexOf("WARNING: ") == 0)
            {
                OnProcess(NewMessage, "Image Processing", BibID_VID, String.Empty, ParentLogID);
            }
            else
            {
                OnError(NewMessage, BibID_VID, String.Empty, ParentLogID);
            }
        }
    }
}
