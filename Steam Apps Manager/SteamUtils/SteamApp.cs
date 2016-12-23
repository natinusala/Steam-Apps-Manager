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

    class SteamApp
    {
        private string manifestPath;

        private int appId;
        public string appName { get; private set; }
        private string installDir;
        private long bytesToDownload;

        public SteamApp(string manifestPath)
        {
            this.manifestPath = manifestPath;

            ACFNode appState = (ACFNode) ACFNode.ParseACF(manifestPath)["AppState"];

            this.appId = int.Parse(((string)appState["appid"]));
            this.appName = (string)appState["name"];
            this.installDir = (string)appState["installdir"];
            this.bytesToDownload = long.Parse((string)appState["BytesToDownload"]);
        }

        public SteamAppStatus GetStatus()
        {
            return this.bytesToDownload == 0 ? SteamAppStatus.READY_TO_MOVE : SteamAppStatus.UPDATE_NEEDED;
        }
    }
}
