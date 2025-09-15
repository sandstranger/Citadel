using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ConfigurationMenuVideoApply : MonoBehaviour {
	public Dropdown resolutionPicker;
	[Inject] 
	private readonly Config _config;
	[Inject]
	private readonly Const _consts;
	
	public void OnApplyClick () {
#if !UNITY_ANDROID		
		int x = Screen.resolutions[resolutionPicker.value].width;
		int y = Screen.resolutions[resolutionPicker.value].height;
		_consts.sprint(_consts.stringTable[1016] + x.ToString() + ", "
				     + y.ToString() + ", " + _consts.stringTable[1017] + ": "
				     + _consts.GraphicsFullscreen.ToString());
		Screen.SetResolution(x,y,true);
		Screen.fullScreen = _consts.GraphicsFullscreen;
		_consts.GraphicsResWidth = Screen.resolutions[resolutionPicker.value].width;
		_consts.GraphicsResHeight = Screen.resolutions[resolutionPicker.value].height;
		_config.WriteConfig();
#endif
	}
}
