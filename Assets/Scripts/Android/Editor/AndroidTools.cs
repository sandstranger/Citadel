using UnityEditor;
using UnityEngine;

namespace Android.Tools
{
    internal static class AndroidTools
    {
        [MenuItem("Tools/Set all lights to mix render mode")]
        internal static void SetAllLightsToMixRenderMode()
        {
            var allLights = GameObject.FindObjectsOfType<Light>(true);
            foreach (Light light in allLights)
            {
                light.lightmapBakeType = LightmapBakeType.Mixed;
            }
            Debug.Log($"Setted mix render mode for {allLights.Length} lights");
        }
    }
}