using ModLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InternalModBot
{
    public class MultiplayerMenuManager : MonoBehaviour
    {
        GameObject ModdedMultiplayerMenu;
        void Start()
        {
            gameObject.AddComponent<ModdedBoltServerStarter>();

            UnityEngine.Events.UnityEvent _event = new UnityEngine.Events.UnityEvent();
            _event.AddListener(OpenModdedMultiplayerMenu);

            GameModeCardData gameModeCardData = new GameModeCardData
            {
                NameOfMode = "Modded",
                Description = "Play multiplayer with mods!",
                ThumbnailSprite = null,
                ClickedCallback = _event
            };
            List<GameModeCardData> temp = GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.GameModeData.ToList();

            temp.Add(gameModeCardData);

            GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.GameModeData = temp.ToArray();


            ModdedMultiplayerMenu = Instantiate(AssetLoader.getObjectFromFile("twitchmode", "ModdedMultiplayerPanel", "Clone Drone in the Danger Zone_Data/"));

            ModdedMultiplayerMenu.SetActive(false);
            ModdedMultiplayerMenu.transform.SetParent(GameUIRoot.Instance.TitleScreenUI.transform, false);
            moddedObject _moddedObject = ModdedMultiplayerMenu.GetComponent<moddedObject>();
            Font coolFont = GameUIRoot.Instance.TitleScreenUI.DuelInviteMenu.GoButtonText.font;
            ((UnityEngine.UI.Text)_moddedObject.objects[0]).font = coolFont;
            ((UnityEngine.UI.Button)_moddedObject.objects[1]).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().font = coolFont;
            ((UnityEngine.UI.Button)_moddedObject.objects[1]).onClick.AddListener(StartServerClicked);
            ((UnityEngine.UI.Button)_moddedObject.objects[3]).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().font = coolFont;
            ((UnityEngine.UI.Button)_moddedObject.objects[3]).onClick.AddListener(StartClientClicked);

            ((GameObject)_moddedObject.objects[5]).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Hide);
        }

        public void Hide()
        {
            ModdedMultiplayerMenu.SetActive(false);
            GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.Show();
        }




        public void OpenModdedMultiplayerMenu()
        {
            ModdedMultiplayerMenu.SetActive(true);
            GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.Hide();
        }


        public void StartServerClicked()
        {
            GeneralSetup();


            Multiplayer.StartServer();
        }
        public void StartClientClicked()
        {
            GeneralSetup();

            Multiplayer.StartClient();
           
            
        }
        void GeneralSetup()
        {
            Accessor.SetPrivateField(typeof(GameFlowManager), "_gameMode", GameFlowManager.Instance, GameMode.SingleLevelPlaytest);
            


            GameData data = MultiplayerUtils.createNewGameData();
            data.CurentLevelID = "modded-test1";
            Accessor.SetPrivateField(typeof(GameDataManager), "_singleLevelPlaytestData", GameDataManager.Instance, data);

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

            LevelManager.Instance.SpawnCurrentLevel();

            GameFlowManager.Instance.HideTitleScreen(true);
        }



        
    }


    

    public static class MultiplayerUtils
    {
        public static GameData createNewGameData()
        {
            GameData gameData = new GameData();
            gameData.NumClones = Singleton<CloneManager>.Instance.GetNumStartingClones();
            gameData.NumLevelsWon = 0;
            gameData.AvailableSkillPoints = 0;
            gameData.PlayerUpgrades = Singleton<UpgradeManager>.Instance.CreateDefaultPlayerUpgrades();
            gameData.HumanFacts = Singleton<HumanFactsManager>.Instance.GetRandomFactSet();
            gameData.LevelIDsBeatenThisPlaythrough = new List<string>();
            gameData.LevelPrefabsBeatenThisPlaythrough = new List<string>();
            gameData.SetDirty(true);
            gameData.PlayerArmorParts = new List<MechBodyPartType>();
            gameData.PlayerBodyPartDamages = new List<MechBodyPartDamage>();
            return gameData;
        }
    }
    
    public class ModdedBoltServerStarter : BoltGlobalEventListenerSingleton<ModdedBoltServerStarter>
    {
        

        public void StartServerThenCall(CallBackDeligate callback)
        {
            if (this.IsStartingGame())
            {
                return;
            }
            this._startedCallback = callback;
            BoltRuntimeSettings.instance.overrideTimeScale = false;
            BoltLauncher.StartSinglePlayer();
        }
        
        public override void BoltStartDone()
        {
            if (this._startedCallback != null)
            {
                this._startedCallback();
                this._startedCallback = null;
            }
        }
        
        public bool IsStartingGame()
        {
            return this._startedCallback != null;
        }

        public delegate void CallBackDeligate();
        private CallBackDeligate _startedCallback;
    }

}