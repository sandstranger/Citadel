using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigurationMenuShadowsApply : MonoBehaviour {
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

		List<string> shadList = new List<string>();
		for (int i=0;i<3;i++) {
			switch(i) {
				case 0: shadList.Add(_consts.stringTable[785]); break;
				case 1: shadList.Add(_consts.stringTable[786]); break;
				case 2: shadList.Add(_consts.stringTable[787]); break;
			}
		}
		picker.ClearOptions();
		picker.AddOptions(shadList);
	}

	void Initialize() {
		if (picker == null) picker = GetComponent<Dropdown>();
		if (picker == null) Debug.Log("BUG: ConfigurationMenuShadowsApply missing component for picker.");

		SetOptionsText();
		if (picker.value != _consts.GraphicsShadowMode) {
			picker.value = _consts.GraphicsShadowMode;
		}
	}

	public void OnDropdownSelect () {
		if (picker != null)
			_consts.GraphicsShadowMode = picker.value;
		else
			_consts.GraphicsShadowMode = 0; // Default to off, huge performance impact

		_config.WriteConfig();
		_config.SetShadows();
	}
}
