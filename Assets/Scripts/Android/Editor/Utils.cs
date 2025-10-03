using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Citadel.Editor
{
    internal static class Utils
    {
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
            var components = new List<T>(guids.Length);

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
