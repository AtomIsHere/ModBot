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

        public static void OnDeath(Character target, Character killer, DamageSourceType damageSource)
        {
            if (Network.isServer)
            {
                ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithPhysicalPlayer((FirstPersonMover)target);
                ModdedNetworkPlayer killerPlayer = ModdedNetworkPlayer.GetPlayerWithPhysicalPlayer((FirstPersonMover)killer);
                uint playerId = player.netId.Value;
                uint? killerId = killerPlayer == null ? null : (uint?)killerPlayer.netId.Value;

                PlayerDeathMessage deathMsg = new PlayerDeathMessage();
                deathMsg.Id = playerId;
                deathMsg.KillerId = killerId;
                deathMsg.damageSource = damageSource;

            }
        }
    }
}
