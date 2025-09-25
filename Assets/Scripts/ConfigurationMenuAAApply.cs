using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigurationMenuAAApply : MonoBehaviour {
	private Dropdown aaPicker;

	[Inject] private Const _consts;
	[Inject] private Config _config;
	
	void Start() { // Wait for _consts. to initialize.
		Initialize();
	}

	void OnEnable() {
		Initialize();
	}
	
	void Initialize() {
		if (aaPicker == null) aaPicker = GetComponent<Dropdown>();
		if (aaPicker == null) {
			Debug.Log("BUG: ConfigurationMenuAAApply missing component for aaPicker.");
			return;
		}
		
		if (aaPicker.value != _consts.GraphicsAAMode) {
			aaPicker.value = _consts.GraphicsAAMode;
		}
	}

	public void OnDropdownSelect () {
		if (aaPicker != null)
			_consts.GraphicsAAMode = aaPicker.value;
		else
			_consts.GraphicsAAMode = 0; // Default to FXAA Extreme Performance

		_config.WriteConfig();
		_config.SetAA();
	}
}
