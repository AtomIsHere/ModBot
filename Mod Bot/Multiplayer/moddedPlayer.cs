using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using ModLibrary;
using InternalModBot;


namespace InternalModBot.Multiplayer
{
    public class ModdedNetworkPlayer : NetworkBehaviour
    {
        bool isFake = true;

        public override void OnStartClient()
        {
            isFake = false;
            debug.Log("I exist!");
        }

        private void Start()
        {
            debug.Log("nulk");
            if (isServer)
            {
                SpawnPlayerMessage spawnPlayerMessage = new SpawnPlayerMessage();
                Color humanColor = MultiplayerUtils.GetRandomHumanColor();
                spawnPlayerMessage.R = humanColor.r;
                spawnPlayerMessage.G = humanColor.g;
                spawnPlayerMessage.B = humanColor.b;
                spawnPlayerMessage.position = new Vector3(0, 2, 0);
                spawnPlayerMessage.playerId = netId.Value;

                NetworkServer.SendToAll(MsgIds.SpawnPlayerMessage, spawnPlayerMessage);
            }
        }



        public FirstPersonMover Player;
        public void SpawnPhysicalPlayer(Vector3 position, Color color)
        {
            Transform spawnPoint = new GameObject().transform;
            spawnPoint.transform.position = position;
            GameFlowManager.Instance.SpawnPlayer(spawnPoint, true, isLocalPlayer, color);

            GameObject.Destroy(spawnPoint);
        }


        private void Awake()
        {
            moddedNetworkPlayers.Add(this);
        }
        private void OnDestroy()
        {
            moddedNetworkPlayers.Remove(this);
        }
        private static List<ModdedNetworkPlayer> moddedNetworkPlayers = new List<ModdedNetworkPlayer>();
        public static ModdedNetworkPlayer GetPlayerWithUID(uint id)
        {
            for (int i = 0; i < moddedNetworkPlayers.Count; i++)
            {
                if (moddedNetworkPlayers[i].netId.Value == id)
                {
                    return moddedNetworkPlayers[i];
                }
            }
            return null;
        }

    }
}
