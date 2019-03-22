using ModLibrary;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
            ((Text)_moddedObject.objects[0]).font = coolFont;
            ((Button)_moddedObject.objects[1]).transform.GetChild(0).GetComponent<Text>().font = coolFont;
            ((Button)_moddedObject.objects[1]).onClick.AddListener(StartServerClicked);
            port = ((InputField)_moddedObject.objects[2]);
            ((Button)_moddedObject.objects[3]).transform.GetChild(0).GetComponent<Text>().font = coolFont;
            ((Button)_moddedObject.objects[3]).onClick.AddListener(StartClientClicked);
            ip = ((InputField)_moddedObject.objects[4]);

            ((GameObject)_moddedObject.objects[5]).GetComponent<Button>().onClick.AddListener(Hide);
        }

        private InputField ip;
        private InputField port;

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

            int _port = 8606;
            if (port.text != "")
            {
                _port = Convert.ToInt32(port.text);
            }
            Multiplayer.StartServer(_port);
        }
        public void StartClientClicked()
        {
            GeneralSetup();

            if (ip == null)
                Debug.LogError("MODBOT: IP is null");

            if (ip.text == "")
                ip.text = "localhost";

            string[] _ip = ip.text.Split(':');

            int _port = 8606;
            if (_ip.Length > 1)
            {
                _port = Convert.ToInt32(_ip[1]);
            }

            Multiplayer.StartClient(_ip[0], _port);
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

            InternalUtils.SaveFileToDirectory(moddedLevelDownloadLink, moddedLevelJsonPath);

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
        public static GameData CreateNewGameData()
        {
            GameData gameData = new GameData
            {
                NumClones = Singleton<CloneManager>.Instance.GetNumStartingClones(),
                NumLevelsWon = 0,
                AvailableSkillPoints = 0,
                PlayerUpgrades = Singleton<UpgradeManager>.Instance.CreateDefaultPlayerUpgrades(),
                HumanFacts = Singleton<HumanFactsManager>.Instance.GetRandomFactSet(),
                LevelIDsBeatenThisPlaythrough = new List<string>(),
                LevelPrefabsBeatenThisPlaythrough = new List<string>(),
                PlayerArmorParts = new List<MechBodyPartType>(),
                PlayerBodyPartDamages = new List<MechBodyPartDamage>()
            };

            gameData.SetDirty(true);

            return gameData;
        }
        public static void OnSimulateController(GameObject _gameObject)
        {
            if (Multiplayer.LocalPlayer == null)
                return;
            Multiplayer.LocalPlayer.OnSimulateController(_gameObject);

        }

        public static Color GetColorFromRGB(float r, float g, float b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static List<Color> GetHumanFactSetColors()
        {
            List<Color> colors = new List<Color>();
            HumanFavouriteColor[] humanColors = HumanFactsManager.Instance.FavouriteColors;

            for (int i = 0; i < humanColors.Length; i++)
            {
                colors.Add(humanColors[i].ColorValue);
            }

            return colors;
        }

        public static Color GetNewPlayerColor()
        {
            return Multiplayer.PossiblePlayerColors[UnityEngine.Random.Range(0, Multiplayer.PossiblePlayerColors.Count - 1)];
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