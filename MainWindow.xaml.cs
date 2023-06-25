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

namespace TDC_Extractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public List<string> GameNamesAll;
        //private List<string> gameFilenames;
        private List<Game> Games;
        //public List<Game> GameNamesSelected;
        //public List<List<string>> GameNamesShortNames;
        //public List<Game> GameNamesExcluded;        
        //public List<(bool Flag, string GameName)> GameNamesManual;

        public string ZipFileName;

        // TO DO: This is a bit of a work around. Would be better if this was handled in the drop method somehow.
        ListBox? sourceListBox;

        public MainWindow()
        {
            InitializeComponent();

            //GameNamesAll = new List<string>();
            //GameNamesSelected = new List<string>();
            //GameNamesExcluded = new List<string>();            
            //GameNamesManual = new List<(bool, string)>();

            Games = new List<Game>();

            ZipFileName = "";
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "1981"; // Default file name
            dialog.DefaultExt = ".zip"; // Default file extension
            dialog.Filter = "Zip files|*.zip"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document                
                ZipFileName = dialog.FileName;
                ZipFileNameTextBlock.Text = ZipFileName;

                loadZip();
            }
        }

        private void loadZip()
        {
            // Clear game files as opening a new year
            //GameNamesAll.Clear();
            //GameNamesSelected.Clear();   
            //GameNamesExcluded.Clear();
            //GameNamesManual.Clear();
            Games.Clear();
            
            ShortNameCheckBox.IsChecked = false;

            List<string> gameFilenames = FileHelpers.LoadZip(ZipFileName);
            for (int i = 0; i < gameFilenames.Count; i++)
            {
                Games.Add(new Game(i, gameFilenames[i]));
            }

            // Initially select all games
            //GameNamesSelected.AddRange(GameNamesAll);
            //GameNamesSelected.Sort();

            // Reset switches to the default, if 2nd zip file is opened
            // TO DO: Too much messing around with code for now
            // Alternative, just apply the existing switches
            handleUpdate();
        }

        private void updateCount_List()
        {
            GameZipFilesListBox.ItemsSource = null;
            GameZipFilesListBox.ItemsSource = Games.Where(game => game.Selected == true);
            NumberTextBlock.Text = GameZipFilesListBox.Items.Count.ToString();

            ExcludedZipFilesListBox.ItemsSource = null;
            ExcludedZipFilesListBox.ItemsSource = Games.Where(game => game.Selected != true);
            ExcludedNumberTextBlock.Text = ExcludedZipFilesListBox.Items.Count.ToString();
        }

        /*
         * This whole method is still a bit of a monster
         * TO DO: If the Suggestion part is reworked, can move a lot of this logic out of the Main Window class
         * Which should just really be handling things that  inferface with the UI
         */
        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            string fullPath = FileHelpers.GetFullPath(ZipFileName);

            // Open the yearly zip file
            using (ZipArchive archive = ZipFile.OpenRead(ZipFileName))
            {
                int currentIndex = 0;
                int total = archive.Entries.Count;

                // Re-check for duplicates. If user has manually altered any entry, and it clashes, rename here
                if (SuggestRadioButton.IsChecked == true)
                {
                    ShortNameHelpers.RenameShortNameDuplicates(Games, true);
                }                

                // Process each child zip game
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Skip folder entry
                    if (entry.Length == 0)
                    {
                        currentIndex++;
                        continue;
                    }

                    //string gameName = Path.GetFileNameWithoutExtension(entry.Name);
                    //Game game = Games[currentIndex - 1];
                    Game game = Games.Where(game => game.ID == currentIndex - 1).Single();

                    // Only extract games that are selected
                    //if (GameNamesSelected.Contains(gameName))
                    //if (Games.Any(game => game.FullName.Contains(gameName)))
                    //{
                    //    if (ShortNameCheckBox.IsChecked == true) // Truncate. If already exists, truncate a further 2 chars and add ~n, as per DOS convention                        
                    //    {
                    //        if (TruncateRadioButton.IsChecked == true)
                    //        {
                    //            // Get a list of files (in case zips weren't deleted) or folders already in the dir just in case there is a clash                                
                    //            string[] entries = Directory.GetFileSystemEntries(fullPath).Distinct().ToArray();

                    //            // Get the truncated name
                    //            gameName = Helpers.GetTruncatedName(gameName, entries);
                    //        }
                    //        // TO DO: Really need to re-think how this is done
                    //        // It is very tedious if there is a large number
                    //        // IDEA: Open a new pane, auto-populate this with the below suggestions
                    //        // Set the default to the first one
                    //        // Allow user to edit this choice IF THEY WANT TO
                    //        else if (SuggestRadioButton.IsChecked == true)
                    //        {
                    //            // Suggestions, are a combination of executable file names, words from the game title, usually abbreviated, joined or concatenated in a number of ways
                    //            List<string> suggestions = new List<string>();

                    //            // Get EXE file names
                    //            suggestions.AddRange(ShortNameHelpers.GetEXEFilenames(entry));

                    //            // Add the EXE File names to the Words generated from the Game Name.
                    //            suggestions = suggestions.Union(ShortNameHelpers.GetGameNameWords(gameName)).ToList();

                    //            // Remove and/or rename duplicates that already exist within file system
                    //            suggestions = ShortNameHelpers.RenameFileSystemDuplicates(fullPath, suggestions);

                    //            ShortNameDialog dialog = new ShortNameDialog(currentIndex, total, gameName, suggestions);

                    //            // If no suggestions were found (not sure if this is even possible!) - just show the custom text box
                    //            if (suggestions.Count == 0)
                    //            {
                    //                dialog.ShortNameLabel.Visibility = Visibility.Collapsed;
                    //                dialog.ShortNameListBox.Visibility = Visibility.Collapsed;

                    //                if (dialog.ShowDialog() == true)
                    //                {
                    //                    {
                    //                        gameName = dialog.CustomName;
                    //                    }
                    //                }
                    //            }
                    //            else if (suggestions.Count == 1)
                    //            {
                    //                if (dialog.ShowDialog() == true)
                    //                {
                    //                    if (dialog.CustomName == "")
                    //                    {
                    //                        gameName = dialog.ShortName;
                    //                    }
                    //                    else
                    //                    {
                    //                        gameName = dialog.CustomName;
                    //                    }
                    //                }
                    //            }
                    //            else if (suggestions.Count > 1)
                    //            {
                    //                bool? showed = dialog.ShowDialog();

                    //                if (showed == true)
                    //                {
                    //                    if (dialog.CustomName == "")
                    //                    {
                    //                        gameName = dialog.ShortName;
                    //                    }
                    //                    else
                    //                    {
                    //                        gameName = dialog.CustomName;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    // They cancelled, so leave name as is
                    //                }
                    //            }
                    //        }
                    //    }
                    if (game.Selected)
                    {                    
                        // Extract the Inner Zip file (A ZIP of a single game)
                        // Using the naming chosen by the user
                        try
                        {
                            if (AlphabetCheckBox.IsChecked == false)
                            {
                                entry.ExtractToFile(fullPath + "\\" + game.CurrentName + ".ZIP");
                            }
                            else
                            {
                                char alphaNumChar = game.CurrentName.First();
                                if (Char.IsDigit(alphaNumChar))
                                {
                                    alphaNumChar = '0';
                                }    

                                entry.ExtractToFile(fullPath + "\\" + alphaNumChar + "\\" + game.CurrentName + ".ZIP");
                            }    
                        }
                        catch (Exception e2)
                        {
                            Debug.WriteLine(e2.Message);
                        }

                        // Extract each file into a directory with the same name as the Zip File
                        FileHelpers.ExtractInnerZip(fullPath, game.CurrentName);
                    }
                    else
                    {
                        Debug.WriteLine(game.FullName + " not selected.");
                    }

                    currentIndex++;
                }

                // Delete zip file as this has been extracted to dir as this is no longer needed
                // (Unless the user chooses to keep it as well).
                if (DeleteZipsCheckBox.IsChecked == true)
                {
                    FileHelpers.DeleteZips(fullPath);
                }
            }
        }

        private void handleUpdate()
        {
            //Start with everything
            //By doing this, we don't have to worry about logic to "remove" items from the selected game names, instead we start with a fresh slate like a database
            //GameNamesSelected.Clear();
            //GameNamesSelected.AddRange(GameNamesAll);
            //for (int i = 0; i < gameFilenames.Count; i++)
            //{
            //    GameNamesSelected.Add(new Game(i, gameFilenames[i]));
            // }
            //GameNamesSelected = new ObservableCollection<string>(GameNamesAll);
            Games.ForEach(game => { game.Selected = true; });

            if (ImagesCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.DC)).ToList();
                Games.ForEach(game => { if (game.FullName.EndsWith(".img", StringComparison.CurrentCultureIgnoreCase)) game.Selected = false; });
            }

            //***TAGS***
            // In the current version, Tags will be considered an additonal filter, so when checked, will be included, when unchecked, will not be included
            // Unlike flags, tags will be optionally included if checked and exclusively excluded
            if (DCCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.DC)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.DC)) game.Selected = false; });
            }
            if (HnCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.HN)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.HN)) game.Selected = false; });
            }
            if (AnCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.AN)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.AN)) game.Selected = false; });
            }
            if (FnCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.FN)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.FN)) game.Selected = false; });
            }
            if (OnCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.ON)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.ON)) game.Selected = false; });
            }
            if (BnCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.BN)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.BN)) game.Selected = false; });
            }
            if (SWCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.SW_SWR_FW)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.SW)) game.Selected = false; });
            }
            if (SWCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.SW_SWR_FW)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.SWR_FW)) game.Selected = false; });
            }
            if (GoodCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.GOOD)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.GOOD)) game.Selected = false; });
            }
            if (MostlyGoodCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.MOSTLY_GOOD)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.MOSTLY_GOOD)) game.Selected = false; });
            }
            if (TrCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.TRANSLATED)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.TRANSLATED) && !game.FullName.Contains("[tr En]")) game.Selected = false; });
            }
            if (TrXXCheckBox.IsChecked == true)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.TRANSLATED)).ToList();
                Games.ForEach(game => { if (!game.FullName.Contains("[tr XX]".Replace("XX", OtherTrLanguageTextBox.Text))) game.Selected = false; });
            }

            //***FLAGS***
            // In the current version, Flags are considering an exclusive filter so the name MUST include this string if checked, optionally if unchecked
            if (InstallerCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.INSTALLER)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.INSTALLER)) game.Selected = false; });
            }
            if (DemoCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.DEMO)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.DEMO)) game.Selected = false; });
            }
            // Graphics Modes
            if (PCjrCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.PCJR)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.PCJR)) game.Selected = false; });
            }
            if (CGACheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.CGA)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.CGA)) game.Selected = false; });
            }
            if (TandyCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.TANDY)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.TANDY)) game.Selected = false; });
            }
            if (EGACheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.EGA)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.EGA)) game.Selected = false; });
            }
            if (VGACheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.VGA)).ToList();
                Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.VGA)) game.Selected = false; });
            }

            // Custom Flag
            if (CustomFlagCheckBox.IsChecked == true)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.CUSTOM_FLAG.Replace("custom", CustomFlagTextBox.Text))).ToList();
                Games.ForEach(game => { if (!Regex.IsMatch(game.FullName, Regexs.CUSTOM_FLAG.Replace("custom", CustomFlagTextBox.Text))) game.Selected = false; });
            }

            // Languages
            if (NonEnglishCheckBox.IsChecked == false)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.ENGLISH)).ToList();
                //Games.ForEach(game => { if (Regex.IsMatch(game.FullName, Regexs.ENGLISH)) game.Selected = true; else game.Selected = false; });
                Games.ForEach(game => { if (Constants.NON_ENGLISH.Any(language => game.FullName.Contains("(" + language + ")")) && !game.FullName.Contains("(En)")) game.Selected = false; });
            }
            if (OtherLanguageCheckBox.IsChecked == true)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.JAPANESE)).ToList();
                Games.ForEach(game => { if (!game.FullName.Contains("(" + OtherLanguageTextBox.Text + ")")) game.Selected = false; });
            }

            // Needs regex for a more consise match
            if (PublisherCheckBox.IsChecked == true)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.PUBLISHER.Replace("publisher", PublisherTextBox.Text))).ToList();
                Games.ForEach(game => { if (!Regex.IsMatch(game.FullName, Regexs.PUBLISHER.Replace("publisher", PublisherTextBox.Text))) game.Selected = false; });
            }

            // Needs regex for a more consise match
            if (GenreCheckBox.IsChecked == true)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => Regex.IsMatch(item.Name, Regexs.GENRE.Replace("genre", GenreTextBox.Text))).ToList();
                Games.ForEach(game => { if (!Regex.IsMatch(game.FullName, Regexs.GENRE.Replace("genre", GenreTextBox.Text))) game.Selected = false; });
            }

            if (FreeTextCheckBox.IsChecked == true)
            {
                //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, Regexs.TRANSLATED)).ToList();
                Games.ForEach(game => { if (!game.FullName.Contains(FreeTextTextBox.Text)) game.Selected = false; });
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
                        //GameNamesSelected = GameNamesSelected.Where(item => !Regex.IsMatch(item.Name, regex)).ToList();
                        Games.ForEach(game => { if (!Regex.IsMatch(game.FullName, regex)) game.Selected = false; });
                    }
                    else if (Regex.IsMatch(versionNumber, regex)) // Specified version only, number format is correct
                    {
                        // Remove everything EXCEPT the matching version or release no                        
                        //GameNamesSelected = GameNamesSelected.Where(item => item.Name.Contains(versionNumber)).ToList();
                        Games.ForEach(game => { if (game.FullName.Contains(versionNumber)) game.Selected = false; });
                    }
                    else // Invalid input, do nothing
                    {
                        return;
                    }

                }
                else if (HighestRadioButton.IsChecked == true)
                {
                    //GameNamesSelected = GameNamesSelected
                    //            .GroupBy(game => Helpers.GetLikeVersionsTitleOnly(game.Name, regex))
                    //            .Select(group => group.OrderByDescending(game => Helpers.GetVersion(game.Name, regex))
                    //            .First())
                    //            .ToList();
                    var groupedGames = Games.GroupBy(game => Helpers.GetLikeVersionsTitleOnly(game.FullName, regex));

                    foreach (var group in groupedGames)
                    {
                        var highestVersionGame = group
                            .OrderByDescending(game => Helpers.GetVersion(game.FullName, regex))
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

            //GameNamesExcluded = GameNamesAll.Except(GameNamesSelected).ToList();

            // Process manually moved entries. These override any conditions above.
            //foreach ((bool condition, string value) in GameNamesManual)
            //{
            //    if (condition)
            //    {
            //        GameNamesSelected.Add(value);
            //    }
            //    else
            //    {
            //        GameNamesExcluded.Add(value);
            //    }
            //}
            foreach (var game in Games)
            {
                if (game.Manual != null)
                {
                    game.Selected = (bool)game.Manual; // cast is OK as bool? can't be null due to above check
                }
            }

            //GameNamesSelected.Sort();

            //GameNamesExcluded.Sort();

            Games = Games.OrderBy(game => game.FullName).ToList();

            updateCount_List();
        }



        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            handleUpdate();
        }

        // This is a work around so you do not have to click twice for the first drag/drop
        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            DependencyObject? originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null) { return; }

            var listBox = (ListBox)sender;

            // Find the item that is being hovered over
            Game? hoveredGame = FindItemUnderMouse(listBox, originalSource) as Game;

            // Select the hovered item or the first item if none is hovered over
            if (hoveredGame != null)
            {
                listBox.SelectedItem = hoveredGame;
            }
            else if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }
            else
            {
                listBox.SelectedIndex = -1;
            }

            // Set focus to the list box
            listBox.Focus();
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("PreviewMouseLeftButtonDown");

            // Start drag and drop operation
            sourceListBox = sender as ListBox;
            if (sourceListBox == null) { return; }

            if (e.ClickCount == 2)
            {
                // Handle double-click edit
                handleDoubleClickedItem();
                e.Handled = true;
            }

            // This ensures the scroll bars still work (if present).
            if (!e.Handled)
            {
                ListBox listBox = (ListBox)sender;

                // Check if the mouse click occurred on a ListBoxItem
                var item = ItemsControl.ContainerFromElement(listBox, e.OriginalSource as DependencyObject) as ListBoxItem;
                if (item != null)
                {
                    // Now we get the selected item and start the drag / drop operation.
                    Game? selectedItem = sourceListBox.SelectedItem as Game;
                    if (selectedItem != null)
                    {
                        DragDrop.DoDragDrop(sourceListBox, selectedItem, DragDropEffects.Move);
                    }
                }
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("Drop");
            if (sourceListBox == null) { return; }

            // Handle the drop operation
            ListBox? targetListBox = sender as ListBox;
            if (targetListBox == null) { return; }

            if (sourceListBox == targetListBox) { return; }

            Game? droppedItem = e.Data.GetData(typeof(Game)) as Game;

            if (droppedItem != null)
            {
                //souceListBox.Items.Remove(droppedItem);
                //List<Game>? itemsSource = sourceListBox.ItemsSource as List<Game>;
                //if (itemsSource != null)
                //{
                //    itemsSource.Remove(droppedItem);
                //    List<Game> targetItemsSource = (List<Game>)targetListBox.ItemsSource;
                //    targetItemsSource.Add(droppedItem);

                //int index = GameNamesManual.FindIndex(item => item.Item2 == droppedItem);

                //bool addRemove = targetListBox.Name == "GameZipFilesListBox";
                //if (index == -1)
                //{
                //    if (addRemove)
                //    {
                //        GameNamesManual.Add((true, droppedItem));
                //    }
                //    else
                //    {
                //        GameNamesManual.Add((false, droppedItem));
                //    }
                //}
                //else
                //{
                //    GameNamesManual[index] = (addRemove, droppedItem);
                //}

                droppedItem.Selected = !droppedItem.Selected;
                droppedItem.Manual = droppedItem.Selected;

                //targetItemsSource.Sort();
                updateCount_List();
                //}
            }
        }

        private object? FindItemUnderMouse(ListBox listBox, DependencyObject source)
        {
            var itemContainer = listBox.ContainerFromElement(source) as ListBoxItem;

            if (itemContainer != null)
            {
                return itemContainer.DataContext;
            }

            return null;
        }

        private void ShortNameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //CheckBox? checkBox = sender as CheckBox;
            //if (checkBox == null) { return; }
            CheckBox checkBox = ShortNameCheckBox;

            if (checkBox.IsChecked == true)
            {
                if (TruncateRadioButton.IsChecked == true)
                {
                    // do stuff
                    handleTruncateGameName();

                    //GameZipFilesListBox.DisplayMemberPath = "TruncatedName";
                    updateCount_List();
                }
                else if (SuggestRadioButton.IsChecked == true)
                {
                    handleSuggestedGameNames();

                    //GameZipFilesListBox.DisplayMemberPath = "DefaultSuggestedName";
                    updateCount_List();
                }
                else
                {
                    return; // shouldn't happen...
                }
            }
            else
            {
                // do other stuff
                foreach (Game game in Games)
                {
                    game.CurrentName = game.FullName;
                }

                updateCount_List();
            }
        }

        private void handleTruncateGameName()
        {
            // Get a list of files (in case zips weren't deleted) or folders already in the dir just in case there is a clash
            string fullPath = FileHelpers.GetFullPath(ZipFileName);
            string[] entries = Directory.GetFileSystemEntries(fullPath).Distinct().ToArray();

            // Get the truncated names
            foreach (Game game in Games)
            {
                game.TruncatedName = Helpers.GetTruncatedName(game.FullName, entries);

                game.CurrentName = game.TruncatedName;
            }

            ShortNameHelpers.RenameShortNameDuplicates(Games, false);
        }

        private void handleSuggestedGameNames()
        {
            string fullPath = FileHelpers.GetFullPath(ZipFileName);

            // Open the yearly zip file
            using (ZipArchive archive = ZipFile.OpenRead(ZipFileName))
            {
                //int currentIndex = 0;
                //int total = archive.Entries.Count;
                int count = archive.Entries.Count;

                // Process each child zip game
                // Start at 1 to skip folder entry
                //foreach (ZipArchiveEntry entry in archive.Entries)
                for (int i = 1; i < count; i++)
                {
                    var entry = archive.Entries[i];
                    // Get matching game entry for archive entry
                    Game game = Games.Where(game => game.ID == i - 1).Single();
                    // Skip folder entry
                    //if (entry.Length == 0)
                    //{
                    //    continue;
                    //}
                    // Suggestions, are a combination of executable file names, words from the game title, usually abbreviated, joined or concatenated in a number of ways
                    List<string> suggestions = new List<string>();

                    // Get EXE file names
                    suggestions.AddRange(ShortNameHelpers.GetEXEFilenames(entry));

                    // Add the EXE File names to the Words generated from the Game Name. -1 as Games has already had folder entry at index 0 removed, whereas zip will have it
                    //suggestions = suggestions.Union(ShortNameHelpers.GetGameNameWords(Games[i - 1].FullName)).ToList();
                    
                    suggestions = suggestions.Union(ShortNameHelpers.GetGameNameWords(game.FullName)).ToList();

                    // Remove and/or rename duplicates that already exist within file system
                    suggestions = ShortNameHelpers.RenameFileSystemDuplicates(fullPath, suggestions);

                    // Add suggestions to game object
                    game.SuggestedNames = suggestions;

                    game.CurrentName = suggestions.First();
                }
            }

            ShortNameHelpers.RenameShortNameDuplicates(Games, true);

            //Debug.WriteLine("test");
        }

        private void handleDoubleClickedItem()
        {
            if (GameZipFilesListBox.SelectedItem != null)
            {
                // Double-clicked item logic here
                Game selectedGame = (Game)GameZipFilesListBox.SelectedItem;
                // Perform your action with the selected item

                ShortNameDialog dialog = new ShortNameDialog(selectedGame.ID, Games.Count, selectedGame.FullName, selectedGame.SuggestedNames);

                // If no suggestions were found (not sure if this is even possible!) - just show the custom text box
                if (selectedGame.SuggestedNames.Count == 0)
                {
                    dialog.ShortNameLabel.Visibility = Visibility.Collapsed;
                    dialog.ShortNameListBox.Visibility = Visibility.Collapsed;

                    if (dialog.ShowDialog() == true)
                    {
                        {
                            selectedGame.CurrentName = dialog.CustomName;
                        }
                    }
                }
                else if (selectedGame.SuggestedNames.Count == 1)
                {
                    if (dialog.ShowDialog() == true)
                    {
                        if (dialog.CustomName == "")
                        {
                            selectedGame.CurrentName = dialog.ShortName;
                        }
                        else
                        {
                            selectedGame.CurrentName = dialog.CustomName;
                        }
                    }
                }
                else if (selectedGame.SuggestedNames.Count > 1)
                {
                    bool? showed = dialog.ShowDialog();

                    if (showed == true)
                    {
                        if (dialog.CustomName == "")
                        {
                            selectedGame.CurrentName = dialog.ShortName;
                        }
                        else
                        {
                            selectedGame.CurrentName = dialog.CustomName;
                        }
                    }
                    else
                    {
                        // They cancelled, so leave name as is
                    }
                }

                updateCount_List();
            }
        }
    }
}
