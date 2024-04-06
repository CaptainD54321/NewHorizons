# New Horizons
A mod to easily load new islands into Sailwind.

This mod is currently in an alpha pre-release state. If you encounter any issues, please let me know either in the Sailwind discord, or via an issue report on this github.

New Horizons loads in community-created islands using Unity's AssetBundle system, and automatically sets up and configures all the necessary scripts with as little work required from pack creators as possible.

To install islandpacks: On first launch, New Horizons will automatically create a folder named `islandpacks` inside the `BepInEx` folder in your game directory; Unzip islandpacks and place them inside this folder, and they will automatically load when you start the game.

Pack creators: Please see the [documentation on islandpacks here](documentation/islandpacks.md)

## Current features:
* Loading in terrain and scenery assets from islandpacks and placing them in the correct place in the ocean.
* Horizon effects working.
* Island scenery loading and unloading at distance for performance
* Ocean waves properly reacting to terrain depth
* Docks and dock moorings at custom islands automatically set up and working.

## Planned features for 1.0 release:
* Economies at custom islands, with missions & trade cargo
* Full integration with Raw Lion's economy system
* Possibly shipyards at custom islands

## Planned post-1.0 features:
* Custom regions, possibly with custom currencies as well.
* Custom maps for mod regions.
* Ability to expand map boundaries.
* Integration with NANDbrew's [Simple Tides](https://github.com/NANDbrew/SimpleTides).
* Possibly other features, if suggested by the community or if I think of other features.
