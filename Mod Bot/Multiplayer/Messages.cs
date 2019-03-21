using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using Newtonsoft.Json;
using ModLibrary;

namespace InternalModBot
{
    internal enum MsgIds : short
    {
        test = 252,
        UpdatePlayerTransform = 253,
        KeyPressEvent = 254,
        MouseOffsetEvent = 255,
        CreatePlayerMessage = 256

    }

    public static class NetworkMessageManager
    {
        public static void SetupHandelers()
        {
            Multiplayer.Client.RegisterHandler((short)MsgIds.test, TestMsg);
            NetworkServer.RegisterHandler((short)MsgIds.test, TestMsg);

            Multiplayer.Client.RegisterHandler((short)MsgIds.UpdatePlayerTransform, HandleMovePlayerClient);
            NetworkServer.RegisterHandler((short)MsgIds.UpdatePlayerTransform, HandleMovePlayerServer);


            Multiplayer.Client.RegisterHandler((short)MsgIds.KeyPressEvent, HandleKeyPressClient);
            NetworkServer.RegisterHandler((short)MsgIds.KeyPressEvent, HandleKeyPressServer);


            Multiplayer.Client.RegisterHandler((short)MsgIds.MouseOffsetEvent, HandleMouseOffsetMessageClient);
            NetworkServer.RegisterHandler((short)MsgIds.MouseOffsetEvent, HandleMouseOffsetMessageServer);


            Multiplayer.Client.RegisterHandler((short)MsgIds.CreatePlayerMessage, PlayerCreateMessageClient);
            NetworkServer.RegisterHandler((short)MsgIds.CreatePlayerMessage, PlayerCreateMessageServer);
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

            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(myMsg.Id);

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
            KeyPressEventMessage myMsg = msg.ReadMessage<KeyPressEventMessage>();
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(myMsg.Id);
            if (player == null || player.isLocalPlayer)
                return;
            myMsg.SetKeyOfPlayer(player);
        }

        static void HandleMouseOffsetMessageServer(NetworkMessage netMsg)
        {
            MouseOffsetMessage myMsg = netMsg.ReadMessage<MouseOffsetMessage>();
            NetworkServer.SendToAll((short)MsgIds.MouseOffsetEvent, myMsg);
        }

        static void HandleMouseOffsetMessageClient(NetworkMessage netMsg)
        {
            MouseOffsetMessage myMsg = netMsg.ReadMessage<MouseOffsetMessage>();
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(myMsg.Id);
            if (player == null || player.isLocalPlayer)
                return;
            myMsg.AddRotationToPlayer(player);
        }

        static void PlayerCreateMessageServer(NetworkMessage netMsg)
        {
            debug.Log("Client attempted to create player!", Color.red);
        }

        static void PlayerCreateMessageClient(NetworkMessage netMsg)
        {
            PlayerCreateMessage myMsg = netMsg.ReadMessage<PlayerCreateMessage>();

            Console.WriteLine("nulk-1");

            myMsg.CreatePhysicalPlayer();
        }

        static void StandardMsg(NetworkMessage msg)
        {

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
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(Id);
            if (player == null || player.PhysicalPlayer == null)
                return;
            player.PhysicalPlayer.transform.position = Position;
            player.PhysicalPlayer.transform.rotation = Rotation;
        }
    }

    public class KeyPressEventMessage : MessageBase
    {
        public KeyPressEventMessage()
        {
            KeyActions = new Dictionary<KeyCode, fakeAction>()
            {
                { KeyCode.Mouse0, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventMouse0"), this) },
                { KeyCode.Mouse1, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventMouse1"), this) },
                { KeyCode.W, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventW"), this) },
                { KeyCode.A, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventA"), this) },
                { KeyCode.S, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventS"), this) },
                { KeyCode.D, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventD"), this) },
                { KeyCode.F, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventF"), this) },
                { KeyCode.LeftShift, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventShiftL"), this) },
                { KeyCode.E, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventE"), this) },
                { KeyCode.Space, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEventSpace"), this) },
                { KeyCode.Alpha1, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEvent1"), this) },
                { KeyCode.Alpha2, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEvent2"), this) },
                { KeyCode.Alpha3, new fakeAction(typeof(KeyPressEventMessage).GetMethod("KeyEvent3"), this) },
            };
        }

        public KeyCode Key;
        public bool Down;
        public uint Id;
        ModdedNetworkPlayer player;
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
        public void SetKeyOfPlayer(ModdedNetworkPlayer player)
        {
            if (player == null || player.PhysicalPlayer == null)
                return;
            this.player = player;
            if (!KeyActions.ContainsKey(Key))
                throw new NotImplementedException("Can not find a method to go along with key \"" + Key + "\" in KeyPressEventMessage");

            KeyActions[Key].Invoke();
        }

        Dictionary<KeyCode, fakeAction> KeyActions;
        public void KeyEventMouse0()
        {
            if (Down)
            {

                player.PhysicalPlayer.SetAttackKeyDown(true);
            }
            else
            {
                player.PhysicalPlayer.SetAttackKeyUp(true);
            }
            player.PhysicalPlayer.SetAttackKeyHeld(Down);
        }
        public void KeyEventMouse1()
        {
            if (Down)
            {

                player.PhysicalPlayer.SetSecondAttackKeyDown(true);
            }
            else
            {
                player.PhysicalPlayer.SetSecondAttackKeyUp(true);
            }
            player.PhysicalPlayer.SetSecondAttackKeyHeld(Down);
        }
        public void KeyEventW()
        {
            player.PhysicalPlayer.SetUpKeyDown(Down);
            
        }
        public void KeyEventA()
        {
            player.PhysicalPlayer.SetLeftKeyDown(Down);
        }
        public void KeyEventS()
        {
            player.PhysicalPlayer.SetDownKeyDown(Down);
        }
        public void KeyEventD()
        {
            player.PhysicalPlayer.SetRightKeyDown(Down);
            
        }
        public void KeyEventF()
        {
            player.PhysicalPlayer.SetUseAbilityButtonDown(Down);
            
            player.PhysicalPlayer.SetUseAbilityButtonHeld(Down);
        }
        public void KeyEventShiftL()
        {
            /*
            player.PhysicalPlayer.SetJetpackEngaged(Down);
            
            player.PhysicalPlayer.SetJetPackKeyHeld(Down);
            */ // crashes the game for some weird reason
        }
        public void KeyEventE()
        {
            player.PhysicalPlayer.SetUseKeyDown(Down);
        }
        public void KeyEventSpace()
        {
            player.PhysicalPlayer.SetJumpKeyDown(Down);
        }
        public void KeyEvent1()
        {
            player.PhysicalPlayer.SetWeapon1ButtonDown(Down);
        }
        public void KeyEvent2()
        {
            player.PhysicalPlayer.SetWeapon2ButtonDown(Down);
        }
        public void KeyEvent3()
        {
            player.PhysicalPlayer.SetWeapon3ButtonDown(Down);
        }
    }

    public class MouseOffsetMessage : MessageBase
    {
        public float MouseX;
        public float MouseY;
        public uint Id;
        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            MouseOffsetMessage msg = JsonConvert.DeserializeObject<MouseOffsetMessage>(json);
            MouseX = msg.MouseX;
            MouseY = msg.MouseY;
            Id = msg.Id;
        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }

        public void AddRotationToPlayer(ModdedNetworkPlayer player)
        {
            if (player == null || player.PhysicalPlayer == null)
                return;
            player.PhysicalPlayer.SetHorizontalCursorMovement(MouseX);
            player.PhysicalPlayer.SetVerticalCursorMovement(MouseY);
        }
    }

    public class PlayerCreateMessage : MessageBase
    {
        public uint Id;
        public Color PlayerColor;

        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            PlayerCreateMessage msg = JsonConvert.DeserializeObject<PlayerCreateMessage>(json);
            Id = msg.Id;
            PlayerColor = msg.PlayerColor;
        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }

        public void CreatePhysicalPlayer()
        {
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(Id);

            if (player == null)
                return;

            Console.WriteLine("nulk0");

            player.CreatePhysicalPlayer(new Vector3(0, 0, 0), PlayerColor);
        }
    }
}
