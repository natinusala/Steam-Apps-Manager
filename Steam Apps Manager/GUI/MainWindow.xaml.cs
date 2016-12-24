using Steam_Apps_Manager.SteamUtils;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            listBox.Items.Clear();
            steamApps.Clear();

            List<LibraryFolder> directories = LibraryFolder.GetAllLibraryFolders();
            foreach (LibraryFolder directory in directories)
            {
                foreach (SteamUtils.App app in directory.apps)
                {
                    steamApps.Add(app);
                    listBox.Items.Add(app.appName);
                }
            }
        }

        //from http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        private string ConvertSizeFromBytesToString(ulong size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && ++order < sizes.Length)
            {
                len = len / 1000;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SteamUtils.App selectedApp = steamApps[listBox.SelectedIndex];

            this.appNameLabel.Content = selectedApp.appName;
            this.appPathLabel.Content = "Installed in " + selectedApp.directory.path;
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
    }
}
