using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using Zenject;

public class WeaponCurrent : MonoBehaviour {
	public GameObject ViewModelAssault;
	public GameObject ViewModelBlaster;
	public GameObject ViewModelDartgun;
	public GameObject ViewModelFlechette;
	public GameObject ViewModelIon;
	public GameObject ViewModelRapier;
	public GameObject ViewModelPipe;
	public GameObject ViewModelMagnum;
	public GameObject ViewModelMagpulse;
	public GameObject ViewModelPistol;
	public GameObject ViewModelPlasma;
	public GameObject ViewModelRailgun;
	public GameObject ViewModelRiotgun;
	public GameObject ViewModelSkorpion;
	public GameObject ViewModelSparq;
	public GameObject ViewModelStungun;
	public GameObject owner;

	public int weaponCurrent = new int(); // save
	public int weaponIndex = new int(); // save

	[HideInInspector] public bool justChangedWeap = true; // save
	[HideInInspector] public float[] weaponEnergySetting; // save
	[HideInInspector] public int[] currentMagazineAmount; // save
	[HideInInspector] public int[] currentMagazineAmount2; // save
	[HideInInspector] public int lastIndex = 0; // save
	[HideInInspector] public bool bottomless = false; // Don't use any ammo and
													  // energy weapons no
													  // energy, save
	[HideInInspector] public bool redbull = false; // No energy usage, save
	private static StringBuilder s1 = new StringBuilder(100);
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;
	[Inject] private WeaponFire _weaponFire;

	public int weaponCurrentPending; // save
	public int weaponIndexPending; // save

	void Start() {
		weaponCurrent = 0; // Current slot in the weapon inventory (7 slots)
		weaponIndex = -1; // Current index to the weapon look-up tables

		// Put energy settings to lowest energy level as default
		for (int j=0;j<7;j++) {
			weaponEnergySetting[j] = 0f;
			currentMagazineAmount[j] = 0;
			currentMagazineAmount2[j] = 0;
		}
		weaponCurrentPending = -1;
		weaponIndexPending = -1;
	}

	public void SetAllViewModelsDeactive() {
		Utils.Deactivate(ViewModelAssault);
		Utils.Deactivate(ViewModelBlaster);
		Utils.Deactivate(ViewModelDartgun);
		Utils.Deactivate(ViewModelFlechette);
		Utils.Deactivate(ViewModelIon);
		Utils.Deactivate(ViewModelRapier);
		Utils.Deactivate(ViewModelPipe);
		Utils.Deactivate(ViewModelMagnum);
		Utils.Deactivate(ViewModelMagpulse);
		Utils.Deactivate(ViewModelPistol);
		Utils.Deactivate(ViewModelPlasma);
		Utils.Deactivate(ViewModelRailgun);
		Utils.Deactivate(ViewModelRiotgun);
		Utils.Deactivate(ViewModelSkorpion);
		Utils.Deactivate(ViewModelSparq);
		Utils.Deactivate(ViewModelStungun);
		Utils.Deactivate(_mfdManager.energySliderLH);
		Utils.Deactivate(_mfdManager.energyHeatTicksLH);
		Utils.Deactivate(_mfdManager.overloadButtonLH);
		Utils.Deactivate(_mfdManager.unloadButtonLH);
		Utils.Deactivate(_mfdManager.loadNormalAmmoButtonLH);
		Utils.Deactivate(_mfdManager.loadAlternateAmmoButtonLH);

		Utils.Deactivate(_mfdManager.energySliderRH);
		Utils.Deactivate(_mfdManager.energyHeatTicksRH);
		Utils.Deactivate(_mfdManager.overloadButtonRH);
		Utils.Deactivate(_mfdManager.unloadButtonRH);
		Utils.Deactivate(_mfdManager.loadNormalAmmoButtonRH);
		Utils.Deactivate(_mfdManager.loadAlternateAmmoButtonRH);
	}

	public void RemoveWeapon(int weaponButton7Index) {
		WeaponButtonsManager wepbutMan = _mfdManager.wepbutMan;
		WeaponButton wepbut = wepbutMan.wepButtonsScripts[0];
		if (weaponButton7Index != weaponCurrent) {
			if (weaponButton7Index > weaponCurrent) return; // No list shift.

			weaponCurrent--;
			return; // Don't continue down and change the weapon, keep current.
		}

		SetAllViewModelsDeactive();
		_weaponFire.reloadFinished = 0;
		int initialIndex = weaponCurrent;
		if (initialIndex < 0) initialIndex = 0;
		if (initialIndex > 6) initialIndex = 0;
		int nextIndex = initialIndex - 1; // add 1 to get slot above this
		if (nextIndex < 0) nextIndex = 6; // wraparound to top
		int countCheck = 0;
		bool buttonNotValid = (_inventory.weaponInventoryIndices[nextIndex] == -1);
		while (buttonNotValid) {
			countCheck++;
			if (countCheck > 13) return; // no weapons!  don't runaway loop

			nextIndex--;
			if (nextIndex < 0) nextIndex = 6;
			buttonNotValid = (_inventory.weaponInventoryIndices[nextIndex] == -1);
		}

		wepbut = wepbutMan.wepButtonsScripts[nextIndex];
		if (!wepbut.gameObject.activeSelf) {
			wepbut = wepbutMan.wepButtonsScripts[0];
		}

		if (wepbut.gameObject.activeSelf && nextIndex != initialIndex) {
			weaponIndexPending = wepbut.useableItemIndex;
			weaponCurrentPending = wepbut.WepButtonIndex;
		} else {
			weaponCurrentPending = 0;
			weaponIndexPending = -1;
		}

		lastIndex = weaponCurrent;
		justChangedWeap = true;
		weaponCurrent = -1;
		weaponIndex = -1;
		_weaponFire.StartWeaponDip(0);
		currentMagazineAmount[weaponButton7Index] = 0; // Zero out ammo
		currentMagazineAmount2[weaponButton7Index] = 0;
		_mfdManager.UpdateHUDAmmoCountsEither();
		_mfdManager.SetWepInfo(-1);
		_mfdManager.OpenTab(0, true, TabMSG.Weapon, 0,Handedness.LH);
	}

	public void WeaponChange(int useableItemIndex, int buttonIndex) {
		if (_weaponFire.reloadFinished > _pauseScript.relativeTime) return;
		if (useableItemIndex == -1 || buttonIndex > 6 || buttonIndex < 0) {
			_mfdManager.SetAmmoIcons(-1,false); // Clear the ammo icons.
			//Debug.Log("Early exit on WeaponChange() in WeaponCurrent.cs!");
			return;
		}

		Utils.PlayUIOneShotSavable(_consts,80); // changeweapon
		if (buttonIndex == weaponCurrent) return; // Already there!

		int wep16index =  // Get index into the list of 16 weapons
			  WeaponFire.Get16WeaponIndexFromConstIndex(useableItemIndex);

		_weaponFire.StartWeaponDip(_consts.reloadTime[wep16index]);
		weaponCurrentPending = buttonIndex;
		weaponIndexPending = useableItemIndex;
		_mfdManager.SetWepInfo(-1);
		_mfdManager.UpdateHUDAmmoCountsEither();
	}

	void Update() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		if (justChangedWeap) {
			justChangedWeap = false;
			_mfdManager.SetAmmoIcons(-1,false); // Clear it.
			UpdateWeaponViewModels();
		}

		// Compare weaponCurrent since we might have more of the same type.
		if (lastIndex != weaponCurrent) lastIndex = weaponCurrent;
	}

	public void UpdateWeaponViewModels() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		int useableIndex = weaponIndex;
		int setWep = weaponIndex;
		if (weaponIndexPending >= 0) {
			setWep = weaponIndexPending;
			useableIndex = -1;
		}

		_mfdManager.HideAmmoAndEnergyItems();
		SetAllViewModelsDeactive();
		switch (setWep) {
			case 36: // "LOAD MAGNESIUM", "LOAD PENETRATOR"
				_mfdManager.ShowAmmoItems(539,540);
				Utils.Activate(ViewModelAssault);
				break;
			case 37:
				_mfdManager.ShowEnergyItems();
				Utils.Activate(ViewModelBlaster);
				break;
			case 38: // "LOAD NEEDLE", "LOAD TRANQ"
				_mfdManager.ShowAmmoItems(541,542);
				Utils.Activate(ViewModelDartgun);
				break;
			case 39: // "LOAD HORNET", "LOAD SPLINTER"
				_mfdManager.ShowAmmoItems(543,544);
				Utils.Activate(ViewModelFlechette);
				break;
			case 40:
				_mfdManager.ShowEnergyItems();
				Utils.Activate(ViewModelIon);
				break;
			case 41:
				Utils.Activate(ViewModelRapier);
				break;
			case 42:
				Utils.Activate(ViewModelPipe);
				break;
			case 43: // "LOAD HOLLOW TIP", "LOAD HEAVY SLUG"
				_mfdManager.ShowAmmoItems(545,546);
				Utils.Activate(ViewModelMagnum);
				break;
			case 44: // "LOAD CARTRIDGE"
				_mfdManager.ShowAmmoItems(547,-1);
				Utils.Activate(ViewModelMagpulse);
				_mfdManager.HideAlternateAmmoButton();
				break;
			case 45: // "LOAD STANDARD", "LOAD TEFLON"
				_mfdManager.ShowAmmoItems(548,549);
				Utils.Activate(ViewModelPistol);
				break;
			case 46:
				_mfdManager.ShowEnergyItems();
				Utils.Activate(ViewModelPlasma);
				break;
			case 47: // "LOAD RAIL CLIP"
				_mfdManager.ShowAmmoItems(550,-1);
				Utils.Activate(ViewModelRailgun);
				_mfdManager.HideAlternateAmmoButton();
				break;
			case 48:  // "LOAD RUBBER SLUG"
				_mfdManager.ShowAmmoItems(551,-1);
				Utils.Activate(ViewModelRiotgun);
				_mfdManager.HideAlternateAmmoButton();
				break;
			case 49: // "LOAD SLAG", "LOAD LARGE SLAG"
				_mfdManager.ShowAmmoItems(552,553);
				Utils.Activate(ViewModelSkorpion);
				break;
			case 50:
				_mfdManager.ShowEnergyItems();
				Utils.Activate(ViewModelSparq);
				break;
			case 51:
				_mfdManager.ShowEnergyItems();
				Utils.Activate(ViewModelStungun);
				break;
		}
	}

	public void ChangeAmmoType() {
		int wep16index = WeaponFire.Get16WeaponIndexFromConstIndex(weaponIndex);
		if (wep16index < 0) return;
		if (wep16index == 5 || wep16index == 6) {
			_consts.sprint(315);
			return; // Do nothing for pipe or rapier.
		}

		if (wep16index == 1 || wep16index == 4 || wep16index == 10 || wep16index == 14 || wep16index == 15) {
			if (_mfdManager.overloadButtonLH.activeInHierarchy) {
				_mfdManager.overloadButtonLH.GetComponent<EnergyOverloadButton>().OverloadButtonAction();
			}

			if (_mfdManager.overloadButtonRH.activeInHierarchy) {
				_mfdManager.overloadButtonRH.GetComponent<EnergyOverloadButton>().OverloadButtonAction();
			}
		} else {
			if (_inventory.wepLoadedWithAlternate[weaponCurrent]) {
				if (_inventory.wepAmmo[wep16index] > 0) {
				_inventory.wepLoadedWithAlternate[weaponCurrent] = false;
				// Take bullets out of the clip, put them back into the ammo stockpile, then zero out the clip amount, did I say clip?  I mean magazine but whatever
				_inventory.wepAmmoSecondary[wep16index] += currentMagazineAmount2[weaponCurrent];
				currentMagazineAmount2[weaponCurrent] = 0;
				LoadPrimaryAmmoType(false);
				} else {
					_consts.sprint(535); //No more of ammo type to load.
				}
			} else {
				if (_inventory.wepAmmoSecondary[wep16index] > 0) {
					_inventory.wepLoadedWithAlternate[weaponCurrent] = true;
					_inventory.wepAmmo[wep16index] += currentMagazineAmount[weaponCurrent];
					currentMagazineAmount[weaponCurrent] = 0;
					LoadSecondaryAmmoType(false);
				} else {
					_consts.sprint(535); //No more of ammo type to load.
				}
			}
		}
	}

	public void LoadPrimaryAmmoType(bool isSilent) {
		int wep16index = WeaponFire.Get16WeaponIndexFromConstIndex(weaponIndex);
		if (!_inventory.wepLoadedWithAlternate[weaponCurrent]) { // Already loaded with normal.
			if (currentMagazineAmount[weaponCurrent] == _consts.magazinePitchCountForWeapon[wep16index]) {
				_consts.sprint(191); //Current weapon magazine already full.
				return;
			}
			
			if (currentMagazineAmount[weaponCurrent] == _inventory.wepAmmo[wep16index]) {
				_consts.sprint(535); // No more of ammo type to load
				return;
			}
		}

		Unload(true);
		_inventory.wepLoadedWithAlternate[weaponCurrent] = false;

		// Put bullets into the magazine
		if (_inventory.wepAmmo[wep16index] >= _consts.magazinePitchCountForWeapon[wep16index]) {
			currentMagazineAmount[weaponCurrent] = _consts.magazinePitchCountForWeapon[wep16index];
		} else {
			currentMagazineAmount[weaponCurrent] = _inventory.wepAmmo[wep16index];
		}

		// Take bullets out of the ammo stockpile
		_inventory.wepAmmo[wep16index] -= currentMagazineAmount[weaponCurrent];

		if (!isSilent) {
			if (wep16index == 0 || wep16index == 3) {
				Utils.PlayUIOneShotSavable(_consts,248); // wlocknload
			} else {
				Utils.PlayUIOneShotSavable(_consts,260); // wreload
			}
		}

		// Update the counter on the HUD
		_mfdManager.UpdateHUDAmmoCounts(currentMagazineAmount[weaponCurrent]);
		_weaponFire.StartWeaponDip(_consts.reloadTime[wep16index]);

		// Pop it back to start to be sure
		_weaponFire.reloadContainer.localPosition =
			_weaponFire.reloadContainerHome;
	}

	public void LoadSecondaryAmmoType(bool isSilent) {
		int wep16index = WeaponFire.Get16WeaponIndexFromConstIndex(weaponIndex);
		if (_inventory.wepLoadedWithAlternate[weaponCurrent]) { // Already loaded with alternate
			if (currentMagazineAmount2[weaponCurrent] == _consts.magazinePitchCountForWeapon2[wep16index]) {
				_consts.sprint(191); //Current weapon magazine already full.
				return;
			}
			
			if (currentMagazineAmount2[weaponCurrent] == _inventory.wepAmmoSecondary[wep16index]) {
				_consts.sprint(535); // No more of ammo type to load
				return;
			}
		}

		Unload(true);
		_inventory.wepLoadedWithAlternate[weaponCurrent] = true;

		// Put bullets into the magazine
		if (_inventory.wepAmmoSecondary[wep16index] >= _consts.magazinePitchCountForWeapon2[wep16index]) {
			currentMagazineAmount2[weaponCurrent] = _consts.magazinePitchCountForWeapon2[wep16index];
		} else {
			currentMagazineAmount2[weaponCurrent] = _inventory.wepAmmoSecondary[wep16index];
		}

		// Take bullets out of the ammo stockpile
		_inventory.wepAmmoSecondary[wep16index] -= currentMagazineAmount2[weaponCurrent];

		if (!isSilent) {
			if (wep16index == 0 || wep16index == 3) {
				Utils.PlayUIOneShotSavable(_consts,248); // wlocknload
			} else {
				Utils.PlayUIOneShotSavable(_consts,260); // wreload
			}
		}

		// Update the counter on the HUD
		_mfdManager.UpdateHUDAmmoCounts(currentMagazineAmount2[weaponCurrent]);
		_weaponFire.StartWeaponDip(_consts.reloadTime[wep16index]);

		// Pop it back to start to be sure
		_weaponFire.reloadContainer.localPosition =
			_weaponFire.reloadContainerHome;
	}

	public void Unload(bool isSilent) {
		if (weaponIndex < 0) return;
		int wep16index = WeaponFire.Get16WeaponIndexFromConstIndex (weaponIndex);
		if (wep16index == 5 || wep16index == 6) {
			return; // do nothing for pipe or rapier
		}

		if (wep16index == -1) return; // we don't have a weapon at all right now :)

		// Take bullets out of the clip, put them back into the ammo stockpile, then zero out the clip amount, did I say clip?  I mean magazine but whatever
		if (_inventory.wepLoadedWithAlternate[weaponCurrent]) {
			_inventory.wepAmmoSecondary[wep16index] += currentMagazineAmount2[weaponCurrent];
			currentMagazineAmount2[weaponCurrent] = 0;

			// Update the counter on the HUD
			_mfdManager.UpdateHUDAmmoCounts(currentMagazineAmount2[weaponCurrent]);
		} else {
			_inventory.wepAmmo[wep16index] += currentMagazineAmount[weaponCurrent];
			currentMagazineAmount[weaponCurrent] = 0;

			// Update the counter on the HUD
			_mfdManager.UpdateHUDAmmoCounts(currentMagazineAmount[weaponCurrent]);
		}
		if (!isSilent) Utils.PlayUIOneShotSavable(_consts,260); // wreload
	}

	public void ReloadSecret(bool isSilent) {
		int wep16index = WeaponFire.Get16WeaponIndexFromConstIndex(weaponIndex);
		if (wep16index < 0) return;

		if (wep16index == 5 || wep16index == 6) {
			_consts.sprint(315); // Weapon does not use ammo.
			return; // do nothing for pipe or rapier
		}

		if (wep16index == 1 || wep16index == 4 || wep16index == 10 || wep16index == 14 || wep16index == 15) {
			_consts.sprint(538); // Weapon does not need reloaded.
			return; // do nothing for energy weapons
		}

		if (weaponCurrent < 0) return;

		if (_inventory.wepLoadedWithAlternate[weaponCurrent]) {
			if (currentMagazineAmount2[weaponCurrent] == _consts.magazinePitchCountForWeapon2[wep16index]) {
				_consts.sprint(191); //Current weapon magazine already full.
				return;
			}

			if (_inventory.wepAmmoSecondary[wep16index] <= 0) {
				if (_inventory.wepAmmo[wep16index] <= 0) {
					_consts.sprint(305); //No more of any ammo type to load.
					return;
				} else {
					_consts.sprint(192); //No more of current ammo type to load, loading with alternate.
					LoadPrimaryAmmoType(isSilent);
					return;
				}
			}
			LoadSecondaryAmmoType(isSilent);
		} else {
			if (currentMagazineAmount[weaponCurrent] == _consts.magazinePitchCountForWeapon[wep16index]) {
				_consts.sprint(191); //Current weapon magazine already full.
				return;
			}

			if (_inventory.wepAmmo[wep16index] <= 0) {
				if (_inventory.wepAmmoSecondary[wep16index] <= 0) {
					_consts.sprint(305); //No more of any ammo type to load.
					return;
				} else {
					_consts.sprint(192); //No more of current ammo type to load, loading with alternate.
					LoadSecondaryAmmoType(isSilent);
					return;
				}
			}
			LoadPrimaryAmmoType(isSilent);
		}
	}

	public void Reload() {
		ReloadSecret(false);
	}

	public static string Save(GameObject go) {
		WeaponCurrent wc = go.GetComponent<WeaponCurrent>();
		if (wc == null) {
			Debug.Log("WeaponCurrent missing on Player!  GameObject.name: " + go.name);
			return "0|0|0";
		}

		int j =0;
		s1.Clear();
		s1.Append(Utils.UintToString(wc.weaponCurrent,"weaponCurrent"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(wc.weaponIndex,"weaponIndex"));
		for (j=0;j<7;j++) {
			s1.Append(Utils.splitChar);
			s1.Append(Utils.FloatToString(wc.weaponEnergySetting[j],"weaponEnergySetting[" + j.ToString() + "]"));
		}
		
		for (j=0;j<7;j++) {
			s1.Append(Utils.splitChar);
			s1.Append(Utils.UintToString(wc.currentMagazineAmount[j],"currentMagazineAmount[" + j.ToString() + "]"));
		}
		
		for (j=0;j<7;j++) {
			s1.Append(Utils.splitChar);
			s1.Append(Utils.UintToString(wc.currentMagazineAmount2[j],"currentMagazineAmount2[" + j.ToString() + "]"));
		}
		
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(wc.lastIndex,"lastIndex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(wc.bottomless,"bottomless"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(wc.redbull,"redbull"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(wc.weaponCurrentPending,"weaponCurrentPending"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(wc.weaponIndexPending,"weaponIndexPending"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		WeaponCurrent wc = go.GetComponent<WeaponCurrent>();
		if (wc == null) {
			Debug.Log("WeaponCurrent.Load failure, wc == null");
			return index + 31;
		}

		if (index < 0) {
			Debug.Log("WeaponCurrent.Load failure, index < 0");
			return index + 31;
		}

		if (entries == null) {
			Debug.Log("WeaponCurrent.Load failure, entries == null");
			return index + 31;
		}

		int j =0;
		wc.weaponCurrent = Utils.GetIntFromString(entries[index],"weaponCurrent"); index++;
		wc.weaponIndex = Utils.GetIntFromString(entries[index],"weaponIndex"); index++;
		for (j=0;j<7;j++) { wc.weaponEnergySetting[j] = Utils.GetFloatFromString(entries[index],"weaponEnergySetting[" + j.ToString() + "]"); index++; }
		for (j=0;j<7;j++) { wc.currentMagazineAmount[j] = Utils.GetIntFromString(entries[index],"currentMagazineAmount[" + j.ToString() + "]"); index++; }
		for (j=0;j<7;j++) { wc.currentMagazineAmount2[j] = Utils.GetIntFromString(entries[index],"currentMagazineAmount2[" + j.ToString() + "]"); index++; }
		wc.SetAllViewModelsDeactive();
		wc.lastIndex = Utils.GetIntFromString(entries[index],"lastIndex"); index++;
		wc.bottomless = Utils.GetBoolFromString(entries[index],"bottomless"); index++;
		wc.redbull = Utils.GetBoolFromString(entries[index],"redbull"); index++;
		wc.weaponCurrentPending = Utils.GetIntFromString(entries[index],"weaponCurrentPending"); index++;
		wc.weaponIndexPending = Utils.GetIntFromString(entries[index],"weaponIndexPending"); index++;
		wc.justChangedWeap = true;
		return index;
	}
}
