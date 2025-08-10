using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Android.Tools
{
    internal static class LevelsGenerator
    {
        private const int MaxLevels = 12;
        private static readonly string LevelsPrefabsLocations = Path.Combine("Assets","Resources", "Prefabs", "Levels");
        
        [MenuItem("Tools/Generate levels prefabs")]
        internal static void GenerateLevelsPrefabs()
        {
            if (LevelManager.a == null)
            {
                return;
            }

            if (Directory.Exists(LevelsPrefabsLocations))
            {
                Directory.Delete(LevelsPrefabsLocations, true);
            }
            
            for (var i = 0; i <= MaxLevels; ++i)
            {
                var currentLevelPrefabsLocation = Path.Combine(LevelsPrefabsLocations, $"Level_{i}");
                var levelGeometryParent = new GameObject("LevelGeometry");
                var levelLightsParent = new GameObject("LevelLights");
                LoadLevelGeometry(i, levelGeometryParent, levelLightsParent);
                LoadLevelLights(i, levelLightsParent);
                
                SavePrefab(levelGeometryParent, currentLevelPrefabsLocation);
                SavePrefab(levelLightsParent, currentLevelPrefabsLocation);
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
        
        private static void LoadLevelGeometry(int curlevel, GameObject levelGeometryParent, GameObject lightsParent ) {
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
				
                    go = SaveLoad.LoadPrefab(ref entries,lineNum,curlevel, levelGeometryParent, lightsParent);
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
                    lit.gameObject.name = "ChunkLight_" + lit.gameObject.name;
                    lit.transform.SetParent(lightsParent.transform,true);
// 				UnityEngine.Debug.Log("Moved light off of " + lit.gameObject.name);
                }
			
                sf.Close();
            }
        }
        
        private static void LoadLevelLights(int curlevel,GameObject lightParent) {
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
                    var light = SaveLoad.LoadPrefab(ref entries,lineNum,curlevel, null, lightParent);
                    light.transform.SetParent(lightParent.transform, false);
                    lineNum++;
                } while (!sf.EndOfStream);
                sf.Close();
            }
        }
    }
}