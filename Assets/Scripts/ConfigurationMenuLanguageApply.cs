using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigurationMenuLanguageApply : MonoBehaviour {
	private Dropdown picker;

	[Inject] private Const _consts;
	[Inject] private Config _config;

	void Start() { // Wait for _consts. to initialize.
		Initialize();
	}

	void OnEnable() {
		Initialize();
	}

	void Initialize() {
		if (picker == null) picker = GetComponent<Dropdown>();
		if (picker == null) Debug.Log("BUG: ConfigurationMenuLanguageApply missing component for picker.");

		picker.value = _consts.AudioLanguage;
	}

	public void OnDropdownSelect () {
		Debug.Log("Language select");
		if (picker != null)
			_consts.AudioLanguage = picker.value;
		else
			_consts.AudioLanguage = 0; // Default to English

		_config.WriteConfig();
		_config.SetLanguage();
	}
}
