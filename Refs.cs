global using Coord = (float latitude,float longitude);
using System.Collections.Generic;
using UnityEngine;


namespace NewHorizons;



internal static class ModRefs {
    public const int NUM_RL_ISLANDS = 31; // there are 31 islands, array needs to be 32 items because 1-indexing

    internal static float[] TEST_ISLAND_COORDS = [31.5f,5.50f];
    internal static Vector3 TEST_ISLAND_CENTER = new Vector3(300f,0f,375f);
    internal static Vector3 TEST_ISLAND_SIZE = new Vector3(1500f,0f,1500f);

    internal static int num_islands = 32; 

    internal static List<Island> IslandList;
}

internal class Island {
    public string scenePath;
    public int index;
    public Coord location;
    public GameObject terrainPrefab;
    public IslandHorizon horizon;
    //public IslandEconomy economy;
    //public IslandMarket market;

    public bool valid;


    public IslandConfig config;
}
