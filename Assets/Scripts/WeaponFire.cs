using UnityEngine;
using System.Collections;
using System.Text;
using Citadel.Game;
using Zenject;

public class WeaponFire : MonoBehaviour {
	// External references, required
    public GameObject impactEffect;
	public GameObject noDamageIndicator;
    public Camera playerCamera; // assign in the editor
    public GameObject playerCapsule;
    public EnergyOverloadButton energoverButton;
    public EnergyHeatTickManager energheatMgr;
    public Animator anim; // assign in the editor
	public Animator rapieranim; // assign in the editor
	public GameObject muzFlashMK3;
	public GameObject muzSmokeMK3;
	public GameObject muzFlashBlaster;
	public GameObject muzFlashDartgun;
	public GameObject muzFlashFlechette;
	public GameObject muzSmokeFlechette;
	public GameObject muzFlashIonBeam;
	public GameObject muzSmokeMagnum;
	public GameObject muzFlashMagnum;
	public GameObject muzSmokePistol;
	public GameObject muzFlashPistol;
	public GameObject muzFlashMagpulse;
	public GameObject muzFlashPlasma;
	public GameObject muzSmokeRailgun;
	public GameObject muzFlashRailgun;
	public GameObject muzSmokeRiotgun;
	public GameObject muzFlashRiotgun;
	public GameObject muzSmokeSkorpion;
	public GameObject muzFlashSkorpion;
	public GameObject muzFlashSparq;
	public GameObject muzFlashStungun;
	public float[] fogBaseDensityForLevel;
	public Color[] fogColorForLevel;
	public SSMS.SSMSGlobalFog ssmsGlobalFog;
	public Transform reloadContainer; // Recoil the weapon view models
    public bool overloadEnabled; // save
	public float reloadFinished; // save
	public float lerpStartTime; // save
	public float reloadLerpValue; // save
	public int fogFac;

    [HideInInspector] public DamageData damageData;
    [HideInInspector] public float waitTilNextFire = 0f; // save
    [HideInInspector] public float sparqSetting = 50f; // save
    [HideInInspector] public float ionSetting = 100f; // save
    [HideInInspector] public float blasterSetting = 15f; // save
    [HideInInspector] public float plasmaSetting = 40f; // save
    [HideInInspector] public float stungunSetting = 20f;  // save
	[HideInInspector] public Vector3 reloadContainerHome;
	[HideInInspector] public bool recoiling; // save
	[HideInInspector] public int lerpUp = 0; // 0 = not lerping, 1 = up, 2 = dn
	[HideInInspector] public float justFired; // save
	[HideInInspector] public float energySliderClickedTime; // save
	[HideInInspector] public float cyberWeaponAttackFinished; // save
	[HideInInspector] public float reloadContainerDropAmount = 0.66f;
	[HideInInspector] public float targetY; // save

	// Internal references
    private float hitOffset = 0f;
    private float verticalOffset = -0.2f; // For laser beams
    private float fireDistance = 200f;
    private float hitscanDistance = 200f;
    private float meleescanDistance = 3.2f;
	private float overheatedPercent = 80f;
    private float magpulseShotForce = 2.2f;
    private float stungunShotForce = 2.2f;
    private float railgunShotForce = 5f;
    private float plasmaShotForce = 1.5f;
	private float inventoryModeViewRotateMax = 48f;
    private float clipEnd;
    private RaycastHit tempHit;
    private Vector3 tempVec;
    private HealthManager tempHM;
    private float retval;
    private float heatTickFinished;
    private float heatTickTime = 0.50f;
	private Rigidbody playercapRbody;
	private float wepYRot;
	private static readonly StringBuilder s1 = new StringBuilder(100 * 500);

	[Inject] private LevelManager _levelManager;
	[Inject] private ConsoleEmulator _consoleEmulator;
	[Inject] private PlayerEnergy _playerEnergy;
	[Inject] private BiomonitorGraphSystem _biomonitorGraphSystem;
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private Automap _automap;
	[Inject] private GUIState _guiState;
	[Inject] private PlayerPatch _playerPatch;
	[Inject] private GetInput _getInput;
	[Inject] private Inventory _inventory;
	[Inject] private MouseCursor _mouseCursor;
	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private Music _music;
	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerHealth _playerHealth;
	[Inject] private PlayerMovement _playerMovement;
	[Inject] private WeaponCurrent _weaponCurrent;

	// Not needed on Const as this only exists in one unique place on player.
	private float[] driftForWeapon = new float[16]{5f,0f,15f,50f,0f,0f,0f,8f,
												   3f,3f,3f,12f,10f,30f,0f,3f};

    void Start() {
        damageData = new DamageData(_consts);
        tempHit = new RaycastHit();
        tempVec = new Vector3(0f, 0f, 0f);
        heatTickFinished = _pauseScript.relativeTime + heatTickTime;
		reloadContainerHome = reloadContainer.localPosition;

		// Set less than 30s before _pauseScript.relativeTime to guarantee we
		// don't immediately play action music.
		justFired = (_pauseScript.relativeTime - 31f);

		energySliderClickedTime = _pauseScript.relativeTime;
		playercapRbody = playerCapsule.GetComponent<Rigidbody>();
		cyberWeaponAttackFinished = _pauseScript.relativeTime;
		wepYRot = 0f;
		sparqSetting = 50f;
		ionSetting = 100f;
		blasterSetting = 15f;
		plasmaSetting = 40f;
		stungunSetting = 20f;
		reloadLerpValue = 0;
		reloadFinished = _pauseScript.relativeTime;
		lerpStartTime = _pauseScript.relativeTime;
		fogFac = 0;
    }

    void GetWeaponData(int index) {
        if (index < 0) return;
		if (_weaponCurrent.weaponCurrent < 0) return;

        if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
			// Alternate (2)
            damageData.damage = _consts.damagePerHitForWeapon2[index];
            damageData.delayBetweenShots = 
				_consts.delayBetweenShotsForWeapon2[index];

            damageData.penetration = _consts.penetrationForWeapon2[index];
            damageData.offense = _consts.offenseForWeapon2[index];
        } else {
			// Normal
            damageData.damage = _consts.damagePerHitForWeapon[index];
            damageData.delayBetweenShots =
				_consts.delayBetweenShotsForWeapon[index];

            damageData.penetration = _consts.penetrationForWeapon[index];
            damageData.offense = _consts.offenseForWeapon[index];
        }

        damageData.damageOverload = _consts.damageOverloadForWeapon[index];
        damageData.energyDrainLow = _consts.energyDrainLowForWeapon[index];
        damageData.energyDrainHi = _consts.energyDrainHiForWeapon[index];
        damageData.energyDrainOver = _consts.energyDrainOverloadForWeapon[index];
        damageData.attackType = _consts.attackTypeForWeapon[index];
        damageData.berserkActive = (Utils.CheckFlags(_playerPatch.patchActive,PlayerPatch.PATCH_BERSERK));
    }

    public static int Get16WeaponIndexFromConstIndex(int index) {
        switch (index) {
            case 36: return 0; // Mark3 Assault Rifle
            case 37: return 1; // ER-90 Blaster
            case 38: return 2; // SV-23 Dartgun
            case 39: return 3; // AM-27 Flechette
            case 40: return 4; // RW-45 Ion Beam
            case 41: return 5; // TS-04 Laser Rapier
            case 42: return 6; // Lead Pipe
            case 43: return 7; // Magnum 2100
            case 44: return 8; // SB-20 Magpulse
            case 45: return 9; // ML-41 Pistol
            case 46: return 10;// LG-XX Plasma Rifle
            case 47: return 11;// MM-76 Railgun
            case 48: return 12;// DC-05 Riotgun
            case 49: return 13;// RF-07 Skorpion
            case 50: return 14;// Sparq Beam
            case 51: return 15;// DH-07 Stungun
        }
        return -1;
    }

    bool CurrentWeaponUsesEnergy () {
        if (_weaponCurrent.weaponIndex == 37 || _weaponCurrent.weaponIndex == 40 ||
			_weaponCurrent.weaponIndex == 46 || _weaponCurrent.weaponIndex == 50 ||
			_weaponCurrent.weaponIndex == 51)
			return true;
        return false;
    }

    bool WeaponsHaveAnyHeat() {
		if (_weaponCurrent.redbull) return false;
		if (_inventory.currentEnergyWeaponHeat[0] > 0f) return true;
		if (_inventory.currentEnergyWeaponHeat[1] > 0f) return true;
		if (_inventory.currentEnergyWeaponHeat[2] > 0f) return true;
		if (_inventory.currentEnergyWeaponHeat[3] > 0f) return true;
		if (_inventory.currentEnergyWeaponHeat[4] > 0f) return true;
		if (_inventory.currentEnergyWeaponHeat[5] > 0f) return true;
		if (_inventory.currentEnergyWeaponHeat[6] > 0f) return true;
        return false;
    }

    void HeatBleedOff() {
        if (heatTickFinished < _pauseScript.relativeTime) {
			fogFac--;
			if (fogFac < 0) fogFac = 0;
			if (WeaponsHaveAnyHeat() || CurrentWeaponUsesEnergy()) {
				_inventory.currentEnergyWeaponHeat[0] -= 10f; if (_inventory.currentEnergyWeaponHeat[0] <= 0f) _inventory.currentEnergyWeaponHeat[0] = 0f;
				_inventory.currentEnergyWeaponHeat[1] -= 10f; if (_inventory.currentEnergyWeaponHeat[1] <= 0f) _inventory.currentEnergyWeaponHeat[1] = 0f;
				_inventory.currentEnergyWeaponHeat[2] -= 10f; if (_inventory.currentEnergyWeaponHeat[2] <= 0f) _inventory.currentEnergyWeaponHeat[2] = 0f;
				_inventory.currentEnergyWeaponHeat[3] -= 10f; if (_inventory.currentEnergyWeaponHeat[3] <= 0f) _inventory.currentEnergyWeaponHeat[3] = 0f;
				_inventory.currentEnergyWeaponHeat[4] -= 10f; if (_inventory.currentEnergyWeaponHeat[4] <= 0f) _inventory.currentEnergyWeaponHeat[4] = 0f;
				_inventory.currentEnergyWeaponHeat[5] -= 10f; if (_inventory.currentEnergyWeaponHeat[5] <= 0f) _inventory.currentEnergyWeaponHeat[5] = 0f;
				_inventory.currentEnergyWeaponHeat[6] -= 10f; if (_inventory.currentEnergyWeaponHeat[6] <= 0f) _inventory.currentEnergyWeaponHeat[6] = 0f;
				if (CurrentWeaponUsesEnergy()) energheatMgr.HeatBleed(_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent]); // update hud heat ticks if current weapon uses energy
			}
			
            heatTickFinished = _pauseScript.relativeTime + heatTickTime;
        }
    }

	public void Recoil (int i) {
		float strength = _consts.recoilForWeapon[i];
		//Debug.Log("Recoil from gun index: "+i.ToString()+" with strength of " +strength.ToString());
		if (strength <= 0f) return;
		if (_playerMovement.fatigue > 80) strength = strength * 2f;
		strength = strength * 0.25f;
		Vector3 wepJoltPosition = new Vector3(reloadContainer.localPosition.x - (strength * 0.5f * Random.Range(-1f,1f)), reloadContainer.localPosition.y, (reloadContainerHome.z - strength));
		if (wepJoltPosition.x > 999f) wepJoltPosition.x = 0;
		if (wepJoltPosition.y > 999f) wepJoltPosition.y = 0;
		if (wepJoltPosition.z > 999f) wepJoltPosition.z = 0;
		reloadContainer.localPosition = wepJoltPosition;
		recoiling = true;
	}

	void WeaponLerpGetTargetUp() {
		// Percentage of this half of the trip.
		reloadLerpValue = (0.5f - (1 - reloadLerpValue))/0.5f;
		targetY = (-1 * reloadContainerDropAmount * (1 - reloadLerpValue));
		if (targetY > reloadContainerHome.y) targetY = reloadContainerHome.y;
	}

	void WeaponLerpGetTargetDown() {
		// Percentage of this half of the trip.
		reloadLerpValue = reloadLerpValue/0.5f;
		targetY = reloadContainerHome.y - reloadContainerDropAmount;
		targetY *= reloadLerpValue;
	}

	void Recoiling() {
		if (!recoiling) return;

		float x = reloadContainer.localPosition.x; // side to side
		float z = reloadContainer.localPosition.z; // forward and back
		z = Mathf.Lerp(z,reloadContainerHome.z,Time.deltaTime);
		x = Mathf.Lerp(x,reloadContainerHome.x,Time.deltaTime);
		reloadContainer.localPosition = 
			new Vector3(x,reloadContainer.localPosition.y,z);
	}

	void UpdateWeaponReloadDip() {
		// Move weapon transform up/down for reload "animation" & weapon swap.
		int i = Get16WeaponIndexFromConstIndex(_weaponCurrent.weaponIndex);
		if (i < 0 || i > 15) i = 0;
		if (reloadFinished > _pauseScript.relativeTime) {
			float elapsed = (_pauseScript.relativeTime - lerpStartTime);

			// Percent towards goal time total (both halves of the action).
			reloadLerpValue = (elapsed/(reloadFinished-lerpStartTime));//_consts.reloadTime[i]);
			if (reloadLerpValue >= 0.5f) { // Flip back to lerp up.
				lerpUp = 1;
				WeaponLerpGetTargetUp();
				CompleteWeaponChange();
			} else {
				lerpUp = 2;
				WeaponLerpGetTargetDown();
			}

			Mathf.Clamp(targetY, -100f, 100f);
			Vector3 pos = new Vector3(reloadContainer.localPosition.x,
									  targetY,
									  reloadContainer.localPosition.z);

			reloadContainer.localPosition = pos;
		} else {
			lerpUp = 0;
			Vector3 pos = new Vector3(reloadContainer.localPosition.x,
									  reloadContainerHome.y,
									  reloadContainer.localPosition.z);

			reloadContainer.localPosition = pos;
		}
	}

    void Update() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		// Slowly cool off any weapons that have been heated from firing
		HeatBleedOff();
		if (fogFac > 255) fogFac = 255;
		ssmsGlobalFog.fogDensity = 0.451f * (fogBaseDensityForLevel[LevelManager.currentLevel] + ((((float)fogFac)/255f) * fogBaseDensityForLevel[LevelManager.currentLevel]));
		ssmsGlobalFog.fogColor = ssmsGlobalFog.fogTint = fogColorForLevel[LevelManager.currentLevel];
		UpdateWeaponReloadDip();
		RotateViewWeapon();
		Recoiling();
		CheckAttackInput();
		CheckReloadInput();
		CheckAmmoChangeInput();
    }

	public void CompleteWeaponChange() {
		if (_weaponCurrent.weaponCurrentPending == -1) return;

		// Set current weapon 7 slot
		_weaponCurrent.weaponCurrent = _weaponCurrent.weaponCurrentPending;
        if (CurrentWeaponUsesEnergy()) {
			// Update hud heat ticks if current weapon uses energy
			int iC = _weaponCurrent.weaponCurrent;
			energheatMgr.HeatBleed(_inventory.currentEnergyWeaponHeat[iC]);
		}

		// Set current weapon inventory lookup index
		_weaponCurrent.weaponIndex = _weaponCurrent.weaponIndexPending;

		// Reset pending indices now that transition is done
		_weaponCurrent.weaponCurrentPending = -1;
		_weaponCurrent.weaponIndexPending = -1;

		// Update the ammo icons.
		int ind = _weaponCurrent.weaponIndex;
		bool alt = false;
		if (ind >= 0 && ind < 16) alt = _inventory.wepLoadedWithAlternate[ind];
		_mfdManager.SetAmmoIcons(ind,alt);
		_mfdManager.SetWepInfo(_weaponCurrent.weaponIndex);
		_weaponCurrent.UpdateWeaponViewModels();
	}

	public void StartWeaponDip(float delay) {
		if (delay < 0) delay = 0;
		reloadFinished = _pauseScript.relativeTime + delay;
		lerpStartTime = _pauseScript.relativeTime;
	}

	void RotateViewWeapon() {
		if (_mouseLookScript.inventoryMode) {
			float screenHalf = (Screen.width/2f);
			float cursorX = _mouseCursor.drawTexture.center.x;
			float distFromCenter = (cursorX - screenHalf);
			float percentRotated = (distFromCenter / screenHalf);
			wepYRot = percentRotated * inventoryModeViewRotateMax;
			reloadContainer.localRotation = Quaternion.Euler(0f,wepYRot,0f);
		} else {
			reloadContainer.localRotation = Quaternion.Euler(0f,0f,0f);
		}
	}

	void CheckAttackInput() {
		// Check for other things that must capture and override clicks
		if (_getInput.Attack()) {
			if (_mouseLookScript.vmailActive) {
				_inventory.DeactivateVMail();
				_mouseLookScript.vmailActive = false;
				waitTilNextFire = _pauseScript.relativeTime + 0.8f;
				return;
			}

			if (_mouseLookScript.inCyberSpace) {
				FireCyberWeapon();
				return;
			}

			if (_mouseLookScript.holdingObject
				&& !_mfdManager.mouseClickHeldOverGUI) { // !Just clicked
				if (!_guiState.isBlocking) {
					// Drop it
					_mouseLookScript.DropHeldItem ();
					return;
				} else {
					_mouseLookScript.AddItemToInventory(_mouseLookScript.heldObjectIndex,_mouseLookScript.heldObjectCustomIndex);
					_mouseLookScript.ResetHeldItem();
					return;
				}
			}
		}

		int wepdex = Get16WeaponIndexFromConstIndex(_weaponCurrent.weaponIndex);
		if (wepdex == -1) return; // No weapon.
		if (_guiState.isBlocking) return;
		if (_mouseLookScript.holdingObject) return;
		if (_mfdManager.mouseClickHeldOverGUI) return;

		StartNormalAttack(wepdex);
	}

	public void StartNormalAttack(int wep16Index) {
		if (wep16Index < 0 || wep16Index > 15) return;

		GetWeaponData(wep16Index);
		if (_getInput.Attack()
			&& waitTilNextFire < _pauseScript.relativeTime
			&& (_pauseScript.relativeTime - energySliderClickedTime) > 0.1f
			&& reloadFinished < _pauseScript.relativeTime) {

			StartCoroutine(CheckUIStateAndAttack(wep16Index));
		}
	}

	IEnumerator CheckUIStateAndAttack(int wepdex) {
		yield return null; // Ensure next frame

		if (_guiState.isBlocking) yield break;
		if (_mouseLookScript.holdingObject) yield break;
		if (_mfdManager.mouseClickHeldOverGUI) yield break;
		if (reloadFinished >= _pauseScript.relativeTime) yield break;
		if (waitTilNextFire >= _pauseScript.relativeTime) yield break;
		if (wepdex < 0 || wepdex > 15) yield break;
		if (_automap.inFullMap) yield break;

		justFired = _pauseScript.relativeTime; // set justFired so that Music.cs can see it and play corresponding music in a little bit from now or keep playing action music
		// Check weapon type and check ammo before firing
		switch (wepdex) {
			case 1: goto case 15;
			case 4: goto case 15;
			case 5: goto case 6;
			case 6:
				// Pipe or Laser Rapier, attack without prejudice.
				// isSilent == false here so play normal SFX.
				FireWeapon(wepdex, false); 
				break;
			case 10: goto case 15;
			case 14: goto case 15;
			case 15: 
				// Energy weapons so check energy level
				// Even if we have only 1 energy, we still fire with all we've got up to the energy level setting of course
				if (_playerEnergy.energy > 0
					|| _weaponCurrent.bottomless
					|| _weaponCurrent.redbull) {
					if (_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] > overheatedPercent
						&& !_weaponCurrent.bottomless
						&& !_weaponCurrent.redbull) {
						Utils.PlayUIOneShotSavable(_consts,238); // noammo
						waitTilNextFire = _pauseScript.relativeTime + 0.8f;
						_consts.sprint(11);
					} else {
						FireWeapon(wepdex, false); // weapon index, isSilent == false so play normal SFX
					}
				} else {
					_consts.sprint(207); // Not enough energy to fire weapon.
				}
				break;
			default:
				// Uses normal ammo, check versus alternate or normal to see if we have ammo then fire
				if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
					if (_weaponCurrent.currentMagazineAmount2[_weaponCurrent.weaponCurrent] > 0
						|| _weaponCurrent.bottomless) {
						FireWeapon(wepdex, false); // weapon index, isSilent == false so play normal SFX
					} else {
						Utils.PlayUIOneShotSavable(_consts,238); // noammo
						waitTilNextFire = _pauseScript.relativeTime + 0.8f;
					}
				} else {
					if (_weaponCurrent.currentMagazineAmount[_weaponCurrent.weaponCurrent] > 0
						|| _weaponCurrent.bottomless) {
						FireWeapon(wepdex, false); // weapon index, isSilent == false so play normal SFX
					} else {
						Utils.PlayUIOneShotSavable(_consts,238); // noammo
						waitTilNextFire = _pauseScript.relativeTime + 0.8f;
					}
				}
				break;
		}
	}

	void CheckReloadInput() {
		if (reloadFinished >= _pauseScript.relativeTime) return;
		if (!_getInput.Reload()) return;

		if (_consts.InputQuickReloadWeapons) {
			// Press reload once, to do both unload then reload
			_weaponCurrent.Reload();
			return;
		}

		if (_weaponCurrent.weaponCurrent < 0) return;

		// First press reload to unload, then press again to load
		int wep16index = WeaponFire.Get16WeaponIndexFromConstIndex(_weaponCurrent.weaponIndex);
		if (wep16index < 0) return;

		if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
			if (_weaponCurrent.currentMagazineAmount2[_weaponCurrent.weaponCurrent] <= 0
				|| _inventory.wepAmmoSecondary[wep16index] <= 0) { // True for no wepAmmoSecondary causes Reload to run and display no ammo message.
				_weaponCurrent.Reload();
				// Debug.Log("Reload step");
			} else {
				_weaponCurrent.Unload(false);
				// Debug.Log("Unload step");
			}
		} else {
			if (_weaponCurrent.currentMagazineAmount[_weaponCurrent.weaponCurrent] <= 0
				|| _inventory.wepAmmo[wep16index] <= 0) { // True for no wepAmmo causes Reload to run and display no ammo message.
				_weaponCurrent.Reload();
				// Debug.Log("Reload step");
			} else {
				_weaponCurrent.Unload(false);
				// Debug.Log("Unload step");
			}
		}
	}

	void CheckAmmoChangeInput() {
		if (reloadFinished >= _pauseScript.relativeTime) return;
		if (!_getInput.ChangeAmmoType()) return;

		_weaponCurrent.ChangeAmmoType();
// 		if (_consts.InputQuickReloadWeapons) {
// 			// Press change ammo type button once, to both unload then reload.
// 			_weaponCurrent.ChangeAmmoType();
// 		} else {
// 			// First press ammo type button to unload, then again to load.
// 			int wep16index = WeaponFire.Get16WeaponIndexFromConstIndex(_weaponCurrent.weaponIndex);
// 			if (wep16index < 0) return;
// 
// 			int ammoAvailable = 0;
// 			if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
// 				ammoAvailable = _inventory.wepAmmoSecondary[wep16index];
// 			} else {
// 				ammoAvailable = _inventory.wepAmmo[wep16index];
// 			}
// 
// 			if (ammoAvailable <= 0) _weaponCurrent.ChangeAmmoType();
// 			else {
// 				if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
// 									_weaponCurrent.Unload(false);
// 
// 				} else if () {
// 					
// 						_weaponCurrent.Unload(false);
// 				}
// 				} else {
// 					_weaponCurrent.ChangeAmmoType();
// 				}
// 			}
// 		}
	}

	public void FireCyberWeapon() {
		if (cyberWeaponAttackFinished < _pauseScript.relativeTime) {
			if (_inventory.isPulserNotDrill) {
				if (_inventory.hasSoft[1]) {
					// Fire pulser
					_consts.shotsFired++;
					if (_inventory.hasSoft[1]) FireCyberBeachball(true,railgunShotForce,492);
					Utils.PlayUIOneShotSavable(_consts,258); // wpulser
					cyberWeaponAttackFinished = _pauseScript.relativeTime + 0.08f;
				}
			} else {
				if (_inventory.hasSoft[0]) {
					// Fire I.C.E. drill
					_consts.shotsFired++;
					if (_inventory.hasSoft[0]) FireCyberBeachball(false,plasmaShotForce,495);
					Utils.PlayUIOneShotSavable(_consts,241); // wdrill baby drill
					cyberWeaponAttackFinished = _pauseScript.relativeTime + 0.5f;
				}
			}
		}
	}

	void FireCyberBeachball(bool isPulser, float shoveForce, int prefabID) {
        // Create and hurl a beachball-like object.  On the developer commentary they said that the projectiles act
        // like a beachball for collisions with enemies, but act like a baseball for walls/floor to prevent hitting corners
        GameObject beachball = _consoleEmulator.SpawnDynamicObject(prefabID,-1);
        if (beachball != null) {
			damageData.damage = 10f * _inventory.softVersions[0];
			if (isPulser) {
				// Cyberspace enemies don't have much health.
				damageData.damage = 1f + (0.25f * _inventory.softVersions[1]);
			}

            damageData.owner = playerCapsule;
            damageData.attackType = AttackType.ProjectileLaunched;
			if (!isPulser) damageData.attackType = AttackType.Drill;
            beachball.GetComponent<ProjectileEffectImpact>().dd = damageData;
            beachball.GetComponent<ProjectileEffectImpact>().host = playerCapsule;
            beachball.transform.position = playerCamera.transform.position;
			_mouseLookScript.SetCameraFocusPoint();
            tempVec = _mouseLookScript.cameraFocusPoint - playerCamera.transform.position;
            beachball.transform.forward = tempVec.normalized;
            beachball.SetActive(true);
            Vector3 shove = beachball.transform.forward * shoveForce;
            beachball.GetComponent<Rigidbody>().linearVelocity = _consts.vectorZero; // prevent random variation from the last shot's velocity
            beachball.GetComponent<Rigidbody>().AddForce(shove, ForceMode.Impulse);
        }
	}

    // index is used to get recoil down at the bottom and pass along ref for damageData, otherwise the cases use _weaponCurrent.weaponIndex
    void FireWeapon(int index, bool isSilent) {
		_playerHealth.makingNoise = true;
		_playerHealth.noiseFinished = _pauseScript.relativeTime + 0.5f;
		GameObject smoke = null;
        switch (_weaponCurrent.weaponIndex) {
            case 36:
                //Mark3 Assault Rifle
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,251); // wmarksman
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashMK3.SetActive(true);
				smoke = RootInstaller.InstantiatePrefab(muzSmokeMK3,muzFlashMK3.transform.position,_consts.quaternionIdentity) as GameObject;
				smoke.transform.parent = reloadContainer;
				smoke.SetActive(true);
				fogFac += 2;
                break;
            case 37:
                //ER-90 Blaster
				blasterSetting = _weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent];
				//Debug.Log("Blaster fired with energy setting of " + blasterSetting.ToString());
				if (!isSilent) Utils.PlayUIOneShotSavable(_consts,239); // wblaster
				if (DidRayHit(index)) HitScanFire(index);
				muzFlashBlaster.SetActive(true);
                if (overloadEnabled) {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f;
                } else {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] += blasterSetting;
					if (_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] > 100f) _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f; // cap it
                }
                break;
            case 38:
                //SV-23 Dartgun
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,240); // wdartgun
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashDartgun.SetActive(true);
                break;
            case 39:
                //AM-27 Flechette
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,243); // wflechette
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashFlechette.SetActive(true);
				smoke = RootInstaller.InstantiatePrefab(muzSmokeFlechette,muzFlashFlechette.transform.position,_consts.quaternionIdentity) as GameObject;
				smoke.transform.parent = reloadContainer;
				smoke.SetActive(true);
				fogFac += 1;
				break;
            case 40:
                //RW-45 Ion Beam
				ionSetting = _weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent];
				//Debug.Log("Ion rifle fired with energy setting of " + ionSetting.ToString());
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,245); // wion
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashIonBeam.SetActive(true);
                if (overloadEnabled) {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f;
                } else {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] += ionSetting;
					if (_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] > 100f) _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f; // cap it
                }
                break;
            case 41:
                //TS-04 Laser Rapier
                FireRapier(index, isSilent);
                break;
            case 42:
                //Lead Pipe
                FirePipe(index, isSilent);
                break;
            case 43:
                //Magnum 2100
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,249); // wmagnum
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashMagnum.SetActive(true);
				smoke = RootInstaller.InstantiatePrefab(muzSmokeMagnum,muzFlashMagnum.transform.position,_consts.quaternionIdentity) as GameObject;
				smoke.transform.parent = reloadContainer;
				smoke.SetActive(true);
				fogFac += 3;
                break;
            case 44:
                //SB-20 Magpulse
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,250); // wmagpulse
                FireMagpulse(index);
				muzFlashMagpulse.SetActive(true);
                break;
            case 45:
                //ML-41 Pistol
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,255); // wpistol
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashPistol.SetActive(true);
				smoke = RootInstaller.InstantiatePrefab(muzSmokePistol,muzFlashPistol.transform.position,_consts.quaternionIdentity) as GameObject;
				smoke.transform.parent = reloadContainer;
				smoke.SetActive(true);
				fogFac += 1;
                break;
            case 46:
                //LG-XX Plasma Rifle
				plasmaSetting = _weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent];
				//Debug.Log("Plasma rifle fired with energy setting of " + plasmaSetting.ToString());
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,257); // wplasma
                FirePlasma(index);
				muzFlashPlasma.SetActive(true);
                if (overloadEnabled) {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f;
                } else {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] += plasmaSetting;
					if (_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] > 100f) _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f; // cap it
                }
                break;
            case 47:
                //MM-76 Railgun
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,259); // wrailgun
                FireRailgun(index);
				muzFlashRailgun.SetActive(true);
				smoke = RootInstaller.InstantiatePrefab(muzSmokeRailgun,muzFlashRailgun.transform.position,_consts.quaternionIdentity) as GameObject;
				smoke.transform.parent = reloadContainer;
				smoke.SetActive(true);
				fogFac += 2;
                break;
            case 48:
                //DC-05 Riotgun
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,262); // wriotgun
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashRiotgun.SetActive(true);
				smoke = RootInstaller.InstantiatePrefab(muzSmokeRiotgun,muzFlashRiotgun.transform.position,_consts.quaternionIdentity) as GameObject;
				smoke.transform.parent = reloadContainer;
				smoke.SetActive(true);
				fogFac += 4;
                break;
            case 49:
                //RF-07 Skorpion
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,263); // wskorpion
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashSkorpion.SetActive(true);
				smoke = RootInstaller.InstantiatePrefab(muzSmokeSkorpion,muzFlashSkorpion.transform.position,_consts.quaternionIdentity) as GameObject;
				smoke.transform.parent = reloadContainer;
				smoke.SetActive(true);
				fogFac += 2;
                break;
            case 50:
                //Sparq Beam
				sparqSetting = _weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent];
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,264); // wsparq
                if (DidRayHit(index)) HitScanFire(index);
				muzFlashSparq.SetActive(true);
                if (overloadEnabled) {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f;
                } else {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] += sparqSetting;
					if (_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] > 100f) _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f; // cap it
                }
                break;
            case 51:
                //DH-07 Stungun
				stungunSetting = _weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent];
                if (!isSilent) Utils.PlayUIOneShotSavable(_consts,265); // wstungun
                FireStungun(index);
				muzFlashStungun.SetActive(true);
                if (overloadEnabled) {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f;
                } else {
                    _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] += stungunSetting;
					if (_inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] > 100f) _inventory.currentEnergyWeaponHeat[_weaponCurrent.weaponCurrent] = 100f; // cap it
                }
                break;
        }

        // TAKE AMMO
        // no weapons subtract more than 1 at a time in a shot except for energy weapons, subtracting 1
        // Check weapon type before subtracting ammo or energy
        if (index == 5 || index == 6) {
            // Melee don't count towards _consts.shotsFired
            // Pipe or Laser Rapier
            // ammo is already 0, do nothing.  This is here to prevent subtracting ammo on the first slot of .wepAmmo[index] on the last else clause below
        } else {
            // Energy weapons so check energy level
            if (index == 1 || index == 4 || index == 10 || index == 14 || index == 15) {
                if (overloadEnabled) {
                    energoverButton.OverloadFired();
                    if (!_weaponCurrent.bottomless && !_weaponCurrent.redbull) {
						_playerEnergy.TakeEnergy(_consts.energyDrainOverloadForWeapon[index]); //take large amount
						_biomonitorGraphSystem.EnergyPulse(_consts.energyDrainOverloadForWeapon[index]);
					}
                } else {
                    float takeEnerg = (_weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent] / 100f) * (_consts.energyDrainHiForWeapon[index] - _consts.energyDrainLowForWeapon[index]);
                    if (!_weaponCurrent.bottomless && !_weaponCurrent.redbull) {
						_playerEnergy.TakeEnergy(takeEnerg);
						_biomonitorGraphSystem.EnergyPulse(takeEnerg);
					}
                }
            } else {
                if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
                    if (!_weaponCurrent.bottomless) _weaponCurrent.currentMagazineAmount2[_weaponCurrent.weaponCurrent]--; // Take ammo away
                } else {
                    if (!_weaponCurrent.bottomless) _weaponCurrent.currentMagazineAmount[_weaponCurrent.weaponCurrent]--; // Take ammo away
                }
            }
            
            _consts.shotsFired++;
        }

		Recoil(index);
        if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]
			|| overloadEnabled) {

            overloadEnabled = false;
            waitTilNextFire = _pauseScript.relativeTime
							  + _consts.delayBetweenShotsForWeapon2[index];
        } else {
            waitTilNextFire = _pauseScript.relativeTime
							  + _consts.delayBetweenShotsForWeapon[index];
        }

		_inventory.UpdateAmmoText();
    }

    bool DidRayHit(int wep16Index) {
		tempHM = null;
        tempHit = new RaycastHit();
		tempVec = _mouseCursor.GetCursorScreenPointForRay();
		tempVec.x += UnityEngine.Random.Range(-driftForWeapon[wep16Index],
											  driftForWeapon[wep16Index]);

		tempVec.y += UnityEngine.Random.Range(-driftForWeapon[wep16Index],
											  driftForWeapon[wep16Index]);

        if (Physics.Raycast(playerCamera.ScreenPointToRay(tempVec),out tempHit,
							fireDistance,_consts.layerMaskPlayerAttack)) {

			tempHM = Utils.GetMainHealthManager(tempHit);
            return true;
        }

        return false;
    }

	void CreateStandardImpactMarks(int wep16index) {
		// Don't create bullet holes on objects that move
		if (tempHit.collider == null) return;
		if (tempHit.collider.transform.gameObject == null) return;

		GameObject hitGO = tempHit.collider.transform.gameObject;
		if (hitGO.GetComponent<Rigidbody>() != null) return;
		if (hitGO.GetComponent<HealthManager>() != null) return; // don't create bullet holes on objects that die
		if (hitGO.GetComponent<Animator>() != null) return; // don't create bullet holes on objects that animate
		if (hitGO.GetComponent<Animation>() != null) return; // don't create bullet holes on objects that animate
		if (hitGO.GetComponent<Door>() != null) return; // don't create bullet holes on doors, makes them ghost and flicker through walls

		// Add bullethole
		tempVec = tempHit.normal * 0.16f;
		GameObject holetype = _consts.GetPrefab(522);
		switch(wep16index) {
			case 0:  holetype = _consts.GetPrefab(518); break;
			case 1:  holetype = _consts.GetPrefab(520); break;
			case 2:  holetype = _consts.GetPrefab(522); break;
			case 3:  holetype = _consts.GetPrefab(521); break;
			case 4:  holetype = _consts.GetPrefab(519); break;
			case 5:  holetype = _consts.GetPrefab(520); break;
			case 6:  holetype = _consts.GetPrefab(522); break;
			case 7:  holetype = _consts.GetPrefab(518); break;
			case 8:  holetype = _consts.GetPrefab(519); break;
			case 9:  holetype = _consts.GetPrefab(521); break;
			case 10: holetype = _consts.GetPrefab(519); break;
			case 11: holetype = _consts.GetPrefab(519); break;
			case 12: holetype = _consts.GetPrefab(523); break;
			case 13: holetype = _consts.GetPrefab(518); break;
			case 14: holetype = _consts.GetPrefab(520); break;
			case 15: holetype = _consts.GetPrefab(520); break;
		}

		GameObject impactMark = RootInstaller.InstantiatePrefab(holetype,
			(tempHit.point + tempVec),
			Quaternion.LookRotation(tempHit.normal*-1,Vector3.up),
			hitGO.transform);

		Quaternion roll = impactMark.transform.localRotation;
		roll *= Quaternion.Euler(0f,0f,Random.Range(0,3) * 90f);
		impactMark.transform.localRotation = roll;
		GameObject dynamicObjectsContainer = _levelManager.GetCurrentDynamicContainer();
		impactMark.transform.parent = dynamicObjectsContainer.transform;
	}

    void CreateStandardImpactEffects() {
        // Determine blood type of hit target and spawn corresponding blood particle effect from the Const.Pool
        if (tempHM != null) {
            GameObject impact = _consts.GetImpactType(tempHM);
            if (impact != null) {
                tempVec = tempHit.normal * hitOffset;
				impact.transform.SetPositionAndRotation(tempHit.point + tempVec,Quaternion.FromToRotation(Vector3.up, tempHit.normal));
                impact.SetActive(true);
            }
        } else {
            // Allow for skipping adding sparks after special override impact effects per attack functions below
			GameObject impact = _consts.GetObjectFromPool(PoolType.SparksSmall); //Didn't hit an object with a HealthManager script, use sparks
			if (impact != null) {
				tempVec = tempHit.normal * hitOffset;
				impact.transform.SetPositionAndRotation(tempHit.point + tempVec,Quaternion.FromToRotation(Vector3.up, tempHit.normal));
				impact.SetActive(true);
			}
        }
    }

    void CreateBeamImpactEffects(int wep16index) {
		int impactConstdex = 731; // Cyan for sparqbeam
		if (wep16index == 1) {
			impactConstdex = 739;  //Red laser for blaster
        } else if (wep16index == 4) {
			impactConstdex = 740; // Yellow laser for ion
        }

        GameObject impact = _consoleEmulator.SpawnDynamicObject(impactConstdex);
		impact.transform.SetPositionAndRotation(tempHit.point,Quaternion.FromToRotation(Vector3.up, tempHit.normal));
		impact.SetActive(true);
    }

    void CreateBeamEffects(int wep16index) {
        int laserIndex = 405; // Turquoise/Pale-Teal for sparq
        if (wep16index == 1) laserIndex = 406;  //Red laser for blaster
        else  if (wep16index == 4) laserIndex = 407; // Yellow laser for ion

		GameObject dynamicObjectsContainer = _levelManager.GetCurrentDynamicContainer();
		GameObject lasertracer = RootInstaller.InstantiatePrefab(_consts.GetPrefab(laserIndex),transform.position,_consts.quaternionIdentity) as GameObject;

		// Temporary object only, no need to save or mark as instantiated.
		if (lasertracer != null) {
			lasertracer.transform.SetParent(dynamicObjectsContainer.transform,true);
			tempVec = transform.position;
			tempVec.y += verticalOffset;
			lasertracer.GetComponent<LaserDrawing>().startPoint = tempVec;
			lasertracer.GetComponent<LaserDrawing>().endPoint = tempHit.point;
			lasertracer.SetActive(true);
		}
    }

	// dmg_min is _consts.damagePerHitForWeapon[wep16Index], dmg_max is _consts.damagePerHitForWeapon2[wep16Index]
	float DamageForPower(int wep16Index) {
	    float retval, dmg_min, dmg_max, ener_min, ener_max;

        // overload overrides current setting and uses overload damage
        if (overloadEnabled) {
            retval = _consts.damageOverloadForWeapon[wep16Index];
            return retval;
        }

		dmg_min = _consts.damagePerHitForWeapon[wep16Index];
		dmg_max = _consts.damagePerHitForWeapon2[wep16Index];
        ener_min = _consts.energyDrainLowForWeapon[wep16Index];
        ener_max = _consts.energyDrainHiForWeapon[wep16Index];
		// Calculates damage based on min and max values and applies a curve of the slopes based on the linear plotting of the slope from min at min to max at max...that makes sense right?
		// Right then, the beautifully ugly formula:
		retval = ((_weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent]/100f)*((dmg_max/ener_max)-(dmg_min/ener_min)) + 3f) * (((_weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent])/100f)*(ener_max-ener_min) + ener_min);
		//Debug.Log("returning DamageForPower of " + retval.ToString() + ", for wep16Index of " + wep16Index.ToString());
		return retval;
		// You gotta love maths!  There is a spreadsheet for this (.ods LibreOffice file format, found with src code) that shows the calculations to make this dmg curve. 
	}

	// TargetID Instance
	public void CreateTargetIDInstance(float dmgFinal, HealthManager hm, float tranq) {
		 if (hm == null || !hm.isNPC || hm.health <= 0f) return;
		if (!_inventory.hasHardware[4] && tranq <= 0f && dmgFinal > 0f) return;
		if (hm.linkedTargetID != null) return; // Let SendDamageReceive handle updates

		float linkDistForTargID = TargetID.GetTargetIDTetherRange(_inventory);
		bool showHealth = _inventory.hasHardware[4] && _inventory.hardwareVersion[4] > 2;
		bool showRange = _inventory.hasHardware[4];
		bool showAttitude = _inventory.hasHardware[4] && _inventory.hardwareVersion[4] > 1;
		bool showName = _inventory.hasHardware[4] && _inventory.hardwareVersion[4] > 1;

		GameObject idFrame = RootInstaller.InstantiatePrefab(_consts.GetPrefab(736), hm.transform.position, _consts.quaternionIdentity) as GameObject;
		if (idFrame == null) return;

		TargetID tid = idFrame.GetComponent<TargetID>();
		if (tid == null) return;

		tid.parent = hm.transform;
		tid.linkedHM = hm;
		hm.linkedTargetID = tid;

		if (!_inventory.hasHardware[4] || tranq > 0f || dmgFinal == 0f) {
			tid.currentText = tranq > 0f ? _consts.stringTable[536] : (dmgFinal == 0f ? _consts.stringTable[511] : "");
			tid.lifetime += tranq;
			tid.damageTimeFinished = Mathf.Max(_pauseScript.relativeTime + tranq,tid.damageTimeFinished + tranq);
			tid.lifetimeFinished = _pauseScript.relativeTime + tid.lifetime;
		} else {
			tid.currentText = ""; // Set by SendDamageReceive
			tid.lifetime = 9999999f;
			tid.lifetimeFinished = _pauseScript.relativeTime + tid.lifetime;
			tid.damageTime = 2.5f;
			if (tranq > 2.5f) tid.damageTime = tranq;
			tid.damageTimeFinished = _pauseScript.relativeTime + tid.damageTime;
		}

		// Center on what we just shot
		float yOfs = 0f;
		float xSize = 1.2f;
		float ySize = 2f;
		float textname_Ofs = 1.28f; // e.g. HOPPER5
		if (hm.aic != null) {
			switch(hm.aic.index) {
				case 0: yOfs = 0.5f; ySize = 1.0f; break; // Autobomb
				case 1: /* GOOD */ break; // Cyborg Assassin
				case 2: yOfs = -0.1f; ySize = 1.4f; xSize = 1.4f; textname_Ofs = 0.76f; break; // Avian Mutant
				case 3: /* GOOD */ break; // Exec-Bot
				case 4: /* GOOD */ break; // Cyborg Drone
				case 5: yOfs = 0.15f; ySize = 3f; xSize = 2.8f; textname_Ofs = 2.14f; break; // Cortex Reaver
				case 6: /* GOOD */ break; // Cyborg Warrior
				case 7: /* GOOD */ break; // Cyborg Enforcer
				case 8: yOfs = 0.05f; ySize = 2.3f; textname_Ofs = 1.48f;  break; // Cyborg Elite Guard
				case 9: ySize = 2.3f; textname_Ofs = 1.36f; break; // Cyborg of Edward Diego
				case 10: yOfs = -0.1f; ySize = 1.8f; xSize = 1.4f; textname_Ofs = 0.92f; break; // Sec-1 Bot
				case 11: yOfs = 0.04f; ySize = 2.4f; xSize = 2.4f; textname_Ofs = 1.51f; break; // Sec-2 Bot
				case 12: yOfs = -0.2f; ySize = 1.6f; xSize = 1.6f; textname_Ofs = 0.68f; break; // Maintenance Robot
				case 13: yOfs = 0.05f; ySize = 2.5f; xSize = 1.6f; textname_Ofs = 1.58f; break; // Mutant Cyborg
				case 14: yOfs = 0.5f; ySize = 2.3f; textname_Ofs = 2.5f; break; // Hopper
				case 15: /* GOOD */ break; // Humanoid Mutant
				case 16: yOfs = 0.12f; ySize = 0.8f; xSize = 1.8f; textname_Ofs = 0.7f; break; // Invisible Mutant
				case 17: /* GOOD */ break; // Virus Mutant
				case 18: yOfs = -0.22f; ySize = 1.5f; textname_Ofs = 0.63f; break; // Servbot
				case 19: yOfs = 0.2f; ySize = 1f; xSize = 1.55f; textname_Ofs = 0.92f;  break; // Flier Bot
				case 20: ySize = 1.1f;  xSize = 1.1f; textname_Ofs = 0.74f; break; // Zero-G Mutant
				case 21: yOfs = -0.25f; ySize = 1.5f; textname_Ofs = 0.53f; xSize = 2f; break; // Gorilla Tiger Mutant
				case 22: yOfs = -0.7f; ySize = 1f; textname_Ofs = 0f; break; // Repair Bot
				case 23: yOfs = -0.24f; ySize = 1.4f; textname_Ofs = 0.58f; break; // Plant Mutant
			}
		}

		// Set after setting textname_Ofs so all 3 can adapt to size.
		float textdmg_Ofs = textname_Ofs - 0.18f; // def: 1.1f, e.g. MINOR
		float textnum_Ofs = textname_Ofs - 0.09f; // def: 1.19f, e.g. 3.8M Idle

		idFrame.transform.position = hm.transform.position;
		idFrame.SetActive(true);
		tid.linkedHM = hm;
		hm.linkedTargetID = tid;
		tid.partSys.Play();

		//tid.partSys.
		ParticleSystemRenderer rd =
			tid.partSys.GetComponent<ParticleSystemRenderer>();

		rd.pivot = new Vector3(0f,yOfs,0f);

		ParticleSystem.MainModule pm = tid.partSys.main;
		pm.startSizeX = xSize;
		pm.startSizeY = ySize;
		pm.startSizeZ = xSize;

		RectTransform rt = tid.nameText.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(0f,textname_Ofs);

		RectTransform rtnums = tid.secondaryText.GetComponent<RectTransform>();
		rtnums.anchoredPosition = new Vector2(0f,textnum_Ofs);

		RectTransform rtdmg = tid.text.GetComponent<RectTransform>();
		rtdmg.anchoredPosition = new Vector2(0f,textdmg_Ofs);

		tid.playerCapsuleTransform = playerCapsule.transform;
		tid.playerLinkDistance = linkDistForTargID;
		tid.displayRange = showRange;
		tid.displayHealth = showHealth;
		tid.displayAttitude = showAttitude;
		tid.displayName = showName;
		tid.linkedHM.aic.hasTargetIDAttached = true;
	}

    // WEAPON FIRING CODE:
    // ==============================================================================================================================
    // Hitscan Weapons
    //----------------------------------------------------------------------------------------------------------
    // Guns and laser beams, used by most weapons
    void HitScanFire(int wep16Index) {
        damageData.other = tempHit.transform.gameObject;
		tempHM = Utils.GetMainHealthManager(tempHit);
		if (tempHM != null) {
			if (damageData.other != tempHM.gameObject) {
				damageData.other = tempHM.gameObject;
			}
		}

        if (wep16Index == 1 || wep16Index == 4 || wep16Index == 14) {
            CreateBeamImpactEffects(wep16Index); // laser burst effect overrides standard blood spurts/robot sparks
        } else {
            CreateStandardImpactEffects(); // standard blood spurts/robot sparks

			// the only exception
			if (wep16Index == 2 && _inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
				damageData.attackType = AttackType.Tranq; // tranquilize the untranquil....yes
			}
        }

        // Fill the damageData container
		// -------------------------------
		// Using tempHit.transform instead of tempHit.collider.transform to ensure we get overall NPC parent instead of its children.
		float tranq = -1f;
        if (damageData.other.CompareTag("NPC")) {
            damageData.isOtherNPC = true;
			if (damageData.attackType == AttackType.Tranq) {
				// Using tempHit.transform instead of tempHit.collider.transform to ensure we get overall NPC parent instead of its children.
				AIController taic = damageData.other.GetComponent<AIController>();
				if (taic !=null) {
					tranq = taic.Tranquilize(3f + Random.Range(0f,4f),false);
				}
			}
        } else {
            damageData.isOtherNPC = false;
			if (damageData.other.CompareTag("Geometry")) {
				CreateStandardImpactMarks(wep16Index);
			}
        }
        damageData.hit = tempHit;
		damageData.attacknormal = _mouseCursor.GetCursorScreenPointForRay();
        damageData.attacknormal = playerCamera.ScreenPointToRay(damageData.attacknormal).direction;
        if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
            damageData.damage = _consts.damagePerHitForWeapon2[wep16Index];
			damageData.offense = _consts.offenseForWeapon2[wep16Index];
			damageData.penetration = _consts.penetrationForWeapon2[wep16Index];
        } else {
			if (CurrentWeaponUsesEnergy()) {
                damageData.damage = DamageForPower(wep16Index);
			} else {
				damageData.damage = _consts.damagePerHitForWeapon[wep16Index];
			}
			damageData.offense = _consts.offenseForWeapon[wep16Index];
			damageData.penetration = _consts.penetrationForWeapon[wep16Index];
        }
        
		if (damageData.attackType != AttackType.Tranq) damageData.attackType = _consts.attackTypeForWeapon[wep16Index]; // If check to handle exception setting it above
        damageData.damage = DamageData.GetDamageTakeAmount(damageData);
        damageData.owner = playerCapsule;
		damageData.impactVelocity = 80f;
		if (wep16Index == 12) {
			damageData.impactVelocity = 120f;
			if (tempHM != null) { // babamm boxes be like, u ded
				if (tempHM.isObject) damageData.damage *= 10f;
			}
		}

		float dmgFinal = 0f;
		GameObject hitGO = tempHit.collider.transform.gameObject;
        if (tempHM != null && tempHM.health > 0) {
			damageData.damage *= 0.8f; // Bit of heavy handed rebalancing lol.
			dmgFinal = tempHM.TakeDamage(damageData); // send the damageData container to HealthManager of hit object and apply damage
			damageData.impactVelocity += damageData.damage;
			if (!damageData.isOtherNPC || wep16Index == 12) {
				Utils.ApplyImpactForce(hitGO,damageData.impactVelocity,
									   damageData.attacknormal,
									   damageData.hit.point);
			}
			if (tempHM.isNPC && !tempHM.aic.asleep) _music.inCombat = true;
		}

		if (dmgFinal < 0f) dmgFinal = 0f; // Less would = blank.
		CreateTargetIDInstance(dmgFinal,tempHM,tranq);

		UseableObjectUse uou = hitGO.GetComponent<UseableObjectUse>();
		if (uou != null) uou.HitForce(damageData); // knock objects around

        // Draw a laser beam for beam weapons
        if (wep16Index == 1 || wep16Index == 4 || wep16Index == 14) {
			CreateBeamEffects(wep16Index);
		}
    }

    // Melee weapons
    //-------------------------------------------------------------------------
    // Rapier and pipe.  Need extra code to handle anims for view model and
	// sound for swing-and-a-miss! vs. hit
	IEnumerator ApplyMeleeHit(int index16, GameObject targ, bool isRapier, 
							  bool silent,AudioClip hit, AudioClip miss,
							  AudioClip hitflesh) {
		if (targ.layer == gameObject.layer) yield break;

		if (isRapier) yield return new WaitForSeconds(0.28f);
		else yield return new WaitForSeconds(0.15f);

		damageData.other = targ;
		damageData.isOtherNPC = false;
		if (targ.CompareTag("NPC")) damageData.isOtherNPC = true;
		damageData.attacknormal = _mouseCursor.GetCursorScreenPointForRay();
		damageData.attacknormal =
			playerCamera.ScreenPointToRay(damageData.attacknormal).direction;
		damageData.damage = _consts.damagePerHitForWeapon[index16]; 
		damageData.damage = DamageData.GetDamageTakeAmount(damageData);
		damageData.offense = _consts.offenseForWeapon[index16];
		damageData.penetration = _consts.penetrationForWeapon[index16];
		damageData.owner = playerCapsule;
		if (isRapier) {
			damageData.attackType = AttackType.MeleeEnergy;
			if (_playerEnergy.energy < 4f) {
				// Half pipe
				damageData.damage = _consts.damagePerHitForWeapon[6] / 2f;
			}
		} else {
			damageData.attackType = AttackType.Melee;
		}

		UseableObjectUse uou = targ.GetComponent<UseableObjectUse>();
		if (uou != null) uou.HitForce(damageData); // knock objects around
		tempHM = Utils.GetMainHealthManager(targ);
		if (tempHM != null) {
			if (damageData.other != tempHM.gameObject) {
				damageData.other = tempHM.gameObject;
			}
		}

		CreateStandardImpactEffects();
		if (damageData.other.CompareTag("Geometry")) {
			CreateStandardImpactMarks(index16);
		}

		if (tempHM == null) {
			if (!silent) {
				PrefabIdentifier prefID = targ.GetComponent<PrefabIdentifier>();
				if (prefID == null) {
					if (targ.transform.parent != null) {
						prefID = targ.transform.parent.gameObject.GetComponent<PrefabIdentifier>();
					}
				}
				
				if (prefID != null && !isRapier) {
					FootStepType fstep = _playerMovement.GetFootstepTypeForPrefab(prefID.constIndex);
					AudioClip stcp = _playerMovement.JumpLandSound(fstep);
					Utils.PlayTempAudio(_consts,transform.position,stcp,1f);
					Utils.PlayTempAudio(_consts,transform.position,hit,0.65f);	
				} else {
					Utils.PlayTempAudio(_consts,transform.position,hit,1f);	
				}

				_playerHealth.makingNoise = true;
				_playerHealth.noiseFinished = _pauseScript.relativeTime+0.5f;
			}
			yield break;
		}

		damageData.impactVelocity = 80f + damageData.damage;
		if (!damageData.isOtherNPC || index16 == 12) {
			if (!isRapier || (isRapier && _playerEnergy.energy >= 4f)) {
				Utils.ApplyImpactForce(targ, damageData.impactVelocity,
					damageData.attacknormal,damageData.hit.point);
			}
		}

		float dmgFinal = tempHM.TakeDamage(damageData);
		if (dmgFinal < 0f) dmgFinal = 0f; // Less would = blank.
		CreateTargetIDInstance(dmgFinal,tempHM,-1f);
		if (tempHM.isNPC && !tempHM.aic.asleep) _music.inCombat = true;
		if (!silent) {
			_playerHealth.makingNoise = true;
			_playerHealth.noiseFinished = _pauseScript.relativeTime + 0.5f;
			if ((tempHM.bloodType == BloodType.Red)
				|| (tempHM.bloodType == BloodType.Yellow)
				|| (tempHM.bloodType == BloodType.Green)) {
				Utils.PlayUIOneShotSavable(_mfdManager,hitflesh);
			} else if (isRapier && _playerEnergy.energy < 4f) {
				Utils.PlayUIOneShotSavable(_consts,67);
			} else {
				Utils.PlayUIOneShotSavable(_mfdManager,hit);
			}
		}

		if (isRapier) {
			_playerEnergy.TakeEnergy(3.666f); // 3 hits per tick.
			_biomonitorGraphSystem.EnergyPulse(3.666f);
		}
	}

	// These are a bit silly.
    void FireRapier(int i16, bool sil) {
		FireMelee(i16,true,sil,_consts.sounds[246],_consts.sounds[247],_consts.sounds[246],true); // wlaserrapier_hit, wlaserrapier_swing, wlaserrapier_hit
	}

    void FirePipe(int i16, bool sil) {
		FireMelee(i16,false,sil,_consts.sounds[253],_consts.sounds[254],_consts.sounds[252],false); // wpipe_hit, wpipe_swing, wpipe_dmg
	}

	void FireMelee(int index16, bool isRapier, bool silent, AudioClip hit,
				   AudioClip miss,AudioClip hitflesh, bool rapier) {
		// Do normal straightline raytrace at center first.
		fireDistance = meleescanDistance;
		if (DidRayHit(index16)) {
			fireDistance = hitscanDistance; // Reset before any returns.
			if (rapier) {
				if (rapieranim != null) {
					rapieranim.Play("Attack2");
					//rapieranim.Play("Attack2",-1,float.NegativeInfinity);
				}
			} else {
				if (anim != null) {
					anim.Play("Attack2");
					//anim.Play("Attack2",-1,float.NegativeInfinity);
				}
			}

			GameObject hitGO = tempHit.collider.transform.gameObject;
			StartCoroutine(ApplyMeleeHit(index16,hitGO,isRapier,silent,hit,
										 miss,hitflesh));
			return;
		}

		fireDistance = hitscanDistance; // Reset since raycast failed.

		// Check all objects we can hurt have HealthManager, that they are in
		// meleescanDistance range, that they are within player facing angle by
		// 60° (±30°)	
		for (int i=0;i<_consts.healthObjectsRegistration.Length;i++) {
			if (_consts.healthObjectsRegistration[i] == null) continue;

			HealthManager hm = _consts.healthObjectsRegistration[i];
			// Don't hurt deactive objects, like you know, corpse on
			// living entities...at least don't do it again please.
			if (hm == null) continue;
			if (!hm.gameObject.activeInHierarchy) continue;

			if (Vector3.Distance(hm.transform.position,
								 playerCapsule.transform.position)
				>= meleescanDistance) {

				continue;
			}

			_mouseLookScript.SetCameraFocusPoint();
			tempVec = _mouseLookScript.cameraFocusPoint
						- playerCamera.transform.position;

			tempVec = tempVec.normalized;
			Vector3 ang = hm.transform.position
							- playerCamera.transform.position;

			ang = ang.normalized;
			float dot = Vector3.Dot(tempVec,ang);
			if (dot <= 0.666f) continue;

			if (rapier) {
				if (rapieranim != null) rapieranim.Play("Attack2");
			} else {
				if (anim != null) anim.Play("Attack2");
			}

			StartCoroutine(ApplyMeleeHit(index16,hm.gameObject,isRapier,silent,
										 hit,miss,hitflesh));
			return;
		}

		// Swing and a miss, steeeerike!!
		if (!silent) Utils.PlayUIOneShotSavable(_mfdManager,miss);
		if (rapier) {
			if (rapieranim != null) rapieranim.Play("Attack2");
		} else {
			if (anim != null) anim.Play("Attack1");
		}
	}

    // Projectile weapons
    //-------------------------------------------------------------------------
    void FirePlasma(int index16) { FireBeachball(index16,plasmaShotForce,485); }
    void FireRailgun(int index16) { FireBeachball(index16,railgunShotForce,484); }
    void FireMagpulse(int index16) { FireBeachball(index16,magpulseShotForce,482); }
    void FireStungun(int index16) { FireBeachball(index16,stungunShotForce,483); }

	void FireBeachball(int index16, float shoveForce, int prefabID) {
        // Create and hurl a beachball-like object.  On the developer
		// commentary they said that the projectiles act like a beachball for
		// collisions with enemies, but act like a baseball for walls/floor to
		// prevent hitting corners.
        GameObject beachball = _consoleEmulator.SpawnDynamicObject(prefabID,1);
        if (beachball != null) {
			if (CurrentWeaponUsesEnergy()) {
                damageData.damage = DamageForPower(index16);
			} else {
				damageData.damage = _consts.damagePerHitForWeapon[index16];
			}
            damageData.owner = playerCapsule;
            damageData.attackType = _consts.attackTypeForWeapon[index16];
			damageData.offense = _consts.offenseForWeapon[index16];
			damageData.penetration = _consts.penetrationForWeapon[index16];
            beachball.GetComponent<ProjectileEffectImpact>().dd = damageData;
            beachball.GetComponent<ProjectileEffectImpact>().host = playerCapsule;
            beachball.transform.position = playerCamera.transform.position;
			_mouseLookScript.SetCameraFocusPoint();
            tempVec = _mouseLookScript.cameraFocusPoint - playerCamera.transform.position;
            beachball.transform.forward = tempVec.normalized;
            beachball.SetActive(true);
            Vector3 shove = beachball.transform.forward * shoveForce;

			// Force starting with zero pior to adding impulse force.
            beachball.GetComponent<Rigidbody>().linearVelocity = _consts.vectorZero;
            beachball.GetComponent<Rigidbody>().AddForce(shove,ForceMode.Impulse);
        }
	}

    public Vector3 ScreenPointToDirectionVector() {
        Vector3 retval = _consts.vectorZero;
        retval = playerCamera.transform.forward;
        return retval;
    }

	public static string Save(GameObject go) {
		WeaponFire wf = go.GetComponent<WeaponFire>();
		var pauseScript = wf._pauseScript;
		s1.Clear();
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,wf.waitTilNextFire,"waitTilNextFire"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(wf.overloadEnabled,"overloadEnabled"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(wf.sparqSetting,"sparqSetting"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(wf.ionSetting,"ionSetting"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(wf.blasterSetting,"blasterSetting"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(wf.plasmaSetting,"plasmaSetting"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(wf.stungunSetting,"stungunSetting"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(wf.recoiling,"recoiling"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(wf.reloadLerpValue,"reloadLerpValue"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,wf.reloadFinished,"reloadFinished"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,wf.lerpStartTime,"lerpStartTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,wf.justFired,"justFired"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,wf.energySliderClickedTime,"energySliderClickedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,wf.cyberWeaponAttackFinished,"cyberWeaponAttackFinished"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveTransform(wf.reloadContainer.transform));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(wf.targetY,"targetY"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		WeaponFire wf = go.GetComponent<WeaponFire>();
		var pauseScript = wf._pauseScript;
		wf.waitTilNextFire = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"waitTilNextFire"); index++;
		wf.overloadEnabled = Utils.GetBoolFromString(entries[index],"overloadEnabled"); index++;
		wf.sparqSetting = Utils.GetFloatFromString(entries[index],"sparqSetting"); index++;
		wf.ionSetting = Utils.GetFloatFromString(entries[index],"ionSetting"); index++;
		wf.blasterSetting = Utils.GetFloatFromString(entries[index],"blasterSetting"); index++;
		wf.plasmaSetting = Utils.GetFloatFromString(entries[index],"plasmaSetting"); index++;
		wf.stungunSetting = Utils.GetFloatFromString(entries[index],"stungunSetting"); index++;
		wf.recoiling = Utils.GetBoolFromString(entries[index],"recoiling"); index++;
		wf.reloadLerpValue = Utils.GetFloatFromString(entries[index],"reloadLerpValue"); index++;
		wf.reloadFinished = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"reloadFinished"); index++;
		wf.lerpStartTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"lerpStartTime"); index++;
		wf.justFired = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"justFired"); index++;
		wf.energySliderClickedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"energySliderClickedTime"); index++;
		wf.cyberWeaponAttackFinished = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"cyberWeaponAttackFinished"); index++;
		index = Utils.LoadTransform(wf.reloadContainer.transform,ref entries,index);
		wf.targetY = Utils.GetFloatFromString(entries[index],"targetY"); index++;
		return index;
	}
}
