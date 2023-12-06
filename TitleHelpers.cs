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
    public static class TitleHelpers
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
            else if (Char.IsLetter(versionString.Last())) // Converts a trailing letter to a build no for comparison purposes
            {
                int buildNo = Char.ToUpper(versionString.Last()) - 64;

                versionString = versionString.Replace(versionString.Last().ToString(), "." + buildNo.ToString());
            }
            // TO DO: There are a couple of other version types that are slipping through, e.g.
            // v1.0002.0001
            // v1.2g

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

        // Get the Game Varient Metadata, e.g. alternate 3, hack 1, fix 2, etc
        public static string GetVarientMeta(string gameString)
        {
            string varientMeta;

            // Match all fields enclosed in parentheses or square brackets, version information, or optional [!] or [.]
            MatchCollection matchCollection = Regex.Matches(gameString, Regexs.VARIENT_META);

            // Convert MatchCollection to List
            List<string> matches = new List<string>();
            foreach (Match match in matchCollection)
            {
                for (int i = 1; i < match.Groups.Count; i++) // Start from 1 to skip the full match
                {
                    // Add non-empty matches
                    if (!string.IsNullOrEmpty(match.Groups[i].Value))
                    {
                        matches.Add(match.Groups[i].Value);
                    }
                }
            }

            // Post-processing: drop the last three matches which should be the year, publisher and genre
            // Also account for optional [!] or [.]
            if (matches[matches.Count - 1] == "[.]" || matches[matches.Count - 1] == "[!]")
            {
                matches.RemoveRange(matches.Count - 4, 4);
            }
            else
            {
                matches.RemoveRange(matches.Count - 3, 3);
            }

            StringBuilder builder = new StringBuilder();
            foreach (string match in matches)
            {
                builder.Append(match.Trim());
            }
            varientMeta = builder.ToString();

            if (varientMeta.Length <= 0)
            {
                varientMeta = "v1.0";
            }

            return varientMeta;
        }

        // Get Game Name without Varients
        public static string GetGameNameWOVarients(string gameString)
        {
            string gameNameWOVarients = "";

            int yearIndex = Regex.Match(gameString, Regexs.YEAR).Index;
            string yearPublisherGenre = gameString.Substring(yearIndex);
            yearPublisherGenre = yearPublisherGenre.Replace("[!]", "").Replace("[.]", "").TrimEnd();

            gameNameWOVarients = GetGameNameWithoutMeta(gameString) + " " + yearPublisherGenre;

            return gameNameWOVarients;
        }


        // Get Company from Game Name
        //public static string GetPublisher(Game game, string year)
        //{
        //    string gameName = game.FullName;
        //    string yearW_Brackets = "(" + year + ")";

        //    string publisher;

        //    int startIndex = gameName.IndexOf(yearW_Brackets) + yearW_Brackets.Length + 1;
        //    int endIndex = gameName.IndexOf(")", startIndex);

        //    publisher = gameName.Substring(startIndex, endIndex - startIndex);

        //    return publisher;
        //}

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
    }
}
