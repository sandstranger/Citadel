using System;
using System.Globalization;
using Zenject;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// Handles configuration parsing for user settings.
public sealed class Config
{
	public static bool EnablePostProcessEffects { get; set; } = false;

	private const string AUDIO_MODE_KEY = "AudioSpeakerMode";
	private static bool _configDataWasSetted = false;

	[Inject]
	private readonly MainMenuHandler _mainMenuHandler;
	[Inject]
	private readonly Const _const;
	[Inject]
	private readonly MFDManager _mfdManager;
	[Inject]
	private readonly MissionTimer _missionTimer;
	[Inject]
	private readonly Music _music;
	[Inject]
	private readonly DynamicCulling _dynamicCulling;
	[Inject]
	private readonly PostProcessProfile _postProcessingProfile;
	[Inject] 
	private readonly Camera _camera;
	[Inject] private PostProcessLayer[] _postProcessLayers;

	private readonly Lazy<ScreenSpaceReflections> _screenSpaceReflections;
	private readonly Lazy<AmbientOcclusion> _ambientOcclusion;
	private readonly Lazy<Bloom> _bloom;
	private readonly Lazy<ColorGrading> _colorGrading;
	
	private int lastAudioMode = -1;

	public Config()
	{
		_colorGrading = new Lazy<ColorGrading>(() => _postProcessingProfile.GetSetting<ColorGrading>());
		_bloom = new Lazy<Bloom>(() => _postProcessingProfile.GetSetting<Bloom>());
		_ambientOcclusion = new Lazy<AmbientOcclusion>(()=> _postProcessingProfile.GetSetting<AmbientOcclusion>());
		_screenSpaceReflections = new Lazy<ScreenSpaceReflections> ( () => _postProcessingProfile.GetSetting<ScreenSpaceReflections>());
	}

	public void LoadConfig()
	{
		// The currently used config is always Config.ini.
		string basePath = Utils.GetAppropriateDataPath();
		Utils.ConfirmExistsMakeIfNot(basePath, "Config.ini");

#if UNITY_ANDROID
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
		_const.GraphicsResWidth = Mathf.RoundToInt(Screen.width/1.8f);
		_const.GraphicsResHeight = Mathf.RoundToInt(Screen.height/1.8f);
#else
		_const.GraphicsResWidth = AssignConfigInt("Graphics","ResolutionWidth");
		_const.GraphicsResHeight = AssignConfigInt("Graphics","ResolutionHeight");
#endif

#if UNITY_ANDROID
		_const.GraphicsFullscreen = true;
#else
		_const.GraphicsFullscreen = AssignConfigBool("Graphics","Fullscreen");
#endif
		_const.GraphicsSSAO = AssignConfigBool("Graphics", "SSAO");
		_const.GraphicsBloom = AssignConfigBool("Graphics", "Bloom");
		_const.GraphicsSEGI = AssignConfigBool("Graphics", "SEGI");
		_const.GraphicsFOV = AssignConfigInt("Graphics", "FOV");
		_const.GraphicsAAMode = AssignConfigInt("Graphics", "AA");
		_const.GraphicsShadowMode = AssignConfigInt("Graphics", "Shadows");
		_const.GraphicsSSRMode = AssignConfigInt("Graphics", "SSR");
		_const.GraphicsGamma = AssignConfigInt("Graphics", "Gamma");
		_const.GraphicsModelDetail = AssignConfigInt("Graphics", "ModelDetail");
		_const.GraphicsVSync = AssignConfigBool("Graphics", "VSync");

		// Audio Configurations
		_const.AudioSpeakerMode = AssignConfigInt("Audio", "SpeakerMode");
		_const.AudioReverb = AssignConfigBool("Audio", "Reverb");
		_const.AudioVolumeMaster = AssignConfigInt("Audio", "VolumeMaster");
		_const.AudioVolumeMusic = AssignConfigInt("Audio", "VolumeMusic");
		_const.AudioVolumeMessage = AssignConfigInt("Audio", "VolumeMessage");
		_const.AudioVolumeEffects = AssignConfigInt("Audio", "VolumeEffects");
		_const.AudioLanguage = AssignConfigInt("Audio", "Language"); // defaults to 0 = english
		_const.DynamicMusic = AssignConfigBool("Audio", "DynamicMusic");
		_const.Footsteps = AssignConfigBool("Audio", "Footsteps");
		_const.HeadBob = AssignConfigBool("Input", "HeadBob");

		_const.MouseSensitivity = ((AssignConfigInt("Input", "MouseSensitivity") / 100f) * 2f) + 0.01f;

		string inputCapture;
		// Input Configurations
		for (int i = 0; i < 40; i++)
		{
			inputCapture = INIWorker.IniReadValue("Input", _const.InputCodes[i]);
			for (int j = 0; j < 159; j++)
			{
				if (_const.InputValues[j] == inputCapture) _const.InputCodeSettings[i] = j;
			}
		}

		_const.InputInvertLook = AssignConfigBool("Input", "InvertLook");
		_const.InputInvertCyberspaceLook = AssignConfigBool("Input", "InvertCyberspaceLook");
		_const.InputInvertInventoryCycling = AssignConfigBool("Input", "InvertInventoryCycling");
		_const.InputQuickItemPickup = AssignConfigBool("Input", "QuickItemPickup");
		_const.InputQuickReloadWeapons = AssignConfigBool("Input", "QuickReloadWeapons");
		_const.NoShootMode = AssignConfigBool("Input", "NoShootMode");

		if (!_configDataWasSetted)
		{
			SetVolume();
			Debug.Log("Setting screen resolution to "
			          + _const.GraphicsResWidth.ToString()
			          + ", " + _const.GraphicsResHeight.ToString()
			          + ", Fullscreen: "
			          + _const.GraphicsFullscreen.ToString());
		
			Screen.SetResolution(_const.GraphicsResWidth, _const.GraphicsResHeight, true);
			Screen.fullScreen = _const.GraphicsFullscreen;
			SetShadows();
			SetAudioMode();
		}
		
		if (_const.GraphicsShadowMode > 2) _const.GraphicsShadowMode = 2;
		if (_const.GraphicsShadowMode < 0) _const.GraphicsShadowMode = 0;
		
		SetModelDetail();
		SetBloom();
		SetSEGI();
		SetSSR();
		SetBrightness();
		SetSSAO();
		SetFOV();
		SetAA();
		SetVSync();
		SetLanguage();
		_configDataWasSetted = true;
	}
	
	public void WriteConfig() {
		INIWorker.IniWriteValue("Graphics","ResolutionWidth",_const.GraphicsResWidth.ToString());
		INIWorker.IniWriteValue("Graphics","ResolutionHeight",_const.GraphicsResHeight.ToString());
		INIWorker.IniWriteValue("Graphics","Fullscreen",Utils.BoolToStringConfig(_const.GraphicsFullscreen));
		INIWorker.IniWriteValue("Graphics","SSAO",Utils.BoolToStringConfig(_const.GraphicsSSAO));
		INIWorker.IniWriteValue("Graphics","Bloom",Utils.BoolToStringConfig(_const.GraphicsBloom));
		INIWorker.IniWriteValue("Graphics","SEGI",Utils.BoolToStringConfig(_const.GraphicsSEGI));
		INIWorker.IniWriteValue("Graphics","FOV",_const.GraphicsFOV.ToString());
		INIWorker.IniWriteValue("Graphics","AA",_const.GraphicsAAMode.ToString());
		INIWorker.IniWriteValue("Graphics","Shadows",_const.GraphicsShadowMode.ToString());
		INIWorker.IniWriteValue("Graphics","SSR",_const.GraphicsSSRMode.ToString());
		INIWorker.IniWriteValue("Graphics","Gamma",_const.GraphicsGamma.ToString());
		INIWorker.IniWriteValue("Graphics","ModelDetail",_const.GraphicsModelDetail.ToString());
		INIWorker.IniWriteValue("Graphics","VSync",Utils.BoolToStringConfig(_const.GraphicsVSync));
		INIWorker.IniWriteValue("Audio","SpeakerMode",_const.AudioSpeakerMode.ToString());
		INIWorker.IniWriteValue("Audio","Reverb",Utils.BoolToStringConfig(_const.AudioReverb));
		INIWorker.IniWriteValue("Audio","VolumeMaster",_const.AudioVolumeMaster.ToString());
		INIWorker.IniWriteValue("Audio","VolumeMusic",_const.AudioVolumeMusic.ToString());
		INIWorker.IniWriteValue("Audio","VolumeMessage",_const.AudioVolumeMessage.ToString());
		INIWorker.IniWriteValue("Audio","VolumeEffects",_const.AudioVolumeEffects.ToString());
		INIWorker.IniWriteValue("Audio","Language",_const.AudioLanguage.ToString());
		INIWorker.IniWriteValue("Audio","DynamicMusic",Utils.BoolToStringConfig(_const.DynamicMusic));
		INIWorker.IniWriteValue("Audio","Footsteps",Utils.BoolToStringConfig(_const.Footsteps));

		int ms = (int)(_const.MouseSensitivity/2f*100f);
		INIWorker.IniWriteValue("Input","MouseSensitivity",ms.ToString());
		for (int i=0;i<40;i++) {
			INIWorker.IniWriteValue("Input",_const.InputCodes[i],_const.InputValues[_const.InputCodeSettings[i]]);
		}
		INIWorker.IniWriteValue("Input","InvertLook",Utils.BoolToStringConfig(_const.InputInvertLook));
		INIWorker.IniWriteValue("Input","InvertCyberspaceLook",Utils.BoolToStringConfig(_const.InputInvertCyberspaceLook));
		INIWorker.IniWriteValue("Input","InvertInventoryCycling",Utils.BoolToStringConfig(_const.InputInvertInventoryCycling));
		INIWorker.IniWriteValue("Input","QuickItemPickup",Utils.BoolToStringConfig(_const.InputQuickItemPickup));
		INIWorker.IniWriteValue("Input","QuickReloadWeapons",Utils.BoolToStringConfig(_const.InputQuickReloadWeapons));
		INIWorker.IniWriteValue("Input","NoShootMode",Utils.BoolToStringConfig(_const.NoShootMode));
		INIWorker.IniWriteValue("Input","HeadBob",Utils.BoolToStringConfig(_const.HeadBob));

		SetBloom();
		SetSEGI();
		SetSSAO();
		SetFOV();
		SetAA();
		SetVSync();
		if (_mainMenuHandler != null) _mainMenuHandler.RenderConfigView();
		SaveConfigToPlayerPrefs();
		if (_const.difficultyMission < 3) {
			_missionTimer.text.text = System.String.Empty;
			_missionTimer.timerTypeText.text = System.String.Empty;
			_mfdManager.overallMissionTimerT.SetActive(false);
			_mfdManager.overallMissionTimer.SetActive(false);
		} else {
			_mfdManager.overallMissionTimerT.SetActive(true);
			_mfdManager.overallMissionTimer.SetActive(true);
		}
	}

	public void SaveConfigToPlayerPrefs() {
		#if UNITY_EDITOR
			// Don't bother with PlayerPrefs from Editor.
		#else
			// Force to potato PlayerPref settings for faster startup, and to prevent users with potato systems from being able to run initially.
			PlayerPrefs.SetInt("Screenmanager Resolution Width", _const.GraphicsResWidth);
			PlayerPrefs.SetInt("Screenmanager Resolution Height", _const.GraphicsResHeight);
			PlayerPrefs.SetInt("Screenmanager Is Fullscreen", _const.GraphicsFullscreen ? 1 : 0); // 0 = windowed
			PlayerPrefs.Save();
		#endif
	}
	
	public void SetVolume() {
		if (_mainMenuHandler.dataFound) {
			AudioListener.volume = (_const.AudioVolumeMaster/100f);
			_const.mainmenuMusic.volume = (_const.AudioVolumeMusic/100f);
			if (_music != null) {
				if (_music.SFXMain != null) _music.SFXMain.volume = (_const.AudioVolumeMusic/100f);
				if (_music.SFXOverlay != null) _music.SFXOverlay.volume = (_const.AudioVolumeMusic/100f);
			}
		} else {
			AudioListener.volume = 0f;
		}
	}

	public void SetFOV() {
		_camera.fieldOfView = _const.GraphicsFOV;
	}

	public void SetBloom()
	{
		_bloom.Value.active = _const.GraphicsBloom;
	}
	
	public void SetSEGI() {
		SetBrightness();
	}

	public void SetVSync() {
#if !UNITY_ANDROID		
		if (_const.GraphicsVSync) {
			Application.targetFrameRate = _const.TARGET_FPS * 2;
			QualitySettings.vSyncCount = 1;
		} else {
			Application.targetFrameRate = _const.TARGET_FPS;
			QualitySettings.vSyncCount = 0;
		}
		
		Debug.Log("Set VSYNC to " + _const.GraphicsVSync.ToString() + ", target framerate is " + Application.targetFrameRate.ToString());
#endif
	}

	public void UpdateFog(float fogDensity, Color fogColor)
	{
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
		
		foreach (var postProcessLayer in _postProcessLayers)
		{
			postProcessLayer.fog.enabled = EnablePostProcessEffects;
		}
	}
	
	public void SetAA() {
		if (_const.GraphicsAAMode < 0) _const.GraphicsAAMode = 0;
		if (_const.GraphicsAAMode > 4) _const.GraphicsAAMode = 4;
		switch (_const.GraphicsAAMode) {
			case 0: // No Antialiasing, turn off the profile's antialiasing entirely.
				UpdateAntiAntianalising(PostProcessLayer.Antialiasing.None);
				break;
			case 1: // FXAA Extreme Performance, FXAA is a bit different so we call a helper function to set it.
				UpdateAntiAntianalising(PostProcessLayer.Antialiasing.FastApproximateAntialiasing, enableFastFxaa: true);
				break;
			case 2:
				UpdateAntiAntianalising(PostProcessLayer.Antialiasing.FastApproximateAntialiasing, enableFastFxaa: false);
				break;
			case 3:
				UpdateAntiAntianalising(PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing);
				break;
			case 4: 
				UpdateAntiAntianalising(PostProcessLayer.Antialiasing.TemporalAntialiasing);
				break;
		}
	}

	// No Shadows
	// Hard Shadows,
	// Soft Shadows
	public void SetShadows() {
		if (_const.GraphicsShadowMode > 2) _const.GraphicsShadowMode = 2;
		if (_const.GraphicsShadowMode < 0) _const.GraphicsShadowMode = 0;
		switch (_const.GraphicsShadowMode) {
			case 0: // No Shadows
				QualitySettings.shadows = ShadowQuality.Disable;
				QualitySettings.shadowDistance = 1.0f;
				break;
			case 1: // Hard Shadows
				QualitySettings.shadows = ShadowQuality.HardOnly;
				QualitySettings.shadowResolution = ShadowResolution.Low;
				QualitySettings.shadowDistance = 50.0f; // Check layers in LevelManager shadCullArray
				break;
			case 2: // Soft Shadows
				QualitySettings.shadows = ShadowQuality.All;
				QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
				QualitySettings.shadowDistance = 50.0f; // Check layers in LevelManager shadCullArray
				break;
		}
	}
	
	// No Detail (aka flat cards, ala original)
	// High Detail (Citadel intended graphics)
	public void SetModelDetail() {
		return;
		if (_const.GraphicsModelDetail == 0) {
			_dynamicCulling.lodSqrDist = 0f;
		} else {
			_dynamicCulling.lodSqrDist = DynamicCulling.lodSqrDistDefault;
		}
		
		_dynamicCulling.forceRecull = true; // Recull to reapply meshes.
	}
	
	// No SSR
	// Low SSR,
	// High SSR
	public void SetSSR() {
		if (_const.GraphicsSSRMode > 2) _const.GraphicsSSRMode = 2;
		if (_const.GraphicsSSRMode < 0) _const.GraphicsSSRMode = 0;
		_screenSpaceReflections.Value.active = _const.GraphicsSSRMode > 0;
	}

	public void SetSSAO()
	{
		_ambientOcclusion.Value.active = _const.GraphicsSSAO;
	}

	public void SetBrightness() {
		_colorGrading.Value.brightness.value = _const.GraphicsGamma;
	}

	public void SetLanguage() {
		_const.LoadTextForLanguage(_const.AudioLanguage);
		_const.LoadAudioLogMetaData();
		foreach(TextLocalization txtLoc in _const.TextLocalizationRegister) {
			txtLoc.UpdateText();
		}
		
		_mainMenuHandler.shadApply.SetOptionsText();
		_mainMenuHandler.ssrApply.SetOptionsText();
		_mainMenuHandler.audModeApply.SetOptionsText();
		_mainMenuHandler.mdlDetApply.SetOptionsText();
	}
	
	public void SetAudioMode() {
		if (lastAudioMode == -1) lastAudioMode = PlayerPrefs.GetInt(AUDIO_MODE_KEY, 1); // Stereo as default

		AudioConfiguration audconf = AudioSettings.GetConfiguration();
		AudioSpeakerMode targetMode = AudioSpeakerMode.Stereo;
		switch(_const.AudioSpeakerMode) {
			case 0: targetMode = AudioSpeakerMode.Mono; break;
			case 1: targetMode = AudioSpeakerMode.Stereo; break;
			case 2: targetMode = AudioSpeakerMode.Quad; break;
			case 3: targetMode = AudioSpeakerMode.Surround; break;
			case 4: targetMode = AudioSpeakerMode.Mode5point1; break;
			case 5: targetMode = AudioSpeakerMode.Mode7point1; break;
			case 6: targetMode = AudioSpeakerMode.Prologic; break;
		}
		
		if (audconf.speakerMode != targetMode) {
			audconf.speakerMode = targetMode;
			AudioSettings.Reset(audconf);
		}

		if (lastAudioMode != _const.AudioSpeakerMode) {
			PlayerPrefs.SetInt(AUDIO_MODE_KEY, _const.AudioSpeakerMode);
			PlayerPrefs.Save(); // Ensure it’s written to disk
		}
	}

	private int AssignConfigInt(string section, string keyname) {
		int inputInt = -1;
		string inputCapture = System.String.Empty;
		inputCapture = INIWorker.IniReadValue(section,keyname);
		if (inputCapture == null) inputCapture = "NULL";
		bool parsed = Int32.TryParse(inputCapture, NumberStyles.Integer, Utils.en_US_Culture, out inputInt);
		if (parsed) return inputInt; else _const.sprint("Warning: Could not parse config key " + keyname + " as integer: " + inputCapture);
		return 0;
	}

	private bool AssignConfigBool(string section, string keyname) {
		int inputInt = -1;
		string inputCapture = System.String.Empty;
		inputCapture = INIWorker.IniReadValue(section,keyname);
		if (inputCapture == null) inputCapture = "NULL";
		bool parsed = Int32.TryParse(inputCapture, NumberStyles.Integer, Utils.en_US_Culture, out inputInt);
		if (parsed) {
			if (inputInt > 0) return true; else return false;
		} else _const.sprint("Warning: Could not parse config key " + keyname + " as bool: " + inputCapture);
		return false;
	}
	
	private void UpdateAntiAntianalising(PostProcessLayer.Antialiasing antialiasing, bool enableFastFxaa = true)
	{
		foreach (var postProcessLayer in _postProcessLayers)
		{
			postProcessLayer.antialiasingMode = antialiasing;
			postProcessLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Low;
			postProcessLayer.fastApproximateAntialiasing.fastMode = enableFastFxaa;
		}
	}
}
