using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        

        public static List<LibraryFolder> GetAllLibraryFolders()
        {
            List<LibraryFolder> list = new List<LibraryFolder>();

            //Add the default install directory
            string baseSteamApps = Utils.GetSteamInstallDirectory() + "\\steamapps";
            string libraryFoldersVdf = baseSteamApps + "\\libraryfolders.vdf";
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

        public LibraryFolder(string path)
        {
            this.path = path;
            this.apps = new List<App>();

            //Listing of all the files in this directory
            //to detect the manifests

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
