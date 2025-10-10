using System;
using System.Collections.Generic;
using FidelityFX.FSR2;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using ZLinq;

namespace Citadel.Game
{
    [RequireComponent(typeof(Dropdown))]
    internal sealed class FsrQualityDropdown : MonoBehaviour
    {
        private const string QualityPrefix = "FSR Quality: ";
        private static readonly List<Dropdown.OptionData> _fsrQualityOptions = BuildFsrQualityOptions();

        [SerializeField]
        private Dropdown _dropDown;
        [Inject] private readonly Config _config;

        private void Awake()
        {
            _dropDown.options = _fsrQualityOptions;
            _dropDown.value = (int)_config.SuperResolutionUpscalerQuality;
            _dropDown.onValueChanged.AddListener(newValue=> _config.SuperResolutionUpscalerQuality = (Fsr2.QualityMode) newValue);
        }

        private static List<Dropdown.OptionData> BuildFsrQualityOptions()
        {
            var dropDownOptions = new List<Dropdown.OptionData>();
            var fsr2QualityModes = Enum.GetValues(typeof(Fsr2.QualityMode)).Cast<Fsr2.QualityMode>();
            foreach (var fsr2QualityMode in fsr2QualityModes)
            {
                dropDownOptions.Add(new Dropdown.OptionData($"{QualityPrefix}{fsr2QualityMode}"));
            }

            return dropDownOptions;
        }
    }
}