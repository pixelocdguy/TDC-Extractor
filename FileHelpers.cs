using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

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

                    if (System.IO.Path.GetExtension(name) == Constants.ZIP.ToLower())
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
        public static void ExtractInnerZip(string gameFolder, string gameZip)
        {
            // The zip file is the same name as the folder + zip extension
            //string gameZip = new DirectoryInfo(gameFolder).Name + Constants.ZIP;

            try
            {
                using (ZipArchive innerArchive = ZipFile.OpenRead(gameFolder + gameZip))
                {
                    foreach (ZipArchiveEntry gameFile in innerArchive.Entries)
                    {
                        try
                        {
                            gameFile.ExtractToFile(gameFolder + "\\" + gameFile.Name);
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
         * When passed a Zip File Name (including the path and full file name)
         * This will return the full path of extraction         
         */
        public static string GetBasePath(string zipFileName)
        {
            // Get base path. This is will be the same folder that the Yearly Game zip is  in, or TEMP if that fails.
            string basePath = System.IO.Path.GetDirectoryName(zipFileName) ?? "%TEMP%";

            return basePath;
        }

        /*
         * NOTE: Directory current path MUST be set to base path for this to work correctly
         */
        public static string GetInnerZipPath(string gameFolder, Game game)
        {
            // Start with base dir, minus game name and add as required.
            string innerZipPath = game.Year + "\\";

            string alphabet = "";
            //string publisher = "";
            //string gameFolder = game.CurrentName + "\\";
            gameFolder += "\\";
            string metaFolder = "";

            if (game.Alphabet)
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

            if (game.Group)
            {
                innerZipPath += gameFolder;

                if (!game.ShortName)
                {
                    //if (game.FullName == game.CurrentName)
                    //{
                    //    gameFolder = TitleHelpers.GetGameNameWOVarients(game.FullName) + "\\";

                    //    innerZipPath += gameFolder;
                    //}
                    //else
                    //{
                    //    innerZipPath += gameFolder;
                    //}                    

                    metaFolder = game.VarientMeta + "\\";
                }
                // TO DO: Truncated?
                else
                {
                    //innerZipPath += gameFolder;

                    string[] entries;
                    if (Directory.Exists(innerZipPath))
                    {
                        entries = Directory.GetFileSystemEntries(innerZipPath).Distinct().ToArray();
                    }
                    else
                    {
                        entries = new string[0];
                    }

                    metaFolder = GetTruncatedMetaFolder(game.VarientMeta, entries);
                }

                innerZipPath += metaFolder;
            }
            else
            {
                innerZipPath += gameFolder;
            }

            return innerZipPath;
        }

        // This is simliar to the GetTruncatedName (game name), only it's for varient meta data sub-folders.        
        public static string GetTruncatedMetaFolder(string metaFolder, string[] files)
        {
            metaFolder = Regex.Replace(metaFolder, Regexs.SHORT_META, "").ToUpper();

            // If game name is greater than 8 chars, truncate now, otherwise, use as is
            if (metaFolder.Length > 8)
            {
                metaFolder = metaFolder.Substring(0, 6) + "~1";
            }

            // This should be pretty rare, unless the same game is extracted several times with the same base path while grouped.
            if (files.Length > 0)
            {
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
            }

            return metaFolder + "\\";
        }

        public static string CheckTruncatedNameForFileSystemDuplicates(string basePath, string extractPath, string currentName, string truncatedName)
        {
            // Check for duplicate file system name & increment number if found
            // This should include the full path incase alphabet or grouping is used!
            //If the yearly extract folder doesn't exist, then there are no file system clashes.
            string fullPath = basePath + "\\" + extractPath.Replace(currentName, truncatedName);
            string checkedTruncatedName = truncatedName;

            string[] entries;            
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo directoryInfo = Directory.GetParent(fullPath) ?? new DirectoryInfo(fullPath);
                DirectoryInfo parentDirectoryInfo = directoryInfo.Parent ?? directoryInfo;
                string parentFolderPath = parentDirectoryInfo.FullName;

                entries = Directory.GetFileSystemEntries(parentFolderPath).Distinct().ToArray();                
            }
            else
            {
                entries = new string[0];
            }
                        
            if (entries.Length > 0)
            {                
                while (entries.Any(file => file.Contains(checkedTruncatedName)))
                {
                    Match match = Regex.Match(checkedTruncatedName, Regexs.TRUNCATED_NUMBER);
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
                        checkedTruncatedName = checkedTruncatedName.Replace("~" + i, "~" + (i + 1));
                    }
                    else
                    {
                        checkedTruncatedName = checkedTruncatedName.Substring(0, Math.Min(6, checkedTruncatedName.Length)) + "~1";
                    }
                }
            }

            return checkedTruncatedName;
        }

        // Loops through JUST the Suggestions List for EACH Game
        public static List<string> CheckSuggestionsForFileSystemDuplicates(string basePath, string year, string extractPath, string currentName, List<string> suggestions)
        {
            // Check for duplicate name/s within the file system, for each suggestion
            // And remove these as a suggestion                                
            // We are going to make the assumption, to be on the safe side,
            // The if either a zip file or a folder with the same name still exists then this needs to be checked against the proposed name
            // Hence getting either of these and removing duplicates
            // As the user may have kept the zip as well as the folder.

            // If the yearly extract folder doesn't exist, there are no file system duplciates
            if (!Directory.Exists(basePath + "\\" + year + "\\"))
            {
                return suggestions;
            }

            string currentSuggestion;
            List<string> bannedNames = new List<string>(); // Keep a list of duplicates in case user acidently sets a custom name as this
            List<string> newNames = new List<string>(); // Keep a list of duplicates in case user acidently sets a custom name as this

            foreach (string suggestion in suggestions)
            {
                currentSuggestion = suggestion;

                // If the full path of the current suggestion exists, we have to ban it
                string fullPath = basePath + "\\" + extractPath.Replace(currentName, currentSuggestion);

                int i = 2;
                while (Directory.Exists(fullPath))
                {
                    int iLength = i.ToString().Length + 1; // +1 for ~
                                                           // Truncate name for digit/s, exluding the current digit if it exists...
                    string suggestionWithoutNumber = currentSuggestion.Replace("~" + (i - 1), "");
                    if (suggestionWithoutNumber.Length + iLength > 8)
                    {
                        currentSuggestion = currentSuggestion.Substring(0, currentSuggestion.Length - (currentSuggestion.Length + iLength - 8));
                    }
                    // Remove preivous number (if any)
                    currentSuggestion = currentSuggestion.Replace("~" + (i - 1).ToString(), "");

                    currentSuggestion = currentSuggestion + "~" + i.ToString();

                    fullPath = basePath + "\\" + extractPath.Replace(currentName, currentSuggestion);

                    i = i + 1;
                }

                if (currentSuggestion != suggestion)
                {
                    bannedNames.Add(suggestion);
                    newNames.Add(currentSuggestion);
                }
            }
            return suggestions.Where(suggestion => !bannedNames.Contains(suggestion)).Concat(newNames).Distinct().ToList();

        }
    }
}
