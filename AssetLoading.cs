using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using SailwindModdingHelper;
using Newtonsoft.Json;
using System.Linq;

namespace NewHorizons;

[Serializable]
public struct PackManifest {
    public string guid;
    public string name;
    public IslandConfig[] islands;
}

[Serializable]
public struct IslandConfig {
    public string name;
    public float[] location;
    public string scenery_path;
    public string terrain_path;
    public Vector3 center = new Vector3(0,0,0);
    public float size = 1500f;

    public IslandConfig() {}
}


internal static class AssetLoad {
    internal const string ISLANDPACKS_PATH = "../BepInEx/islandpacks";

    internal static string load_path = Path.Combine(Application.dataPath,ISLANDPACKS_PATH);

    internal static void LoadIslands() {
        // if the islandpacks folder doesn't exist, create it
        if (!Directory.Exists(load_path)) {
            Plugin.logger.LogWarning("no islandpacks folder, creating one now");
            Directory.CreateDirectory(load_path);
        }

        var packPaths = Directory.EnumerateDirectories(load_path);
        int modIslands = 0; // counter for number of islands loaded
        ModRefs.IslandList = new();
        // iterate over all folders in /islandpacks
        foreach (string pack in packPaths) {
            if (File.Exists(Path.Combine(pack,"manifest.json"))) {
                // if there is a manifest file, try to load it
                string json = File.ReadAllText(Path.Combine(pack,"manifest.json"));
                PackManifest manifest = JsonConvert.DeserializeObject<PackManifest>(json);
                Plugin.logger.LogInfo($"Loading islandpack {manifest.name}.");
                var scenery = AssetBundle.LoadFromFile(Path.Combine(pack,"scenery"));
                var terrain = AssetBundle.LoadFromFile(Path.Combine(pack,"terrain"));
                foreach (var islandConfig in manifest.islands) {
                    Plugin.logger.LogInfo($"Loading island {islandConfig.name} from pack {manifest.name}.");                    
                    modIslands++; // increment island counter
                    var island = new Island();
                    island.scenePath = islandConfig.scenery_path;
                    island.terrainPrefab = terrain.LoadAsset(islandConfig.terrain_path) as GameObject;
                    island.location = (islandConfig.location[0],islandConfig.location[1]);
                    island.index = ModRefs.NUM_RL_ISLANDS + modIslands;
                    island.config = islandConfig;

                    island.valid = true;                    
                    // value checker
                    if (!scenery.GetAllScenePaths().Contains(island.scenePath)) {
                        Plugin.logger.LogError($"scenery scene not found for island {islandConfig.name}! Cannot load island!");
                        island.valid = false;
                        Plugin.logger.LogError($"Valid scene paths in pack: {String.Join(", ", scenery.GetAllScenePaths())}");
                    }
                    if (island.terrainPrefab == null) {
                        Plugin.logger.LogError($"terrain prefab not found for island {islandConfig.name}! Cannot load island!");
                        island.valid = false;
                        Plugin.logger.LogError($"Valid objects in pack: {String.Join(", ", terrain.GetAllAssetNames())}");
                    }
                    if (false) {
                        // TODO: check location is valid
                    }

                    // if island is validated, add to list
                    if (island.valid) {
                        ModRefs.IslandList.Add(island);
                        Plugin.logger.LogInfo($"successfully loaded island {islandConfig.name}.");
                    } else { // otherwise ignore it, and decrement the counter to keep indexes synced up
                        modIslands--;
                    }
                }
                Plugin.logger.LogInfo($"Finished loading islandpack {manifest.name}.");
            }
            else {
                // log warning and continue looping
                var temp = pack.Split([Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar]);
                Plugin.logger.LogWarning($"No manifest.json found in folder {temp[temp.Length-1]}! Ignoring folder.");
            }
        }
        Plugin.logger.LogInfo($"Finished loading all islandpacks.");
        ModRefs.num_islands = ModRefs.NUM_RL_ISLANDS + modIslands + 1; // needs an extra +1 because 1-indexing
    }

}