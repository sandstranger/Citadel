using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Citadel.Editor
{
    internal static class Utils
    {
        public static T[] GetComponentsInChildren<T>(this GameObject gameObject, bool includeInactive = false, bool includeSelf = false) where T : Component
        {
            var components = gameObject.GetComponentsInChildren<T>(includeInactive);

            if (includeSelf)
            {
                var selfComponent = gameObject.GetComponent<T>();
                return selfComponent!=null ? components.Prepend(selfComponent).ToArray() : components;
            }

            return components;
        }
        
        public static void RemoveStaticBatchingFlag(GameObject targetObject)
        {
            StaticEditorFlags currentFlags = GameObjectUtility.GetStaticEditorFlags(targetObject);
            StaticEditorFlags newFlags = currentFlags & ~StaticEditorFlags.BatchingStatic;
            GameObjectUtility.SetStaticEditorFlags(targetObject, newFlags);
        }
        
        public static void InstantiatePrefab(GameObject prefab, Func<GameObject,bool> onPrefabInstantiated)
        {
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            bool saveInstantiatedPrefab = onPrefabInstantiated?.Invoke(prefabInstance) ?? false;
            if (saveInstantiatedPrefab)
            {
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, AssetDatabase.GetAssetPath(prefab));
                    EditorUtility.SetDirty(prefab);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Can not save prefab = {prefab.name} due to exception: {e}");
                }
            }
            Object.DestroyImmediate(prefabInstance);
        }
        
        public static IReadOnlyList<T> FindAllComponentsInProject<T>() where T : Object
        {
            return FindAllComponentsInProject<T>(typeof(T).Name);
        }

        public static IReadOnlyList<T> FindAllComponentsInProject<T>(string typeName) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeName}", new[] { "Assets" });
            return FindComponents<T>(guids);
        } 

        public static IReadOnlyList<T> FindAllComponentsInProjectByName<T>(string name) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"\"{name}\"", new[] { "Assets" });
            return FindComponents<T>(guids);
        }

        private static IReadOnlyList<T> FindComponents<T>(IReadOnlyCollection<string> guids)  where T : Object
        {
            var components = new List<T>(guids.Count);
            
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var @object = AssetDatabase.LoadAssetAtPath<T>(path);

                if (@object != null)
                {
                    components.Add(@object);
                }
            }

            return components;
        }
    }
}
