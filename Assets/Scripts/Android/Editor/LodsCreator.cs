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
        private const float Lod1TransitionScreen = 0.27f;
        private const float Lod2TransitionScreen = 0.125f;
        private const float MinMeshTrianglesCountToGenerateLods = 399.0f;

        [MenuItem("Tools/Update lods visibility")]
        private static void UpdateLodsVisibility()
        {
            foreach (var projectPrefab in FindAllComponentsInProject<GameObject>("Prefab"))
            {
                var lod = projectPrefab.GetComponent<LODGroup>();

                if (lod != null && Mathf.Approximately(lod.size, OldObjectSize))
                {
                    InstantiatePrefab(projectPrefab, prefabInstance =>
                    {
                        lod = prefabInstance.GetComponent<LODGroup>();
                        lod.size = NewObjectSize;
                        lod.fadeMode = LODFadeMode.CrossFade;
                        lod.animateCrossFading = true;
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

                if (lod != null)
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

                var meshFilters = projectPrefab.GetComponentsInChildren<MeshRenderer>(true, true)
                    .Select(render => render.GetComponent<MeshFilter>()).Where(mesh => mesh!=null && mesh.sharedMesh!=null).ToArray();

                InstantiatePrefab(projectPrefab, prefabInstance =>
                {
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
                        }
                    }

                    var lod = prefabInstance.GetComponent<LODGroup>();

                    if (lod != null)
                    {
                        var lods = lod.GetLODs();
                        lods[0].screenRelativeTransitionHeight = Lod0TransitionScreen;
                        lods[1].screenRelativeTransitionHeight = Lod1TransitionScreen;
                        lods[2].screenRelativeTransitionHeight = Lod2TransitionScreen;
                        lod.SetLODs(lods);
                        lod.RecalculateBounds();
                        lod.size = NewObjectSize;
                        lod.fadeMode = LODFadeMode.CrossFade;
                        lod.animateCrossFading = true;
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
            
            var createdLods = gameObject.GetComponentsInChildren<LODGroup>(true, true);

            if (createdLods.Length > 0)
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