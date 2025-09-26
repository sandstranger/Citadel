using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Citadel.Game
{
    internal sealed class FogToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [Inject] private readonly Config _config;
        
        private void Awake()
        {
            _toggle.isOn = _config.EnableFog;
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            _config.EnableFog = isOn;
        }
    }
}