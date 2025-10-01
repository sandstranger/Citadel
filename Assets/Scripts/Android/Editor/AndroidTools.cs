using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Citadel.Editor.Utils;
using Object = UnityEngine.Object;

namespace Citadel.Android.Tools
{
    internal static class AndroidTools
    {
        [MenuItem("Tools/Find all canvases")]
        private static void FindAllCanvases()
        {
            Debug.Log($"Found canvases {String.Join(",",Object.FindObjectsOfType<Canvas>(true).Select(canvas => canvas.gameObject.name).ToArray())}");
        }     
        
        [MenuItem("Tools/Set all lights to not important mode")]
        private static void SetAllLightsToAutoMode()
        {
            foreach (var light in GameObject.FindObjectsOfType<Light>(true))
            { 
                light.renderMode = LightRenderMode.ForceVertex;
            }
        }

        [MenuItem("Tools/Set all shadowcasters to static mode")]
        private static void SetAllShadowCastersToStaticMode()
        {
            foreach (var prefab in FindAllComponentsInProject<GameObject>("Prefab"))
            {
                InstantiatePrefab(prefab, prefabInstance =>
                {
                    var renderers = prefabInstance.GetComponentsInChildren<Renderer>(true).Where(render => render!=null).ToArray();
                    var shadowCasters = renderers.Where(render => render.sharedMaterials!=null && render.sharedMaterials.Any(material => material!=null && material.name == "black_shadowhelper")).ToArray();
                    var setShadowCastersToStaticMode = shadowCasters.Length > 0 && renderers.Any(render => render.gameObject.isStatic);

                    if (setShadowCastersToStaticMode)
                    {
                        foreach (var shadowCaster in shadowCasters)
                        {
                            shadowCaster.gameObject.isStatic = true;
                        }
                    }

                    return setShadowCastersToStaticMode;
                });
            }
        }
        
        [MenuItem("Tools/Remove double materials from Render components")]
        private static void RemoveDoubleMaterialsFromRenderComponents()
        {
            var prefabs = FindAllComponentsInProject<GameObject>("Prefab");

            foreach (var prefab in prefabs)
            {
                InstantiatePrefab(prefab, prefabInstance =>
                {
                    bool saveInstantiatedPrefab = false;
                    foreach (var renderer in prefabInstance.GetComponentsInChildren<Renderer>(true))
                    {
                        bool updateSharedMaterialsOnRender = renderer.sharedMaterials.Any(sharedMaterial => sharedMaterial == null);
                        var materials = renderer.sharedMaterials.Where(sharedMaterial => sharedMaterial != null).ToArray();
                        var materialsToSet = new List<Material>(materials);

                        foreach (var material in materials)
                        {
                            var materialsCount = materials.Count(sharedMaterial => sharedMaterial.name == material.name);

                            if (materialsCount > 1)
                            {
                                materialsToSet.RemoveAll(sharedMaterial => sharedMaterial.name == material.name);
                                materialsToSet.Add(material);
                                updateSharedMaterialsOnRender = true;
                            }

                            if (updateSharedMaterialsOnRender)
                            {
                                saveInstantiatedPrefab = true;
                                renderer.sharedMaterials = materialsToSet.ToArray();
                            }
                        }
                    }

                    return saveInstantiatedPrefab;
                });
            }
        }

        [MenuItem("Tools/Set custom material to all ui elements")]
        private static void SetCustomMaterialToAllUIElements()
        {
            const string materialNameToIgnore = "ui_automapmasker";

            var materialToReplace = FindAllComponentsInProject<Material>()
                .First(material => material.name == "DefaultUIMaterial");

            var playerPrefab = FindAllComponentsInProject<GameObject>("Prefab")
                .First(prefab => prefab.name == "Player");

            InstantiatePrefab(playerPrefab, prefabInstance =>
            {
                var uiElements = prefabInstance.GetComponentsInChildren<RawImage>(true).Cast<ICanvasElement>()
                    .Union(prefabInstance.GetComponentsInChildren<Image>(true))
                    .Union(prefabInstance.GetComponentsInChildren<Text>(true)).Cast<Graphic>().ToArray();

                foreach (var uiElement in uiElements)
                {
                    if (uiElement.material != null && uiElement.material.name == materialNameToIgnore)
                    {
                        continue;
                    }

                    uiElement.material = materialToReplace;
                }

                return true;
            });
        }
        
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