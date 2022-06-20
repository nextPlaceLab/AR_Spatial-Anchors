using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Storage;
#endif
namespace LLE.Unity
{
    class FileIO
    {
        public static string home;
        public static bool Write(string file, string content)
        {
            string filePath = ResolveFullPath(file);
            if (!File.Exists(filePath))
                File.Create(filePath);
            if (File.Exists(filePath))
            {
                File.WriteAllText(filePath, content);
                Debug.Log("Saved to file: " + filePath);
                return true;
            }
            else
            {
                Debug.LogError("Could NOT write to file: " + filePath);
                return false;
            }

        }
        private static string ResolveFullPath(string filePath)
        {
            return Path.Combine(home, filePath);
        }

        internal static void InitHomeOnUiThread()
        {
            home = GetHome();
        }

        // uh...read non exisitng file will create it -> refactor
        public static string Read(string file)
        {
            string filePath = ResolveFullPath(file);

            string content = "";
            try
            {
                if (File.Exists(filePath))
                    content = File.ReadAllText(filePath).Trim();
                else
                    Debug.Log("File not exist: " + filePath);

            }
            catch (Exception e)
            {
                Debug.LogError("Exception: " + e);
            }
            return content;
        }

        private static string GetHome()
        {
            string homePath = Application.persistentDataPath;
            Debug.Log("PersistentDataPath: " + homePath);
#if WINDOWS_UWP
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            homePath = storageFolder.Path.Replace('\\', '/') + "/";
            Debug.Log("UWP Path: " + homePath);
#endif
            return homePath;
        }
    }
}

