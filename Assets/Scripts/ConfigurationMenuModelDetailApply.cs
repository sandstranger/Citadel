using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigurationMenuModelDetailApply : MonoBehaviour {
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
				case 0: shadList.Add(_consts.stringTable[914]); break; // No Detail
				case 1: shadList.Add(_consts.stringTable[915]); break; // High Detail
			}
		}
		picker.ClearOptions();
		picker.AddOptions(shadList);
	}

	void Initialize() {
		if (picker == null) picker = GetComponent<Dropdown>();
		if (picker == null) Debug.Log("BUG: ConfigurationMenuModelDetailApply missing component for picker.");

		SetOptionsText();
		if (picker.value != _consts.GraphicsModelDetail) {
			picker.value = _consts.GraphicsModelDetail;
		}
	}

	public void OnDropdownSelect () {
		if (picker != null)
			_consts.GraphicsModelDetail = picker.value;
		else
			_consts.GraphicsModelDetail = 0; // Default to off, huge performance impact

		_config.WriteConfig();
		_config.SetModelDetail();
	}
}
