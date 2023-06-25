using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDC_Extractor
{
    public static class FileHelpers
    {
        /*
         * Loads a specified Zip File and returns a List of game names
         * The Game Name is the zip archive file name minus the extension
         */
        public static List<string> LoadZip(string zipFileName)
        {
            List<string> gameFilenames = new List<string>();            

            using (ZipArchive archive = ZipFile.OpenRead(zipFileName))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string name = entry.Name;

                    if (Path.GetExtension(name) == ".zip")
                    {
                        //gameNamesAll.Add(Path.GetFileNameWithoutExtension(name));
                        gameFilenames.Add(name);
                    }
                    else
                    {
                        Debug.WriteLine("Unexpected file type: " + name);
                    }
                }
            }

            return gameFilenames;
        }

        /*
         * Extracts the Inner zip file to the given path (Game Name)
         */
        public static void ExtractInnerZip(string fullPath, string gameName)
        {
            // Extract game (inner zip) file
            Directory.SetCurrentDirectory(fullPath);
            if (!Directory.Exists(fullPath + "\\" + gameName))
            {
                Directory.CreateDirectory(gameName);
            }

            try
            {
                using (ZipArchive innerArchive = ZipFile.OpenRead(gameName + ".ZIP"))
                {
                    foreach (ZipArchiveEntry gameFile in innerArchive.Entries)
                    {
                        try
                        {
                            gameFile.ExtractToFile(fullPath + "\\" + gameName + "\\" + gameFile.Name);
                        }
                        catch (Exception e3)
                        {
                            Debug.WriteLine(e3.Message);
                        }
                    }
                }
            }
            catch (Exception e4)
            {
                Debug.WriteLine(e4.Message);
            }
        }

        /*
         * Deletes all zip files in the given directory
         */
        public static void DeleteZips(string fullPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                if (fileInfo.Extension == ".ZIP")
                {
                    fileInfo.Delete();
                }
            }
        }

        /*
         * When passed a Zip File Name (including the path and full file name)
         * This will return the full path of extraction
         * Rename this to something a little less confusing?!!
         */
        public static string GetFullPath(string zipFileName)
        {
            // Get base path. This is will be the same folder that the Yearly Game zip is  in, or TEMP if that fails.
            string? basePath = Path.GetDirectoryName(zipFileName);
            if (basePath == null)
            {
                basePath = "%TEMP%";
            }

            // Create Year sub-dir if it doesn't exist already
            string yearFolder = Path.GetFileNameWithoutExtension(zipFileName);
            Directory.SetCurrentDirectory(basePath);
            if (!Directory.Exists(basePath + "\\" + yearFolder))
            {
                Directory.CreateDirectory(yearFolder);
            }

            return basePath + "\\" + yearFolder;
        }
    }
}
