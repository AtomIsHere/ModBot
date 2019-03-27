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

        private static List<SyncAcrossClients> ObjectsToSyncAcrossClients = new List<SyncAcrossClients>(); // will be used later

        public static NetworkClient Client;

        // called from the UI when we want to start a new server
        public static void StartServer(int port = 8606)
        {
            NetworkServer.Reset();          //stop some crashes
            NetworkManager.Shutdown();      //stop some crashes

            GameObject networkHolder = new GameObject();
            ModNetworkManager networkManager = networkHolder.AddComponent<ModNetworkManager>();
            networkManager.networkPort = port;
            GeneralSetup();
            Client = networkManager.StartHost();
            NetworkMessageManager.SetupHandelers();
        }
        // called from the UI when we want to start a new client
        public static void StartClient(string ip = "localhost", int port = 8606)
        {
            NetworkManager.Shutdown();
            Network.Disconnect();

            GameObject networkHolder = new GameObject();
            ModNetworkManager networkManager = networkHolder.AddComponent<ModNetworkManager>();
            GeneralSetup();


            Client = networkManager.StartClient();
            


            Client.Connect(ip,port);

            NetworkMessageManager.SetupHandelers();
        }
        public static bool IsMultiplayerOn()
        {
            return Client != null;
        }
        // will be used to send custom messages from a mod later
        public static void SendMsg()
        {
            throw new NotImplementedException();
        }

        // will be used to sync non player objects positon and rotation later
        public static void StartTrackingObject(GameObject objectToTrack)
        {
            SyncAcrossClients sync = objectToTrack.AddComponent<SyncAcrossClients>();

            ObjectsToSyncAcrossClients.Add(sync);
        }
        // will be used stop to syncing non player objects positon and rotation later
        public static void StopTrackingObject(GameObject objectToTrack)
        {
            SyncAcrossClients sync = objectToTrack.GetComponent<SyncAcrossClients>();
            if (sync == null)
                return;

            Component.Destroy(sync);
        }




        // stuff that should be called when we both start a client or a server
        static void GeneralSetup()
        {
            GameObject player = new GameObject();
            player.AddComponent<ModdedNetworkPlayer>();
            player.AddComponent<NetworkIdentity>();
            ClientScene.RegisterPrefab(player, NetworkHash128.Parse(player.name));
            NetworkManager.singleton.playerPrefab = player;

            NetworkManager.singleton.connectionConfig.NetworkDropThreshold = 45;
            NetworkManager.singleton.connectionConfig.OverflowDropThreshold = 45;
            NetworkManager.singleton.connectionConfig.AckDelay = 200;
            NetworkManager.singleton.connectionConfig.AcksType = ConnectionAcksType.Acks128;
            NetworkManager.singleton.connectionConfig.MaxSentMessageQueueSize = 300;
        }

    }

    // doesnt do much at the moment, but can be used later to do some checks (this overwrites the normal networkmanager so we can overwrite methods in it)
    public class ModNetworkManager : NetworkManager
    {
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
        }
    }

    // TODO: do this
    public class SyncAcrossClients : MonoBehaviour
    {

    }

    public class ModdedNetworkPlayer : NetworkBehaviour
    {
        // both have to do with the physical player
        public FirstPersonMover PhysicalPlayer;
        public Color PhysicalColor;

        // used to tell if bolt is running so that we can spawn players
        static bool hasStartedBolt = false;

        // used to tell if this is a prefab player or a real player
        bool isFakePlayer = true;

        // how often we should send the position and rotation of the player (in seconds)
        public static float sendRate = 0.1f;
        float timer; // used to keep track how long it has been since the latest time we sent the positon and rotation of the player

        // keys that we should send to the server when they are pressed / released
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
                if (!isServer)
                {
                    PlayerCreateMessage playerCreateMessage = new PlayerCreateMessage();
                    playerCreateMessage.MissingPlayers = GetAllPlayersWithoutAPhysicalPlayer();
                    playerCreateMessage.Id = netId.Value;
                    connectionToServer.Send((short)MsgIds.CreatePlayerMessage, playerCreateMessage);
                }
                BoltGlobalEventListenerSingleton<ModdedBoltServerStarter>.Instance.StartServerThenCall(delegate   // Not the actual server, clone drone needs bolt to survive.
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
                // creates and sends a message to the clients containing info about the player color and id
                PlayerCreateMessage playerCreateMessage = new PlayerCreateMessage();
                Color playerColor = MultiplayerUtils.GetNewPlayerColor();
                playerCreateMessage.PlayerR = playerColor.r;
                playerCreateMessage.PlayerG = playerColor.g;
                playerCreateMessage.PlayerB = playerColor.b;
                playerCreateMessage.Id = base.netId.Value;
                NetworkServer.SendToAll((short)MsgIds.CreatePlayerMessage, playerCreateMessage);
            }
            
            
        }
        
        // called on the clients when the server tells them to spawn a player
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
            PhysicalColor = PlayerColor;
            if (!isLocalPlayer)
            {
                PlayerInputController playerController = PhysicalPlayer.GetComponent<PlayerInputController>();
                if (playerController != null)
                {
                    Destroy(playerController);
                }

                AISwordsmanController aISwordsmanController = PhysicalPlayer.GetComponent<AISwordsmanController>();
                if (aISwordsmanController != null)
                {
                    Destroy(aISwordsmanController);
                }

                PhysicalPlayer.entity.TakeControl(); // This took like 4 hours to find out that you need this, it turns out that you need this for the player to work.
            }
            

        }

        // does nothing, is mostly here cuz it used to to something
        public override void OnStartServer()
        {
            
           
            

        }

        // this function will not be called if the player object is not a real player (this means that it is a prefab)
        public override void OnStartClient()
        {
            isFakePlayer = false;

        }

        // called from SimulateController in FirstPersonMover, and is used to send data to the server
        public void OnSimulateController(GameObject _gameObject)
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


            if (timer > sendRate) // to make sure we dont send too many packets over the network
            {
                if (PhysicalPlayer == null)
                    return;

                // sends position and rotation to server
                PlayerMoveMessage msg = new PlayerMoveMessage();
                msg.Id = netId.Value;
                msg.Position = PhysicalPlayer.transform.position;
                msg.Rotation = PhysicalPlayer.transform.rotation;
                base.connectionToServer.SendUnreliable((short)MsgIds.UpdatePlayerTransform, msg);

                timer = 0; // resets the timer
            }
            timer += Time.deltaTime;
        }

        // every player has a uniqe id and this method returns the player that goes with a spesific id
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

        // gets the player that goes with a physical player
        public static ModdedNetworkPlayer GetPlayerWithPhysicalPlayer(FirstPersonMover player)
        {
            for (int i = 0; i < Multiplayer.Players.Count; i++)
            {
                if (Multiplayer.Players[i].PhysicalPlayer == player)
                {
                    return Multiplayer.Players[i];
                }
            }
            return null;
        }

        

        // used on the clients to check what players are not in the world, so that they can ask the server to spawn them for them
        public static uint[] GetAllPlayersWithoutAPhysicalPlayer()
        {
            List<uint> ids = new List<uint>();
            for (int i = 0; i < Multiplayer.Players.Count; i++)
            {
                if (Multiplayer.Players[i].PhysicalPlayer == null)
                {
                    ids.Add(Multiplayer.Players[i].netId.Value);
                }
            }
            if (Multiplayer.LocalPlayer != null)
            {
                if (ids.Contains(Multiplayer.LocalPlayer.netId.Value))
                {
                    ids.Remove(Multiplayer.LocalPlayer.netId.Value);
                }
            }
            return ids.ToArray();
        }

        // adds the player to a list of all players so that we can easily do stuff to all players
        void Awake()
        {
            Multiplayer.Players.Add(this);
        }

        // was supposed to be called when this player disconnects from the server, but it doesnt work so rip
        void OnDestroy()
        {
            if (isFakePlayer)
                return;

            Multiplayer.Players.Remove(this);
            PhysicalPlayer.entity.ReleaseControl();
            Destroy(PhysicalPlayer);
            if (isLocalPlayer)
            {
                NetworkManager.singleton.StopClient();
                NetworkManager.singleton.StopHost();
                NetworkManager.singleton.StopMatchMaker();
                Network.Disconnect();
                NetworkServer.Reset();
                NetworkManager.Shutdown(); //TODO: fix this so that the server and client disconnect properly (this doesnt do that).
                GameObject gameObject = ModdedBoltServerStarter.Instance.gameObject;
                Component.Destroy(ModdedBoltServerStarter.Instance);
                gameObject.AddComponent<ModdedBoltServerStarter>(); // to restart bolt
                ModdedNetworkPlayer.hasStartedBolt = false;
                
            }
        }
    }

    
}
