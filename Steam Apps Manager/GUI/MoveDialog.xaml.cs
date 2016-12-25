using Steam_Apps_Manager.SteamUtils;
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

namespace Steam_Apps_Manager.GUI
{
    /// <summary>
    /// Logique d'interaction pour MoveDialog.xaml
    /// </summary>
    public partial class MoveDialog : Window
    {
        private SteamUtils.App steamApp;
        private List<LibraryFolder> libraryFolders;
        public MoveDialog(SteamUtils.App steamApp)
        {
            this.steamApp = steamApp;

            InitializeComponent();

            this.Title = "Move " + steamApp.appName;
            this.appNameLabel.Content = "Move " + steamApp.appName + " to :";

            RefreshLibraryPaths();
        }

        private void RefreshLibraryPaths()
        {
            this.comboBox.Items.Clear();
            libraryFolders = LibraryFolder.GetAllLibraryFolders();

            foreach (LibraryFolder folder in libraryFolders)
            {
                if (folder.path != steamApp.directory.path)
                {
                    this.comboBox.Items.Add(folder.path);
                }
            }

            this.comboBox.Items.Add("Create a new Steam library folder...");
            this.comboBox.SelectedIndex = 0;
        }
    }
}
