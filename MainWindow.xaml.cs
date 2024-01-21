using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Data;
using System.Windows.Threading;
using System.Security.Policy;
//using System.Windows.Data;
//using System.Windows.Navigation;

namespace TDC_Extractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Game> Games;
        public string ZipFileName;
        public string Year;

        DispatcherTimer timer;
        int tick;

        public MainWindow()
        {
            InitializeComponent();

            Title = "TDC Extractor v0.6";
            
            Games = new ObservableCollection<Game>();
            this.DataContext = Games;

            ZipFileName = "";
            Year = "1980";

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += Timer_Tick;
            tick = 0;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            tick++;

            if (tick > 1)
            {
                tick = 0;
                timer.Stop();

                handleUpdate();
            }                
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "1981"; // Default file name
            dialog.DefaultExt = Constants.ZIP; // Default file extension
            dialog.Filter = "Zip files|*.zip"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document                
                ZipFileName = dialog.FileName;
                // TO DO: Do this better. E.g, check the folder inside the zip.
                Year = Path.GetFileNameWithoutExtension(ZipFileName);
                ZipFileNameTextBlock.Text = ZipFileName;

                loadZip();
            }
        }

        private void loadZip()
        {
            // Clear game files as opening a new year
            Games.Clear();
            
            ShortNameCheckBox.IsChecked = false;

            string year = Path.GetFileNameWithoutExtension(ZipFileName);

            List<string> gameFilenames = FileHelpers.LoadZip(ZipFileName);
            for (int i = 0; i < gameFilenames.Count; i++)
            {
                Games.Add(new Game(i, gameFilenames[i], year));
            }

            // Reset switches to the default, if 2nd zip file is opened
            // TO DO: Too much messing around with code for now
            // Just apply the existing switches

            handleUpdate();
        }

        private void updateCount_List()
        {
            SelectedTextBlock.Text = Games.Where(game => game.Selected == true).Count().ToString() + " of " + Games.Count();
        }

        /*
         * This whole method is still a bit of a monster
         * Which should just really be handling things that  inferface with the UI
         */
        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            string basePath = FileHelpers.GetBasePath(ZipFileName) + "\\";
            Directory.SetCurrentDirectory(basePath);

            // Open the yearly zip file
            using (ZipArchive archive = ZipFile.OpenRead(ZipFileName))
            {
                int currentIndex = 0;
                int total = archive.Entries.Count;

                // Process each child zip game
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Skip folder entry
                    if (entry.Length == 0)
                    {
                        currentIndex++;
                        continue;
                    }

                    Game game = Games.Where(game => game.Index == currentIndex - 1).Single();

                    if (game.Selected)
                    {                    
                        // Extract the Inner Zip file (A ZIP of a single game)
                        // Using the naming chosen by the user
                        try
                        {
                            // Create the game name folder if it doesn't already exist
                            if (!Directory.Exists(basePath + game.ExtractPath))
                            {
                                Directory.CreateDirectory(basePath + game.ExtractPath);
                            }

                            // Extract the game zip to this folder
                            Directory.SetCurrentDirectory(basePath + game.ExtractPath);
                            entry.ExtractToFile(game.CurrentName + Constants.ZIP);

                            // Extract the zip contents to this folder
                            FileHelpers.ExtractInnerZip(basePath + game.ExtractPath, game.CurrentName + Constants.ZIP);


                            if (DeleteZipsCheckBox.IsChecked == true)
                            {
                                File.Delete(game.CurrentName + Constants.ZIP);
                            }
                        }
                        catch (Exception e2)
                        {
                            Debug.WriteLine(e2.Message);
                        }
                    }
                    else
                    {
                        Debug.WriteLine(game.FullName + " not selected.");
                    }

                    currentIndex++;
                }
            }
        }

        private void handleUpdate()
        {
            //Start with everything
            //By doing this, we don't have to worry about logic to "remove" items from the selected game names, instead we start with a fresh slate like a database
            foreach (Game game in Games)
            {
                game.Selected = true;
            }

            // This deselected Disk Images
            // TO DO: ISOs?
            if (ImagesCheckBox.IsChecked == false)
            {                
                Games.ToList().ForEach(game => { if (game.FullName.EndsWith(".img", StringComparison.CurrentCultureIgnoreCase)) game.Selected = false; });
            }

            //***TAGS***
            // Tags will be considered an additonal filter, so when checked, will be included, when unchecked, will not be included
            if (DCCheckBox.IsChecked == false)
            {                
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.DC)) game.Selected = false; });
            }
            if (HnCheckBox.IsChecked == false)
            {               
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.HN)) game.Selected = false; });
            }
            if (AnCheckBox.IsChecked == false)
            {             
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.AN)) game.Selected = false; });
            }
            if (FnCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.FN)) game.Selected = false; });
            }
            if (OnCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.ON)) game.Selected = false; });
            }
            if (BnCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.BN)) game.Selected = false; });
            }
            if (SWCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.SW)) game.Selected = false; });
            }
            if (SWCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.SWR_FW)) game.Selected = false; });
            }
            if (GoodCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.GOOD)) game.Selected = false; });
            }
            if (MostlyGoodCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.MOSTLY_GOOD)) game.Selected = false; });
            }

            // Some of these are considered tags and some flags according to TDC docs. However, it might make more sense to group these
            if (TrCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.TRANSLATED) && !game.FullName.Contains("[tr En]")) game.Selected = false; });
            }
            if (TrXXCheckBox.IsChecked == true) // EXCLUSIVE INCLUDE
            {
                Games.ToList().ForEach(game => { if (!game.FullName.Contains("[tr XX]".Replace("XX", OtherTrLanguageTextBox.Text))) game.Selected = false; });
            }

            //***FLAGS***
            // Flags are considering an inclusive filter
            if (InstallerCheckBox.IsChecked == false)
            {                
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.INSTALLER)) game.Selected = false; });
            }
            if (DemoCheckBox.IsChecked == false)
            {             
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.DEMO)) game.Selected = false; });
            }
            // Graphics Modes
            if (PCjrCheckBox.IsChecked == false)
            {             
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.PCJR)) game.Selected = false; });
            }
            if (CGACheckBox.IsChecked == false)
            {             
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.CGA)) game.Selected = false; });
            }
            if (TandyCheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.TANDY)) game.Selected = false; });
            }
            if (EGACheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.EGA)) game.Selected = false; });
            }
            if (VGACheckBox.IsChecked == false)
            {
                Games.ToList().ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.VGA)) game.Selected = false; });
            }

            // Custom Flag
            if (CustomFlagCheckBox.IsChecked == true)
            {                
                Games.ToList().ForEach(game => { if (!Regex.IsMatch(game.FullName, Regexs.CUSTOM_FLAG.Replace("custom", CustomFlagTextBox.Text))) game.Selected = false; });
            }

            // Languages - as per above comment on translated Games
            if (NonEnglishCheckBox.IsChecked == false)
            {                
                Games.ToList().ForEach(game => { if (Constants.NON_ENGLISH.Any(language => game.FullName.Contains("(" + language + ")")) && !game.FullName.Contains("(En)")) game.Selected = false; });
            }
            if (OtherLanguageCheckBox.IsChecked == true)
            {             
                Games.ToList().ForEach(game => { if (!game.FullName.Contains("(" + OtherLanguageTextBox.Text + ")")) game.Selected = false; });
            }
                        
            if (PublisherCheckBox.IsChecked == true)
            {            
                Games.ToList().ForEach(game => { if (!Regex.IsMatch(game.FullName, Regexs.PUBLISHER.Replace("publisher", PublisherTextBox.Text))) game.Selected = false; });
            }            
            if (GenreCheckBox.IsChecked == true)
            {             
                Games.ToList().ForEach(game => { if (!Regex.IsMatch(game.FullName, Regexs.GENRE.Replace("genre", GenreTextBox.Text))) game.Selected = false; });
            }

            //if (FreeTextCheckBox.IsChecked == true)
            if (SearchTextBox.Text != "Search" && SearchTextBox.Text.Length > 2)
            {                
                Games.ToList().ForEach(game => { if (!game.FullName.Contains(SearchTextBox.Text, StringComparison.CurrentCultureIgnoreCase)) game.Selected = false; });
            }

            //***SPECIAL CASES***
            // Version needs special logic due to comparison between version numbers and v1.0 / r1 being omitted
            if (VersionCheckBox.IsChecked == true)
            {
                string regex = Regexs.VERSION;

                if (SpecifiedRadioButton.IsChecked == true)
                {
                    string versionNumber = VersionTextBox.Text.Trim();

                    // Three cases:
                    // Version 1 / release 1 only - Blank should be v1[.0] / r1/01. However, user may type this into the above field
                    if (versionNumber == "v1.0" || versionNumber == "v1" || versionNumber == "r1" || versionNumber == "r01" || versionNumber == "")
                    {
                        // Remove everything with a version number, as v1.0 / r1 is NOT specified in the zip name by design                                                
                        Games.ToList().ForEach(game => { if (!Regex.IsMatch(game.FullName, regex)) game.Selected = false; });
                    }
                    else if (Regex.IsMatch(versionNumber, regex)) // Specified version only, number format is correct
                    {
                        // Remove everything EXCEPT the matching version or release no                                                
                        Games.ToList().ForEach(game => { if (game.FullName.Contains(versionNumber)) game.Selected = false; });
                    }
                    else // Invalid input, do nothing
                    {
                        return;
                    }

                }
                else if (HighestRadioButton.IsChecked == true)
                {
                    var groupedGames = Games.GroupBy(game => TitleHelpers.GetLikeVersionsTitleOnly(game.FullName, regex));

                    foreach (var group in groupedGames)
                    {
                        var highestVersionGame = group
                            .OrderByDescending(game => TitleHelpers.GetVersion(game.FullName, regex))
                            .First();

                        //highestVersionGame.Selected = true;
                        foreach (var game in group)
                        {
                            if (game != highestVersionGame)
                            {
                                game.Selected = false;
                            }
                        }
                    }
                }
            }

            // Manually selected/de-selected Games override the above filters
            foreach (var game in Games)
            {
                if (game.Manual != null)
                {
                    game.Selected = (bool)game.Manual;
                }
            }

            updateCount_List();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            handleUpdate();
        }

        private void ShortNameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = ShortNameCheckBox;

            if (TruncateRadioButton.IsChecked == true)
            {                    
                SuggestedNamesColumn.Visibility = Visibility.Collapsed;
                                        
                handleTruncateGameName();

                updateCount_List();
            }
            else if (SuggestRadioButton.IsChecked == true)
            {
                handleSuggestedGameNames();
                                        
                SuggestedNamesColumn.Visibility = Visibility.Visible;

                updateCount_List();
            }             
                        
            if (((FrameworkElement)sender).Name == "ShortNameCheckBox" && (checkBox.IsChecked != true)) // This is not needed if just switching between above radio buttons
            {
                // Revert to full name
                foreach (Game game in Games)
                {
                    game.ShortName = false;
                    //game.CurrentName = game.FullName;
                }

                SuggestedNamesColumn.Visibility = Visibility.Collapsed;                
            }
            // else do nothing
        }

        private void handleTruncateGameName()
        {
            // Get a list of files (in case zips weren't deleted) or folders already in the dir just in case there is a clash
            string basePath = FileHelpers.GetBasePath(ZipFileName);

            // Get the truncated names
            for (int i = 0; i < Games.Count; i++)
            {
                Game game = Games[i];

                game.ShortName = true;
                game.TruncatedName = ShortNameHelpers.GetTruncatedName(game.FullName);

                //game.CurrentName = game.TruncatedName; //updates extract path
                //game.SetExtractPath(game.TruncatedName);
                //string extractPath = FileHelpers.GetInnerZipPath("TEMP", game);
                game.TruncatedName = FileHelpers.CheckTruncatedNameForFileSystemDuplicates(basePath, game.ExtractPath, game.CurrentName, game.TruncatedName);
                //TESTING
                //game.CurrentName = game.TruncatedName;

                //game.CurrentName = "TEMP"; // This saves passing another variable although it seems a bit hacky...
                List<string> truncatedNameInList = new List<string>() { game.TruncatedName };
                List<Game> gamesAsList = Games.ToList();
                // This has to go after File System check, as the file system check itself can cause duplicates
                // KNOWN BUG: If there is a gap in the tilda numbers, this could cause another file system clash
                game.CurrentName = ShortNameHelpers.CheckShortNamesForDuplicates(game.CurrentName, game.ExtractPath, truncatedNameInList, gamesAsList).First();
            }

            //ShortNameHelpers.RenameShortNameDuplicates(Games.ToList(), false);
        }

        private void handleSuggestedGameNames()
        {
            string basePath = FileHelpers.GetBasePath(ZipFileName);
            // Add game varient to full path to prevent renaming of suggestion when it is contained with a game varient sub-folder
            // This takes into account suggestions where there is a different version of a game with the same name
            // VS a varient of a game which the user has chosen to be in a sub-folder


            // Open the yearly zip file
            using (ZipArchive archive = ZipFile.OpenRead(ZipFileName))
            {
                int count = archive.Entries.Count;

                // Process each child zip game
                // Start at 1 to skip folder entry
                for (int i = 1; i < count; i++)
                {
                    var entry = archive.Entries[i];
                    // Get matching game entry for archive entry. -1 as Games has already had folder entry at index 0 removed, whereas zip will have it
                    Game game = Games.Where(game => game.Index == i - 1).Single();

                    game.ShortName = true;

                    // Suggestions, are a combination of executable file names, words from the game title, usually abbreviated, joined or concatenated in a number of ways
                    List<string> suggestions = ShortNameHelpers.GetSuggestions(entry, game.FullName);

                    //Move and/or rename duplicates that already exist within file system
                    suggestions = FileHelpers.CheckSuggestionsForFileSystemDuplicates(basePath, Year, game.ExtractPath, game.CurrentName, suggestions);

                    //game.CurrentName = "TEMP";

                    List<Game> gamesAsList = Games.ToList();
                    ShortNameHelpers.CheckShortNamesForDuplicates(game.CurrentName, game.ExtractPath, suggestions, gamesAsList);

                    // Add suggestions to game object
                    game.SuggestedNames = new ObservableCollection<string>(suggestions);

                    game.CurrentName = suggestions.First();

                }
            }

            //ShortNameHelpers.RenameShortNameDuplicates(Games.ToList(), true);                        
        }

        private void SelectedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null) { return; }

            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.IsFocused)
            {
                Game game = (Game)checkBox.DataContext;

                game.Selected = checkBox.IsChecked == true ? true : false;
                game.Manual = game.Selected;
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                timer.Start();

                return;
            }
            else
            {
                timer.Stop();
                timer.Start();

                return;
            }
        }

        private void TruncateOrSuggestRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ShortNameCheckBox.IsChecked == true)
            {
                ShortNameCheckBox_Click(sender, e);
            }
        }

        //private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var textBox = sender as TextBox;
        //    if (textBox != null)
        //    {
        //        string text = textBox.Text;
        //        if (text.Length == 0)
        //        {
        //            return;
        //        }

        //        Game game = (Game)textBox.DataContext;
        //        if (game == null) { return; }

        //        if (text != game.CurrentName)
        //        {
        //            game.Alphabet = AlphabetCheckBox.IsChecked == true;
        //            game.Group = GroupCheckBox.IsChecked == true;
        //            game.ShortName = ShortNameCheckBox.IsChecked == true;
        //            game.SetExtractPath();
        //        }
        //    }        
        //}

        private void AlphabetCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            handleCheckedOrUnchecked(sender, e);
        }

        private void AlphabetCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            handleCheckedOrUnchecked(sender, e);
        }

        private void handleCheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            bool alphabet = AlphabetCheckBox.IsChecked == true;

            foreach (Game game in Games)
            {
                game.Alphabet = alphabet;
                //game.SetExtractPath();
            }
        }

        private void GroupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            handleGroupCheckedOrUnchecked(sender, e);
        }

        private void GroupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            handleGroupCheckedOrUnchecked(sender, e);
        }

        private void handleGroupCheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            bool group = GroupCheckBox.IsChecked == true;

            foreach (Game game in Games)
            {
                game.Group = group;

                //game.SetExtractPath();
            }
        }
    }
}
