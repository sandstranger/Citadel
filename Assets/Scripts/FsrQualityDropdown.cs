using System;
using System.Collections.Generic;
using System.Linq;
using FidelityFX.FSR2;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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
            var fsr2QualityModes = Enum.GetValues(typeof(Fsr2.QualityMode)).OfType<Fsr2.QualityMode>().ToArray();
            var dropDownOptions = new List<Dropdown.OptionData>(fsr2QualityModes.Length);

            foreach (var fsr2QualityMode in fsr2QualityModes)
            {
                dropDownOptions.Add(new Dropdown.OptionData($"{QualityPrefix}{fsr2QualityMode}"));
            }

            return dropDownOptions;
        }
    }
}