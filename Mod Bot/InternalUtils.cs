using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ModLibrary;
using UnityEngine;

namespace InternalModBot
{
    public static class InternalUtils
    {
        public static bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool result = true;
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        if (!chain.Build((X509Certificate2)certificate))
                        {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        public static void SaveFileToDirectory(string URL, string Path)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(MyRemoteCertificateValidationCallback);
            byte[] data = new WebClient
            {
                Headers =
                {
                    "User-Agent: Other"
                }
            }.DownloadData(URL);

            if (!Directory.Exists(AssetLoader.getSubdomain(Path)))
            {
                Directory.CreateDirectory(AssetLoader.getSubdomain(Path));
                debug.Log("Successfully created directory '" + AssetLoader.getSubdomain(Path) + "'", Color.green);
            }
            if (!File.Exists(Path))
            {
                FileStream file = File.Create(Path);
                file.Close();
                debug.Log("Successfully created file '" + Path + "'", Color.green);
            }
            if (File.ReadAllBytes(Path) == data)
            {
                debug.Log("Did not write to file '" + Path + "' (URL and local data is identical)", Color.green);
            }
            else
            {
                File.WriteAllBytes(Path, data);
                debug.Log("Successfully wrote data from '" + URL + "' into '" + Path + "'", Color.green);
            }
        }
    }
}
