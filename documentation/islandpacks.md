# Islandpack Format
An island pack consists of two Unity AssetBundles, one containing the terrain prefabs for all islands in the pack, and the other containing the scenery scenes for the islands.
## AssetBundles

To create the assetbundles for an islandpack, you will need a Unity 2019 project; as the new project templates for Unity 2019 are broken, [here is an empty project] with an editor script to build the assetbundles.

Each island in an islandpack needs a prefab containing the terrain that will be spawned in when the game loads, and a scene containing scenery objects that will be loaded only when the player is near the island, for performance. I recommend building the island in one "test" scene with both the terrain and the scenery, and then making the terrain a prefab, and copying the scenery to a second scene.

A few notes that apply to both the terrain and the scenery:
* Each piece of the island should be a single empty parent GameObject with as many children as necessary.
  * For the terrain this object will be the root of the prefab.
  * For the scenery this parent should be the *only* object at the root of the scenery scene (delete the camera and light Unity creates in new scenes)
* Certain objects in both the terrain and the scenery require specific names to be set up properly in code; any names listed below that must be exact will be written in code blocks `like this`. These *are* case sensitive.
* For some named objects, only the exact object with that name will be configured, for others, all direct children of the object (i.e. not grandchildren etc) will be configured.
* If any named objects are missing, the island will still be spawned, although that object will obviously not be configured; this may be desired if you are making an island with no objects of that type (e.g. an island with no dock).
* Any child objects which do not have one of these names will not be modified.
* The origin position of the parent object for each will be considered the "center" of the island, and should be the same between both terrain and scenery, or else they will not align properly (I recommend making both parent objects be at 0,0,0 worldspace).
* Base sea level is Y height 0.
### Terrain Prefab
The terrain prefab should contain the base terrain of the island, and any other large objects that should be visible on the island from far away. Also, the terrain prefab contains the docking hooks for tying up boats, as these must remain loaded when the player leaves the island.

The following children of the prefab will be configured:
* `terrain`: This object *and* all its children will be considered to be terrain for ocean depth and for boat anchors.
* `dock_cleats`: All mooring cleats which boats can be tied up to must be in the terrain prefab (so that stay loaded and boats stay tied up when the player leaves the island), and must be direct children of this.

### Scenery Scene
The scenery scene should contain all detailed scenery (buildings, docks, structures, lights, etc), but no terrain. 

The following children will be configured:
* `docks`: For the player to be able to push away from the dock while on their boat, all docks (at least that a boat could be next to) must be children of this.

### Building AssetBundles
All terrain prefabs should be marked as part of an assetbundle named `terrain`, and all scenery scenes should be marked as part of an assetbundle named `scenery`. For more information on setting up assetbundles, see [this Unity manual page](https://docs.unity3d.com/2019.1/Documentation/Manual/AssetBundles-Workflow.html). The script included with the blank Unity project I linked above will add a menu item in the "Assets" menu at the top of your Unity window to build all assetbundles, and will place them in `Assets/Assetbundles`. If you have your own Unity project already, [here is that script] on its own. 

## JSON Manifest
