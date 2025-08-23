using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigSlider : MonoBehaviour {
	public int index;
	private Slider slideControl;

	[Inject] private Const _consts;
	[Inject] private Config _config;
	
	void Start () { // Wait for _consts. to initialize.
		AlignSettingToConfigFile();
	}

	void OnEnable() {
		AlignSettingToConfigFile();
	}

	void AlignSettingToConfigFile() {
		if (index < 0 || index >= 7) { index = 0; Debug.Log("BUG: Setting index on ConfigSlider to 0 because it was outside the range >=0index<7");}

		if (slideControl == null) slideControl = GetComponent<Slider>();
		if (slideControl == null) Debug.Log("BUG: No slider component for object with ConfigSlider script");

		switch(index) {
			case 0: slideControl.value = _consts.GraphicsFOV; break;
			case 1: slideControl.value = _consts.GraphicsGamma; break;
			case 2: slideControl.value = _consts.AudioVolumeMaster; break;
			case 3: slideControl.value = _consts.AudioVolumeMusic; break;
			case 4: slideControl.value = _consts.AudioVolumeMessage; break;
			case 5: slideControl.value = _consts.AudioVolumeEffects; break;
			case 6: slideControl.value = (_consts.MouseSensitivity/2.01f*100f); break;
		}
	}

	public void SetValue() {
		switch(index) {
			case 0: _consts.GraphicsFOV = (int)slideControl.value; _config.SetFOV(); break;
			case 1: _consts.GraphicsGamma = (int)slideControl.value; _config.SetBrightness(); break;
			case 2: _consts.AudioVolumeMaster = (int)slideControl.value; _config.SetVolume(); break;
			case 3: _consts.AudioVolumeMusic = (int)slideControl.value; _config.SetVolume(); break;
			case 4: _consts.AudioVolumeMessage = (int)slideControl.value; _config.SetVolume(); break;
			case 5: _consts.AudioVolumeEffects = (int)slideControl.value; _config.SetVolume(); break;
			case 6: _consts.MouseSensitivity = ((slideControl.value/100f) * 2f) + 0.01f; break;
		}
		_config.WriteConfig();
	}
}
