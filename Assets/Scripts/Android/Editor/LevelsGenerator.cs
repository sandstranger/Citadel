using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Citadel.Android.Tools
{
    internal static class LevelsGenerator
    {
        private const int MaxLevels = 12;
        private static readonly string LevelsPrefabsLocations = Path.Combine("Assets","Resources", "Prefabs", "Levels");
        
        [MenuItem("Tools/Generate levels prefabs")]
        internal static void GenerateLevelsPrefabs()
        {
            LevelManager levelManager = GameObject.FindAnyObjectByType<LevelManager>();
            Const @consts = GameObject.FindAnyObjectByType<Const>();
            ConsoleEmulator consoleEmulator = @consts?.ConsoleEmulator;

            if (levelManager == null || consoleEmulator == null || consts == null)
            {
                return;
            }
            
            for (var i = 0; i <= MaxLevels; ++i)
            {
                var currentLevelPrefabsLocation = Path.Combine(LevelsPrefabsLocations, $"Level_{i}");
                var levelGeometryParent = new GameObject("LevelGeometry");
                var levelLightsParent = new GameObject("LevelLights");
                LoadLevelGeometry(consts,consoleEmulator, levelManager,i, levelGeometryParent, levelLightsParent);
             //   LoadLevelLights(consts,consoleEmulator, levelManager,i, levelLightsParent);

             if (File.Exists(currentLevelPrefabsLocation))
             {
                 File.Delete(currentLevelPrefabsLocation);
             }
             SavePrefab(levelGeometryParent, currentLevelPrefabsLocation);
              //  SavePrefab(levelLightsParent, currentLevelPrefabsLocation);
                Object.DestroyImmediate(levelGeometryParent);
                Object.DestroyImmediate(levelLightsParent);
            }
            
            AssetDatabase.Refresh();
        }

        private static void SavePrefab(GameObject go, string saveDir)
        {
            if (go.transform.childCount > 0)
            {
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                
                PrefabUtility.SaveAsPrefabAsset(go, Path.Combine(saveDir, $"{go.name}.prefab"));
            }
        }
        
        private static async UniTask LoadLevelGeometry(Const @const,ConsoleEmulator consoleEmulator, LevelManager levelManager, 
            int curlevel, GameObject levelGeometryParent, GameObject lightsParent ) {
            if (curlevel < 0) return;
		
            string gName = "CitadelScene_geometry_level"+curlevel.ToString()+".txt";
            StreamReader sf = Utils.ReadStreamingAsset(gName);
            if (sf == null) {
                UnityEngine.Debug.Log("Geometry input file path invalid");
                return;
            }

            string readline;
            int lineNum = 0;
            char splitter = Convert.ToChar(SaveLoad.splitChar);
            Transform parent,child;
            int count = 0;
            Light lit;
            List<Light> chunkLights = new List<Light>();
            GameObject go;
            using (sf) {
                do {
                    readline = sf.ReadLine();
                    if (readline == null) break;

                    string[] entries = readline.Split(splitter);
				
                    go = await SaveLoad.LoadPrefab(@const,consoleEmulator,levelManager, entries,lineNum,curlevel, levelGeometryParent, lightsParent);
                    if (go != null)
                    {
                        go.transform.SetParent(levelGeometryParent.transform,false);
                        parent = go.transform;
                    }
                    else
                    {
                        parent = null;
                    }
				
                    if (parent != null) {
                        // Move all lights off of the prefab and into the cullable lights container.
                        child = null;
                        count = parent.childCount;
                        for (int i=0;i<count;i++) {
                            child = parent.GetChild(i);
                            lit = child.GetComponent<Light>();
                            if (lit != null) chunkLights.Add(lit);
                        }
                    }
                    lineNum++;
                } while (!sf.EndOfStream);
			
                for (int i=0;i<chunkLights.Count;i++) {
                    lit = chunkLights[i];
                    lit.enabled = true;
                    lit.lightmapBakeType = LightmapBakeType.Mixed;
                    lit.gameObject.name = "ChunkLight_" + lit.gameObject.name;
                    GameObject.DestroyImmediate(lit.gameObject);
//                    lit.transform.SetParent(lightsParent.transform,true);
// 				UnityEngine.Debug.Log("Moved light off of " + lit.gameObject.name);
                }
			
                sf.Close();
            }
        }
        
        private static async UniTask LoadLevelLights(Const @const,ConsoleEmulator consoleEmulator, 
            LevelManager levelManager,int curlevel,GameObject lightParent) {
            if (curlevel > MaxLevels) return;
            if (curlevel < 0) return;

            string lName = "CitadelScene_lights_level"+curlevel.ToString()+".txt";
            StreamReader sf = Utils.ReadStreamingAsset(lName);
            if (sf == null) {
                UnityEngine.Debug.Log("Lights input file path invalid");
                return;
            }

            string readline;
            int lineNum = 0;
            char splitter = Convert.ToChar(SaveLoad.splitChar);
            using (sf) {
                do {
                    readline = sf.ReadLine();
                    if (readline == null) break;
				
                    string[] entries = readline.Split(splitter);
                    var light = await SaveLoad.LoadPrefab(@const,consoleEmulator, 
                        levelManager, entries,lineNum,curlevel, null, lightParent);
                    var lightComponent = light.GetComponent<Light>();
                    lightComponent.enabled = true;
                    lightComponent.lightmapBakeType = LightmapBakeType.Mixed;
                    light.transform.SetParent(lightParent.transform, false);
                    lineNum++;
                } while (!sf.EndOfStream);
                sf.Close();
            }
        }
    }
}