using Steam_Apps_Manager.GUI;
using Steam_Apps_Manager.SteamUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Apps_Manager
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.steamApps = new List<SteamUtils.App>();

            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (SteamUtils.Utils.IsSteamRunning())
            {
                MessageBox.Show("Steam Apps Manager cannot be used while Steam is running ; please close Steam and try again.", "Steam is running", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                Application.Current.Shutdown();
                return;
            }

            RefreshApps();
        }

        private List<SteamUtils.App> steamApps;

        private void RefreshApps()
        {
            this.infosGrid.Visibility = Visibility.Collapsed;
            this.welcomeLabelGrid.Visibility = Visibility.Visible;

            listBox.Items.Clear();
            steamApps.Clear();

            List<LibraryFolder> directories = LibraryFolder.GetAllLibraryFolders();
            foreach (LibraryFolder directory in directories)
            {
                foreach (SteamUtils.App app in directory.apps)
                {
                    steamApps.Add(app);
                }
            }

            //Sort steamApps
            steamApps = steamApps.OrderBy(o=>o.appName).ToList();

            //Add to the List
            foreach (SteamUtils.App app in steamApps)
            {
                listBox.Items.Add(app.appName);
            }
        }

        //from http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        private string ConvertSizeFromBytesToString(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && ++order < sizes.Length)
            {
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.Items.Count == 0)
                return;

            SteamUtils.App selectedApp = steamApps[listBox.SelectedIndex];

            this.appNameLabel.Content = selectedApp.appName;
            this.appPathLabel.Content = "Installed in " + selectedApp.folder.path;
            this.appSizeLabel.Content = "Size : " + ConvertSizeFromBytesToString(selectedApp.sizeOnDisk);

            SteamAppStatus status = selectedApp.GetStatus();

            switch (status)
            {
                case SteamAppStatus.READY_TO_MOVE:
                {
                    this.appStatusLabel.Visibility = Visibility.Collapsed;
                    this.appMoveButton.Visibility = Visibility.Visible;
                    break;
                }
                case SteamAppStatus.UPDATE_NEEDED:
                {
                        this.appMoveButton.Visibility = Visibility.Collapsed;
                        this.appStatusLabel.Visibility = Visibility.Visible;
                        this.appSizeLabel.Content += " (not accurate because of the pending update)";
                        break;
                }
            }

            this.welcomeLabelGrid.Visibility = Visibility.Collapsed;
            this.infosGrid.Visibility = Visibility.Visible;
        }

        private void appMoveButton_Click(object sender, RoutedEventArgs e)
        {
           MoveDialog dialog = new MoveDialog(steamApps[listBox.SelectedIndex]);
           dialog.Owner = this;
           dialog.ShowDialog();
           RefreshApps();
        }
    }
}
