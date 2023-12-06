using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

                    if (Path.GetExtension(name) == Constants.ZIP.ToLower())
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
         * CAUTION: Inner zip file must have been extracted already
         */
        public static void ExtractInnerZip(string innerZipPath, string currentName)
        {
            //string? path = Path.GetDirectoryName(innerZipPath);
            //if (path == null)
            //{
            //    Debug.WriteLine("Unable to get directory name from inner zip path: " + innerZipPath);

            //    return;
            //}
            //string gameFolder = currentName;

            // Extract game (inner zip) file
            //Directory.SetCurrentDirectory(path);
            //if (!Directory.Exists(innerZipPath))
            //{
            //    Directory.CreateDirectory(gameFolder);
            //}

            string gameFolder = innerZipPath + "\\" + currentName;

            try
            {
                using (ZipArchive innerArchive = ZipFile.OpenRead(gameFolder + Constants.ZIP))
                {
                    foreach (ZipArchiveEntry gameFile in innerArchive.Entries)
                    {
                        try
                        {
                            gameFile.ExtractToFile(innerZipPath + "\\" + gameFile.Name);
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
         * This is for cleanup after inner zip files have been extracted
         */
        //public static void DeleteZips(string fullPath)
        //{
        //    DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

        //    foreach (FileInfo fileInfo in dirInfo.GetFiles())
        //    {
        //        if (fileInfo.Extension == Constants.ZIP)
        //        {
        //            fileInfo.Delete();
        //        }
        //    }
        //}

        /*
         * When passed a Zip File Name (including the path and full file name)
         * This will return the full path of extraction         
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

            // Get Full path and return;
            return basePath + "\\" + yearFolder;
        }

        public static string GetTruncatedName(string gameName, string[] files)
        {            
            // Get just the game title minus metadata
            gameName = Regex.Match(gameName, Regexs.NAME_W_O_META).Value.Trim();

            // Remove unwanted punctuation / symbols
            gameName = Regex.Replace(gameName, Regexs.EXCLUDED_SYMBOLS, "");

            // Convert to upper case
            gameName = gameName.ToUpper();

            // If game name is greater than 8 chars, truncate now, otherwise, use as is
            if (gameName.Length > 8)
            {
                gameName = gameName.Substring(0, 6) + "~1";
            }

            // Check for duplicate name & increment number if found
            while (files.Any(file => file.Contains(gameName)))
            {
                Match match = Regex.Match(gameName, Regexs.TRUNCATED_NUMBER);
                string numberAsString;
                if (match.Success)
                {
                    numberAsString = match.Groups[1].Value;
                }
                else
                {
                    numberAsString = "0";
                }
                int i = Int16.Parse(numberAsString);

                if (i > 0)
                {
                    gameName = gameName.Replace("~" + i, "~" + (i + 1));
                }
                else
                {
                    gameName = gameName.Substring(0, Math.Min(6, gameName.Length)) + "~1";
                }
            }

            return gameName;
        }

        // This is simliar to the GetTruncatedName (game name), only it's for varient meta data sub-folders.
        // TO DO: Merge with the above?
        public static string GetTruncatedFolder(string metaFolder, string[] files)
        {
            metaFolder = Regex.Replace(metaFolder, Regexs.SHORT_META, "").ToUpper();

            // If game name is greater than 8 chars, truncate now, otherwise, use as is
            if (metaFolder.Length > 8)
            {
                metaFolder = metaFolder.Substring(0, 6) + "~1";
            }

            // Check for duplicate name & increment number if found
            while (files.Any(file => file.Contains(metaFolder)))
            {
                Match match = Regex.Match(metaFolder, Regexs.TRUNCATED_NUMBER);
                string numberAsString;
                if (match.Success)
                {
                    numberAsString = match.Groups[1].Value;
                }
                else
                {
                    numberAsString = "0";
                }
                int i = Int16.Parse(numberAsString);

                if (i > 0)
                {
                    metaFolder = metaFolder.Replace("~" + i, "~" + (i + 1));
                }
                else
                {
                    metaFolder = metaFolder.Substring(0, Math.Min(6, metaFolder.Length)) + "~1";
                }
            }

            return metaFolder + "\\";
        }

        public static string GetInnerZipPath(string basePathW_Year, Game game, bool groupSubFolders, bool shortName, bool alphabetSubFolders)
        {
            // Start with base dir, minus game name and add as required.
            string innerZipPath = basePathW_Year + "\\";
            string alphabet = "";
            //string publisher = "";
            string gameFolder = game.CurrentName + "\\";
            string metaFolder = "";

            if (alphabetSubFolders)
            {
                char alphaNumChar = game.CurrentName.First();
                if (Char.IsDigit(alphaNumChar))
                {
                    alphaNumChar = '0';
                }

                alphabet = alphaNumChar.ToString() + "\\";

                innerZipPath += alphabet;
            }
            // Commenting out for this release
            // As it really needs a lot of code if the user wants to have 8.3 name formats
            //else if (PublisherFolderCheckBox.IsChecked == true)
            //{
            //    // TO DO: Short name version
            //    publisher = TitleHelpers.GetPublisher(game, Path.GetFileNameWithoutExtension(ZipFileName)) + "\\";

            //    innerZipPath += publisher;
            //}

            // TO DO: Remove undo duplicate check somehow?
            // E.g. Same game (version, year, publisher) but different varient.
            // Work around for now: User can manually delete ~ if not required.
            if (groupSubFolders)
            {
                if (!shortName)
                {
                    gameFolder = TitleHelpers.GetGameNameWOVarients(game.FullName) + "\\";
                    innerZipPath += gameFolder;

                    metaFolder = game.VarientMeta + "\\";
                }
                else
                {
                    innerZipPath += gameFolder;

                    // Create the path so far
                    Directory.CreateDirectory(innerZipPath);
                    string[] entries = Directory.GetFileSystemEntries(innerZipPath).Distinct().ToArray();

                    metaFolder = FileHelpers.GetTruncatedFolder(game.VarientMeta, entries);
                }
            }
            else
            {
                innerZipPath += gameFolder;
            }
            
            return innerZipPath += metaFolder;
        }
    }
}
