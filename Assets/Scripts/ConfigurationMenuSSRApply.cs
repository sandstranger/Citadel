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
	
	void Initialize() {
		if (picker == null) picker = GetComponent<Dropdown>();
		if (picker == null) {
			Debug.Log("BUG: ConfigurationMenuSSRApply missing component for "
					  + "aaPicker.");
		}

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
