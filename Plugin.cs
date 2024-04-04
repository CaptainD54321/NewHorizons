using System;
using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Animations;
using Newtonsoft.Json;


namespace NewHorizons;


[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency("com.app24.sailwindmoddinghelper", "2.0.3")]
public class Plugin : BaseUnityPlugin
{
    internal const string PLUGIN_GUID = "com.captaind54321.newhorizons";
    internal const string PLUGIN_NAME = "New Horizons";
    internal const string PLUGIN_VERSION = "0.1.0";
    internal static string BUNDLE_PATH = Path.Combine(Application.dataPath,"../BepinEx/islandpacks");


    internal static Plugin instance;
    internal static ManualLogSource logger;
    internal static Harmony harmony;

    
    private void Awake()
    {
        instance = this;
        logger = Logger;
        harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(),PLUGIN_GUID);
        logger.LogInfo($"assetbundle path is: {BUNDLE_PATH}");
        
        if (!Directory.Exists(BUNDLE_PATH)) {
            Directory.CreateDirectory(BUNDLE_PATH);
        }

        AssetLoad.LoadIslands();

    }

    private void OnDestroy() {
        harmony.UnpatchSelf();
    }
}