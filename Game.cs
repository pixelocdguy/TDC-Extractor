using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDC_Extractor
{
    // Stores information about a Game
    // Specifically, it's full name, short name (if enabled by user), index in List, index and/or ID?
    public class Game : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // This is the position in the original zip file which is useful for other methods also.
        public int Index { get; set; }

        // This is the year (set from the Zip File Name)
        public string Year { get; set; }

        // This is the filename including the extension        
        public string Filename { get; set; }

        // This is the full name of the game, including metadata
        public string FullName { get; private set; }

        // This is the full name of the game, WITHOUT the metadata
        // public string NameWOMeta { get; private set; }
        // This is ONLY the game varient meta data
        // For example, a1, b1, etc.
        // But not including Year, Publisher, Genre, etc.

        // Opposite of the above
        public string VarientMeta { get; private set; }
        
        // Is this needed?
        public string? NameWOVarients { get; private set; }

        // Need to set before extract path calc
        private bool _group;
        public bool Group
        {
            get { return _group; }
            set
            {
                _group = value;

                if (!ShortName) // Leave shortname as is as varient meta is already removed
                {
                    if (_group)
                    {
                        // Remove variennt information as this would be a double up of information when you can see this in the sub-folder name
                        _currentName = TitleHelpers.GetGameNameWOVarients(FullName);
                    }
                    else
                    {
                        _currentName = FullName;
                    }
                }
                //KNOWN BUG: If changing to group AFTER shortname is selected, it doesn't recalculate the name which might not have a ~ anymore due to allowing the same name with different varient sub-folders
                // This will need some re-factoring as the methods to do this are in the code behind class                
                else
                {
                    if (_group)
                    {
                        // TO DO: Recalculate Shortname.
                    }
                }    
                
                OnPropertyChanged(nameof(CurrentName));

                setExtractPath();
            }
        }

        private bool _alphabet;
        public bool Alphabet
        {
            get { return _alphabet; }
            set
            {
                _alphabet = value;

                setExtractPath();
            }
        }
        private bool _shortName;
        public bool ShortName
        {
            get { return _shortName; }
            // Need to recalc extract path, as if grouping is also set the path will change, even if the actual stored current name hasn't changed
            // E.g.
            // This is a Game\v1.1[a1]\
            // THISIS~1\V1A1\
            set
            {
                _shortName = value;

                // Fix for this scenario
                if (!_shortName && _group)
                {
                    _currentName = TitleHelpers.GetGameNameWOVarients(FullName);

                    OnPropertyChanged(nameof(CurrentName));
                }
                else if (!_shortName && !_group)
                {
                    _currentName = FullName;

                    OnPropertyChanged(nameof(CurrentName));
                }

                setExtractPath();
            }
        }

        // This is the extract path (reletive to the yearly zip file).
        private string _extractPath;
        public string ExtractPath
        {
            get { return _extractPath; }
            set
            {
                if (_extractPath != value)
                {
                    _extractPath = value;
                    OnPropertyChanged(nameof(ExtractPath));
                }
            }
        }

        // As per below comment.
        private string _currentName;
        public string CurrentName
        {
            get { return _currentName; }
            set
            {
                if (_currentName != value)
                {
                    _currentName = value;
                    OnPropertyChanged(nameof(CurrentName));

                    setExtractPath();
                    //OnPropertyChanged(nameof(ExtractPath));
                }
            }
        }
        // This is  used to store the truncated name when renaming in 8.3 Windows format for extraction.        
        private string? _truncatedName;
        public string? TruncatedName
        {
            get { return _truncatedName; }
            set
            {
                if (_truncatedName != value)
                {
                    _truncatedName = value;
                    //OnPropertyChanged(nameof(CurrentName));
                }
            }
        }
        // This is  used to store suggested when the user wants something more meaningful than the above.
        public ObservableCollection<string> SuggestedNames { get; set; }

        // True: Game is selected. False: Game is excluded
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }
        // Null: Game has not been manually [de-]selected, so leave as automatic. True: Game name has been manually selected. Fales: Game name has been manually excluded.
        public bool? Manual { get; set; }



        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Game(int id, string filename, string year)
        {
            Index = id;
            
            Filename = filename;
            Year = year;

            FullName = Path.GetFileNameWithoutExtension(Filename);
                            
            //NameWOMeta = TitleHelpers.GetGameNameWithoutMeta(Filename); // Remove these from constructor and only run when needed?
            VarientMeta = TitleHelpers.GetVarientMeta(Filename); // Remove these from constructor and only run when needed?

            _group = false;
            Alphabet = false;
            ShortName = false;

            _currentName = FullName;
            
            SuggestedNames = new ObservableCollection<string>();

            _extractPath = Year + "\\" + _currentName + "\\";
            
            _selected = true;
            Manual = null;
        }

        private void setExtractPath()
        {
            string gameFolder = _currentName;

            ExtractPath = FileHelpers.GetInnerZipPath(gameFolder, this);
        }
    }
}
