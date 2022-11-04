BailSkinLoader - Bail or Jail Skin Loader (it loads skins for the game Bail or Jail)

poorly coded by craftersshaft

WARNING: This mod has not been tested online, and it might be against the game's terms of service if used online in public lobbies.
It's also very early work-in-progress, and some double checks haven't been implemented. Also this readme might suck at explaining certain things.
This mod is not associated with or sponsored by Konami or FreeStyle, and this mod does not include any code directly from the game.



HOW TO INSTALL:

If you don't already have bepinex, get the 6.0.0-pre1 build, specifically IL2CPP. https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1

Put the compiled DLL file in the bepinex/plugins folder, not a subfolder for now.

on launch, the bepinex folder should have a CustomTextures folder.


HOW TO ADD SKIN MODS:

In the CustomTextures folder, you need to make a folder corresponding to the number of the character you want to replace.
Right now, the folder with the character id should contain only .png or .assets files.



HOW TO MAKE SKIN MODS:

I don't have a premade list yet, but you can use IL2CPP (NOT the CoreCLR Version) UnityExplorer and find the ChrAsset singleton: https://github.com/sinai-dev/UnityExplorer/releases/

For creating .png mods, Any texture size should work unless something messed up, but to get the textures you might need to search around resources.assets with AssetStudioGUI: https://github.com/Perfare/AssetStudio
(also to note: there is no checks for whether the texture always applies to the character you chose, so be careful. there are checks to ensure that only Texture2D objects get created.)

For creating .assets mods, you'll need to install the Unity Editor (2020.3.15 worked for me) and make sure you can make/already have a project with the fbx files corresponding to models you want to replace. 
Make a prefab with the name of the asset you want to replace (no folder names/extensions before .prefab) and make the first two children in the hiearchy the SkinnedMeshRenderer and the skeleton.
You can drag and drop an FBX file onto the stage, and Unpack it if the nodes don't move. also make sure read/write is enabled on the fbx
In the assets list at the bottom of the inspector, there should be an AssetBundle thing with two dropdowns. Name the first one whatever you want, but make sure the second one is just "assets".
Find a BuildAssetBundles script that can export your new bundle into an .assets file, then drag into the bepinex/CustomTextures/(character id) folder.