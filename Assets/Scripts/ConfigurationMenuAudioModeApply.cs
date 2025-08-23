using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigurationMenuAudioModeApply : MonoBehaviour {
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
		for (int i=0;i<7;i++) {
			switch(i) {
				case 0: ssrList.Add(_consts.stringTable[795]); break;
				case 1: ssrList.Add(_consts.stringTable[796]); break;
				case 2: ssrList.Add(_consts.stringTable[797]); break;
				case 3: ssrList.Add(_consts.stringTable[798]); break;
				case 4: ssrList.Add(_consts.stringTable[799]); break;
				case 5: ssrList.Add(_consts.stringTable[800]); break;
				case 6: ssrList.Add(_consts.stringTable[801]); break;
			}
		}
		picker.ClearOptions();
		picker.AddOptions(ssrList);
	}

	void Initialize() {
		if (picker == null) picker = GetComponent<Dropdown>();
		if (picker == null) {
			Debug.Log("BUG: ConfigurationMenuAudioModeApply missing component for "
					  + "aaPicker.");
		}

		SetOptionsText();
		if (picker.value != _consts.AudioSpeakerMode) {
			picker.value = _consts.AudioSpeakerMode;
		}
	}

	public void OnDropdownSelect () {
		if (picker != null) _consts.AudioSpeakerMode = picker.value;
		else _consts.AudioSpeakerMode = 1; // Default to Stereo

		_config.WriteConfig();
		_config.SetAudioMode();
	}
}
