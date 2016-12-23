using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam_Apps_Manager.SteamUtils
{
    class AppsDirectory
    {
        public List<SteamApp> apps { get; private set; }

        public string path { get; private set; }

        public static List<AppsDirectory> GetAllAppsDirectories()
        {
            List<AppsDirectory> list = new List<AppsDirectory>();

            //Add the default install directory
            string baseSteamApps = SteamUtils.GetSteamInstallDirectory() + "\\steamapps";
            string libraryFoldersVdf = baseSteamApps + "\\libraryfolders.vdf";
            list.Add(new AppsDirectory(baseSteamApps));

            //Add all the others
            if (File.Exists(libraryFoldersVdf))
            {
                ACFNode libraryFolders = (ACFNode) ACFNode.ParseACF(libraryFoldersVdf)["LibraryFolders"];
                int i = 1;
                while (true)
                {
                    if (libraryFolders.ContainsKey(i.ToString()))
                    {
                        string path = (string)libraryFolders[i.ToString()];
                        list.Add(new AppsDirectory(path.Replace("\\\\", "\\") + "\\steamapps"));
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

        public AppsDirectory(string path)
        {
            this.path = path;
            this.apps = new List<SteamApp>();

            //Listing of all the files in this directory
            //to detect the manifests

            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (fileName.StartsWith("appmanifest_") && fileName.EndsWith(".acf"))
                {
                    //Creation of one SteamApp per app manifest
                    SteamApp steamApp = new SteamApp(file, this);
                    apps.Add(steamApp);
                }
            }
        }
    }
}
