

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

                                SkinPatches.originalObjects.Add(fileNameWithoutExtension, OrgResources.GetInstance().m_unityObjectMap[realAssetName].objects[0]);


                                GameObject tempBoner = OrgResources.GetInstance().Load<GameObject>("body103003000");
                                BailSkinLoaderPlugin.Instance.Log.LogInfo("got TempBoner: " + tempBoner.ToString());
                                SkinnedMeshRenderer tempBoney = tempBoner.transform.Find("mesh_body103003000").gameObject.GetComponent<SkinnedMeshRenderer>();
                                BailSkinLoaderPlugin.Instance.Log.LogInfo("got BoneRenderer: " + tempBoney.ToString());

                                for (var eye = 0; eye < (realAsset.transform.childCount - 1); eye++)
                                {
                                    var skinnyMesh = realAsset.transform.GetChild(eye).gameObject.GetComponent<SkinnedMeshRenderer>();

                                    if (skinnyMesh)
                                    {
                                        var childName = realAsset.transform.GetChild(eye).name;
                                        tempBoney.sharedMesh = skinnyMesh.sharedMesh;
                                        tempBoney.bones = skinnyMesh.bones;
                                        //tempBoney.material.mainTexture = skinnyMesh.material.mainTexture;
                                        if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(childName) && originalObjects.TryGetValue(childName, out tempObject) == false)
                                        {
                                            if (!SkinPatches.originalObjects.ContainsKey(childName))
                                            {
                                                SkinPatches.originalObjects.Add(childName, OrgResources.GetInstance().m_unityObjectMap[childName].objects[0]);
                                            }

                                            if (OrgResources.GetInstance().m_unityObjectMap[childName].objects[0].GetIl2CppType().FullName.StartsWith("UnityEngine.SkinnedMeshRenderer"))
                                            {
                                                OrgResources.GetInstance().m_unityObjectMap[childName].objects[0] = tempBoney;
                                            }
                                            else
                                            {
                                                BailSkinLoaderPlugin.Instance.Log.LogError("this asset was not a SkinnedMeshRenderer! " + childName);
                                            }
                                        }
                                        else
                                        {
                                            BailSkinLoaderPlugin.Instance.Log.LogInfo("no key exists for object " + childName + " so we added it");
                                            OrgResources.GetInstance().m_unityObjectMap.Add(childName, new ResourceManager.ResourceInfo());
                                            OrgResources.GetInstance().m_unityObjectMap[childName].objects.AddItem(tempBoney);
                                        }

                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("tried to replace skinnedmeshrenderer: " + tempBoner.name);

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
                            OrgResources.GetInstance().Load(fileNameWithoutExtension);


                            Texture2D tex = null;
                            byte[] fileData;

                            //if (__instance.m_unityObjectMap[path].objects[0].GetType().Name == "Texture2D")
                            //{
                            fileData = File.ReadAllBytes(text);
                            tex = new Texture2D(2, 2);
                            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                            if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(fileNameWithoutExtension) && originalObjects.TryGetValue(fileNameWithoutExtension, out tempObject) == false)
                            {
                                SkinPatches.originalObjects.Add(fileNameWithoutExtension, OrgResources.GetInstance().m_unityObjectMap[fileNameWithoutExtension].objects[0]);

                                if (OrgResources.GetInstance().m_unityObjectMap[fileNameWithoutExtension].objects[0].GetIl2CppType().FullName.StartsWith("UnityEngine.Texture2D")) {
                                    OrgResources.GetInstance().m_unityObjectMap[fileNameWithoutExtension].objects[0] = tex;
                                } else
                                {
                                    BailSkinLoaderPlugin.Instance.Log.LogError("this asset was not a texture! " + fileNameWithoutExtension);
                                }
                            }
                            else
                            {
                                BailSkinLoaderPlugin.Instance.Log.LogInfo("no key exists for texture " + fileNameWithoutExtension + " so we added it");
                                OrgResources.GetInstance().m_unityObjectMap.Add(fileNameWithoutExtension, new ResourceManager.ResourceInfo());
                                OrgResources.GetInstance().m_unityObjectMap[fileNameWithoutExtension].objects.AddItem(tex);
                            }



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