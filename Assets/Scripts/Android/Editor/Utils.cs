using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Citadel.Editor
{
    public static class Utils
    {
        public static IReadOnlyList<T> FindAllComponentsInProject<T>() where T : Object
        {
            return FindAllComponentsInProject<T>($"t:{typeof(T).Name}");
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
