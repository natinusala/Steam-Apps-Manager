using Steam_Apps_Manager.GUI;
using Steam_Apps_Manager.SteamUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

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
                System.Windows.MessageBox.Show("Steam Apps Manager cannot be used while Steam is running. Please close Steam and try again.", "Steam is running", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                System.Windows.Application.Current.Shutdown();
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
            steamApps = steamApps.OrderBy(o => o.appName).ToList();

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

            List<SteamUtils.App> selectedApps = GetSelectedApps();

            this.appNameLabel.Content = Utils.FormatGameList(selectedApps, 90);
            this.appPathLabel.Content = "Installed in " + selectedApps[0].folder.path.Substring(0, selectedApps[0].folder.path.Length - 10);

            long totalBytes = 0;
            this.appMoveButton.Visibility = Visibility.Visible;
            foreach (SteamUtils.App app in selectedApps)
            {

                if (!this.appPathLabel.Content.Equals("Installed in " + app.folder.path.Substring(0, app.folder.path.Length - 10)))
                {
                    this.appPathLabel.Content = "Installed in multiple folders (cannot move games)";
                    this.appMoveButton.Visibility = Visibility.Collapsed;
                }

                totalBytes += app.sizeOnDisk;
            }


            this.appSizeLabel.Content = "Size: " + ConvertSizeFromBytesToString(totalBytes);


            this.appStatusLabel.Visibility = Visibility.Collapsed;
            this.welcomeLabelGrid.Visibility = Visibility.Collapsed;
            this.infosGrid.Visibility = Visibility.Visible;

        }

        private List<SteamUtils.App> GetSelectedApps()
        {
            List<SteamUtils.App> selectedApps = new List<SteamUtils.App>();

            foreach (object o in listBox.SelectedItems)
            {
                selectedApps.Add(steamApps[listBox.Items.IndexOf(o)]);
            }

            return selectedApps;
        }

        private void appMoveButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDialog dialog = new MoveDialog(GetSelectedApps());
            dialog.Owner = this;
            dialog.ShowDialog();

        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Please select an existing Steam library folder (containing /steamapps/).", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;
                if (Directory.GetFiles(selectedPath, "steam.dll").Length == 0)
                {
                    MessageBoxResult msgResult = System.Windows.MessageBox.Show("This folder is missing steam.dll and doesn't seem to be a valid Steam library folder.\nImport it anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (msgResult == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                LibraryFolder folder = new LibraryFolder(selectedPath + "\\steamapps");

                if (folder.apps.Count == 0)
                {
                    MessageBoxResult msgResult = System.Windows.MessageBox.Show("This library folder doesn't contain any Steam apps.\nImport it anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (msgResult == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                if (LibraryFolder.AddLibraryFolderToVDF(selectedPath) == -1)
                {
                    System.Windows.MessageBox.Show("This library folder is already in your Steam library.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string games = "";

                foreach (Steam_Apps_Manager.SteamUtils.App app in folder.apps)
                {
                    games += "\n" + app.appName;
                }

                if (folder.apps.Count == 0)
                {
                    System.Windows.MessageBox.Show("The library folder was successfully imported, but no apps were added to your library." + games, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("The library folder was successfully imported. The following games were added to your library:" + games, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }



            }
        }
    }
}
