using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModLibrary;
using InternalModBot;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

namespace InternalModBot.Multiplayer
{
    public static class MultiplayerMessages
    {
        private static NetworkManager networkManager;
        public static void SetupHandelers()
        {
            networkManager = NetworkManager.singleton;

            networkManager.client.RegisterHandler(MsgIds.SpawnPlayerMessage, OnSpawnedPlayerMsg);
            NetworkServer.RegisterHandler(MsgIds.SpawnPlayerMessage, OnSpawnedPlayerMsg);
        }

        private static void OnSpawnedPlayerMsg(NetworkMessage netMsg)
        {
            SpawnPlayerMessage spawnPlayerMessage = netMsg.ReadMessage<SpawnPlayerMessage>();
            Color color = new Color(spawnPlayerMessage.R, spawnPlayerMessage.G, spawnPlayerMessage.B);
            ModdedNetworkPlayer.GetPlayerWithUID(spawnPlayerMessage.playerId).SpawnPhysicalPlayer(spawnPlayerMessage.position, color);
        }


    }
    public static class MsgIds
    {
        public static short SpawnPlayerMessage = 100;
    }
    public class SpawnPlayerMessage : MessageBase
    {
        public uint playerId;
        public float R = 0;
        public float G = 0;
        public float B = 0;
        public Vector3 position;

        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            SpawnPlayerMessage msg = JsonConvert.DeserializeObject<SpawnPlayerMessage>(json);
            playerId = msg.playerId;
            R = msg.R;
            G = msg.G;
            B = msg.B;
            position = msg.position;
        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }


    }
}
