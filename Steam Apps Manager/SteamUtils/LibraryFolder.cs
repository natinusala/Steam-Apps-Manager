using Steam_Apps_Manager.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Steam_Apps_Manager.SteamUtils
{
    public class LibraryFolder
    {
        public List<App> apps { get; private set; }

        public string path { get; private set; }

        public long GetAvailableFreeDiskSpace()
        {
            return new DriveInfo(path.Substring(0, 1)).AvailableFreeSpace;
        }

        public static int AddLibraryFolderToVDF(string selectedPath)
        {
            selectedPath = selectedPath.Replace("\\", "\\\\");
            //Add to VDF
            string libraryFoldersVdf = Utils.GetLibraryFolderVDFPath();
            ACFNode libraryFoldersVdfNode = ACFNode.ParseACF(libraryFoldersVdf);

            ACFNode libraryFoldersVdfNodeRoot = ((ACFNode)libraryFoldersVdfNode["LibraryFolders"]);

            int i = 1;

            while (libraryFoldersVdfNodeRoot.ContainsKey(i.ToString()))
            {
                if ((string)libraryFoldersVdfNodeRoot[i.ToString()] == selectedPath)
                {
                    return -1;
                }
                i++;
            }

            libraryFoldersVdfNodeRoot[i.ToString()] = selectedPath;

            File.WriteAllText(libraryFoldersVdf, libraryFoldersVdfNode.ToString(), new UTF8Encoding(false));

            return 0;
        }

        public static LibraryFolder CreateNew()
        {
            System.Windows.MessageBox.Show("Please select an empty folder for your new Steam library.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;
                string steamAppsPath = selectedPath + "\\steamapps";

                if (Utils.IsDirectoryEmpty(selectedPath))
                {
                    //DLL
                    string dllPath = Utils.GetSteamDLLPath();
                    string newDllPath = selectedPath + "\\steam.dll";

                    Directory.CreateDirectory(steamAppsPath);
                    File.Copy(dllPath, newDllPath);

                    AddLibraryFolderToVDF(selectedPath);

                    return new LibraryFolder(steamAppsPath);
                }
                else
                {
                    System.Windows.MessageBox.Show("The selected folder is not empty. Aborting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return null;
        }
        

        public static List<LibraryFolder> GetAllLibraryFolders()
        {
            List<LibraryFolder> list = new List<LibraryFolder>();

            //Add the default install directory
            string baseSteamApps = Utils.GetBaseSteamAppsPath();
            string libraryFoldersVdf = Utils.GetLibraryFolderVDFPath();
            list.Add(new LibraryFolder(baseSteamApps));

            //Add all the others
            if (File.Exists(libraryFoldersVdf))
            {
                ACFNode libraryFolders = (ACFNode)ACFNode.ParseACF(libraryFoldersVdf)["LibraryFolders"];
                int i = 1;
                while (true)
                {
                    if (libraryFolders.ContainsKey(i.ToString()))
                    {
                        string path = (string)libraryFolders[i.ToString()];
                        list.Add(new LibraryFolder(path.Replace("\\\\", "\\") + "\\steamapps"));
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return list;
        }

        public string GetManifestPathForAppId(uint appid)
        {
            return this.path + "\\" + "appmanifest_" + appid + ".acf";
        }

        public LibraryFolder(string path)
        {
            this.path = path;
            this.apps = new List<App>();

            //Listing of all the files in this directory
            //to detect the manifests

            if (!Directory.Exists(path))
            {
                return;
            }

            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (fileName.StartsWith("appmanifest_") && fileName.EndsWith(".acf"))
                {
                    //Creation of one SteamApp per app manifest
                    App steamApp = new App(file, this);
                    apps.Add(steamApp);
                }
            }
        }
    }
}
