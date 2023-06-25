using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace TDC_Extractor
{
    public static class Helpers
    {
        /*
         * Gets a Version object from a Game Title
         * Will also convert "r1, r2, etc..." format to Version for comparison purposes
         */
        public static Version GetVersion(string title, string regex)
        {
            // Get the version string from the game title
            string versionString = Regex.Match(title, regex).Value;
            if (versionString.Length == 0) // This is v1.0 or r1 as version is implied in these cases
            {
                versionString = "v1.0";
            }
            else if (versionString.StartsWith('v') && !versionString.Contains('.')) // Some versions do not have a trailing ".0"
            {
                versionString += ".0";
            }
            else if (versionString.StartsWith('r')) // Convert "Release" to Version format, as Version is more common / complicated it is easier to do it in this direction
            {
                versionString = versionString.Replace('r', 'v') + ".0";
            }
            else if (Char.IsLetter(versionString.Last()))
            {
                int buildNo = Char.ToUpper(versionString.Last()) - 64;

                versionString = versionString.Replace(versionString.Last().ToString(), "." + buildNo.ToString());
            }

            Version version;

            try
            {
                version = new Version(versionString.Substring(1));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                version = new Version(1, 0);
            }

            return version;
        }

        // Get the Game Name/Title without the Metadata
        public static string GetGameNameWithoutMeta(string gameName)
        {
            string gameNameNoMeta;

            Match match = Regex.Match(gameName, Regexs.NAME_W_O_META);
            if (match.Success)
            {
                gameNameNoMeta = match.Value.Trim();
            }
            else
            {
                // Is this case, there may be some crap in the suggestions, but this should only happen for malformed Game Titles
                gameNameNoMeta = gameName;
            }

            return gameNameNoMeta;
        }

        /*
         * Gets a Game Title without the version
         * This is used when comparing versions
         * So in this case, we do leave a couple of meta data's which we will consider the same "version"
         */
        public static string GetLikeVersionsTitleOnly(string title, string regex)
        {
            // Get the title only without the version, year, etc...
            string titleOnly;

            // Account for white space, just for this check. Otherwise we end up with a double space in the returned title
            regex = @"\s" + regex;

            if (Regex.IsMatch(title, regex))
            {
                //titleOnly = title.Substring(0, Regex.Match(title, regex).Index - 1);
                titleOnly = title.Replace(Regex.Match(title, regex).ToString(), "");
            }
            else // No version info, assume v1.0
            {
                titleOnly = title;
            }

            // Because known [mostly] good dump doesn't change the actual content of the game, isn't used that often, we won't count this as it a unique "version" (unlike say a hack, or translation).
            titleOnly = titleOnly.Replace("[!]", "").Replace("[.]", "");

            return titleOnly;
        }

        public static string GetTruncatedName(string gameName, string[] files)
        {
            // TO DO: Full 8.3 wierd characters support.
            // Get just the game title minus metadata
            // TO DO: Add check for no match
            gameName = Regex.Match(gameName, Regexs.NAME_W_O_META).Value.Trim();
            // Replace spaces with underscore's
            //gameName = gameName.Replace(" ", "_");
            // Remove spacees
            //gameName = gameName.Replace(" ", "");
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
            // TO DO: Optimise
            // BUG: If more than 1 match
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
    }
}
