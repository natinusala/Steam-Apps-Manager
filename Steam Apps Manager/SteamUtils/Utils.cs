﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam_Apps_Manager.SteamUtils
{
    class Utils
    {
        private static readonly string[] STEAM_REG_KEYS = {
            "SOFTWARE\\WOW6432Node\\Valve\\Steam",
            "SOFTWARE\\Valve\\Steam"
        }; 

        public static string GetBaseSteamAppsPath()
        {
            return GetSteamInstallDirectory() + "\\steamapps";
        }

        public static string GetLibraryFolderVDFPath()
        {
            return GetBaseSteamAppsPath() + "\\libraryfolders.vdf";
        }

        public static long GetDirectorySize(string path)
        {
            try
            {
                long totalBytes = 0;
                DirectoryInfo rootDirectory = new DirectoryInfo(path);

                FileInfo[] files = rootDirectory.GetFiles();

                foreach (FileInfo file in files)
                {
                    totalBytes += file.Length;
                }

                DirectoryInfo[] directories = rootDirectory.GetDirectories();

                foreach (DirectoryInfo directory in directories)
                {
                    totalBytes += GetDirectorySize(directory.FullName);
                }

                return totalBytes;
            } catch(Exception e)
            {
                return 0;
            }
        }

        public static string FormatGameList(List<SteamUtils.App> games, int maxChars)
        {
            string formattedList = "";
            int overflowAmount = 0;

            foreach (SteamUtils.App app in games)
            {
                if (formattedList.Length + app.appName.Length + 13 <= maxChars)
                    formattedList += ", " + app.appName;
               
                else overflowAmount++;
            }

            formattedList = formattedList.Substring(2);

            if (overflowAmount > 0)
                formattedList += " + " + overflowAmount + " game";
            if (overflowAmount > 1)
                formattedList += "s";

            return formattedList;
        }

        public static bool IsDirectoryEmpty(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            FileInfo[] files = directory.GetFiles();
            DirectoryInfo[] subdirs = directory.GetDirectories();

            return (files.Length == 0 && subdirs.Length == 0);
        }

        public static string GetSteamInstallDirectory()
        {
            RegistryKey key = null;

            foreach(string currentKey in STEAM_REG_KEYS)
            {
                key = Registry.LocalMachine.OpenSubKey(currentKey);

                if (key != null)
                    break;
            }

            if (key == null)
            {
                return null;
            }
            else
            {
                return (string) key.GetValue("InstallPath");
            }
        }

        public static string GetSteamDLLPath()
        {
            return GetSteamInstallDirectory() + "\\Steam.dll";
        }

        public static bool IsSteamRunning()
        {
            Process[] Processes = Process.GetProcessesByName("Steam");
            return Processes.Length > 0;
        }
    }
}
