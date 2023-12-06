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
        // This is the filename including the extension
        
        public string Filename { get; set; }
        // This is the full name of the game, including metadata
        public string FullName { get; private set; }
        // This is the full name of the game, WITHOUT the metadata
        // public string NameWOMeta { get; private set; }
        // This is ONLY the game varient meta data
        // For example, a1, b1, etc.
        // But not including Year, Publisher, Genre, etc.
        public string VarientMeta { get; private set; }
        // Opposite of the above
        public string? NameWOVarients { get; private set; }

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
                    OnPropertyChanged(nameof(CurrentName));
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

        public Game(int id, string filename)
        {
            Index = id;
            
            Filename = filename;
            FullName = Path.GetFileNameWithoutExtension(Filename);            
            //NameWOMeta = TitleHelpers.GetGameNameWithoutMeta(Filename); // Remove these from constructor and only run when needed?
            VarientMeta = TitleHelpers.GetVarientMeta(Filename); // Remove these from constructor and only run when needed?
            _currentName = FullName;

            SuggestedNames = new ObservableCollection<string>();            

            _selected = true;
            Manual = null;
        }
    }
}
