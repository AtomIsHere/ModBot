﻿using ModLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace InternalModBot
{
    public static class ConsoleInputManager
    {
        public static void OnCommandRan(string command)
        {
            command = command.ToLower();
            string[] subCommands = command.Split(' ');

            if (subCommands[0] == "ignoreallcrashes")
            {
                if (subCommands.Length <= 1)
                {
                    debug.Log("usage: ignoreallcrashes <number 0-1>");
                    return;
                }

                IgnoreCrashesManager.SetIsIgnoringCrashes(subCommands[1] == "1");
            }

            if (subCommands[0] == "crash")
            {
                Delayed.TriggerAfterDelay(new fakeAction(typeof(ConsoleInputManager).GetMethod("Crash"), null), 1);
            }

            if (CommandManager.Instance.HasCommand(subCommands[0]))
            {
                List<string> args = new List<string>(subCommands);
                //Convert so we can remove the first value giving us just the command arguments.
                args.RemoveAt(0);

                CommandManager.Instance.RunCommand(subCommands[0], args);
            }
            else 
            {
                debug.Log("Invalid Command");
            }
        }

        public static void Crash()
        {
            throw new Exception("-Crashed from console-");
        }
    }
    
    public static class IgnoreCrashesManager
    {
        private static bool isIgnoringCrashes = false;

        public static void Start()
        {
            int get = PlayerPrefs.GetInt("IgnoreCrashes", 0);
            isIgnoringCrashes = get == 1;

            if (isIgnoringCrashes)
            {
                Delayed.TriggerAfterDelay(new fakeAction(typeof(IgnoreCrashesManager).GetMethod("Alert"), null), 1);
            }
        }
        
        public static void Alert()
        {
            debug.Log("Saved option message (IgnoreCrashes): All crashes are being ignored, this should only be enabled for testing purposes, turn it off by typing \"ignoreallcrashes 0\" into the console.", Color.red);
        }
        
        public static void SetIsIgnoringCrashes(bool state)
        {
            isIgnoringCrashes = state;
            int ignoreCrashesIntValue = isIgnoringCrashes ? 1 : 0;
            
            PlayerPrefs.SetInt("IgnoreCrashes", ignoreCrashesIntValue);
            
            if (state)
            {
                debug.Log("The game is now ignoring all crashes, this option should only ever be enabled for testing. Having this option enabled will ignore soft crashes, but hard crashes can still occur.", Color.red);
            }
            else
            {
                debug.Log("The game is no longer ignoring crashes, this option should always be turned off for stability purposes.", Color.green);
            }
        }

        public static bool GetIsIgnoringCrashes()
        {
            return isIgnoringCrashes;
        }
    }
}