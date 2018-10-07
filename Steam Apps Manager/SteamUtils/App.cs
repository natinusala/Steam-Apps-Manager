using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam_Apps_Manager.SteamUtils
{
    public enum SteamAppStatus
    {
        READY_TO_MOVE,
        UPDATE_NEEDED
    }

    public class App
    {
        private string manifestPath;

        public uint appId { get; private set; }
        public string appName { get; private set; }
        private string installDir;
        private string fullInstallDir;
        private int appState;
        public long sizeOnDisk { get; private set; }

        public LibraryFolder folder { get; private set; }

        public App(string manifestPath, LibraryFolder directory)
        {
            this.manifestPath = manifestPath;
            this.folder = directory;

            ACFNode appState = (ACFNode) ACFNode.ParseACF(manifestPath)["AppState"];

            this.appId = uint.Parse(((string)appState["appid"]));
            this.appName = (string)appState["name"];
            this.installDir = (string)appState["installdir"];
            this.fullInstallDir = this.folder.path + "\\common\\" + installDir;
            this.appState = int.Parse((string)appState["StateFlags"]);

            this.sizeOnDisk = Utils.GetDirectorySize(this.fullInstallDir);

            Console.WriteLine(this.fullInstallDir);
            Console.WriteLine("Size of " + this.appName + " is " + this.sizeOnDisk);
        }

        public string GetManifestPath()
        {
            return this.folder.path + "\\" + "appmanifest_" + this.appId + ".acf";
        }

        public SteamAppStatus GetStatus()
        {
            return this.appState == 4 ? SteamAppStatus.READY_TO_MOVE : SteamAppStatus.UPDATE_NEEDED;
        }

        public void MoveAppFiles(LibraryFolder newFolder, BackgroundWorker worker)
        {
            string newFullDir = newFolder.path + "\\common\\" + installDir;

            foreach(string path in Directory.GetDirectories(this.fullInstallDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(path.Replace(fullInstallDir, newFullDir));
            }

            foreach (string file in Directory.GetFiles(fullInstallDir, "*.*", SearchOption.AllDirectories))
            {
                FileInfo info = new FileInfo(file);
                long fileSize = info.Length;

                File.Move(file, file.Replace(fullInstallDir, newFullDir));

                if (worker != null && fileSize != 0)
                {
                    worker.ReportProgress((int)Math.Round(((double)fileSize / (double)sizeOnDisk) * 1000.0f));
                }
            }

            worker.ReportProgress(-1);

            Directory.Delete(this.fullInstallDir, true);

            this.folder = newFolder;
            this.fullInstallDir = this.folder.path + "\\common\\" + installDir;
        }
    }
}
