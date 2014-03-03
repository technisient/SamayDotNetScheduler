using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Technisient;
using System.IO;

namespace CopyFiles_Task
{
    class CopyFiles_Task : TaskBase
    {
        static void Main(string[] args)
        {

        }

        public override object Run(object input)
        {
            try
            {
                LogTrace("Starting CopyFile_Task");
                LogDebug("Calling DirectoryCopy Method");
                DirectoryCopy(sourceDirectory, destinationDirectory, includeSubDirectories);
                LogTrace("CopyFile_Task Completed");
                return true;
            }
            catch(Exception ex)
            {
                LogError("Error! " + ex.ToString());
                return ex;
            }
        }

        string sourceDirectory;

        [SamayParameter(LabelText = "Source Directory", Help = "The Source Directory", IsRequired = true, Index = 1)]
        public string SourceDirectory
        {
            get { return sourceDirectory; }
            set { sourceDirectory = value; }
        }

        string destinationDirectory;

        [SamayParameter(LabelText = "Destination Directory", Help = "The Destination Directory", IsRequired = true, Index = 2)]
        public string DestinationDirectory
        {
            get { return destinationDirectory; }
            set { destinationDirectory = value; }
        }

        bool includeSubDirectories;

        [SamayParameter(LabelText = "Copy Sub-Directories", 
            Help = "Check box to recursively copy all direcotries within the source directory", 
            DefaultValue= "true", IsRequired = true,
            Index = 3)]
        public bool IncludeSubDirectories
        {
            get { return includeSubDirectories; }
            set { includeSubDirectories = value; }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}