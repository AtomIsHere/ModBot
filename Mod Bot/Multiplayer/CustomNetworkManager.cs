using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using ModLibrary;
using InternalModBot;

namespace InternalModBot.Multiplayer
{
    public class ModdedNetworkManager : NetworkManager
    {
        public override void OnStartServer()
        {
            debug.Log("started server");
        }
        public override void OnStartClient(NetworkClient client)
        {
            debug.Log("started client " + client.serverIp);
        }
        public override void OnClientConnect(NetworkConnection conn)
        {
            debug.Log(conn.address + " connected!");
        }
    }
}
