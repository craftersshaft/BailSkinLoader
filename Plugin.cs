using BepInEx;
using BepInEx.IL2CPP;
using BailSkinLoader;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using FreeStyle.Unity.Obakeidoro;
using FreeStyle.Unity.Menu;
using AccountDefines;
using Il2CppSystem.IO;

namespace BailSkinLoader
{

    [BepInPlugin("craftersshaft.bailorjailmods.bailskinloader", "Bail or Jail Skin Loader", "0.0.2")]
    public class BailSkinLoaderPlugin : BasePlugin
    {
        internal static BailSkinLoaderPlugin Instance;
        public static string rootCustomTexPath;
        public override void Load()
        {
            Instance = this;
            // Plugin startup logic
            Log.LogInfo($"Plugin BailSkinLoader is loaded!");
            rootCustomTexPath = Path.Combine(Paths.BepInExRootPath, "CustomTextures");
            rootCustomTexPath = rootCustomTexPath.Replace("\\", "/");
            Directory.CreateDirectory(rootCustomTexPath);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

}