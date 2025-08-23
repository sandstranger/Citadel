using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigToggles : MonoBehaviour {
	// External references
	public ConfigToggleType ToggleType;

	[Inject] private Const _consts;
	[Inject] private Config _config;
	[Inject] private Music _music;

	// Internal references
	private Toggle self;

	void Start () { // Wait for _consts. to initialize.
		AlignWithConfigFile();
	}

	void OnEnable() {
		AlignWithConfigFile();
	}

	public void AlignWithConfigFile() {
		if (self == null) self = GetComponent<Toggle>();
		switch (ToggleType) {
			case ConfigToggleType.Fullscreen: self.isOn = _consts.GraphicsFullscreen; break;
			case ConfigToggleType.SSAO: self.isOn = _consts.GraphicsSSAO; break;
			case ConfigToggleType.Bloom: self.isOn = _consts.GraphicsBloom; break;
			case ConfigToggleType.SEGI: self.isOn = _consts.GraphicsSEGI; break;
			case ConfigToggleType.Reverb: self.isOn = _consts.AudioReverb; break;
			case ConfigToggleType.InvertLook: self.isOn = _consts.InputInvertLook; break;
			case ConfigToggleType.InvertCyber: self.isOn = _consts.InputInvertCyberspaceLook; break;
			case ConfigToggleType.InvertInventoryCycling: self.isOn = _consts.InputInvertInventoryCycling; break;
			case ConfigToggleType.QuickPickup: self.isOn = _consts.InputQuickItemPickup; break;
			case ConfigToggleType.QuickReload: self.isOn = _consts.InputQuickReloadWeapons; break;
			case ConfigToggleType.Vsync: self.isOn = _consts.GraphicsVSync; break;
			case ConfigToggleType.NoShootMode: self.isOn = _consts.NoShootMode; break;
			case ConfigToggleType.DynamicMusic: self.isOn = _consts.DynamicMusic; break;
			case ConfigToggleType.HeadBob: self.isOn = _consts.HeadBob; break;
			case ConfigToggleType.Footsteps: self.isOn = _consts.Footsteps; break;
		}
	}

	public void ToggleFullscreen () { _consts.GraphicsFullscreen = self.isOn; _config.WriteConfig(); }
	public void ToggleSSAO () { _consts.GraphicsSSAO = self.isOn; _config.WriteConfig(); }
	public void ToggleBloom () { _consts.GraphicsBloom = self.isOn; _config.WriteConfig(); }
	public void ToggleSEGI () { _consts.GraphicsSEGI = self.isOn; _config.WriteConfig(); }
	public void ToggleReverb () {
		_consts.AudioReverb = self.isOn;
		if (_consts.AudioReverb) _consts.ReverbOn();
		else _consts.ReverbOff();

		_config.WriteConfig();
	}
	public void ToggleInvertLook () { _consts.InputInvertLook = self.isOn; _config.WriteConfig(); }
	public void ToggleInvertCyberLook () { _consts.InputInvertCyberspaceLook = self.isOn; _config.WriteConfig(); }
	public void ToggleInvertInventoryCycling () { _consts.InputInvertInventoryCycling = self.isOn; _config.WriteConfig(); }
	public void ToggleQuickItemPickup () { _consts.InputQuickItemPickup = self.isOn; _config.WriteConfig(); }
	public void ToggleQuickReloadWeapon () { _consts.InputQuickReloadWeapons = self.isOn; _config.WriteConfig(); }
	public void ToggleVSync () { _consts.GraphicsVSync = self.isOn; _config.WriteConfig(); }
	public void ToggleNoShootMode () { _consts.NoShootMode = self.isOn; _config.WriteConfig(); }
	public void ToggleDynamicMusic () { _consts.DynamicMusic = self.isOn; _config.WriteConfig(); _music.Stop(); }
	public void ToggleHeadBob () { _consts.HeadBob = self.isOn; _config.WriteConfig(); }
	public void ToggleFootsteps () { _consts.Footsteps = self.isOn; _config.WriteConfig(); }
}
