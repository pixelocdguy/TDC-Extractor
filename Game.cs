using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDC_Extractor
{
    // Stores information about a Game
    // Specifically, it's full name, short name (if enabled by user), index in List, index and/or ID?
    public class Game
    {
        // ID is a unqiue ID. This is the position in the original zip file which is useful for other methods also.
        public int ID { get; set; }
        public string Filename { get; set; }

        public string FullName { get; private set; }
        public string NameWOMeta { get; private set; }
        private string _currentName;
        public string CurrentName
        {
            get { return _currentName; }
            set
            {
                _currentName = value;
                if (_currentName != FullName)
                {
                    string tab = "\t\t";
                    if (_currentName.Length > 7)
                    {
                        tab = "\t";
                    }

                    DisplayName = _currentName + tab + FullName;
                }
                else
                {
                    DisplayName = FullName;
                }
            }
        }

        public string DisplayName { get; set; }

        public string? TruncatedName { get; set; }
        //private List<string> _suggestedNames;
        public List<string> SuggestedNames { get; set; }
        //{
        //    get { return _suggestedNames; }
        //    set
        //    {
        //        _suggestedNames = value;

        //        if (_suggestedNames != null && _suggestedNames.Count > 0)
        //        {
        //            DefaultSuggestedName = _suggestedNames.First();
        //        }
        //    }
        //}
        //public string DefaultSuggestedName{ get; private set; }

        // True: Game is selected. False: Game is excluded
        public bool Selected { get; set; }
        // Null: Game has not been dragged / overriden, so leave as automatic (above). True: Game name has been manually selected. Fales: Game name has been manually excluded.
        public bool? Manual { get; set; }

        public Game(int id, string filename)
        {
            ID = id;
            Filename = filename;
            FullName = Path.GetFileNameWithoutExtension(Filename);
            _currentName = FullName;
            DisplayName = _currentName;
            NameWOMeta = Helpers.GetGameNameWithoutMeta(Filename);

            SuggestedNames = new List<string>();
            //DefaultSuggestedName = "";

            Selected = true;
            Manual = null;
        }
    }
}
