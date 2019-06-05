using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModLibrary;

namespace InternalModBot
{
    // methods that I cant really put anywhere else
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
        public static Color GetRandomHumanColor()
        {
            return HumanFactsManager.Instance.FavouriteColors[HumanFactsManager.Instance.GetRandomFactSet().FavouriteColorIndex].ColorValue;
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