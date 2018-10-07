using Steam_Apps_Manager.SteamUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Steam_Apps_Manager.GUI
{
    /// <summary>
    /// Logique d'interaction pour MoveDialog.xaml
    /// </summary>

    public partial class MoveDialog : Window
    {
        private List<SteamUtils.App> steamApps;
        private List<LibraryFolder> libraryFolders;
        private BackgroundWorker worker;
        private string formattedGameList;

        public MoveDialog(List<SteamUtils.App> steamApps)
        {
            this.steamApps = steamApps;
            this.libraryFolders = new List<LibraryFolder>();

            this.worker = new BackgroundWorker();
            this.worker.DoWork += worker_DoWork;
            this.worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            this.worker.ProgressChanged += worker_ProgressChanged;
            this.worker.WorkerReportsProgress = true;

            InitializeComponent();

            this.formattedGameList = Utils.FormatGameList(steamApps, 60);
            this.Title = "Move " + this.formattedGameList;
            this.appNameLabel.Content = "Move " + this.formattedGameList + " to:";

            Refresh();

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (SteamUtils.App steamApp in steamApps)
            {
                //Preparation
                string oldManifestPath = steamApp.GetManifestPath();
                string newManifestPath = selectedFolder.GetManifestPathForAppId(steamApp.appId);

                //Moving the manifest
                Directory.CreateDirectory(newManifestPath.Substring(0, newManifestPath.LastIndexOf('\\')));
                try
                {
                    File.Move(oldManifestPath, newManifestPath);
                }
                catch (FileNotFoundException)
                {

                }

                //Moving the game  
                steamApp.MoveAppFiles(selectedFolder, worker);
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Windows.MessageBox.Show(this.formattedGameList + " have been successfully moved!\n" +
                "Select \"verify integrity of game files\" via Steam if the game(s) do not run.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
                progressBar.IsIndeterminate = true;
            else
                progressBar.IsIndeterminate = false;

            progressBar.Value += e.ProgressPercentage;
        }

        private void Refresh()
        {
            this.movingGrid.Visibility = Visibility.Collapsed;
            this.folderSelectGrid.Visibility = Visibility.Visible;

            selectedFolder = null;

            this.comboBox.Items.Clear();
            this.libraryFolders.Clear();

            List<LibraryFolder> tempLibraryFolders = LibraryFolder.GetAllLibraryFolders();

            foreach (LibraryFolder folder in tempLibraryFolders)
            {
                if (folder.path != steamApps[0].folder.path)
                {
                    this.comboBox.Items.Add(folder.path);
                    libraryFolders.Add(folder);
                }
            }

            this.comboBox.Items.Add("Create a new Steam library folder...");
            this.comboBox.SelectedIndex = 0;
        }

        private LibraryFolder selectedFolder;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBox.SelectedIndex == libraryFolders.Count)
            {
                selectedFolder = LibraryFolder.CreateNew();
                if (selectedFolder == null)
                {
                    Refresh();
                    return;
                }
            }
            else
            {
                selectedFolder = libraryFolders[this.comboBox.SelectedIndex];
            }

            long totalBytes = 0;

            foreach(SteamUtils.App app in this.steamApps)
                totalBytes += app.sizeOnDisk;

            if (selectedFolder.GetAvailableFreeDiskSpace() <= totalBytes)
            {
                System.Windows.MessageBox.Show("You don't have enough free space on disk " + selectedFolder.path[0] + " to move the game(s).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Refresh();
                return;
            }

            //Beginning
            this.appNameMovingLabel.Content = "Moving " + this.formattedGameList + " to " + selectedFolder.path + "\\";

            this.progressBar.Value = 0;
            this.progressBar.Maximum = 1000;

            this.folderSelectGrid.Visibility = Visibility.Collapsed;
            this.movingGrid.Visibility = Visibility.Visible;

            this.worker.RunWorkerAsync();
            
        }
    }
}
