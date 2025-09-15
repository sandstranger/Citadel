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
        [Header("Source lights")] public bool autoFindLights = true;
        [FormerlySerializedAs("manualLights")] [SerializeField]
        private Light[] _manualLights;
        [FormerlySerializedAs("maxDistance")]
        [Header("Culling")] 
        [SerializeField]
        private float _maxDistance = 10f;
        [FormerlySerializedAs("checkInterval")] [SerializeField]
        private float _checkInterval = 0.2f;
        [FormerlySerializedAs("useFade")] [SerializeField]
        private bool _useFade = true;
        [FormerlySerializedAs("fadeSpeed")] [SerializeField]
        private float _fadeSpeed = 10f;

        [FormerlySerializedAs("ignoreLayers")]
        [Header("Filters")]
        [SerializeField]
        private LayerMask _ignoreLayers = 0; // если нужно игнорировать некоторые слои (поставьте слой в биты)

        private Transform _camTransform;
        private float _sqrMaxDistance;
        private float _timer = 0f;

        private readonly HashSet<Light> _trackedLights = new();
        private readonly Dictionary<Light, float> _origIntensity = new();
        private readonly Dictionary<Light, float> _targetIntensity = new();
        private readonly HashSet<Light> _toggleOnlyLights = new();
        [Inject]
        private readonly Camera _cam;

        private void Start()
        {
            _camTransform = (_cam != null) ? _cam.transform : transform;
            _sqrMaxDistance = _maxDistance * _maxDistance;
            RefreshLightList();
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void RefreshLightList()
        {
            Clear();

            Light[] found = autoFindLights ? FindObjectsOfType<Light>(true) : _manualLights;
          
            if (found == null)
            {
                return;
            }

            UnityEngine.Debug.Log($"Found {found.Length} lights (autoFind={autoFindLights}).");

            foreach (var l in found)
            {
                if (l == null || !l.enabled || l.type == LightType.Directional || ((1 << l.gameObject.layer) & _ignoreLayers) != 0)
                {
                    continue;
                }

                // добавляем в набор (HashSet предотвращает дубликаты)
                _trackedLights.Add(l);

                // получим / создадим сторедж и убедимся, что он содержит первоначальные значения
                var storage = l.GetComponent<LightIntensityStorage>();
                if (storage == null)
                {
                    storage = l.gameObject.AddComponent<LightIntensityStorage>();
                    storage.Intensity = l.intensity;
                    storage.WasEnabled = l.enabled;
                    storage.Saved = true;
                }
                else if (!storage.Saved)
                {
                    // если компонент есть, но он не инициализирован нашим скриптом — инициализируем и пометим
                    storage.Intensity = l.intensity;
                    storage.WasEnabled = l.enabled;
                    storage.Saved = true;
                }

                _origIntensity[l] = storage.Intensity;
                _targetIntensity[l] = storage.Intensity;

                var monos = l.gameObject.GetComponents<MonoBehaviour>();
                bool hasLightAnimation = monos.Any(m => m != null && m.GetType().Name == "LightAnimation");
                if (hasLightAnimation)
                {
                    _toggleOnlyLights.Add(l);
                }
            }

            UnityEngine.Debug.Log($"Tracked lights: {_trackedLights.Count}, toggle-only: {_toggleOnlyLights.Count}");
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= _checkInterval)
            {
                _timer = 0f;
                DoCullCheck();

                // Если без fade, применяем изменения сразу (мгновенно)
                if (!_useFade)
                {
                    ApplyImmediate();
                }
            }

            if (_useFade)
            {
                float delta = _fadeSpeed * Time.deltaTime;

                foreach (var l in _trackedLights.ToArray()) // ToArray чтобы безопасно обходить HashSet
                {
                    if (l == null) continue;

                    // Если это toggle-only свет — не трогаем intensity, просто включаем/выключаем
                    if (_toggleOnlyLights.Contains(l))
                    {
                        bool shouldBeOn = _targetIntensity.ContainsKey(l)
                            ? _targetIntensity[l] > 0.001f
                            : _origIntensity[l] > 0.001f;
                        l.enabled = shouldBeOn;
                        continue;
                    }

                    float cur = l.intensity;
                    float targ = _targetIntensity.ContainsKey(l) ? _targetIntensity[l] : _origIntensity[l];

                    if (Mathf.Approximately(cur, targ)) continue;

                    float next = Mathf.MoveTowards(cur, targ, delta);
                    l.intensity = next;

                    // Включаем/выключаем компонент Light для экономии в ряде случаев
                    l.enabled = next > 0.001f;
                }
            }
        }

        private void DoCullCheck()
        {
            if (_camTransform == null) return;
            Vector3 camPos = _camTransform.position;
            float sqrMax = _sqrMaxDistance;

            foreach (var l in _trackedLights.ToArray())
            {
                if (l == null) continue;

                float sqrDist = (l.transform.position - camPos).sqrMagnitude;

                if (sqrDist <= sqrMax)
                {
                    _targetIntensity[l] = _origIntensity.ContainsKey(l) ? _origIntensity[l] : l.intensity;
                }
                else
                {
                    _targetIntensity[l] = 0f;
                }
            }
        }

        private void ApplyImmediate()
        {
            foreach (var l in _trackedLights.ToArray())
            {
                if (l == null) continue;
                float targ = _targetIntensity.ContainsKey(l) ? _targetIntensity[l] : _origIntensity[l];

                if (_toggleOnlyLights.Contains(l))
                {
                    // простой on/off
                    l.enabled = targ > 0.001f;
                }
                else
                {
                    // мгновенно назначаем intensity и включаем/выключаем
                    l.intensity = targ;
                    l.enabled = targ > 0.001f;
                }
            }
        }

        public void SetMaxDistance(float d)
        {
            _maxDistance = Mathf.Max(0.01f, d);
            _sqrMaxDistance = _maxDistance * _maxDistance;
        }

        public void Clear()
        {
            foreach (var trackedLight in _trackedLights.ToArray())
            {
                if (trackedLight == null) continue;
                try
                {
                    LightIntensityStorage storage = trackedLight.GetComponent<LightIntensityStorage>();

                    if (storage != null && storage.Saved)
                    {
                        trackedLight.intensity = storage.Intensity;
                        trackedLight.enabled = storage.WasEnabled;
                    }
                }
                catch (Exception)
                {
                    // безопасно молчим, но желательно логировать при дебаге
                }
            }

            _trackedLights.Clear();
            _origIntensity.Clear();
            _targetIntensity.Clear();
            _toggleOnlyLights.Clear();
        }

        public void Rebuild()
        {
            RefreshLightList();
        }
    }
}
