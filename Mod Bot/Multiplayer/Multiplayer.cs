using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using Newtonsoft.Json;
using InternalModBot;

namespace ModLibrary
{
    public static class Multiplayer
    {
        public static ModNetworkManager NetworkManager;
        public static NetworkClient Client;

        public static ModdedNetworkPlayer LocalPlayer;

        public static List<ModdedNetworkPlayer> Players { get; internal set; } = new List<ModdedNetworkPlayer>();

        public static List<Color> PossiblePlayerColors = new List<Color>(MultiplayerUtils.GetHumanFactSetColors())
        {
            MultiplayerUtils.GetColorFromRGB(255, 050, 050), // Red
            MultiplayerUtils.GetColorFromRGB(109, 054, 000), // Brown
            MultiplayerUtils.GetColorFromRGB(234, 146, 014), // Orange
            MultiplayerUtils.GetColorFromRGB(170, 170, 170), // Gray/Silver
            MultiplayerUtils.GetColorFromRGB(218, 165, 032), // Gold
        };

        private static List<SyncAcrossClients> ObjectsToSyncAcrossClients = new List<SyncAcrossClients>();

        public static void StartServer(int port = 8606)
        {
            GameObject networkHolder = new GameObject();
            NetworkManager = networkHolder.AddComponent<ModNetworkManager>();
            NetworkManager.networkPort = port;
            GeneralSetup();
            Client = NetworkManager.StartHost();
            NetworkMessageManager.SetupHandelers();
        }
        public static void StartClient(string ip = "localhost", int port = 8606)
        {
            GameObject networkHolder = new GameObject();
            NetworkManager = networkHolder.AddComponent<ModNetworkManager>();
            GeneralSetup();


            Client = NetworkManager.StartClient();
            


            Client.Connect(ip,port);

            NetworkMessageManager.SetupHandelers();
        }

        public static void SendMsg()
        {
            throw new NotImplementedException();
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





        static void GeneralSetup()
        {
            GameObject player = new GameObject();
            player.AddComponent<ModdedNetworkPlayer>();
            player.AddComponent<NetworkIdentity>();
            ClientScene.RegisterPrefab(player, NetworkHash128.Parse(player.name));
            NetworkManager.playerPrefab = player;

            NetworkManager.connectionConfig.NetworkDropThreshold = 45;
            NetworkManager.connectionConfig.OverflowDropThreshold = 45;
            NetworkManager.connectionConfig.AckDelay = 200;
            NetworkManager.connectionConfig.AcksType = ConnectionAcksType.Acks128;
            NetworkManager.connectionConfig.MaxSentMessageQueueSize = 300;
        }

    }

    
    public class ModNetworkManager : NetworkManager
    {
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
        }
    }


    public class SyncAcrossClients : MonoBehaviour
    {

    }

    public class ModdedNetworkPlayer : NetworkBehaviour
    {
        public FirstPersonMover PhysicalPlayer;
        static bool hasStartedBolt = false;

        bool isFakePlayer = true;

        public static float sendRate = 0.1f;
        public static List<KeyCode> KeysToSync = new List<KeyCode>()
        {
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.W,
            KeyCode.A,
            KeyCode.S,
            KeyCode.D,
            KeyCode.F,
            KeyCode.LeftShift,
            KeyCode.E,
            KeyCode.Space,
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3
        };
        float timer;
        public void Start()
        {
            if (isFakePlayer) // If player is prefab
            {
                Multiplayer.Players.Remove(this);
                return;
            }

            if (isLocalPlayer)
            {
                Multiplayer.LocalPlayer = this;
                BoltGlobalEventListenerSingleton<InternalModBot.ModdedBoltServerStarter>.Instance.StartServerThenCall(delegate   // Not the actual server, clone drone needs bolt to survive.
                {
                    hasStartedBolt = true;
                });
            }
            else if (!hasStartedBolt)
            {
                Delayed.TriggerAfterDelay(new fakeAction(typeof(ModdedNetworkPlayer).GetMethod("Start"), this), 1);

                return;
            }

            if (isServer)
            {

                PlayerCreateMessage playerCreateMessage = new PlayerCreateMessage();
                Color playerColor = MultiplayerUtils.GetNewPlayerColor();
                playerCreateMessage.PlayerR = playerColor.r;
                playerCreateMessage.PlayerG = playerColor.g;
                playerCreateMessage.PlayerB = playerColor.b;

                playerCreateMessage.Id = base.netId.Value;
                NetworkServer.SendToAll((short)MsgIds.CreatePlayerMessage, playerCreateMessage);
            }

            /*if (!isLocalPlayer)
            {
                else
                {
                    Transform spawnPoint = new GameObject().transform;
                    PhysicalPlayer = GameFlowManager.Instance.SpawnPlayer(spawnPoint, true, false, Color.red);
                    PhysicalPlayer.entity.TakeControl(); // this took like 4 hours to find out that you need this, it turns out that you need this for the player to work.

                    PlayerInputController playerControll = PhysicalPlayer.GetComponent<PlayerInputController>();
                    if (playerControll != null)
                    {
                        Destroy(playerControll);
                    }

                    AISwordsmanController aISwordsmanController = PhysicalPlayer.GetComponent<AISwordsmanController>();
                    if (aISwordsmanController != null)
                    {
                        Destroy(aISwordsmanController);
                    }
                }
            }*/
            
            
        }

        public void CreatePhysicalPlayer(Vector3 SpawnPosition, Color PlayerColor)
        {

            if (!hasStartedBolt)
            {
                Delayed.TriggerAfterDelay(new fakeAction(typeof(ModdedNetworkPlayer).GetMethod("CreatePhysicalPlayer"), this, new object[] { SpawnPosition, PlayerColor }), 1);
                return;
            }

            Transform spawnPoint = new GameObject().transform;
            spawnPoint.position = SpawnPosition;
            PhysicalPlayer = GameFlowManager.Instance.SpawnPlayer(spawnPoint, true, isLocalPlayer, PlayerColor);

            if (!isLocalPlayer)
            {
                PhysicalPlayer.entity.TakeControl(); // This took like 4 hours to find out that you need this, it turns out that you need this for the player to work.
            }
            

        }

        public override void OnStartServer()
        {
            
           
            

        }
        public override void OnStartClient()
        {
            isFakePlayer = false;

            if (!isServer)
            {

            }

        }
        public void Update()
        {
            if (isFakePlayer)
                return;
            
            if (!isLocalPlayer)
                return;
            
            if (!Singleton<InputManager>.Instance.IsCursorEnabled())
            {
                if (PhysicalPlayer != null)
                {
                    float x = (float)Accessor.GetPrivateField(typeof(FirstPersonMover), "_horizontalCursorMovement", PhysicalPlayer);
                    float y = (float)Accessor.GetPrivateField(typeof(FirstPersonMover), "_verticalCursorMovement", PhysicalPlayer);
                    if (x != 0 || y != 0) // to not send any unnessesary packages
                    {
                        MouseOffsetMessage msg = new MouseOffsetMessage();
                        msg.MouseX = x;
                        msg.MouseY = y;
                        msg.Id = netId.Value;
                        base.connectionToServer.Send((short)MsgIds.MouseOffsetEvent, msg);
                    }

                }

                for (int i = 0; i < KeysToSync.Count; i++)
                {
                    if (Input.GetKeyDown(KeysToSync[i]))
                    {
                        KeyPressEventMessage msg = new KeyPressEventMessage();
                        msg.Id = netId.Value;
                        msg.Key = KeysToSync[i];
                        msg.Down = true;
                        base.connectionToServer.Send((short)MsgIds.KeyPressEvent, msg);
                    }
                    else if (Input.GetKeyUp(KeysToSync[i]))
                    {
                        KeyPressEventMessage msg = new KeyPressEventMessage();
                        msg.Id = netId.Value;
                        msg.Key = KeysToSync[i];
                        msg.Down = false;
                        base.connectionToServer.Send((short)MsgIds.KeyPressEvent, msg);
                    }
                }
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

        public static ModdedNetworkPlayer GetPlayerWithID(uint id)
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
            if (isFakePlayer)
                return;

            Multiplayer.Players.Remove(this);
            Destroy(PhysicalPlayer);
            if (isLocalPlayer)
            {
                connectionToServer.Disconnect();
            }
        }
    }

    
}
