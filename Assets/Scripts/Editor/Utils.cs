using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Citadel.Editor
{
    public static class Utils
    {
        public static IReadOnlyList<T> FindAllComponentsInProject<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { "Assets" });
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
