using ModLibrary;
using UnityEngine;
using InternalModBot.Multiplayer;

namespace InternalModBot
{
    public static class StartupManager
    {
        public static void OnStartUp()
        {
            ErrorChanger.ChangeError();

            GameObject gameFlowManager = GameFlowManager.Instance.gameObject;

            gameFlowManager.AddComponent<WaitThenCallClass>();
            gameFlowManager.AddComponent<moddedObjectsManager>();
            gameFlowManager.AddComponent<ModsManager>();
            gameFlowManager.AddComponent<UpdateChecker>();
            gameFlowManager.AddComponent<ModsPanelManager>();
            gameFlowManager.AddComponent<CustomUpgradeManger>();
            gameFlowManager.AddComponent<ModdedMultiplayerUIManager>();
            gameFlowManager.AddComponent<ModdedMultiplayerManager>();
            gameFlowManager.AddComponent<ModdedBoltServerStarter>();

            IgnoreCrashesManager.Start();
        }
    }
}
