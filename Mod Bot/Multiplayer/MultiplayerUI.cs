using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModLibrary;
using InternalModBot;
using InternalModBot.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace InternalModBot.Multiplayer
{
    public class ModdedMultiplayerUIManager : Singleton<ModdedMultiplayerUIManager>
    {
        private GameObject ModdedMultiplayerMenu;
        private InputField ip;
        private InputField port;

        public void Start()
        {

            UnityEngine.Events.UnityEvent _event = new UnityEngine.Events.UnityEvent();
            _event.AddListener(OpenModdedMultiplayerMenu);

            GameModeCardData moddedMultiplayerCard = new GameModeCardData
            {
                NameOfMode = "Modded Multiplayer",
                Description = "a modded version of multiplayer",
                ClickedCallback = _event
            };

            List<GameModeCardData> gameModeCardDatas = GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.GameModeData.ToList();

            gameModeCardDatas.Add(moddedMultiplayerCard);
            GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.GameModeData = gameModeCardDatas.ToArray();


            ModdedMultiplayerMenu = Instantiate(AssetLoader.GetObjectFromFile("twitchmode", "ModdedMultiplayerPanel", "Clone Drone in the Danger Zone_Data/"));

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
        


        public void OpenModdedMultiplayerMenu()
        {
            ModdedMultiplayerMenu.SetActive(true);
            GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.Hide();
        }
        public void Hide()
        {
            ModdedMultiplayerMenu.SetActive(false);
            GameUIRoot.Instance.TitleScreenUI.MultiplayerModeSelectScreen.Show();
        }

        public void StartServerClicked()
        {
            BoltGlobalEventListenerSingleton<ModdedBoltServerStarter>.Instance.StartServerThenCall(delegate   // Not the actual server, clone drone needs bolt to survive.
            {
                int _port = 8606;
                if (port.text != "")
                {
                    _port = Convert.ToInt32(port.text);
                }
                ModdedMultiplayerManager.Instance.StartServer(_port);
            });
        }
        public void StartClientClicked()
        {
            BoltGlobalEventListenerSingleton<ModdedBoltServerStarter>.Instance.StartServerThenCall(delegate   // Not the actual server, clone drone needs bolt to survive.
            {

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

                ModdedMultiplayerManager.Instance.StartClient(_ip[0],_port);
            });
        }

        
    }
}
