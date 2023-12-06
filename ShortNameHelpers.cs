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
    public static class ShortNameHelpers
    {
        /*
         * Get's all executable (bat, exe, com) file names from a Zip Archive Entry,
         * excluding those that clearly aren't the name of a game, e.g. install, setup, run, start, etc...
         */
        public static List<string> GetEXEFilenames(ZipArchiveEntry entry)
        {
            List<string> exeFilenames = new List<string>();

            // Open a stream to the inner zip file's contents
            using (var gameArchiveStream = entry.Open())
            {
                // Create a new ZipArchive object from the inner zip stream
                using (var gameArchiveFile = new ZipArchive(gameArchiveStream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry gameFile in gameArchiveFile.Entries)
                    {
                        // Skip folder entry
                        if (gameFile.Length == 0)
                        {
                            continue;
                        }

                        string extension = Path.GetExtension(gameFile.Name);
                        if (extension.Equals(".bat", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".com", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".exe", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".bas", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".img", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".txt", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".doc", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string exeFile = Path.GetFileNameWithoutExtension(gameFile.Name);

                            bool excluded = false;

                            for (int i = 0; i < Constants.EXCLUDED_NAMES.Count; i++)
                            {
                                if (exeFile.Contains(Constants.EXCLUDED_NAMES[i], StringComparison.CurrentCultureIgnoreCase))
                                {
                                    excluded = true;
                                    break;
                                }
                            }

                            if (!excluded)
                            {
                                // The vast majority of file names are uppercase, so to be consistant for those that aren't this is done here
                                exeFilenames.Add(exeFile.ToUpper());
                            }
                        }
                    }
                }
            }

            return exeFilenames;
        }

        /*
         * Gets a number of suggestions based on the words in the game name
         */
        public static List<string> GetGameNameWords(string gameName)
        {            
            // Game Name Words!
            // This was added after EXE files, as, sometimes these are named poorly, or named dumb things like "start" which could apply to multiple Games
            // And thus could have no matches at all
            // First, we'll start with all words in the game title, minus the meta data.
            List<string> gameNameWords = new List<string>();
            string gameNameNoMeta = TitleHelpers.GetGameNameWithoutMeta(gameName);
            gameNameWords = gameNameNoMeta.Split(' ').ToList();

            // Initials are calculated BEFORE removing any joining words
            // As it was found the initialism was more readable and usually made more sense
            string initials = getInitials(gameNameWords);

            // Remove common joining words
            gameNameWords.RemoveAll(word => Constants.LINKING_WORDS.Contains(word, StringComparer.CurrentCultureIgnoreCase));

            // Add the words together as an additional entry, after joining words removed
            // This is done because it is likely the joined words will be too long anyway, and it is less to truncate
            // Giving a greater chance of a useful title, e.g., "X-Files, The", would become X-FILES
            string joinedWords = "";
            if (gameNameWords.Count > 1)
            {
                joinedWords = String.Concat(gameNameWords);
            }

            // If there are two words, we can try adding each one together with 1 as the intial
            // More than two is likely to be > 8 chars and adds too many more  options
            // E.g. "Warp Factor, The" would have two additional suggestions, WFACTOR & WARPF (As linking word "the" would have been removed).
            string twoWordsFirst = "", twoWordsSecond = "";
            if (gameNameWords.Count == 2)
            {
                twoWordsFirst = gameNameWords[0].First() + gameNameWords[1];
                twoWordsSecond = gameNameWords[0] + gameNameWords[1].First();
            }

            // We add these to the list here and not above so they didn't interfere with each other's own results
            gameNameWords.Add(joinedWords);
            gameNameWords.Add(twoWordsFirst);
            gameNameWords.Add(twoWordsSecond);

            // Remove punctuation and truncate any of these that are longer than 8 chars            
            gameNameWords = gameNameWords.Select(w => new string(w.Where(c => !Constants.PUNCTUATION.Contains(c)).ToArray()).Trim()).ToList();
            gameNameWords = gameNameWords.Select(w => w.Substring(0, Math.Min(w.Length, 8))).ToList();

            // Intials is added here because we want to keep the "-" in this rare case
            // E.g. "Dunjonquest- Curse of Ra" is "D-COR" instead of "DCOR".
            gameNameWords.Add(initials);
            // THIS COMPLETES THE GAME NAME WORDS PART

            // Remove any empty strings
            // BUG FIX: Don't add/create them in the first place
            gameNameWords.RemoveAll(str => str == "");

            // Convert to uppercase as per DOS 8.3 naming conventions
            gameNameWords = gameNameWords.Select(word => word.ToUpper()).ToList();

            return gameNameWords;
        }

        /* Get the initials for the Game Title
         * E.g.
         * Really Long Game Title with Many Words
         * RLGTWMW
         */
        private static string getInitials(List<string> gameNameWords)
        {
            string initials = "";

            // Add the "initials" of each word as an option. Decided to leave joining words initial in as it makes more sense to keep this when abbrivated so much
            if (gameNameWords.Count > 1)
            {
                for (int i = 0; i < gameNameWords.Count; i++) // -1 so we don't get the entry added above
                {
                    string word = gameNameWords[i];
                    // Special case, we want to put the "The" back at the front. While it makes sense to have this at the end for a full title
                    // For a initial style abbrevation the context is lost.
                    // But only for the last "the", e.g. "Game Title, The"
                    if ((i == gameNameWords.Count - 1) && word.Equals("the", StringComparison.CurrentCultureIgnoreCase))
                    {
                        initials = 'T' + initials;
                    }
                    // Because roman numerals are a number we don't want to truncate this. Using contains as this is often followed by a - (in place a of ':')
                    else if (word.Contains("ii", StringComparison.CurrentCultureIgnoreCase) || word.Contains("iii", StringComparison.CurrentCultureIgnoreCase))
                    {
                        initials += word.Replace("-", "");
                    }
                    // Anything else
                    else
                    {
                        initials += word.First();
                    }

                    // This is perhaps a bit of an overkill. But if a game has a ":" in the title,
                    // which is changed to a "- " without a preceding space by TDC naming convenetions
                    // I'd like to keep this when the name is so abbreviated as there is probably space
                    if (gameNameWords[i].EndsWith('-'))
                    {
                        initials += "-";
                    }
                }
            }

            return initials;
        }

        public static List<string> RenameFileSystemDuplicates(string path, List<string> suggestions)
        {
            // Check for duplicate name/s within the file system, for each suggestion
            // And remove these as a suggestion                                
            // We are going to make the assumption, to be on the safe side,
            // The if either a zip file or a folder with the same name still exists then this needs to be checked against the proposed name
            // Hence getting either of these and removing duplicates
            // As the user may have kept the zip as well as the folder.
            List<string> entries = Directory.GetFileSystemEntries(path).ToList();
            
            // Remove file name extensions, if present
            // And select only distinct entries (as the zip file and folder may both be present
            entries = entries.Select(entry => Path.GetFileNameWithoutExtension(entry)).Distinct().ToList();

            string currentSuggestion;
            List<string> bannedNames = new List<string>(); // Keep a list of duplicates in case user acidently sets a custom name as this
            List<string> newNames = new List<string>(); // Keep a list of duplicates in case user acidently sets a custom name as this

            foreach (string suggestion in suggestions)
            {
                currentSuggestion = suggestion;

                int i = 2;
                while (entries.Any(entry => entry.Equals(currentSuggestion, StringComparison.CurrentCultureIgnoreCase)))
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

                    i = i + 1;
                }

                if (currentSuggestion != suggestion)
                {
                    bannedNames.Add(suggestion);
                    newNames.Add(currentSuggestion);
                }
            }
            return suggestions.Where(suggestion => !bannedNames.Contains(suggestion)).Concat(newNames).ToList();            
        }

        // TO DO: Combine with the above. Only difference is above compares file system while this compares other Games
        // TO DO: Need an additional check for subfolders varients.
        public static void RenameShortNameDuplicates(List<Game> Games, bool suggest)
        {
            // Suggest flag makes sure truncate doesn't search for replacements in the SuggestedNames List in the game object.

            Dictionary<string, int> nameCount = new Dictionary<string, int>();

            for (int i = 0; i < Games.Count; i++)
            {
                string originalName = Games[i].CurrentName;
                string baseName = originalName;
                int count = 0;

                int tildeIndex = originalName.LastIndexOf("~");
                if (tildeIndex >= 0)
                {
                    baseName = originalName.Substring(0, tildeIndex);
                    if (int.TryParse(originalName.Substring(tildeIndex + 1), out int parsedCount))
                    {
                        count = parsedCount;
                    }
                }

                string newName = originalName;
                while (nameCount.ContainsKey(newName))
                {
                    count++;
                    newName = $"{baseName}~{count}";

                    if (newName.Length > 8)
                    {
                        baseName = baseName.Substring(0, 7 - count.ToString().Length);
                        newName = $"{baseName}~{count}";
                    }
                }

                nameCount[newName] = 1;
                Games[i].CurrentName = newName;
                
                // This is meant to replace the renamed file in the suggestions list to prevent the user selecting a duplicate
                // As we're now adding the truncated name to the suggested names box... remove if statement
                if (suggest && Games[i].SuggestedNames.Count > 0)
                {
                    List<string> suggestionsList = new List<string>(Games[i].SuggestedNames);
                    int index = suggestionsList.FindIndex(name => name == originalName);
                    if (index != -1)
                    {
                        Games[i].SuggestedNames[index] = newName;
                    }
                }
            }

        }
    }
}
