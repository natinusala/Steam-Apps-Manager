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
            this.steamApps = new List<SteamApp>();

            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (SteamUtils.SteamUtils.IsSteamRunning())
            {
                MessageBox.Show("Steam Apps Manager cannot be used while Steam is running ; please close Steam and try again.", "Steam is running", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                Application.Current.Shutdown();
                return;
            }

            RefreshApps();
        }

        private List<SteamApp> steamApps;

        private void RefreshApps()
        {
            listBox.Items.Clear();
            steamApps.Clear();

            List<AppsDirectory> directories = AppsDirectory.GetAllAppsDirectories();
            foreach (AppsDirectory directory in directories)
            {
                foreach (SteamApp app in directory.apps)
                {
                    steamApps.Add(app);
                    listBox.Items.Add(app.appName);
                }
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SteamApp selectedApp = steamApps[listBox.SelectedIndex];

            this.appNameLabel.Content = selectedApp.appName;
            this.appPathLabel.Content = "Installed in " + selectedApp.directory.path;

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
                        break;
                }
            }

            this.welcomeLabelGrid.Visibility = Visibility.Collapsed;
            this.infosGrid.Visibility = Visibility.Visible;
        }
    }
}
