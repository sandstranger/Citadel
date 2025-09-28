using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Citadel.Game
{
    [DisallowMultipleComponent]
    internal sealed class LightDistanceCuller : MonoBehaviour
    {
        private const int TrackedLightsInitialCapacity = 1500;
        private const float CheckInterval = 0.3f;
        
        [SerializeField] private bool _enableLightsCulling = true;
        [Header("Source lights")] 
        [SerializeField] private bool _autoFindLights = true;
        [SerializeField] private Light[] _manualLights;
        [Header("Culling")] 
        [SerializeField] private float _maxDistance = 20f;
        [SerializeField] private bool _useFade = true;
        [SerializeField] private float _fadeSpeed = 10f;
        [Header("Filters")]
        [SerializeField] private LayerMask _ignoreLayers = 0;

        private Transform _camTransform;
        private float _sqrMaxDistance;
        private float _timer = 0f;

        private readonly List<LightData> _trackedLights = new(TrackedLightsInitialCapacity);
        private bool _needsRebuild = true;

        [Inject]
        private readonly Camera _cam;
        
        private void Start()
        {
            if (!_enableLightsCulling)
            {
                return;
            }
            
            _camTransform = (_cam != null) ? _cam.transform : Camera.main.transform;
            _sqrMaxDistance = _maxDistance * _maxDistance;
        }

        private void OnDestroy()
        {
            RestoreAllLights();
        }

        public void SetMaxDistance(float distance)
        {
            _maxDistance = Mathf.Max(0.01f, distance);
            _sqrMaxDistance = _maxDistance * _maxDistance;
        }

        public void Clear()
        {
            RestoreAllLights();
            _trackedLights.Clear();
        }
        
        public void Rebuild()
        {
            _needsRebuild = true;
        }
        
        private void FixedUpdate()
        {
            if (!_enableLightsCulling)
            {
                return;
            }

            if (_needsRebuild)
            {
                RebuildLightList();
                _needsRebuild = false;
            }

            if (_trackedLights.Count == 0)
            {
                return;
            }
            
            _timer += Time.unscaledDeltaTime;
            if (_timer >= CheckInterval)
            {
                _timer = 0f;
                DoCullCheck();

                if (!_useFade)
                {
                    ApplyImmediate();
                }
            }

            if (_useFade)
            {
                ApplyFade();
            }
        }

        private void RebuildLightList()
        {
            Clear();
            
            Light[] found = _autoFindLights ? FindObjectsByType(typeof (Light), FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Cast<Light>().ToArray() : _manualLights;

            if (found == null)
            {
                return;
            }

            foreach (var light in found)
            {
                if (light == null || !light.enabled || 
                    light.type == LightType.Directional || 
                    ((1 << light.gameObject.layer) & _ignoreLayers) != 0)
                {
                    continue;
                }

                bool hasLightAnimation = light.GetComponent<LightAnimation>() != null;

                var lightData = new LightData(
                    light,
                    light.intensity,
                    light.intensity,
                    hasLightAnimation,
                    light.enabled);

                _trackedLights.Add(lightData);
            }
        }

        private void DoCullCheck()
        {
            if (_camTransform == null)
            {
                return;
            }
            
            Vector3 camPos = _camTransform.position;

            for (int i = 0; i < _trackedLights.Count; ++i)
            {
                var lightData = _trackedLights[i];
                if (lightData.Light == null) continue;

                float sqrDist = (lightData.LightTransform.position - camPos).sqrMagnitude;
                
                var targetIntensity = sqrDist <= _sqrMaxDistance ? 
                    lightData.OriginalIntensity : 0f;
                
                _trackedLights[i] =new LightData(
                    lightData.Light,
                    lightData.OriginalIntensity,
                    targetIntensity,
                    lightData.IsToggleOnly,
                    lightData.WasEnabled);
            }
        }

        private void ApplyFade()
        {
            float delta = _fadeSpeed * Time.deltaTime;

            for (int i = 0; i < _trackedLights.Count; ++i)
            {
                var lightData = _trackedLights[i];
                if (lightData.Light == null) continue;

                if (lightData.IsToggleOnly)
                {
                    lightData.Light.enabled = lightData.TargetIntensity > 0.001f;
                }
                else
                {
                    float current = lightData.Light.intensity;
                    float target = lightData.TargetIntensity;
                    
                    if (!FastApproximately2(current, target))
                    {
                        float next = Mathf.MoveTowards(current, target, delta);
                        lightData.Light.intensity = next;
                        lightData.Light.enabled = next > 0.001f;
                    }
                }
                
                _trackedLights[i] = lightData;
            }
        }

        private void ApplyImmediate()
        {
            for (int i = 0; i < _trackedLights.Count; ++i)
            {
                var lightData = _trackedLights[i];
                if (lightData.Light == null) continue;

                if (lightData.IsToggleOnly)
                {
                    lightData.Light.enabled = lightData.TargetIntensity > 0.001f;
                }
                else
                {
                    lightData.Light.intensity = lightData.TargetIntensity;
                    lightData.Light.enabled = lightData.TargetIntensity > 0.001f;
                }
            }
        }

        private void RestoreAllLights()
        {
            for (int i = 0; i < _trackedLights.Count; ++i)
            {
                var lightData = _trackedLights[i];
                if (lightData.Light == null) continue;

                lightData.Light.intensity = lightData.OriginalIntensity;
                lightData.Light.enabled = lightData.WasEnabled;
            }
        }
        
        private static bool FastApproximately2(float a, float b, float epsilon = 1E-05f)
        {
            float diff = a - b;
            return diff * diff < epsilon * epsilon;
        }
        
        private readonly struct LightData : IEquatable<LightData>
        {
            public readonly Transform LightTransform;
            public readonly Light Light;
            public readonly float OriginalIntensity;
            public readonly float TargetIntensity;
            public readonly bool IsToggleOnly;
            public readonly bool WasEnabled;

            public LightData(Light light, float originalIntensity, float targetIntensity, bool isToggleOnly, bool wasEnabled)
            {
                Light = light;
                OriginalIntensity = originalIntensity;
                TargetIntensity = targetIntensity;
                IsToggleOnly = isToggleOnly;
                WasEnabled = wasEnabled;
                LightTransform = light.transform;
            }

            public bool Equals(LightData other)
            {
                return Equals(Light, other.Light) && 
                       OriginalIntensity.Equals(other.OriginalIntensity) &&
                       TargetIntensity.Equals(other.TargetIntensity) && IsToggleOnly == other.IsToggleOnly && WasEnabled == other.WasEnabled;
            }

            public override bool Equals(object obj)
            {
                return obj is LightData other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Light, OriginalIntensity, TargetIntensity, IsToggleOnly, WasEnabled);
            }
        }
    }
}