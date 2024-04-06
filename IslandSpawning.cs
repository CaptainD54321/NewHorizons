using BepInEx;
using HarmonyLib;
using UnityEngine;
using SailwindModdingHelper;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using Crest;

namespace NewHorizons;

[HarmonyPatch(typeof(IslandHorizon))]
internal class HorizonPatch {
    
    [HarmonyPatch("LoadIslandScene")]
    [HarmonyPrefix]
    internal static bool LoadIslandScene(int ___islandIndex, ref float ___loadUnloadCoolown, ref bool ___sceneLoaded) {
        var index = ___islandIndex;
        if (index > ModRefs.NUM_RL_ISLANDS) {
            int pathIndex = index - ModRefs.NUM_RL_ISLANDS-1;
            Plugin.logger.LogInfo($"loading scene for island #{index} from scenepath {ModRefs.IslandList[pathIndex].scenePath}");
            ___loadUnloadCoolown = 12f;
            ___sceneLoaded = true;
            var loading = SceneManager.LoadSceneAsync(ModRefs.IslandList[pathIndex].scenePath,LoadSceneMode.Additive);
            GameState.loadingScenes++;
            loading.completed += (_)=> SetUpScene(pathIndex); 
            return false;
        }
        else return true;
        
    }

    internal static void SetUpScene(int index) {
        Plugin.logger.LogInfo($"Scene for island #{index+ModRefs.NUM_RL_ISLANDS + 1} loaded, setting up scenery");
        GameState.loadingScenes--;
        var scene = SceneManager.GetSceneByPath(ModRefs.IslandList[index].scenePath);
        if (!scene.IsValid()) {
            Plugin.logger.LogError("loaded scenery scene not valid!!");
            return;
        }
        var scenery = scene.GetRootGameObjects()[0];
        var moveScript = scenery.AddComponent<IslandSceneryScene>();
        moveScript.parentIslandIndex = index + ModRefs.NUM_RL_ISLANDS + 1;
        var docks = scenery.transform.Find("docks");
        if (docks != null) {
            foreach (Transform dock in docks) {
                dock.gameObject.AddComponent<DockPushCol>();
            }
        }
    }

    [HarmonyPatch("DoUnloadScene")]
    [HarmonyPostfix]
    internal static IEnumerator UnloadCoroutinePatch(IEnumerator original, int ___islandIndex, IslandHorizon __instance) {
        var me = __instance;
        var index = ___islandIndex;
        if (index > ModRefs.NUM_RL_ISLANDS) {
            int pathIndex = index - ModRefs.NUM_RL_ISLANDS - 1;
            Plugin.logger.LogInfo($"unloading scene for island #{index}");
            var scene = SceneManager.GetSceneByPath(ModRefs.IslandList[pathIndex].scenePath);
            var async = SceneManager.UnloadSceneAsync(scene);
            //Plugin.logger.LogInfo($"Async operation: {async}");
            do {
                yield return new WaitForEndOfFrame();
            } while (!async.isDone);
            me.SetPrivateField("sceneLoaded",false); //___sceneLoaded = false;
            me.SetPrivateField("unloading",false); //___unloading = false;
        } else yield return original;
    }

    [HarmonyPatch("RegisterIsland")]
    [HarmonyPrefix]
    internal static void RegisterIsland() {
        if(Refs.islands == null) {
            Refs.islands = new Transform[ModRefs.num_islands];
        }
    }
}

[HarmonyPatch(typeof(FloatingOriginManager))]
internal static class WorldPatch {
    internal const float mileRatio = 9000f;
    internal static Vector3 globeOffset = new Vector3(0f, 0f, -36f) * mileRatio;
    internal static FloatingOriginManager shiftingWorld;
    
    internal static bool islandsLoaded = false;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    // When _shiftingworld is created, load in all my islands and put them in the right places.
    // TODO: make this not messy as hell
    internal static void LoadIslands(ref FloatingOriginManager ___instance) {
        shiftingWorld = ___instance;
        //modIslands = new IslandHorizon[1];

        Plugin.logger.LogInfo("Beginning to spawn in islands.");
        foreach (var island in ModRefs.IslandList) {
            SetupIsland(island);
        }

        //SetupIsland(Plugin.terrainPrefab,ModRefs.TEST_ISLAND_COORDS);

        islandsLoaded = true;
        IslandDistanceTracker.instance.SetPrivateField("updating", false);
    }

    // Set up island terrain
    internal static void SetupIsland(Island island) {
        Plugin.logger.LogInfo($"Setting up island {island.config.name}");
        var islandObj = GameObject.Instantiate(island.terrainPrefab,shiftingWorld.transform);
        islandObj.transform.localPosition = shiftingWorld.RealPosToShiftingPos(GlobeCoordsToRealPos(island.location));
        var terrain = islandObj.transform.Find("terrain").gameObject;
        if (terrain != null) {
            terrain.layer = 14;
            terrain.tag = "Terrain";
            foreach (Transform obj in terrain.transform) {
                obj.gameObject.layer = 14;
                obj.gameObject.tag = "Terrain";
            }
        }
        var horizon = islandObj.AddComponent<IslandHorizon>();
        island.horizon = horizon;        

        var depthObj = new GameObject();
        depthObj.transform.SetParent(islandObj.transform);
        depthObj.transform.localPosition = island.config.center;
        depthObj.transform.localScale = new Vector3(island.config.size,1,island.config.size);

        horizon.overrideCenter = depthObj.transform;

        var depth = depthObj.AddComponent<OceanDepthCache>();
        depth._layerNames = ["TerrainDepth"];
        depth.PopulateCache();


        var cleats = islandObj.transform.Find("dock_cleats");
        if (cleats != null) {
            foreach (Transform cleatTrans in cleats) {
                var cleat = cleatTrans.gameObject;

                var rigidbody = cleat.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;

                var col = cleat.AddComponent<BoxCollider>();
                col.isTrigger = true;
                //TODO: update collider size & position from config file

                var spring = cleat.AddComponent<SpringJoint>();
                spring.autoConfigureConnectedAnchor = false;
                spring.anchor = Vector3.zero;
                spring.connectedAnchor = Vector3.zero;
                spring.spring = 400f;
                spring.damper = 1f;
                //spring.maxDistance = 1.5f; this is done by the script for some reason?

                var script = cleat.AddComponent<GPButtonDockMooring>();
            }
        }

        horizon.islandIndex = island.index;
        //modIslands[0] = horizon;
    }

    // Function to convert globe coords to unity coords
    // TODO: is there a better way to pass the lat/long coords? Does C# have tuples?
    internal static Vector3 GlobeCoordsToRealPos(float latitude, float longitude) {
        var coords = new Vector3();

        coords.x = longitude * mileRatio;
        coords.z = latitude * mileRatio;

        coords += globeOffset;

        return coords;
    }
    internal static Vector3 GlobeCoordsToRealPos(params float[] coords) {
        return GlobeCoordsToRealPos(coords[0],coords[1]);
    }
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
    internal static Vector3 GlobeCoordsToRealPos(Coord coords) {
        return GlobeCoordsToRealPos(coords.latitude, coords.longitude);
    }
}

[HarmonyPatch(typeof(IslandSceneryScene))]
internal static class SceneryMovement {
    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    // Patch the script that moves the island scenery with the terrain to work with my out-of-range indexes
    internal static bool Update(int ___parentIslandIndex, ref IslandSceneryScene __instance, ref Vector3 ___lastParentPos) {
        var me = __instance;
        int index = ___parentIslandIndex;
        if (index > ModRefs.NUM_RL_ISLANDS) {
            int i = index - ModRefs.NUM_RL_ISLANDS - 1;
            Vector3 islandPos = ModRefs.IslandList[i].horizon.transform.position;
            if (___lastParentPos != islandPos) {
                me.transform.position = islandPos;
            }
            ___lastParentPos = islandPos;
            return false;
        } else return true;
    }
}

[HarmonyPatch(typeof(IslandDistanceTracker))]
internal static class DistanceTrackerFix {
    [HarmonyPatch("UpdateDistance")]
    [HarmonyFinalizer]
    internal static Exception ErrorCatcher(Exception __exception, ref bool ___updating) {
        Plugin.logger.LogInfo($"UpdateDistance catcher caught error {__exception}, suppressing");
        ___updating = false;
        return null;
        /* if (__exception is InvalidOperationException) {
            return null;
        } else return __exception; */
    }
}