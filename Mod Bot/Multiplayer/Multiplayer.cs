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
        public static ModNetworkManager networkManager;
        public static NetworkClient client;

        internal static NetworkPlayer localPlayer;

        public static List<NetworkPlayer> Players = new List<NetworkPlayer>();

        public static void StartServer(int port = 8606)
        {
            GameObject networkHolder = new GameObject();
            networkManager = networkHolder.AddComponent<ModNetworkManager>();
            networkManager.networkPort = port;
            GeneralSetup();
            client = networkManager.StartHost();
            SetupHandelers();
        }

        public static void StartClient(string ip = "localhost", int port = 8606)
        {
            GameObject networkHolder = new GameObject();
            networkManager = networkHolder.AddComponent<ModNetworkManager>();
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

            networkManager.connectionConfig.NetworkDropThreshold = 45;
            networkManager.connectionConfig.OverflowDropThreshold = 45;
            networkManager.connectionConfig.AckDelay = 200;
            networkManager.connectionConfig.AcksType = ConnectionAcksType.Acks128;
            networkManager.connectionConfig.MaxSentMessageQueueSize = 300;
        }
        static void SetupHandelers()
        {
            client.RegisterHandler((short)MsgIds.test, TestMsg);
            NetworkServer.RegisterHandler((short)MsgIds.test, TestMsg);

            client.RegisterHandler((short)MsgIds.UpdatePlayerTransform, HandleMovePlayerClient);
            NetworkServer.RegisterHandler((short)MsgIds.UpdatePlayerTransform, HandleMovePlayerServer);


            client.RegisterHandler((short)MsgIds.KeyPressEvent, HandleKeyPressClient);
            NetworkServer.RegisterHandler((short)MsgIds.KeyPressEvent, HandleKeyPressServer);
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
        static void HandleMovePlayerServer(NetworkMessage msg)
        {

            PlayerMoveMessage myMsg = msg.ReadMessage<PlayerMoveMessage>();
            NetworkServer.SendUnreliableToAll((short)MsgIds.UpdatePlayerTransform, myMsg);
            

        }
        static void HandleMovePlayerClient(NetworkMessage msg)
        {
            PlayerMoveMessage myMsg = msg.ReadMessage<PlayerMoveMessage>();

            NetworkPlayer player = NetworkPlayer.getPlayerWithID(myMsg.Id);

            if (player == null)
                return;
            if (!player.isLocalPlayer)
            {

                myMsg.SetTransformOfPlayer();
            }
        }
        static void HandleKeyPressServer(NetworkMessage msg)
        {
            KeyPressEventMessage myMsg = msg.ReadMessage<KeyPressEventMessage>();
            NetworkServer.SendToAll((short)MsgIds.KeyPressEvent, myMsg);
        }
        static void HandleKeyPressClient(NetworkMessage msg)
        {
            debug.Log("got keypress...");
            KeyPressEventMessage myMsg = msg.ReadMessage<KeyPressEventMessage>();
            NetworkPlayer player = NetworkPlayer.getPlayerWithID(myMsg.Id);
            if (player == null || player.isLocalPlayer)
                return;
            debug.Log("prossesing keypress...");
            myMsg.SetKeyOfPlayer();
        }

        static void StandardMsg(NetworkMessage msg)
        {

        }

        public static void SendMsg()
        {

        }
        

    }

    
    public class ModNetworkManager : NetworkManager
    {
        public override void OnClientConnect(NetworkConnection conn)
        {

            base.OnClientConnect(conn);
        }
    }


    internal class SyncAcrossClients : MonoBehaviour
    {

    }
    internal enum MsgIds : short
    {
        test = 252,
        UpdatePlayerTransform = 253,
        KeyPressEvent = 254

    }
    public class NetworkPlayer : NetworkBehaviour
    {
        public FirstPersonMover PhysicalPlayer;
        static bool hasStartedBolt = false;

        public static float sendRate = 0.1f;
        float timer;
        public void Start()
        {
            if (!isLocalPlayer)
            {
                if (!hasStartedBolt)
                {
                    Delayed.TriggerAfterDelay(new fakeAction(typeof(NetworkPlayer).GetMethod("Start"), this), 1);
                    
                    return;
                }
                else
                {
                    Transform spawnPoint = new GameObject().transform;
                    PhysicalPlayer = GameFlowManager.Instance.SpawnPlayer(spawnPoint, true, false, Color.red);
                    PlayerInputController playerControll = PhysicalPlayer.GetComponent<PlayerInputController>();
                    if (playerControll != null)
                        Destroy(playerControll);

                    AISwordsmanController aISwordsmanController = PhysicalPlayer.GetComponent<AISwordsmanController>();
                    if (aISwordsmanController != null)
                        Destroy(aISwordsmanController);

                }
            }
            else
            {
                Multiplayer.localPlayer = this;
                BoltGlobalEventListenerSingleton<InternalModBot.ModdedBoltServerStarter>.Instance.StartServerThenCall(delegate   // Not the actual server, clone drone needs bolt to survive.
                {
                    Transform spawnPoint = new GameObject().transform;
                    PhysicalPlayer = GameFlowManager.Instance.SpawnPlayer(spawnPoint, true, true, Color.blue);
                    
                    hasStartedBolt = true;
                });
            }
            
            
        }
        public override void OnStartServer()
        {
            
           
            

        }
        public override void OnStartClient()
        {
            
            

        }
        public void Update()
        {
            if (!isLocalPlayer)
                return;

            /*if (!isServer)
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
            }*/
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                KeyPressEventMessage msg = new KeyPressEventMessage();
                msg.Id = netId.Value;
                msg.Key = KeyCode.Mouse0;
                msg.Down = true;
                base.connectionToServer.Send((short)MsgIds.KeyPressEvent, msg);
            } else if(Input.GetKeyUp(KeyCode.Mouse0))
            {
                KeyPressEventMessage msg = new KeyPressEventMessage();
                msg.Id = netId.Value;
                msg.Key = KeyCode.Mouse0;
                msg.Down = false;
                base.connectionToServer.Send((short)MsgIds.KeyPressEvent, msg);
            }

            if (timer > sendRate)
            {
                if (PhysicalPlayer == null)
                    return;
                PlayerMoveMessage msg = new PlayerMoveMessage();
                msg.Id = netId.Value;
                msg.Position = PhysicalPlayer.transform.position;
                msg.Rotation = PhysicalPlayer.transform.rotation;
                base.connectionToServer.SendUnreliable((short)MsgIds.UpdatePlayerTransform, msg);
                timer = 0;
            }
            timer += Time.deltaTime;
        }

        public static NetworkPlayer getPlayerWithID(uint id)
        {
            for (int i = 0; i < Multiplayer.Players.Count; i++)
            {
                if (Multiplayer.Players[i].netId.Value == id)
                {
                    return Multiplayer.Players[i];
                }
            }
            return null;
        }
        void Awake()
        {
            Multiplayer.Players.Add(this);
        }
        void OnDestroy()
        {
            Multiplayer.Players.Remove(this);
            Destroy(PhysicalPlayer);
            if (isLocalPlayer)
            {
                connectionToServer.Disconnect();
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
    public class PlayerMoveMessage : MessageBase
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public uint Id;
        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            PlayerMoveMessage msg = JsonConvert.DeserializeObject<PlayerMoveMessage>(json);
            Position = msg.Position;
            Rotation = msg.Rotation;
            Id = msg.Id;
        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }

        public void SetTransformOfPlayer()
        {
            NetworkPlayer player = NetworkPlayer.getPlayerWithID(Id);
            if (player == null || player.PhysicalPlayer == null)
                return;
            player.PhysicalPlayer.transform.position = Position;
            player.PhysicalPlayer.transform.rotation = Rotation;
        }
    }

    public class KeyPressEventMessage : MessageBase
    {
        public KeyCode Key;
        public bool Down;
        public uint Id;
        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            KeyPressEventMessage msg = JsonConvert.DeserializeObject<KeyPressEventMessage>(json);
            Key = msg.Key;
            Down = msg.Down;
            Id = msg.Id;
        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }

        public void SetKeyOfPlayer()
        {
            NetworkPlayer player = NetworkPlayer.getPlayerWithID(Id);
            if (player == null || player.PhysicalPlayer == null)
                return;
            if (Key == KeyCode.Mouse0)
            {
                debug.Log("got key mouse0 " + Down);
                Component[] components = player.PhysicalPlayer.gameObject.GetComponents(typeof(Component));
                for (int i = 0; i < components.Length; i++)
                {
                    debug.Log(components[i].ToString());
                    debug.Log("\n\n\n");
                }
                if (Down)
                {
                    player.PhysicalPlayer.SetAttackKeyDown(true);
                }
                player.PhysicalPlayer.SetAttackKeyHeld(Down);
                
            }
        }
    }
}
