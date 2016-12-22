using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam_Apps_Manager.SteamUtils
{
    class SteamUtils
    {
        private static readonly string[] STEAM_REG_KEYS = {
            "SOFTWARE\\WOW6432Node\\Valve\\Steam",
            "SOFTWARE\\Valve\\Steam"
        }; 

        public static string GetSteamInstallDirectory()
        {
            RegistryKey Key = null;

            foreach(string CurrentKey in STEAM_REG_KEYS)
            {
                Key = Registry.LocalMachine.OpenSubKey(CurrentKey);

                if (Key != null)
                    break;
            }

            if (Key == null)
            {
                return null;
            }
            else
            {
                return (string) Key.GetValue("InstallPath");
            }
        }

        public static bool IsSteamRunning()
        {
            Process[] Processes = Process.GetProcessesByName("Steam");
            return Processes.Length > 0;
        }
    }
}
