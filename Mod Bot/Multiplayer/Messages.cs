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
using PicaVoxel;

namespace InternalModBot
{
    // the ids of all the diffrent types of messages
    internal enum MsgIds : short
    {
        test = 252,
        UpdatePlayerTransform = 253,
        KeyPressEvent = 254,
        MouseOffsetEvent = 255,
        CreatePlayerMessage = 251,
        CutEventMessage = 150,
        PlayerDeathEvent = 149

    }

    // used to manage the messages comming in, and rerouting them to the proper method
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

            Multiplayer.Client.RegisterHandler((short)MsgIds.CutEventMessage, CutEventMessageClient);
            NetworkServer.RegisterHandler((short)MsgIds.CutEventMessage, CutEventMessageServer);
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
            PlayerCreateMessage myMsg = netMsg.ReadMessage<PlayerCreateMessage>();
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(myMsg.Id);
            for (int i = 0; i < myMsg.MissingPlayers.Length; i++)
            {
                ModdedNetworkPlayer missingPlayer = ModdedNetworkPlayer.GetPlayerWithID(myMsg.MissingPlayers[i]);
                PlayerCreateMessage playerCreateMessage = new PlayerCreateMessage();
                playerCreateMessage.PlayerR = missingPlayer.PhysicalColor.r;
                playerCreateMessage.PlayerG = missingPlayer.PhysicalColor.g;
                playerCreateMessage.PlayerB = missingPlayer.PhysicalColor.b;
                playerCreateMessage.Id = missingPlayer.netId.Value;
                player.connectionToClient.Send((short)MsgIds.CreatePlayerMessage, playerCreateMessage);
            }
        }

        static void PlayerCreateMessageClient(NetworkMessage netMsg)
        {
            PlayerCreateMessage myMsg = netMsg.ReadMessage<PlayerCreateMessage>();
            

            myMsg.CreatePhysicalPlayer();
        }

        static void CutEventMessageServer(NetworkMessage netMsg)
        {
            debug.Log("some client just tried to send a cut event", Color.red);
        }
        static void CutEventMessageClient(NetworkMessage netMsg)
        {
            CutEventMessage myMsg = netMsg.ReadMessage<CutEventMessage>();
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(myMsg.Id);
            if (player == null || player.isServer)
                return;

            
            myMsg.ApplyToPlayer();
        }

        static void PlayerDeathEventMessageServer(NetworkMessage netMsg)
        {
            debug.Log("some client just tried to send a player death event", Color.red);
        }
        static void PlayerDeathMessageClient(NetworkMessage netMsg)
        {
            PlayerDeathMessage myMsg = netMsg.ReadMessage<PlayerDeathMessage>();
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(myMsg.Id);
            if (player == null)
                return;


            myMsg.KillPlayer();
        }

        static void StandardMsg(NetworkMessage msg)
        {

        }

        
    }

    // from here on down there are just diffrent types of messages

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
        public float PlayerR, PlayerG, PlayerB; // Not a color becuase if we use color JsonConvert.SerializeObject(this) breaks as fuck.

        public uint[] MissingPlayers;

        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            PlayerCreateMessage msg = JsonConvert.DeserializeObject<PlayerCreateMessage>(json);
            Id = msg.Id;
            PlayerR = msg.PlayerR;
            PlayerG = msg.PlayerG;
            PlayerB = msg.PlayerB;
            MissingPlayers = msg.MissingPlayers;
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
            

            player.CreatePhysicalPlayer(new Vector3(0, 0, 0), new Color(PlayerR, PlayerG, PlayerB));
        }
    }

    public class PlayerDeathMessage : MessageBase
    {
        public uint Id;
        public uint? KillerId;
        public DamageSourceType damageSource;

        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            PlayerDeathMessage msg = JsonConvert.DeserializeObject<PlayerDeathMessage>(json);
            Id = msg.Id;
            damageSource = msg.damageSource;
        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }

        public void KillPlayer()
        {
            ModdedNetworkPlayer player = ModdedNetworkPlayer.GetPlayerWithID(Id);
            if (player == null)
                return;

            ModdedNetworkPlayer killer = null;
            if (KillerId != null)
            {
                killer = ModdedNetworkPlayer.GetPlayerWithID(KillerId.Value);
            }
            Character KillerCharacter = killer == null ? null : killer.PhysicalPlayer;
            
            player.PhysicalPlayer.Kill(KillerCharacter, damageSource);
        }
    }

    public class CutEventMessage : MessageBase
    {
        public uint Id;
        public MechBodyPartType damagedPart;
        public VoxelPositionsToken VoxelPositionsToken;
        public DamageSourceType damageSourceType;
        public FireType fireType;
        public Vector3 ImpactDirection;
        public bool IsCuttingDamage;
        public int AttackID;
        public bool KnockBack;

        public override void Deserialize(NetworkReader reader)
        {
            string json = reader.ReadString();
            CutEventMessage msg = JsonConvert.DeserializeObject<CutEventMessage>(json);
            Id = msg.Id;
            damagedPart = msg.damagedPart;
            VoxelPositionsToken = msg.VoxelPositionsToken;
            damageSourceType = msg.damageSourceType;
            fireType = msg.fireType;
            ImpactDirection = msg.ImpactDirection;
            IsCuttingDamage = msg.IsCuttingDamage;
            AttackID = msg.AttackID;
            KnockBack = msg.KnockBack;

        }
        public override void Serialize(NetworkWriter writer)
        {
            string json = JsonConvert.SerializeObject(this);
            writer.Write(json);
        }

        ModdedNetworkPlayer player;

        public void ApplyToPlayer()
        {
            player = ModdedNetworkPlayer.GetPlayerWithID(Id);

            ProcessDamageEvent();
        }
       
        public MechBodyPart GetMechBodyPart()
        {
            if (player.PhysicalPlayer == null)
            {
                return null;
            }
            debug.Log(damagedPart);   

            List<MechBodyPart> parts = player.PhysicalPlayer.GetAllBodyParts();
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].PartType == damagedPart)
                {
                    return parts[i];
                }
            }
            return null;


        }
        public void ProcessDamageEvent() // copied out of clone drone and modified
        {
            debug.Log("recived cut event");
            MechBodyPart part = GetMechBodyPart();
            if (part == null)
            {
                debug.Log("part is null");
                return;
            }
            if (part.GetComponent<Volume>() == null)
            {
                debug.Log("volume is null");
                return;
            }

            Frame currentFrame = part.GetComponent<Volume>().GetCurrentFrame();
            VoxelPositionsToken voxelPositionsToken = VoxelPositionsToken;
            Vector3 voxelWorldPosition = currentFrame.GetVoxelWorldPosition(currentFrame.XSize / 2, currentFrame.YSize / 2, currentFrame.ZSize / 2);
            FireSpreadDefinition fireSpreadDefinition = Singleton<FireManager>.Instance.GetFireSpreadDefinition(fireType);
            List<PicaVoxelPoint> list = new List<PicaVoxelPoint>();
            for (int i = 0; i < voxelPositionsToken.VoxelPositions.Count; i++)
            {
                PicaVoxelPoint voxelArrayPosition = currentFrame.GetVoxelArrayPosition(voxelPositionsToken.VoxelPositions[i]);
                list.Add(voxelArrayPosition);
                Vector3 localPostion = currentFrame.GetLocalPostion(voxelArrayPosition.X, voxelArrayPosition.Y, voxelArrayPosition.Z);
                Voxel? voxelAtArrayPosition = currentFrame.GetVoxelAtArrayPosition(voxelArrayPosition);
                if (voxelAtArrayPosition == null)
                {
                    continue;
                }
                Accessor.CallPrivateMethod(typeof(MechBodyPart), "destroyVoxelAtPositionFromCut", part, new object[] { voxelArrayPosition, voxelAtArrayPosition, localPostion, voxelWorldPosition, ImpactDirection, fireSpreadDefinition, currentFrame });
            }
            Character characterWithNetworkID = ModdedNetworkPlayer.GetPlayerWithID(Id).PhysicalPlayer;
            if (damageSourceType == DamageSourceType.Arrow || damageSourceType == DamageSourceType.DeflectedArrow || damageSourceType == DamageSourceType.SawBlade || damageSourceType == DamageSourceType.SpikeTrap)
            {
                Singleton<AttackManager>.Instance.PlayCutDamageSoundForNewtworkCutEvent(AttackID, fireSpreadDefinition != null, part.transform);
            }
            else if (IsCuttingDamage && characterWithNetworkID != null)
            {
                FirstPersonMover firstPersonMover = characterWithNetworkID as FirstPersonMover;
                if (firstPersonMover != null)
                {
                    firstPersonMover.TryPlayImpactSoundForCurrentWeapon(AttackID, part.transform);
                }
            }
            List<MechBodyPart> list2 = part.DisconnectSegmentsAfterCut(list, AttackID, ImpactDirection.normalized, KnockBack, characterWithNetworkID, damageSourceType);
            if (IsCuttingDamage && fireSpreadDefinition != null)
            {
                Accessor.SetPrivateField(typeof(MechBodyPart), "_lastFireDamageOrigin", part, characterWithNetworkID);
                Accessor.SetPrivateField(typeof(MechBodyPart), "_lastSourceOfFire", part, damageSourceType);

                for (int j = 0; j < list2.Count; j++)
                {
                    Accessor.CallPrivateMethod(typeof(MechBodyPart), "setFireIfNextTo", list2[j], new object[] { list, fireSpreadDefinition });
                }
            }
            debug.Log("2");
        }
    }
}
