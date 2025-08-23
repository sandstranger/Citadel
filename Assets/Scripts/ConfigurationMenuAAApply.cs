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
	
	public void SetOptionsText() {
		if (!_consts.stringTableLoaded) return;
		if (aaPicker == null) return;

		List<string> aaList = new List<string>();
		for (int i=0;i<6;i++) {
			switch(i) {
				case 0: aaList.Add(_consts.stringTable[779]); break;
				case 1: aaList.Add(_consts.stringTable[780]); break;
				case 2: aaList.Add(_consts.stringTable[781]); break;
				case 3: aaList.Add(_consts.stringTable[782]); break;
				case 4: aaList.Add(_consts.stringTable[783]); break;
				case 5: aaList.Add(_consts.stringTable[784]); break;
			}
		}
		aaPicker.ClearOptions();
		aaPicker.AddOptions(aaList);
	}

	void Initialize() {
		if (aaPicker == null) aaPicker = GetComponent<Dropdown>();
		if (aaPicker == null) {
			Debug.Log("BUG: ConfigurationMenuAAApply missing component for aaPicker.");
			return;
		}
		
		SetOptionsText();
		if (aaPicker.value != _consts.GraphicsAAMode) {
			aaPicker.value = _consts.GraphicsAAMode;
		}
	}

	public void OnDropdownSelect () {
		if (aaPicker != null)
			_consts.GraphicsAAMode = aaPicker.value;
		else
			_consts.GraphicsAAMode = 1; // Default to FXAA Extreme Performance

		_config.WriteConfig();
		_config.SetAA();
	}
}
