using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigurationMenuSSRApply : MonoBehaviour {
	private Dropdown picker;

	[Inject] private Const _consts;
	[Inject] private Config _config;
	
	void Start() { // Wait for _consts. to initialize.
		Initialize();
	}

	void OnEnable() {
		Initialize();
	}
	
	public void SetOptionsText() {
		if (_consts == null) return;
		if (!_consts.stringTableLoaded) return;
		if (picker == null) return;

		List<string> ssrList = new List<string>();
		for (int i=0;i<3;i++) {
			switch(i) {
				case 0: ssrList.Add(_consts.stringTable[788]); break;
				case 1: ssrList.Add(_consts.stringTable[789]); break;
				case 2: ssrList.Add(_consts.stringTable[790]); break;
			}
		}
		picker.ClearOptions();
		picker.AddOptions(ssrList);
	}

	void Initialize() {
		if (picker == null) picker = GetComponent<Dropdown>();
		if (picker == null) {
			Debug.Log("BUG: ConfigurationMenuSSRApply missing component for "
					  + "aaPicker.");
		}

		SetOptionsText();
		if (picker.value != _consts.GraphicsSSRMode) {
			picker.value = _consts.GraphicsSSRMode;
		}
	}

	public void OnDropdownSelect () {
		if (picker != null) _consts.GraphicsSSRMode = picker.value;
		else _consts.GraphicsSSRMode = 0; // Default to off

		_config.WriteConfig();
		_config.SetSSR();
	}
}
