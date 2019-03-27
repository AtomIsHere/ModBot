using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ModLibrary;

namespace InternalModBot
{
    public static class DamageProcessor
    {
        // called on the server when someone cuts something with a sword
        public static void OnCutVolume(GameObject partGameObject, BodyPartDamageEvent cutEvent)
        {
            if (!Multiplayer.LocalPlayer.isServer) // make sure we are the server
                return;
            debug.Log("got cut event");
            MechBodyPart mechBodyPart = partGameObject.GetComponent<MechBodyPart>(); // this should never be null, OnCutVolume is called from this

            FirstPersonMover owner = (FirstPersonMover)mechBodyPart.GetOwner();

            if (owner == null)
            {
                debug.Log("stopped cuz owner is null");
                return;
            }
            

            // message to send to clinets
            CutEventMessage cutEventMessage = new CutEventMessage();
            cutEventMessage.Id = ModdedNetworkPlayer.GetPlayerWithPhysicalPlayer(owner).netId.Value;
            cutEventMessage.AttackID = cutEvent.AttackID;
            cutEventMessage.damageSourceType = (DamageSourceType)cutEvent.DamageSourceType;
            cutEventMessage.fireType = (FireType)cutEvent.FireType;
            cutEventMessage.ImpactDirection = cutEvent.ImpactDirection;
            cutEventMessage.IsCuttingDamage = cutEvent.IsCuttingDamage;
            cutEventMessage.KnockBack = cutEvent.KnockBack;
            cutEventMessage.VoxelPositionsToken = (VoxelPositionsToken)cutEvent.VoxelPositionsToken;
            cutEventMessage.damagedPart = mechBodyPart.PartType;

            NetworkServer.SendToAll((short)MsgIds.CutEventMessage, cutEventMessage);
            debug.Log("sent cut event");

        }
    }
}
