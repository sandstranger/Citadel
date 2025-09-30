using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Citadel.Game
{
    internal sealed class AndroidConfig
    {
        private static readonly string PathToPreferVulkanApiFile = Path.Combine(Application.persistentDataPath, "PreferVulkanApi.txt");
        
        public static AndroidConfig Default { get; } = new();
		
        private readonly PlayerPrefsBoolValue _enableLightsCulling = new("enable_lights_culling", true);
        private readonly PlayerPrefsFloatValue _lightsCullingMaxDistance = new("lights_culling_max_distance", LightDistanceCuller.DefaultMaxDistance);
        private readonly PlayerPrefsBoolValue _enableVulkanApi = new("enable_vulkan_api");

        public event Action<bool> OnLightsCullingValueChanged
        {
            add => _enableLightsCulling.OnPlayerPrefsValueChanged += value;
            remove => _enableLightsCulling.OnPlayerPrefsValueChanged -= value;
        }
        
        public bool EnableLightsCulling
        {
            get => _enableLightsCulling.Value; 
            set => _enableLightsCulling.Value = value;
        }

        public float LightsCullingMaxDistance
        {
            get => _lightsCullingMaxDistance.Value;
            set => _lightsCullingMaxDistance.Value = value;
        }

        public bool EnableVulkanApi
        {
            get => _enableVulkanApi.Value;
            set
            {
                _enableVulkanApi.Value = value;
                File.WriteAllText(PathToPreferVulkanApiFile,value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            }
        }
    }
}