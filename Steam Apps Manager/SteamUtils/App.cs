using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam_Apps_Manager.SteamUtils
{
    enum SteamAppStatus
    {
        READY_TO_MOVE,
        UPDATE_NEEDED
    }

    class App
    {
        private string manifestPath;

        private uint appId;
        public string appName { get; private set; }
        private string installDir;
        private string appState;
        public ulong sizeOnDisk { get; private set; }

        public LibraryFolder directory { get; private set; }

        public App(string manifestPath, LibraryFolder directory)
        {
            this.manifestPath = manifestPath;
            this.directory = directory;

            ACFNode appState = (ACFNode) ACFNode.ParseACF(manifestPath)["AppState"];

            this.appId = uint.Parse(((string)appState["appid"]));
            this.appName = (string)appState["name"];
            this.installDir = (string)appState["installdir"];
            this.appState = (string)appState["StateFlags"];

            if (appState.ContainsKey("SizeOnDisk"))
            {
                sizeOnDisk = ulong.Parse(((string)appState["SizeOnDisk"]));
            }
            else
            {
                this.sizeOnDisk = 0;
            }
        }

        public SteamAppStatus GetStatus()
        {
            return this.appState == "4" ? SteamAppStatus.READY_TO_MOVE : SteamAppStatus.UPDATE_NEEDED;
        }
    }
}
