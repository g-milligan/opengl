using System;
using System.Diagnostics;
using System.IO;
namespace g2OpenGLProjectSetup
{
    class Setup
    {
        static void Main(string[] args)
        {
            string copytoprojdirname = "copytoproject";
            //bool foundDir = false;
            //get the current directory of this .exe file
            string currentDirPath=AppDomain.CurrentDomain.BaseDirectory;
            //get the new current directory (up one level)
            string upOneDirPath = currentDirPath;
            string projFolder = upOneDirPath;
            //trim off the trailing "\" if it is there
            if (upOneDirPath.LastIndexOf("\\") == upOneDirPath.Length - 1)
            {
                //trim off "\"
                upOneDirPath = upOneDirPath.Substring(0, upOneDirPath.Length - 1);
                projFolder = upOneDirPath;
            }
            //if upOneDirPath still contains "\"
            if (upOneDirPath.LastIndexOf("\\")!=-1)
            {
                //trim off the last folder in the currentDirPath
                upOneDirPath = upOneDirPath.Substring(0, upOneDirPath.LastIndexOf("\\"));
                //get JUST the project folder without the path
                projFolder = projFolder.Substring(projFolder.LastIndexOf("\\")+1);
            }

            string copyFromPath = upOneDirPath + "\\" + copytoprojdirname;
            if (Directory.Exists(copyFromPath))
            {
                Console.WriteLine("Copying folders and files from " + copytoprojdirname + " --> " + projFolder + "\n");

                //copy the contents of the copytoprojdirname folder to the current project directory
                int folderCount = 0; int fileCount = 0; int skippedFolderCount = 0; int skippedFileCount = 0;
                directoryCopy(copyFromPath, currentDirPath, true, projFolder, ref folderCount, ref fileCount, ref skippedFolderCount, ref skippedFileCount);

                Console.WriteLine("\nFinished copying. \n\nCopied folders: (" + folderCount + ") || Copied files: (" + fileCount + ")");
                Console.WriteLine("Skipped folders: (" + skippedFolderCount + ") || Skipped files: (" + skippedFileCount + ")\n");
            }
            else
            {
                Console.WriteLine("Uh oh; failed to copy contents of \"../" + copytoprojdirname + "\" to your project. Could not find this folder:");
                Console.WriteLine(copyFromPath);
            }
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        private static void directoryCopy(string sourceDirName, string destDirName, bool copySubDirs, string projFolder, ref int folderCount, ref int fileCount, ref int skippedFolderCount, ref int skippedFileCount)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            string shortDestDir = destDirName;
            if (shortDestDir.IndexOf("\\" + projFolder + "\\") != -1)
            {
                //trim off everything before AND including the project's folder name
                shortDestDir = shortDestDir.Substring(shortDestDir.IndexOf("\\" + projFolder + "\\") + ("\\" + projFolder + "\\").Length);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                Console.WriteLine("DIR CREATE: \t" + projFolder + "\\" + shortDestDir);
                folderCount++;
            }
            else
            {
                Console.WriteLine("DIR SKIP (ALREADY EXISTS): \t" + projFolder + "\\" + shortDestDir);
                skippedFolderCount++;
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                //if the file doesn't already exist
                if (!File.Exists(temppath))
                {
                    file.CopyTo(temppath, false);
                    Console.WriteLine("FILE COPY: \t" + projFolder + "\\" + shortDestDir + file.Name);
                    fileCount++;
                }
                else
                {
                    //file already exists
                    Console.WriteLine("FILE SKIP (ALREADY EXISTS): \t" + projFolder + "\\" + shortDestDir + file.Name);
                    skippedFileCount++;
                }
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    directoryCopy(subdir.FullName, temppath, copySubDirs, projFolder, ref folderCount, ref fileCount, ref skippedFolderCount, ref skippedFileCount);
                }
            }
        }
    }
}
