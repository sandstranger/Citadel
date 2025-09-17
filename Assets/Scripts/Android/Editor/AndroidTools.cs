using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Citadel.Editor.Utils;

namespace Citadel.Android.Tools
{
    internal static class AndroidTools
    {
        [MenuItem("Tools/Remove all SEGIEmitters from active scene")]
        private static void RemoveAllSegiEmittersFromActiveScene()
        {
            var emitters = GameObject.FindObjectsOfType<GameObject>(true).Where(gameobject => gameobject.name.Contains("SEGIEmitter"))
                .ToArray();

            foreach (var emitter in emitters)
            {
                GameObject.DestroyImmediate(emitter);
            }
        }

        [MenuItem("Tools/Update grass lods")]
        private static void UpdateGrassLods()
        {
            var lods = GameObject.FindObjectsOfType<LODGroup>(true);
            
            foreach (var lod in lods)
            {
                if (lod.gameObject.name.Contains("prop_foliage_fern"))
                {
                    lod.animateCrossFading = true;
                    lod.fadeMode = LODFadeMode.CrossFade;
                    lod.gameObject.isStatic = true;
                    lod.size = 3;

                    foreach (var lodChild in lod.GetComponentsInChildren<Transform>(true))
                    {
                        lodChild.gameObject.SetActive(true);
                        lodChild.gameObject.isStatic = true;
                    }
                }
            }
        }
        
        [MenuItem("Tools/Set all lights to mix render mode")]
        private static void SetAllLightsToMixRenderMode()
        {
            var allLights = GameObject.FindObjectsOfType<Light>(true);
            foreach (Light light in allLights)
            {
                light.lightmapBakeType = LightmapBakeType.Mixed;
            }

            Debug.Log($"Setted mix render mode for {allLights.Length} lights");
        }

        [MenuItem("Tools/Replace all standard shader to standard optimized shader")]
        private static void ReplaceAllStandardShader()
        {
            var standardShaderOptimized = Shader.Find("Standard Optimized");
            var standardShaderSpecularOptimized = Shader.Find("Standard (Specular setup) Optimized");
            
            foreach (var material in FindAllComponentsInProject<Material>())
            {
                switch (material.shader.name)
                {
                    case "Standard (Specular setup)":
                        material.shader = standardShaderSpecularOptimized;
                        EditorUtility.SetDirty(material);
                        break;
                    case "Standard":
                        material.shader = standardShaderOptimized;
                        EditorUtility.SetDirty(material);
                        break;
                    default:
                        break;
                }
            }
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Tools/Update GPU Instancing on all materials")]
        private static void UpdateGPUInstancing()
        {
            var allMaterials = FindAllComponentsInProject<Material>();
            var allMeshRenderers = 
                FindAllComponentsInProject<GameObject>().SelectMany(gameObject =>
                {
                    var meshRender = gameObject.GetComponent<SkinnedMeshRenderer>();
                    return gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).Append(meshRender);
                }).Where(meshRenderer => meshRenderer!=null).Distinct().ToArray();

            foreach (var material in allMaterials)
            {
                try
                {
                    material.enableInstancing = !allMeshRenderers.Any(meshRenderer => meshRenderer.sharedMaterials.Contains(material));
                }
                catch (Exception e)
                {
                }
            }
        }
        
        [MenuItem("Tools/Change Global Illumination from Realtime to Baked on all materials")]
        private static void ChangeGlobalIlluminationFromRealtimeToBaked()
        {
            foreach (var material in FindAllComponentsInProject<Material>())
            {
                if (material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.RealtimeEmissive))
                {
                    material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.BakedEmissive;
                }
                else
                {
                    material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.BakedEmissive;
                }
            }
        }
        
        [MenuItem("Tools/Find all using shaders in materials (Advanced)")]
        private static void FindAllUsingShaders()
        {
            var allShaders = FindAllComponentsInProject<Shader>().Select(shader => shader.name).Append("Legacy Shaders/Transparent/Bumped Diffuse").ToArray();
            var allMaterials = FindAllComponentsInProject<Material>();

            foreach (var shaderName in allShaders)
            {
                var usedMaterials = allMaterials.Where(material => material.shader.name == shaderName ).Select(material => material.name).ToList();

                if (usedMaterials.Count > 0)
                {
                    Debug.Log($"Shader \"{shaderName}\" are using in {string.Join(",",usedMaterials)} materials");
                }
            }
        }
        
        [MenuItem("Tools/Find MeshRenderers Without Mesh (Advanced)")]
        private static void FindMeshRenderersAdvanced()
        {
            List<MissingMeshInfo> results = new List<MissingMeshInfo>();

            // Находим все префабы в проекте
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            Debug.Log($"Найдено префабов: {guids.Length}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    FindMeshRenderersInPrefabAdvanced(prefab, path, results);
                }
            }

            // Сортируем результаты
            results = results.OrderBy(r => r.prefabPath).ThenBy(r => r.objectPath).ToList();

            // Выводим результаты
            if (results.Count > 0)
            {
                Debug.Log($"<b>Найдено MeshRenderer без меша: {results.Count}</b>");

                for (int i = 0; i < results.Count; i++)
                {
                    string line = $"[{results[i].prefabPath}] {results[i].objectPath}";
                    Debug.Log($"<color=red>MISSING MESH:</color> {line}");
                }
            }
            else
            {
                Debug.Log("<color=green>MeshRenderer без меша не найдены! Всё в порядке.</color>");
            }
        }

        private static void FindMeshRenderersInPrefabAdvanced(GameObject prefab, string prefabPath,
            List<MissingMeshInfo> results)
        {
            // Получаем все MeshRenderer в префабе (включая отключенные)
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in renderers)
            {
                bool hasMesh = false;

                // Проверяем MeshFilter
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    hasMesh = true;
                }

                // Проверяем SkinnedMeshRenderer (если есть)
                SkinnedMeshRenderer skinnedRenderer = renderer.GetComponent<SkinnedMeshRenderer>();
                if (skinnedRenderer != null && skinnedRenderer.sharedMesh != null)
                {
                    hasMesh = true;
                }
                
                // Если меша нет - добавляем в результаты
                if (!hasMesh)
                {
                    string fullPath = GetFullPath(renderer.transform);
                    results.Add(new MissingMeshInfo
                    {
                        prefabPath = prefabPath,
                        objectPath = fullPath,
                        componentType = skinnedRenderer != null ? "SkinnedMeshRenderer" : "MeshRenderer"
                    });
                }
            }
        }

        private static string GetFullPath(Transform transform)
        {
            List<string> pathParts = new List<string>();
            Transform current = transform;

            while (current != null)
            {
                pathParts.Add(current.name);
                current = current.parent;
            }

            // Разворачиваем список и соединяем
            pathParts.Reverse();
            return string.Join("/", pathParts);
        }

        private class MissingMeshInfo
        {
            public string prefabPath;
            public string objectPath;
            public string componentType;
        }
        
    }
}