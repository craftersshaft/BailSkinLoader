

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
using static FreeStyle.Unity.Obakeidoro.AnimEventAsset;
using Zentame2018_Unity;

namespace BailSkinLoader
{	

	public static class SkinPatches
	{
        public static Dictionary<string, ResourceManager.ResourceInfo> objectList;

        public static GameObject examinedModel;

        public static List<AssetBundle> realAssetBundles = new List<AssetBundle>();

        public static bool orbinTime = true;

        public static bool shouldReplaceAudio = true; //setting to true by default for now unless we have An Incident

        public static GameObject obj;

        public static AudioClip debugClip;

        public static List<string> alreadyReplacedAudio = new List<string>();

        public static int increMentally = 0;

        public static bool shouldReplaceTexturesFromBundles = false; //disabling by default so that png textures take priority, if a better solution for managing both arrives/more mods demand it i can turn it on


        public static void AddAnimEvents(string animFileName, Il2CppSystem.Collections.Generic.List<SoundEventData> soundEvents, Il2CppSystem.Collections.Generic.List<EffectEventData> effectEvents, Il2CppSystem.Collections.Generic.List<TagEventData> tagEvents)
        {
            var testible = new AnimEventItem();
            testible.animFileName = animFileName;
            testible.dataName = "Skin Loader Custom Sound " + animFileName;

            if (soundEvents != null) {
                for (var soundNum = 0; soundNum < soundEvents.Count - 1; soundNum++)
                {
                    FreeStyle.Unity.Obakeidoro.AnimEventHelper._AddSoundEvent(FreeStyle.Unity.Common.OrgResources.GetInstance().Load<AnimationClip>(animFileName).Cast<AnimationClip>(), testible.soundEvent[soundNum]);
                }
            } else { soundEvents = new Il2CppSystem.Collections.Generic.List<SoundEventData>(); }
            if (effectEvents != null)
            {
                for (var effectNum = 0; effectNum < effectEvents.Count - 1; effectNum++)
                {
                    FreeStyle.Unity.Obakeidoro.AnimEventHelper._AddEffectEvent(FreeStyle.Unity.Common.OrgResources.GetInstance().Load<AnimationClip>(animFileName).Cast<AnimationClip>(), testible.effectEvent[effectNum]);
                }
            }else { effectEvents = new Il2CppSystem.Collections.Generic.List<EffectEventData>(); }
            if (tagEvents != null)
            {
                for (var tagNum = 0; tagNum < tagEvents.Count - 1; tagNum++)
                {
                    FreeStyle.Unity.Obakeidoro.AnimEventHelper._AddTagEvent(FreeStyle.Unity.Common.OrgResources.GetInstance().Load<AnimationClip>(animFileName).Cast<AnimationClip>(), testible.tagEvent[tagNum]);
                }
            }
            else { tagEvents = new Il2CppSystem.Collections.Generic.List<TagEventData>(); }

            testible.soundEvent = soundEvents; //sorry for all this
            testible.effectEvent = effectEvents; //this code may be spaghetti
            testible.tagEvent = tagEvents; //but at least it works

            if (!AnimEventAsset.entity.animEventItems.Contains(testible)) {
                FreeStyle.Unity.Obakeidoro.AnimEventAsset.entity.animEventItems.Add(testible);
                BailSkinLoaderPlugin.Instance.Log.LogInfo("added AnimEvents to animation " + animFileName);
            }
        }

        public static void QuickAddSoundEvent(string animFileName, int soundId)
        {

            var soundEvents = new Il2CppSystem.Collections.Generic.List<SoundEventData>();

            var soundEventThingy = new SoundEventData();
            soundEventThingy.soundId = soundId;
            soundEventThingy.eventTime = 0;
            soundEvents.Add(soundEventThingy);

            AddAnimEvents(animFileName, soundEvents, null, null);


        }

        public static void QuickAddEffectEvent(string animFileName, int effectId)
        {

            var effectEvents = new Il2CppSystem.Collections.Generic.List<EffectEventData>();

            var effectEventThingy = new EffectEventData();
            effectEventThingy.effectId = effectId;
            effectEventThingy.eventTime = 0;
            effectEvents.Add(effectEventThingy);

            AddAnimEvents(animFileName, null, effectEvents, null);


        }

        public static void QuickAddSoundEffectEvent(string animFileName, int soundId, int effectId)
        {

            var soundEvents = new Il2CppSystem.Collections.Generic.List<SoundEventData>();

            var soundEventThingy = new SoundEventData();
            soundEventThingy.soundId = soundId;
            soundEventThingy.eventTime = 0;
            soundEvents.Add(soundEventThingy);

            var effectEvents = new Il2CppSystem.Collections.Generic.List<EffectEventData>();

            var effectEventThingy = new EffectEventData();
            effectEventThingy.effectId = effectId;
            effectEventThingy.eventTime = 0;
            effectEvents.Add(effectEventThingy);

            AddAnimEvents(animFileName, soundEvents, effectEvents, null);


        }


        public static void ReplaceTexture(string textureNameUpper, UnityEngine.Object theTexture, string theClass = "UnityEngine.Texture2D")
        {
            string textureName = textureNameUpper.ToLower();
            var tempObject = new UnityEngine.Object();
            if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(textureName))
            {
                BailSkinLoaderPlugin.Instance.Log.LogInfo("Trying to replace asset: "+ textureName);


                for (var infuriating = 0; infuriating < OrgResources.GetInstance().m_unityObjectMap[textureName].objects.Length; infuriating++)
                {

                    if (OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating].GetIl2CppType().FullName.StartsWith(theClass))
                    {


                        switch (theClass)
                        {
                            case "UnityEngine.Texture2D":
                                OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<Texture2D>();
                                break;
                            case "UnityEngine.GameObject":
                                OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<GameObject>();
                                break;
                            case "UnityEngine.AnimationClip":
                                OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<AnimationClip>();
                                break;
                            case "UnityEngine.AudioClip":
                                if (shouldReplaceAudio)
                                {
                                    OrgResources.GetInstance().m_unityObjectMap[textureName].objects[infuriating] = theTexture.Cast<AudioClip>();
                                    if (textureName.StartsWith("bgm_"))
                                    {
                                        SoundManager.instance.m_resourceLoader.m_unityObjectMap["Sound/Bgm/"+textureName] = theTexture.Cast<AudioClip>();
                                    } else
                                    {
                                        SoundManager.instance.m_resourceLoader.m_unityObjectMap[textureName] = theTexture.Cast<AudioClip>();
                                    }
                                    debugClip = theTexture.Cast<AudioClip>();
                                    alreadyReplacedAudio.Add(textureName);
                                }
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
                BailSkinLoaderPlugin.Instance.Log.LogInfo("no key exists for "+theClass+" " + textureName + " so we added it");
                OrgResources.GetInstance().m_unityObjectMap.Add(textureName, new ResourceManager.ResourceInfo());
                OrgResources.GetInstance().m_unityObjectMap[textureName].objects = new Il2CppReferenceArray<UnityEngine.Object>(1);
                switch (theClass)
                {
                    case "UnityEngine.Texture2D":
                        OrgResources.GetInstance().m_unityObjectMap[textureName].objects[0] = (theTexture.Cast<Texture2D>());
                        break;
                    case "UnityEngine.GameObject":
                        OrgResources.GetInstance().m_unityObjectMap[textureName].objects[0] = (theTexture.Cast<GameObject>());
                        break;
                    case "UnityEngine.AudioClip":
                        ResourceManager.GetInstance().m_unityObjectMap[textureName].objects[0] = (theTexture.Cast<AudioClip>());
                        var awwdio = new SoundResourceAsset.Item();
                        awwdio.delay = 0;
                        awwdio.distance = 10;
                        awwdio.filePath = textureName;
                        awwdio.volume = 1;
                        awwdio.name = "Skin Loader Custom Sound" + textureName;
                        increMentally++;
                        awwdio.soundId = 69420 + increMentally;
                        SoundResourceAsset.instance.items.Add(awwdio);
                        if (textureName.StartsWith("bgm_"))
                        {
                            BailSkinLoaderPlugin.Instance.Log.LogInfo("we should try to replace Sound/Bgm/" + textureNameUpper);
                            if (SoundManager.instance.m_resourceLoader.m_unityObjectMap.ContainsKey("Sound/Bgm/")) {
                                SoundManager.instance.m_resourceLoader.m_unityObjectMap["Sound/Bgm/" + textureNameUpper] = theTexture.Cast<AudioClip>();
                            } else
                            {
                                SoundManager.instance.m_resourceLoader.m_unityObjectMap.Add("Sound/Bgm/" + textureNameUpper, theTexture.Cast<AudioClip>());
                            }
                        }
                        else
                        {
                            SoundManager.instance.m_resourceLoader.m_unityObjectMap[textureName] = theTexture.Cast<AudioClip>();
                        }

                        break;
                }
            }

        }

        [HarmonyPatch(typeof(ResourceLoader), nameof(ResourceLoader.LoadAsset))]

        public static class Patch_ResourceLoader_LoadAsset
        {
            public static void Postfix (ref ResourceLoader __instance, ref string path)
            {
                UnityEngine.Object tempObject;
                if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(path) && __instance.m_unityObjectMap.TryGetValue(path, out tempObject) == false)
                {
                    __instance.m_unityObjectMap.Add(path, OrgResources.GetInstance().Load(path));
                    BailSkinLoaderPlugin.Instance.Log.LogInfo("manually needed to add sound effect "+path);
                }
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
                                                    if (OrgResources.GetInstance().m_unityObjectMap.ContainsKey(realAssetName))
                                                    {
                                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("replacing GameObject: " + realAssetName);
                                                        //ReplaceTexture(realAssetName, skinnyMesh.material.mainTexture.Cast<Texture2D>(), "UnityEngine.Texture2D");

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

                                    } else if (Path.GetFileName(realAssetBundle.AllAssetNames()[asseteye]).EndsWith(".ogg") || Path.GetFileName(realAssetBundle.AllAssetNames()[asseteye]).EndsWith(".wav"))
                                    {

                                        AudioClip realAsset = realAssetBundle.LoadAsset<AudioClip>(Path.GetFileName(realAssetBundle.AllAssetNames()[asseteye]));

                                        //AudioClip tempBoner = OrgResources.GetInstance().Load<AudioClip>(Path.GetFileNameWithoutExtension(realAssetName));
                                        //this caused issues with new custom sfx
                                        //realAsset.name = tempBoner.name;

                                        BailSkinLoaderPlugin.Instance.Log.LogInfo("got RealAsset: " + realAsset.ToString());
                                        ReplaceTexture(realAssetName, realAsset, "UnityEngine.AudioClip");
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

                        foreach (string text in from x in Directory.GetFiles(thisCharacterTexPath)
                                                where x.ToLower().EndsWith(".wav")
                                                select x)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
                            if (!alreadyReplacedAudio.Contains(fileNameWithoutExtension)) {
                                BailSkinLoaderPlugin.Instance.Log.LogInfo("trying to add wav sfx " + fileNameWithoutExtension);
                                AudioClip aaEeOoAudioJungle = WavUtility.ToAudioClip(text);
                                if (aaEeOoAudioJungle != null)
                                {
                                    ReplaceTexture(fileNameWithoutExtension, aaEeOoAudioJungle, "UnityEngine.AudioClip");
                                }


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