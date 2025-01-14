﻿using System;
using UnityEngine;

namespace ModLibrary
{
    /// <summary>
    /// Base class for all mods, contains virtual implementations for diffrent events in the game.
    /// </summary>
    public abstract class Mod
    {
        /// <summary>
        /// Called when the game scene is changed
        /// </summary>
        /// <param name="gamemode"></param>
        public virtual void OnSceneChanged(GameMode gamemode)
        {

        }

        /// <summary>
        /// Called in FirstPersonMover Start()
        /// </summary>
        /// <param name="me"></param>
        public virtual void OnFirstPersonMoverSpawned(GameObject me)
        {

        }

        /// <summary>
        /// Called in Character Update()
        /// </summary>
        /// <param name="me"></param>
        public virtual void OnFirstPersonMoverUpdate(GameObject me)
        {

        }

        public virtual void OnCharacterSpawned(GameObject me)
        {

        }

        /// <summary>
        /// Called in Character Update()
        /// </summary>
        /// <param name="me"></param>
        public virtual void OnCharacterUpdate(GameObject me)
        {

        }

        /// <summary>
        /// Called when F3 + R is pressed.
        /// </summary>
        public virtual void OnModRefreshed()
        {

        }

        /// <summary>
        /// Called when the level editor is started.
        /// </summary>
        public virtual void OnLevelEditorStarted()
        {

        }

        /// <summary>
        /// Called when a level editor object is placed or spawned (gets called when any object gets spawed).
        /// </summary>
        public virtual void OnObjectPlacedInLevelEditor(GameObject _object)
        {

        }

        /// <summary>
        /// Called when you run a command in the console (mostly for debuging).
        /// </summary>
        /// <param name="command"></param>
        public virtual void OnCommandRan(string command)
        {

        }

        /// <summary>
        /// Returns the name of the mod, override to set the name of you mod
        /// </summary>
        /// <returns></returns>
        public abstract string GetModName();

        /// <summary>
        /// Returns the description of the mod, override to change the description of your mod
        /// </summary>
        /// <returns></returns>
        public virtual string GetModDescription()
        {
            return "";
        }

        /// <summary>
        /// Returns the image displayed in the mods menu, override to set a custom image for your mod
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use GetModImageURL instead, this is never used")]
        public virtual Texture2D GetModImage()
        {
            return null;
        }

        public virtual string GetModImageURL()
        {
            return "";
        }

        /// <summary>
        /// Called when FirstPersonMover.RefreshUpgrades is called.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="upgrades"></param>
        public virtual void OnUpgradesRefreshed(GameObject me, UpgradeCollection upgrades)
        {

        }
    }
}
