using System.Collections.Generic;
using System.Linq;
using Unity.AutoLOD;
using UnityEditor;
using UnityEngine;
using static Citadel.Editor.Utils;

namespace Citadel.Android.Tools
{
    internal static class LodsCreator
    {
        private const float OldObjectSize = 10.0f;
        private const float NewObjectSize = 25.0f;
        private const float Lod0TransitionScreen = 0.50f;
        private const float Lod1TransitionScreen = 0.21f;
        private const float MinMeshTrianglesCountToGenerateLods = 399.0f;

        [MenuItem("Tools/Update lods visibility")]
        private static void UpdateLodsVisibility()
        {
            foreach (var projectPrefab in FindAllComponentsInProject<GameObject>("Prefab"))
            {
                var lod = projectPrefab.GetComponent<LODGroup>();

                if (lod != null && Mathf.Approximately(lod.size, NewObjectSize))
                {
                    InstantiatePrefab(projectPrefab, prefabInstance =>
                    {
                        lod = prefabInstance.GetComponent<LODGroup>();
                        var lods = lod.GetLODs();
                        lods[0].screenRelativeTransitionHeight = Lod0TransitionScreen;
                        lods[1].screenRelativeTransitionHeight = Lod1TransitionScreen;
                        lod.SetLODs(lods);
                        lod.RecalculateBounds();
                        lod.fadeMode = LODFadeMode.CrossFade;
                        lod.animateCrossFading = true;
                        lod.size = NewObjectSize;
                        return true;
                    });
                }
            }
        }
        
        [MenuItem("Tools/Find low object size lods")]
        private static void FindLowObjectSizeLods()
        {
            foreach (var projectPrefab in FindAllComponentsInProject<GameObject>("Prefab"))
            {
                var lod = projectPrefab.GetComponent<LODGroup>();

                if (lod != null && lod.size < NewObjectSize)
                {
                    Debug.Log(lod.gameObject.name);
                }
            }
        }
        
        [MenuItem("Tools/Create LODs on prefabs")]
        private static void CreateLodsOnPrefabs()
        {
            foreach (var projectPrefab in FindAllComponentsInProject<GameObject>("Prefab"))
            {
                if (!projectPrefab.NeedToCreateLods())
                {
                    continue;
                }

                InstantiatePrefab(projectPrefab, prefabInstance =>
                {
                    AutoLOD.RemoveLODs(prefabInstance);

                    var meshFilters = prefabInstance.GetComponentsInChildren<MeshRenderer>(true, true)
                        .Select(render => render.GetComponent<MeshFilter>()).Where(mesh => mesh!=null && mesh.sharedMesh!=null).ToArray();
                    
                    AutoLOD.GenerateLODs(prefabInstance);

                    var createdLods = prefabInstance.GetComponentsInChildren<MeshRenderer>(true)
                        .Where(render => render.gameObject.name.Contains("LOD")).ToArray();

                    foreach (var createdLod in createdLods)
                    {
                        var createdLodName = createdLod.gameObject.name;
                        var originalMeshFilter = meshFilters.FirstOrDefault(meshFilter =>
                            createdLodName.Contains(meshFilter.sharedMesh.name));

                        if (originalMeshFilter != null)
                        {
                            var originalRender = originalMeshFilter.GetComponent<MeshRenderer>();
                            createdLod.gameObject.SetActive(originalMeshFilter.gameObject.activeSelf);
                            createdLod.gameObject.isStatic = originalMeshFilter.gameObject.isStatic;
                            createdLod.receiveGI = originalRender.receiveGI;
                            bool removeStaticBatchingFlagFromLod  = createdLod.gameObject.isStatic && createdLod.sharedMaterials.Any(material => material!=null && material.name == "black_shadowhelper");

                            if (removeStaticBatchingFlagFromLod)
                            {
                                RemoveStaticBatchingFlag(createdLod.gameObject);
                            }
                        }
                    }

                    var lod = prefabInstance.GetComponent<LODGroup>();

                    if (lod != null)
                    {
                        var lods = lod.GetLODs();
                        lods[0].screenRelativeTransitionHeight = Lod0TransitionScreen;
                        lods[1].screenRelativeTransitionHeight = Lod1TransitionScreen;
                        lod.SetLODs(lods);
                        lod.RecalculateBounds();
                        lod.fadeMode = LODFadeMode.CrossFade;
                        lod.animateCrossFading = true;
                        lod.size = NewObjectSize;
                        return true;
                    }

                    return false;
                });
            }
            
            AssetDatabase.Refresh();
        }

        private static bool NeedToCreateLods(this GameObject gameObject)
        {
            var goName = gameObject.name;
            
            if (goName.Contains("cheat_arsenal") || goName.Contains("LevelGeometry") || goName == "Player" )
            {
                return false;
            }
            
            var lod = gameObject.GetComponent<LODGroup>();

            if (lod == null || !Mathf.Approximately(lod.size, NewObjectSize))
            {
                return false;
            }

            var meshFilters = gameObject.GetComponentsInChildren<MeshRenderer>(true, true).Select(meshRender => 
                meshRender.GetComponent<MeshFilter>()).Where(meshFilter => meshFilter!=null && meshFilter.sharedMesh!=null).ToArray();

            return (meshFilters.Length > 0 && 
                    !meshFilters.Any(mesh => mesh.sharedMesh.name.Contains("med1_1_")) && meshFilters.Any(meshFilter => meshFilter.sharedMesh.GetTrianglesCount() >= MinMeshTrianglesCountToGenerateLods));
        }
        
        private static T[] GetComponentsInChildren<T>(this GameObject gameObject, bool includeInactive = false, bool includeSelf = false) where T : Component
        {
            var components = gameObject.GetComponentsInChildren<T>(includeInactive);

            if (includeSelf)
            {
                var selfComponent = gameObject.GetComponent<T>();
                return selfComponent!=null ? components.Prepend(selfComponent).ToArray() : components;
            }

            return components;
        }
        
        private static float GetTrianglesCount(this Mesh mesh)
        {
            return mesh.triangles.Length / 3.0f;
        }
    }
}