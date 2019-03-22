using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
using InternalModBot;

namespace ModLibrary
{
    public static class AssetLoader
    {
        private static Dictionary<string, UnityEngine.Object> cached = new Dictionary<string, UnityEngine.Object>();

       /// <summary>
       /// Gets a GameObject from a file.
       /// </summary>
       /// <param name="file">The asset file (loaded from the mods folder)</param>
       /// <param name="name">Name of the GamObject to get</param>
       /// <returns</returns>
        public static GameObject getObjectFromFile(string file, string name)
        {
            string key = file + ":" + name;
            if (cached.ContainsKey(key))
            {
                return (GameObject)cached[key];

            }
            
            string path = getSubdomain(Application.dataPath) + "mods/";
            if (!Directory.Exists(path))
            {
                Debug.LogError("This should never, ever, ever happen. If it does something is terribly wrong (there is no mods directory, but you are running a mod)");
                return null;
            }
            WWW www = WWW.LoadFromCacheOrDownload("file:///" + path + file, 1);
            AssetBundle assetBundle = www.assetBundle;
            GameObject result = assetBundle.LoadAssetAsync<GameObject>(name).asset as GameObject;
            www.Dispose();
            assetBundle.Unload(false);
            cached[key] = result;

            return result;
        }

        public static GameObject getObjectFromFile(string file, string name,string _path)
        {
            string key = file + ":" + name;
            if (cached.ContainsKey(key))
            {
                return (GameObject)cached[key];

            }

            string path = getSubdomain(Application.dataPath) + _path;
            if (!Directory.Exists(path))
            {
                Debug.LogError("This should never, ever, ever happen. If it does something is terribly wrong (there is no mods directory, but you are running a mod)");
                return null;
            }
            WWW www = WWW.LoadFromCacheOrDownload("file:///" + path + file, 1);
            AssetBundle assetBundle = www.assetBundle;
            GameObject result = assetBundle.LoadAssetAsync<GameObject>(name).asset as GameObject;
            www.Dispose();
            assetBundle.Unload(false);
            cached[key] = result;

            return result;
        }


        public static T getObjectFromFile<T>(string file, string name) where T : UnityEngine.Object
        {
            string key = file + ":" + name;
            if (cached.ContainsKey(key))
            {
                
                return (T)cached[key];

            }

            string path = getSubdomain(Application.dataPath) + "mods/";
            if (!Directory.Exists(path))
            {
                Debug.LogError("This should never, ever, ever happen. If it does something is terribly wrong (there is no mods directory, but you are running a mod)");
                return null;
            }
            WWW www = WWW.LoadFromCacheOrDownload("file:///" + path + file, 1);
            AssetBundle assetBundle = www.assetBundle;
            T result = assetBundle.LoadAssetAsync<T>(name).asset as T;
            www.Dispose();
            assetBundle.Unload(false);
            cached[key] = result;

            return result;
        }

        /// <summary>
        /// if it there is already a file named the same thing as name, this wont do anything)
        /// </summary>
        public static void trySaveFileToMods(string url, string name)
        {
            string path = getSubdomain(Application.dataPath) + "mods/" + name;
            if (File.Exists(path))
            {
                return;
            }
            saveFileToMods(url, name);
         }

        public static void saveFileToMods(string url, string name)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(InternalUtils.MyRemoteCertificateValidationCallback);
            byte[] file = new WebClient
            {
                Headers =
                {
                    "User-Agent: Other"
                }
            }.DownloadData(url);
            string path = getSubdomain(Application.dataPath) + "mods/";
            var a = File.Create(path + name);
            a.Close();
            File.WriteAllBytes(path + name, file);
        }

        public static void ClearCache()
        {
            cached.Clear();
        }
        public static string getSubdomain(string path)
        {
            string[] subDomains = path.Split(new char[]
            {
            '/'
            });
            string newDomain = "";
            for (int i = 0; i < subDomains.Length - 1; i++)
            {
                newDomain = newDomain + subDomains[i] + "/";
            }
            return newDomain;
        }
    }
}
