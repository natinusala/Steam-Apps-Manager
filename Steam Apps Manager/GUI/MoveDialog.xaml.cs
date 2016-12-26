using Steam_Apps_Manager.SteamUtils;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Steam_Apps_Manager.GUI
{
    /// <summary>
    /// Logique d'interaction pour MoveDialog.xaml
    /// </summary>

    public partial class MoveDialog : Window
    {
        private SteamUtils.App steamApp;
        private List<LibraryFolder> libraryFolders;
        private BackgroundWorker worker;

        public MoveDialog(SteamUtils.App steamApp)
        {
            this.steamApp = steamApp;
            this.libraryFolders = new List<LibraryFolder>();

            this.worker = new BackgroundWorker();
            this.worker.DoWork += worker_DoWork;
            this.worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            this.worker.ProgressChanged += worker_ProgressChanged;
            this.worker.WorkerReportsProgress = true;

            InitializeComponent();

            this.Title = "Move " + steamApp.appName;
            this.appNameLabel.Content = "Move " + steamApp.appName + " to :";

            Refresh();

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (createNewInPath != null)
            {
                //TODO Créer un library
            }

            //Preparation
            string oldManifestPath = steamApp.GetManifestPath();
            string newManifestPath = selectedFolder.GetManifestPathForAppId(steamApp.appId);

            //Moving the manifest
            Directory.CreateDirectory(newManifestPath.Substring(0, newManifestPath.LastIndexOf('\\')));
            File.Move(oldManifestPath, newManifestPath);

            //Moving the game  
            steamApp.MoveAppFiles(selectedFolder, worker);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("The app has been successfully moved !\nRun a game cache files verification from Steam if the game doesn't work properly.", "Success !", MessageBoxButton.OK, MessageBoxImage.Information);
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

            createNewInPath = null;
            selectedFolder = null;

            this.comboBox.Items.Clear();
            this.libraryFolders.Clear();

            List<LibraryFolder> tempLibraryFolders = LibraryFolder.GetAllLibraryFolders();

            foreach (LibraryFolder folder in tempLibraryFolders)
            {
                if (folder.path != steamApp.folder.path)
                {
                    this.comboBox.Items.Add(folder.path);
                    libraryFolders.Add(folder);
                }
            }

            this.comboBox.Items.Add("Create a new Steam library folder...");
            this.comboBox.SelectedIndex = 0;
        }

        private LibraryFolder selectedFolder;
        private string createNewInPath = null;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBox.SelectedIndex == libraryFolders.Count)
            {
                //TODO Demander de créer un dossier
                //TODO Vérifier qu'il n'existe pas déjà
                //TODO Remplir createInNewPath
            }
            else
            {
                selectedFolder = libraryFolders[this.comboBox.SelectedIndex];

                if (selectedFolder.GetAvailableFreeDiskSpace() <= steamApp.sizeOnDisk)
                {
                    MessageBox.Show("You don't have enough free space on disk " + selectedFolder.path[0] + " to move the game.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Refresh();
                    return;
                }

                //Beginning
                this.appNameMovingLabel.Content = "Moving " + steamApp.appName + " to " + selectedFolder.path + "\\...";

                this.progressBar.Value = 0;
                this.progressBar.Maximum = 1000;

                this.folderSelectGrid.Visibility = Visibility.Collapsed;
                this.movingGrid.Visibility = Visibility.Visible;

                this.worker.RunWorkerAsync();
            }
        }
    }
}
