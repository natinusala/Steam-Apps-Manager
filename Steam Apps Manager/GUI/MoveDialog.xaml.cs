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
            this.libraryFolders = new List<LibraryFolder>();

            InitializeComponent();

            this.Title = "Move " + steamApp.appName;
            this.appNameLabel.Content = "Move " + steamApp.appName + " to :";

            RefreshLibraryPaths();

        }

        private void RefreshLibraryPaths()
        {    
            this.comboBox.Items.Clear();
            this.libraryFolders.Clear();
            List<LibraryFolder> tempLibraryFolders = LibraryFolder.GetAllLibraryFolders();

            foreach (LibraryFolder folder in tempLibraryFolders)
            {
                if (folder.path != steamApp.directory.path)
                {
                    this.comboBox.Items.Add(folder.path);
                    libraryFolders.Add(folder);
                }
            }

            this.comboBox.Items.Add("Create a new Steam library folder...");
            this.comboBox.SelectedIndex = 0;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBox.SelectedIndex == libraryFolders.Count)
            {
                //TODO
            }
            else
            {
                LibraryFolder selectedFolder = libraryFolders[this.comboBox.SelectedIndex];

                if (selectedFolder.GetAvailableFreeDiskSpace() <= steamApp.sizeOnDisk)
                {
                    MessageBox.Show("You don't have enough free space on disk " + selectedFolder.path[0] + " to move the game.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.movingGrid.Visibility = Visibility.Collapsed;
                    this.folderSelectGrid.Visibility = Visibility.Visible;
                    RefreshLibraryPaths();
                    return;
                }

                //Beginning
                this.appNameMovingLabel.Content = "Moving " + steamApp.appName + " to " + selectedFolder.path + "\\...";

                this.folderSelectGrid.Visibility = Visibility.Collapsed;
                this.movingGrid.Visibility = Visibility.Visible;
            }
        }
    }
}
