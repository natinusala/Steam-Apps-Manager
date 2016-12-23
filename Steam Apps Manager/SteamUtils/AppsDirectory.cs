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
        private string path;
        public List<SteamApp> apps { get; private set; }

        public AppsDirectory(string path)
        {
            apps = new List<SteamApp>();

            //Listing of all the files in this directory
            //to detect the manifests

            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (fileName.StartsWith("appmanifest_") && fileName.EndsWith(".acf"))
                {
                    //Creation of one SteamApp per app manifest
                    SteamApp steamApp = new SteamApp(file);
                    apps.Add(steamApp);
                }
            }
        }
    }
}
