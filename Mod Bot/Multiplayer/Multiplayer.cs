using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModLibrary;
using InternalModBot;
using UnityEngine;
using UnityEngine.Networking;
namespace InternalModBot.Multiplayer
{
    public class ModdedMultiplayerManager : Singleton<ModdedMultiplayerManager>
    {
        public void StartServer(int port)
        {
            GeneralSetup();
            debug.Log(port);
            NetworkManager networkManager = NetworkManager.singleton;
            networkManager.networkPort = port;
            networkManager.StartHost();

            MultiplayerMessages.SetupHandelers();
        }
        public void StartClient(string ip, int port)
        {
            GeneralSetup();
            debug.Log(ip + ":" + port);
            
            NetworkClient client = NetworkManager.singleton.StartClient();
            client.Connect(ip, port);

            MultiplayerMessages.SetupHandelers();
        }


        void GeneralSetup()
        {
           
            Accessor.SetPrivateField(typeof(GameFlowManager), "_gameMode", GameFlowManager.Instance, GameMode.SingleLevelPlaytest);



            GameData data = MultiplayerUtils.CreateNewGameData();
            data.CurentLevelID = "modded-test1";
            Accessor.SetPrivateField(typeof(GameDataManager), "_singleLevelPlaytestData", GameDataManager.Instance, data);

            string moddedLevelsPath = Application.persistentDataPath + "/ModdedLevels/";
            string moddedLevelJsonPath = Application.persistentDataPath + "/ModdedLevels/ModdedMultiplayerTestLevel.json";
            const string moddedLevelDownloadLink = "https://cdn.discordapp.com/attachments/526159007442927648/558703015255867410/ModdedMultiplayerTestLevel.json";
            AssetLoader.SaveFileToCustomPath(moddedLevelDownloadLink, moddedLevelJsonPath);


            List<LevelDescription> levelList = new List<LevelDescription>();
            LevelDescription level = new LevelDescription
            {
                LevelID = "modded-test1",
                LevelJSONPath = Application.persistentDataPath + "/ModdedLevels/ModdedMultiplayerTestLevel.json",
                LevelTags = new List<LevelTags>(),
                DifficultyTier = DifficultyTier.DebugTest,
                EnemyCounts = new List<EnemyCount>(),
                IsPlayfabHostedLevel = false,
                IsStreamedMultiplayerLevel = false
            };
            levelList.Add(level);
            Accessor.SetPrivateField(typeof(WorkshopLevelManager), "_levelsForPlaytest", WorkshopLevelManager.Instance, levelList);

            LevelManager.Instance.SpawnCurrentLevelNow();

            GameFlowManager.Instance.HideTitleScreen(true);

            NetworkManager networkManager = new GameObject().AddComponent<ModdedNetworkManager>();

            GameObject Prefab = new GameObject();
            Prefab.AddComponent<ModdedNetworkPlayer>();
            networkManager.playerPrefab = Prefab;
            networkManager.autoCreatePlayer = true;


            
        }
    }
}
