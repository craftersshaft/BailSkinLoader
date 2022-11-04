

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

namespace BailSkinLoader
{	
	public static class SkinPatches
	{
        public static Dictionary<string, ResourceManager.ResourceInfo> objectList;

        public static Dictionary<string, UnityEngine.Object> originalObjects = new Dictionary<string, UnityEngine.Object>();

        public static Il2CppReferenceArray<UnityEngine.Object> assetList;

        public static GameObject examinedModel;

        public static List<AssetBundle> realAssetBundles = new List<AssetBundle>();

        public static bool orbinTime = true;

        public static bool shouldReplaceTexturesFromBundles = false; //disabling by default so that png textures take priority, if a better solution for managing both arrives/more mods demand it i can turn it on

        public static void ReplaceTexture(string textureName, Texture2D theTexture)
        {
            var tempObject = new UnityEngine.Object();
            if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(textureName) && originalObjects.TryGetValue(textureName, out tempObject) == false)
            {
                SkinPatches.originalObjects.Add(textureName, OrgResources.GetInstance().m_unityObjectMap[textureName].objects[0]);

                if (OrgResources.GetInstance().m_unityObjectMap[textureName].objects[0].GetIl2CppType().FullName.StartsWith("UnityEngine.Texture2D"))
                {
                    OrgResources.GetInstance().m_unityObjectMap[textureName].objects[0] = theTexture;
                }
                else
                {
                    BailSkinLoaderPlugin.Instance.Log.LogError("this asset was not a texture! " + textureName);
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
                                string realAssetName = Path.GetFileNameWithoutExtension(realAssetBundle.AllAssetNames()[0]);

                                GameObject realAsset = (GameObject)realAssetBundle.LoadAsset<GameObject>(realAssetName);
                                examinedModel = realAsset;


                                GameObject tempBoner = OrgResources.GetInstance().Load<GameObject>(realAssetName);
                                BailSkinLoaderPlugin.Instance.Log.LogInfo("got TempBoner: " + tempBoner.ToString());

                                for (var guy = 0; guy < (tempBoner.transform.childCount - 1); guy++)
                                {
                                    SkinnedMeshRenderer tempBoney = tempBoner.transform.GetChild(guy).gameObject.GetComponent<SkinnedMeshRenderer>();

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

                                                    ReplaceTexture(tempBoney.material.mainTexture.name, skinnyMesh.material.mainTexture.Cast<Texture2D>());
                                                }
                                                if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(realAssetName) && originalObjects.TryGetValue(realAssetName, out tempObject) == false)
                                                {
                                                    if (!SkinPatches.originalObjects.ContainsKey(realAssetName))
                                                    {
                                                        SkinPatches.originalObjects.Add(realAssetName, OrgResources.GetInstance().m_unityObjectMap[realAssetName].objects[0]);
                                                    }

                                                    for (var aye = 0; aye < (OrgResources.GetInstance().m_unityObjectMap[realAssetName].objects.Count - 1); aye++)
                                                    {

                                                        if (OrgResources.GetInstance().m_unityObjectMap[realAssetName].objects[aye].GetIl2CppType().FullName.StartsWith("UnityEngine.GameObject"))
                                                        {
                                                            BailSkinLoaderPlugin.Instance.Log.LogInfo("replacing GameObject: " + realAssetName);
                                                            OrgResources.GetInstance().m_unityObjectMap[realAssetName].objects[aye] = tempBoner;
                                                        }
                                                    }
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
                            ReplaceTexture(fileNameWithoutExtension, tex);



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