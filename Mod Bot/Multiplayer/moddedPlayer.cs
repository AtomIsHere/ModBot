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


    }
}
