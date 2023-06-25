using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TDC_Extractor
{
    /// <summary>
    /// Interaction logic for ShortNameDialog.xaml
    /// </summary>
    public partial class ShortNameDialog : Window
    {
        public string ShortName
        {
            get
            {
                string? shortName = ShortNameListBox.SelectedItem.ToString();
                if (shortName == null) // This shouldn't really be possible, but the compilier is nagging...
                {
                    shortName = string.Empty;
                }

                return shortName;
            }
        }

        public string CustomName
        {
            get
            {
                return CustomTextBox.Text;
            }
        }
        public ShortNameDialog(int gameNoOfTotal, int totalGames, string longName, List<string> shortNames)
        {
            InitializeComponent();

            this.Title = gameNoOfTotal.ToString() + "/" + totalGames.ToString() + " - " + longName;

            LongNameTextBlock.Text = longName;

            ShortNameListBox.ItemsSource = shortNames;
            ShortNameListBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ShortNameListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CustomTextBox.Text = ShortNameListBox.SelectedItem.ToString();
        }
    }
}
