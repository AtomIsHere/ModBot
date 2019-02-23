using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using Newtonsoft.Json;

namespace ModLibrary
{
    public static class Multiplayer
    {
        private static List<SyncAcrossClients> ObjectsToSyncAcrossClients = new List<SyncAcrossClients>();
        public static NetworkManager networkManager;
        public static NetworkClient client;

        internal static NetworkPlayer localPlayer;

        public static void StartServer()
        {
            GameObject networkHolder = new GameObject();
            networkManager = networkHolder.AddComponent<NetworkManager>();
            debug.Log(networkManager.networkPort.ToString());
            debug.Log(networkManager.networkAddress);
            GeneralSetup();
            client = networkManager.StartHost();
            SetupHandelers();
        }

        public static void StartClient(string ip = "localhost", int port = 7777)
        {
            GameObject networkHolder = new GameObject();
            networkManager = networkHolder.AddComponent<NetworkManager>();
            GeneralSetup();


            client = networkManager.StartClient();
            


            client.Connect(ip,port);

            SetupHandelers();
        }

        static void GeneralSetup()
        {
            
            GameObject player = new GameObject();
            player.AddComponent<NetworkPlayer>();
            player.AddComponent<NetworkIdentity>();
            ClientScene.RegisterPrefab(player, NetworkHash128.Parse(player.name));
            networkManager.playerPrefab = player;
        }
        static void SetupHandelers()
        {
            client.RegisterHandler((short)MsgIds.test, TestMsg);
            NetworkServer.RegisterHandler((short)MsgIds.test, TestMsg);
        }

        public static void StartTrackingObject(GameObject objectToTrack)
        {
            SyncAcrossClients sync = objectToTrack.AddComponent<SyncAcrossClients>();

            ObjectsToSyncAcrossClients.Add(sync);
        }
        public static void StopTrackingObject(GameObject objectToTrack)
        {
            SyncAcrossClients sync = objectToTrack.GetComponent<SyncAcrossClients>();
            if (sync == null)
                return;

            Component.Destroy(sync);
        }

        

        static void TestMsg(NetworkMessage msg)
        {
            StringMessage myMsg = msg.ReadMessage<StringMessage>();
            debug.Log(myMsg.value);

        }
        static void StandardMsg(NetworkMessage msg)
        {

        }

        public static void SendMsg()
        {

        }
        

    }


    internal class SyncAcrossClients : MonoBehaviour
    {

    }
    internal enum MsgIds : short
    {
        test = 252

    }
    internal class NetworkPlayer : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            
            debug.Log("server running!");
            debug.Log("ip: " + Multiplayer.client.serverIp + ", port: " + Multiplayer.client.hostPort);
            debug.Log(Multiplayer.client.isConnected.ToString());
            

        }
        public override void OnStartClient()
        {
            Multiplayer.localPlayer = this;
            debug.Log("client running!");
            debug.Log("ip: " + Multiplayer.client.serverIp + ", port: " + Multiplayer.client.hostPort);
            debug.Log(Multiplayer.client.isConnected.ToString());
        }
        public void Update()
        {
            if (!isLocalPlayer)
                return;
            if (!isServer)
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    Multiplayer.client.Send((short)MsgIds.test, new StringMessage("test"));
                }
            } else
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    for (int i = 1; i < NetworkServer.connections.Count; i++)
                    {
                        NetworkServer.SendToClient(i, (short)MsgIds.test, new StringMessage("test"));
                    }
                    
                }
            }
        
        }
    }

    public class testMultiplayerMod : ModLibrary.Mod
    {
        public override void OnCommandRan(string command)
        {
            if (command.StartsWith("connect"))
            {
                Multiplayer.StartClient();
                debug.Log("starting client...");
            }
            if (command.StartsWith("server"))
            {
                Multiplayer.StartServer();
                debug.Log("starting server...");
            }
        }
    }

    public class TransformMessage : MessageBase
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            TransformMessage msg = JsonConvert.DeserializeObject<TransformMessage>(json);
            Position = msg.Position;
            Rotation = msg.Rotation;
        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }

        public void SetTransformOfGameObjectObject(GameObject gameObject)
        {
            gameObject.transform.position = Position;
            gameObject.transform.rotation = Rotation;
        }
    }
}
