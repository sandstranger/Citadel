using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;
using UnityStandardAssets.ImageEffects;

public class HardwareButton : MonoBehaviour {
	public Button[] buttons;
	public Sprite[] buttonDeactive;
	public Sprite[] buttonActive1;
	public Sprite[] buttonActive2;
	public Sprite[] buttonActive3;
	public Sprite[] buttonActive4;
	public GameObject sensaroundCenter;
	public GameObject sensaroundLH;
	public GameObject sensaroundRH;
	public GameObject sensaroundCenterCamera;
	public GameObject sensaroundLHCamera;
	public GameObject sensaroundRHCamera;
	public GameObject bioMonitorContainer;
	public Light infraredLight;
	public GameObject playerCamera;
	public Light headlight;
	public EmailContentsButtonsManager ecbm;
	public GameObject ShieldActivateFX;
	public GameObject ShieldDeactivateFX;

	// Hw referenceIndex, ref14Index, button index
	// Bio 27,6, 0
	// Sen 24,3, 1
	// Lan 28,7, 2
	// Shi 26,5, 3
	// Nig 32,11,4
	// Ere 23,2, 5
	// Boo 30,9, 6
	// Jum 31,10,7

	[HideInInspector] public AudioSource SFX;
	private const float defaultZero = 0f;
	private float brightness = 0f;
	private const float lanternVersion1Brightness = 2.5f;
	private const float lanternVersion2Brightness = 4;
	private const float lanternVersion3Brightness = 5;
	private Grayscale gsc;
	private Grayscale gscSensaCenter;
	private Grayscale gscSensaLH;
	private Grayscale gscSensaRH;
	[Inject]
	private BiomonitorGraphSystem _biomonitorGraphSystem;
	[Inject] private PlayerEnergy _playerEnergy;
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private GetInput _getInput;
	[Inject] private Inventory _inventory;
	[Inject] private WeaponCurrent _weaponCurrent;

	void Awake () {
		SFX = GetComponent<AudioSource>();
		gsc = playerCamera.GetComponent<Grayscale>();
		gscSensaCenter = sensaroundCenterCamera.GetComponent<Grayscale>();
		gscSensaLH = sensaroundLHCamera.GetComponent<Grayscale>();
		gscSensaRH = sensaroundRHCamera.GetComponent<Grayscale>();
	}

	public void ListenForHardwareHotkeys () {
		if (_inventory.hasHardware[2] && _getInput.Email())      EReaderAction();
		if (_inventory.hasHardware[3] && _getInput.Sensaround()) SensaroundAction();
		if (_inventory.hasHardware[5] && _getInput.Shield())     ShieldAction();
		if (_inventory.hasHardware[6] && _getInput.Biomonitor()) BioAction();
		if (_inventory.hasHardware[7] && _getInput.Lantern())    LanternAction();
		if (_inventory.hasHardware[9] && _getInput.Booster())    BoosterAction();
		if (_inventory.hasHardware[10]&& _getInput.Jumpjets())   JumpJetsAction();
		if (_inventory.hasHardware[11]&& _getInput.Infrared())   InfraredAction();
	}

	// 0 = bio, 1 = sen, 2 = lan, 3 = shi, 4 = nig, 5 = ere, 6 = boo, 7 = jum
	// verz must come from _inventory.hardwareVersionSetting[] as this value has already subtracted 1 since the version number on prefabs is 1 based but the one needed for images is 0 based.
	public void SetVersionIconForButton(bool isOn, int verz, int button8Index) {
// 		Debug.Log("SetVersionIconForButton with version " + verz.ToString() + ", and button8Index of " + button8Index.ToString());
		if (button8Index < 0 || button8Index > 7) button8Index = 0;
		if (isOn) {
			switch (verz) {
			case 0:
				buttons[button8Index].image.overrideSprite = buttonActive1[button8Index];
				break;
			case 1:
				buttons[button8Index].image.overrideSprite = buttonActive2[button8Index];
				break;
			case 2:
				buttons[button8Index].image.overrideSprite = buttonActive3[button8Index];
				break;
			case 3:
				buttons[button8Index].image.overrideSprite = buttonActive4[button8Index];
				break;
			default:
				buttons[button8Index].image.overrideSprite = buttonActive4[button8Index];
				break;
			}
		} else {
			buttons[button8Index].image.overrideSprite = buttonDeactive[button8Index];
		}
	}

	public void BioClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		BioAction();
	}

	public void BioAction() {
		if (_inventory.BioMonitorVersion() == 0 && _playerEnergy.energy <= 0) {
			_consts.sprint(_consts.stringTable[314],_weaponCurrent.owner);
			return;
		}

		Utils.PlayUIOneShotSavable(_consts,78);
		if (_inventory.BioMonitorActive()) {
			BioOff();
		} else {
			BioOn();
		}
	}

	// Called by PlayerEnergy when exhausted energy to 0 so mustn't play sound.
	public void BioOff() {
		_inventory.hardwareIsActive[6] = false;
		SetVersionIconForButton(_inventory.hardwareIsActive[6],_inventory.hardwareVersionSetting[6],0);
		
		if (_mfdManager.FPS.activeInHierarchy) return;
		_biomonitorGraphSystem.ClearGraphs();
		Utils.Deactivate(bioMonitorContainer);
	}

	public void BioOn() {
		_inventory.hardwareIsActive[6] = true;
		SetVersionIconForButton(_inventory.BioMonitorActive(),_inventory.hardwareVersionSetting[6],0);
		Utils.Activate(bioMonitorContainer);
	}

	public void ActivateSensaroundCenter() {
		_mfdManager.DisableAllCenterTabs();
		Utils.Activate(sensaroundCenterCamera);
		Utils.Activate(sensaroundCenter);
	}

	public void ActivateSensaroundSides() {
		_mfdManager.TabReset(true); // right
		_mfdManager.TabReset(false); // left
		if (sensaroundLHCamera != null) sensaroundLHCamera.SetActive (true);
		if (sensaroundLH != null) sensaroundLH.SetActive (true);
		if (sensaroundRHCamera != null) sensaroundRHCamera.SetActive (true);
		if (sensaroundRH != null) sensaroundRH.SetActive (true);
	}
	
	public void HideSensaround() {
		if (sensaroundCenterCamera != null) sensaroundCenterCamera.SetActive(false);
		if (sensaroundCenter != null) sensaroundCenter.SetActive(false);
		if (sensaroundLHCamera != null) sensaroundLHCamera.SetActive(false);
		if (sensaroundLH != null) sensaroundLH.SetActive(false);
		if (sensaroundRHCamera != null) sensaroundRHCamera.SetActive(false);
		if (sensaroundRH != null) sensaroundRH.SetActive(false);
	}
	
	public void UnhideSensaround() {
		if (!_inventory.hardwareIsActive[3]) return;
		
		if (_inventory.hardwareVersion[3] == 1) {
			ActivateSensaroundCenter(); // Only center on version 1.
		} else {
			ActivateSensaroundCenter();
			ActivateSensaroundSides();
		}
	}

	public void DeactivateSensaroundCameras() {
		HideSensaround();
		_mfdManager.CenterTabButtonClickSilent(_mfdManager.curCenterTab,true);
		_mfdManager.TabReset(true); // right
		_mfdManager.TabReset(false); // left
		_mfdManager.ReturnToLastTab(true);
		_mfdManager.ReturnToLastTab(false);
	}

	public void SensaroundOn() {
		_inventory.hardwareIsActive[3] = true;
		SetVersionIconForButton(_inventory.hardwareIsActive[3], _inventory.hardwareVersionSetting[3],1);
		UnhideSensaround();
	}

	public void SensaroundClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		SensaroundAction();
	}

	public void SensaroundAction() {
		if (_playerEnergy.energy <=0) { _consts.sprint(_consts.stringTable[314],_weaponCurrent.owner); return; }

		if (_inventory.hardwareIsActive[3]) {
			Utils.PlayUIOneShotSavable(_consts,82);
			SensaroundOff();
		} else {
			Utils.PlayUIOneShotSavable(_consts,93);
			SensaroundOn();
		}
	}

	// called by PlayerEnergy when exhausted energy to 0
	public void SensaroundOff() {
		_inventory.hardwareIsActive[3] = false;
		SetVersionIconForButton(_inventory.hardwareIsActive[3],_inventory.hardwareVersionSetting[3],1);
		DeactivateSensaroundCameras();
	}

	public void ShieldClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		ShieldAction();
	}
	
	public void ShieldOff() {
		_inventory.hardwareIsActive[5] = false;
		SetVersionIconForButton(_inventory.hardwareIsActive[5],_inventory.hardwareVersionSetting[5],3);
	}
	
	public void ShieldOn() {
		_inventory.hardwareIsActive[5] = true;
		SetVersionIconForButton(_inventory.hardwareIsActive[5],_inventory.hardwareVersionSetting[5],3);
	}

	public void ShieldAction() {
		if (_playerEnergy.energy <=0) { _consts.sprint(_consts.stringTable[314],_weaponCurrent.owner); return; }
		if (_inventory.hardwareIsActive[5]) {
			Utils.PlayUIOneShotSavable(_consts,95);
			ShieldOffWithEffects();
		} else {
			Utils.PlayUIOneShotSavable(_consts,96);
			ShieldDeactivateFX.SetActive(false);
			ShieldActivateFX.SetActive(true);
			ShieldOn();
			
		}
	}

	// Called by PlayerEnergy when exhausted energy to 0.
	public void ShieldOffWithEffects() {
		ShieldOff();
		ShieldDeactivateFX.SetActive(true);
		ShieldActivateFX.SetActive(false);
	}

	public void LanternClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		LanternAction();
	}

	public void LanternAction() {
		if (_playerEnergy.energy <=0) { _consts.sprint(_consts.stringTable[314],_weaponCurrent.owner); return; }
		Utils.PlayUIOneShotSavable(_consts,78);
		if (_inventory.hardwareIsActive[7]) {
			LanternOff();
		} else {
			LanternOn();
		}
	}

	public void LanternOn() {
		_inventory.hardwareIsActive[7] = true;
		SetVersionIconForButton(_inventory.LanternActive(), _inventory.hardwareVersionSetting[7],2);

		// Figure out which brightness setting to use depending on version.
		switch(_inventory.hardwareVersionSetting[7]) {
			case 0: brightness = lanternVersion1Brightness; break;
			case 1: brightness = lanternVersion2Brightness; break;
			case 2: brightness = lanternVersion3Brightness; break;
			default: brightness = defaultZero; break;
		}

		Utils.EnableLight(headlight);
		headlight.intensity = brightness; // Set the light intensity per version.
	}
	
	// Called by PlayerEnergy when exhausted energy to 0.
	public void LanternOff() {
		_inventory.hardwareIsActive[7] = false;
		SetVersionIconForButton(_inventory.LanternActive(), _inventory.hardwareVersionSetting[7],2);
		Utils.DisableLight(headlight);
		headlight.intensity = defaultZero; // Turn the light off.
	}

	public void InfraredClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		InfraredAction();
	}

	public void InfraredAction() {
		if (_playerEnergy.energy <=0) { _consts.sprint(_consts.stringTable[314],_weaponCurrent.owner); return; }
		if (_inventory.hardwareIsActive[11]) {
			Utils.PlayUIOneShotSavable(_consts,82);
		} else {
			Utils.PlayUIOneShotSavable(_consts,98);
		}
		_inventory.hardwareIsActive[11] = !_inventory.hardwareIsActive[11];
		SetVersionIconForButton(_inventory.hardwareIsActive[11], _inventory.hardwareVersionSetting[11],4);
		if (_inventory.hardwareIsActive[11]) {
			InfraredOn();
		} else {
			InfraredOff();
		}
	}

	public void InfraredOn() {
		Utils.EnableLight(infraredLight);
		Utils.EnableGrayscale(gsc);
		Utils.EnableGrayscale(gscSensaCenter);
		Utils.EnableGrayscale(gscSensaLH);
		Utils.EnableGrayscale(gscSensaRH);
	}

	// called by PlayerMovement when exhausted energy to < 11f
	public void InfraredOff() {
		_inventory.hardwareIsActive[11] = false;
		Utils.DisableLight(infraredLight);
		Utils.DisableGrayscale(gsc);
		Utils.DisableGrayscale(gscSensaCenter);
		Utils.DisableGrayscale(gscSensaLH);
		Utils.DisableGrayscale(gscSensaRH);
		SetVersionIconForButton(false,_inventory.hardwareVersionSetting[11],4);
	}

	public void EReaderClick () {
		_mfdManager.mouseClickHeldOverGUI = true;
		EReaderAction();
	}
	
	public void EReaderOn() {
		_inventory.hardwareIsActive[2] = true;
		_mfdManager.OpenEReaderInItemsTab();
	}

	public void EReaderAction() {
		Utils.PlayUIOneShotSavable(_consts,97);
		EReaderOn();
	}


	public void BoosterClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		BoosterAction();
	}

	public void BoosterAction() {
		if (_inventory.BoosterSetToBoost() && _playerEnergy.energy <= 0) {
			_consts.sprint(_consts.stringTable[314],_weaponCurrent.owner);
			return;
		}

		Utils.PlayUIOneShotSavable(_consts,78);
		if (_inventory.hardwareIsActive[9]) {
			BoosterOff();
		} else {
			BoosterOn();
		}
	}
	
	public void BoosterOn() {
		_inventory.hardwareIsActive[9] = true;
		SetVersionIconForButton(_inventory.hardwareIsActive[9],_inventory.hardwareVersionSetting[9],6);
	}

	// called by PlayerMovement when exhausted energy to < 11f
	public void BoosterOff() {
		_inventory.hardwareIsActive[9] = false;
		SetVersionIconForButton(_inventory.hardwareIsActive[9],_inventory.hardwareVersionSetting[9],6);
	}

	public void JumpJetsClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		JumpJetsAction();
	}

	public void JumpJetsAction() {
		if (_playerEnergy.energy <= 0) {
			_consts.sprint(_consts.stringTable[314],_weaponCurrent.owner);
			return;
		}

		Utils.PlayUIOneShotSavable(_consts,78);
		_inventory.JumpJetsToggle();
		if (_inventory.JumpJetsActive()) {
			JumpJetsOn();
		} else {
			JumpJetsOff();
		}
	}
	
	public void JumpJetsOn() {
		_inventory.hardwareIsActive[10] = true;
		SetVersionIconForButton(_inventory.JumpJetsActive(),_inventory.hardwareVersionSetting[10],7);
	}

	// called by PlayerMovement when exhausted energy to < 11f
	public void JumpJetsOff() {
		_inventory.hardwareIsActive[10] = false;
		SetVersionIconForButton(_inventory.JumpJetsActive(),_inventory.hardwareVersionSetting[10],7);
	}
}
