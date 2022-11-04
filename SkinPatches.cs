

using BepInEx;
using FreeStyle.Unity.Common;
using FreeStyle.Unity.Obakeidoro;
using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using FreeStyle.Unity.Menu;
using Directory = Il2CppSystem.IO.Directory;
using Path = Il2CppSystem.IO.Path;
using System.Linq;
using File = Il2CppSystem.IO.File;
using UnhollowerBaseLib;
using static System.Net.Mime.MediaTypeNames;
using Il2CppDumper;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using UnhollowerRuntimeLib;
using BepInEx.IL2CPP.Utils.Collections;
using Application = UnityEngine.Application;
using Il2CppSystem.Threading.Tasks;

namespace BailSkinLoader
{	

	public static class SkinPatches
	{
        public static Dictionary<string, ResourceManager.ResourceInfo> objectList;

        public static Dictionary<string, UnityEngine.Object> originalObjects = new Dictionary<string, UnityEngine.Object>();

        public static GameObject examinedModel;

        public static List<AssetBundle> realAssetBundles = new List<AssetBundle>();

        public static bool orbinTime = true;

        public static GameObject obj;

        public static bool shouldReplaceTexturesFromBundles = false; //disabling by default so that png textures take priority, if a better solution for managing both arrives/more mods demand it i can turn it on


        public static void ReplaceTexture(string textureName, UnityEngine.Object theTexture, string theClass = "UnityEngine.Texture2D")
        {
            var tempObject = new UnityEngine.Object();
            if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(textureName) && originalObjects.TryGetValue(textureName, out tempObject) == false)
            {
                BailSkinLoaderPlugin.Instance.Log.LogInfo("Trying to replace asset: "+textureName);


                for (var infuriating = 0; infuriating < OrgResources.GetInstance().m_unityObjectMap[textureName].objects.Length; infuriating++)
                {

                    if (OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating].GetIl2CppType().FullName.StartsWith(theClass))
                    {


                        switch (theClass)
                        {
                            case "UnityEngine.Texture2D":
                                SkinPatches.originalObjects.Add(textureName, OrgResources.GetInstance().Load<Texture2D>(textureName));
                                OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<Texture2D>();
                                break;
                            case "UnityEngine.GameObject":
                                SkinPatches.originalObjects.Add(textureName, OrgResources.GetInstance().Load<GameObject>(textureName));
                                OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<GameObject>();
                                break;
                            case "UnityEngine.AnimationClip":
                                SkinPatches.originalObjects.Add(textureName, OrgResources.GetInstance().Load<AnimationClip>(textureName));
                                OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<AnimationClip>();
                                break;
                            case "UnityEngine.AudioClip": 
                                SkinPatches.originalObjects.Add(textureName, OrgResources.GetInstance().Load<UnityEngine.AudioClip>(textureName));
                                OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<AudioClip>();
                                break;

                            default:
                                BailSkinLoaderPlugin.Instance.Log.LogError("class name " + theClass + " was not in the list! " + textureName);
                                break;
                        }
                    }

                    else
                    {
                        BailSkinLoaderPlugin.Instance.Log.LogError("this asset was not a " + theClass + "! " + textureName);
                    }

                }
            }
            else
            {
                BailSkinLoaderPlugin.Instance.Log.LogInfo("no key exists for texture " + textureName + " so we added it");
                OrgResources.GetInstance().m_unityObjectMap.Add(textureName, new ResourceManager.ResourceInfo());
                OrgResources.GetInstance().m_unityObjectMap[textureName].objects.AddItem(theTexture);
            }

        }


        [HarmonyPatch(typeof(ChrAsset.Item), nameof(ChrAsset.Item.GetText))]
        public static class Patch_ChrAsset_Item_GetText
        {

            public static void Postfix(ref ChrAsset.Item __instance)
            {
                try
                {
                    var thisCharacterTexPath = Path.Combine(BailSkinLoaderPlugin.rootCustomTexPath, __instance.id.ToString());
                    if (System.IO.Directory.Exists(thisCharacterTexPath))
                    {
                        UnityEngine.Object tempObject;
                        //var subDirectories = Directory.GetDirectories(thisCharacterTexPath);

                        //foreach (var subDirectory in subDirectories)
                        //{
                        //
                        //}
                        foreach (string text in from x in Directory.GetFiles(thisCharacterTexPath)
                                                where x.ToLower().EndsWith(".assets")
                                                select x)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
                            if (SkinPatches.orbinTime == true)
                            {
                                BailSkinLoaderPlugin.Instance.Log.LogInfo("trying to load assetbundle: " + text);

                                AssetBundle realAssetBundle = AssetBundle.LoadFromFile(text);
                                if (realAssetBundle != null)
                                {
                                    realAssetBundles.Add(realAssetBundle);
                                }

                                for (var asseteye = 0; asseteye < realAssetBundle.AllAssetNames().Length; asseteye++)
                                {

                                    string realAssetName = Path.GetFileNameWithoutExtension(realAssetBundle.AllAssetNames()[asseteye]);

                                    if (Path.GetFileName(realAssetBundle.AllAssetNames()[asseteye]).EndsWith(".prefab"))
                                    {

                                        GameObject realAsset = (GameObject)realAssetBundle.LoadAsset<GameObject>(realAssetName);
                                        examinedModel = realAsset;


                                        GameObject tempBoner = OrgResources.GetInstance().Load<GameObject>(realAssetName);
                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("got TempBoner: " + tempBoner.ToString());


                                        SkinnedMeshRenderer tempBoney = tempBoner.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();

                                        if (tempBoney)
                                        {
                                            tempBoney = tempBoner.transform.Find(tempBoney.name).gameObject.GetComponent<SkinnedMeshRenderer>();
                                            BailSkinLoaderPlugin.Instance.Log.LogInfo("got BoneRenderer: " + tempBoney.ToString());


                                            for (var eye = 0; eye < (realAsset.transform.childCount - 1); eye++)
                                            {
                                                var skinnyMesh = realAsset.transform.GetChild(eye).gameObject.GetComponent<SkinnedMeshRenderer>();

                                                if (skinnyMesh)
                                                {
                                                    var childName = realAsset.transform.GetChild(eye).name;
                                                    BailSkinLoaderPlugin.Instance.Log.LogInfo("got skinnyMesh: " + childName);
                                                    tempBoney.sharedMesh = skinnyMesh.sharedMesh;
                                                    tempBoney.bones = skinnyMesh.bones;



                                                    if (shouldReplaceTexturesFromBundles)
                                                    {
                                                        ReplaceTexture(tempBoney.material.mainTexture.name, skinnyMesh.material.mainTexture.Cast<Texture2D>(), "UnityEngine.Texture2D");
                                                    }
                                                    if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(realAssetName) && originalObjects.TryGetValue(realAssetName, out tempObject) == false)
                                                    {
                                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("replacing GameObject: " + realAssetName);
                                                        ReplaceTexture(tempBoney.material.mainTexture.name, skinnyMesh.material.mainTexture.Cast<Texture2D>(), "UnityEngine.GameObject");

                                                    }
                                                    else
                                                    {
                                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("no key exists for object " + realAssetName + " so we added it");
                                                        OrgResources.GetInstance().m_unityObjectMap.Add(realAssetName, new ResourceManager.ResourceInfo());
                                                        OrgResources.GetInstance().m_unityObjectMap[realAssetName].objects.AddItem(tempBoney);
                                                    }

                                                    BailSkinLoaderPlugin.Instance.Log.LogInfo("tried to replace skinnedmeshrenderer: " + tempBoner.name);

                                                }

                                            }

                                        }
                                    }
                                    else if (Path.GetFileName(realAssetBundle.AllAssetNames()[asseteye]).EndsWith(".anim"))
                                    {

                                        AnimationClip realAsset = realAssetBundle.LoadAsset<AnimationClip>(realAssetName);

                                        AnimationClip tempBoner = OrgResources.GetInstance().Load<AnimationClip>(realAssetName);
                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("got TempBoner: " + tempBoner.ToString());
                                        ReplaceTexture(realAssetName, tempBoner, "UnityEngine.AnimationClip");

                                    } else if (Path.GetFileName(realAssetBundle.AllAssetNames()[asseteye]).EndsWith(".ogg"))
                                    {

                                        AudioClip realAsset = realAssetBundle.LoadAsset<AudioClip>(realAssetName);

                                        AudioClip tempBoner = OrgResources.GetInstance().Load<AudioClip>(realAssetName);
                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("got TempBoner: " + tempBoner.ToString());
                                        ReplaceTexture(realAssetName, tempBoner, "UnityEngine.AudioClip");
                                    }
                                }






                            }

                        }

                        foreach (string text in from x in Directory.GetFiles(thisCharacterTexPath)
                                                where x.ToLower().EndsWith(".png")
                                                select x)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);

                            BailSkinLoaderPlugin.Instance.Log.LogInfo("Found PNG: " + text);
                            BailSkinLoaderPlugin.Instance.Log.LogInfo("Without Extension: " + fileNameWithoutExtension);

                            if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(fileNameWithoutExtension)) {
                                OrgResources.GetInstance().Load(fileNameWithoutExtension);
                            }


                            Texture2D tex = null;
                            byte[] fileData;

                            //if (__instance.m_unityObjectMap[path].objects[0].GetType().Name == "Texture2D")
                            //{
                            fileData = File.ReadAllBytes(text);
                            tex = new Texture2D(2, 2);
                            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                            ReplaceTexture(fileNameWithoutExtension, tex, "UnityEngine.Texture2D");



                        }


                        BailSkinLoaderPlugin.Instance.Log.LogInfo("tried to replace ChrAsset Add");
                    }
                }
                catch (Exception e)
                {
                    BailSkinLoaderPlugin.Instance.Log.LogError($"there was an exception {e}");
                }
            }
        }




        }
	


	
	}