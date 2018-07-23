using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneColorMode
{
    class Plugin : IEnhancedPlugin, IPlugin
    {
        public static string PluginName = "One Color Mode";
        public const string VersionNum = "0.2.0";

        public string Name => PluginName;
        public string Version => VersionNum;
        public string[] Filter { get; }


        private bool _init;
        public const string KeyOneColorMode = "OneColorModeEnabled";
        public const string KeyColorBuleOrRed = "Color0BlueOr1Red";
        public const string KeyNoArrowModeRandLevel = "NoArrowModeRandLevelFrom0To2";

        public static bool IsOneColorModeOn
        {
            get
            {
                return ModPrefs.GetBool(Plugin.PluginName, KeyOneColorMode, false);
            }
        }

        public static bool IsColorRed
        {
            get
            {
                return ModPrefs.GetBool(Plugin.PluginName, KeyColorBuleOrRed, false);
            }
        }

        public static int NoArrowModeRandLevel
        {
            get
            {
                return ModPrefs.GetInt(Plugin.PluginName, KeyNoArrowModeRandLevel, 2);
            }
        }

        public void OnApplicationStart()
        {
            CheckForUserDataFolder();
        }

        public void OnApplicationQuit()
        {
        }

        public void OnLateUpdate()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
            if (_init) return;
            _init = true;
            new GameObject("OneColorModePlugin").AddComponent<OneColorModeBehaviour>();
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }


        private void CheckForUserDataFolder()
        {
            string userDataPath = Environment.CurrentDirectory + "/UserData";
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }
            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyOneColorMode, "")))
            {
                ModPrefs.SetBool(Plugin.PluginName, Plugin.KeyOneColorMode, true);
            }
            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyColorBuleOrRed, "")))
            {
                ModPrefs.SetBool(Plugin.PluginName, Plugin.KeyColorBuleOrRed, true);
            }
            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyNoArrowModeRandLevel, "")))
            {
                ModPrefs.SetInt(Plugin.PluginName, Plugin.KeyNoArrowModeRandLevel, 2);
            }
        }
    }
}
