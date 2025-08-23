using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using Citadel.Game;
using Zenject;

public class PlayerPatch : MonoBehaviour,IInitializer {
	public GameObject playerCamera;
	public HealthManager hm;
	public Texture2D b1;
	public Texture2D b2;
	public Texture2D b3;
	public Texture2D b4;
	public Texture2D b5;
	public Texture2D b6;
	public Texture2D b7;
	public Light sightLight;
	public Image sightDimming;
	public PuzzleWire wirePuzzle;
	public BerserkEffect berserk;
	public BerserkEffect sensaroundCamCenterBerserk;
	public BerserkEffect sensaroundCamLeftBerserk;
	public BerserkEffect sensaroundCamRightBerserk;

	[HideInInspector] public float berserkFinishedTime; // save
	[HideInInspector] public float berserkIncrementFinishedTime; // save
	[HideInInspector] public float detoxFinishedTime; // save
	[HideInInspector] public float geniusFinishedTime; // save
	[HideInInspector] public float mediFinishedTime; // save
	[HideInInspector] public float reflexFinishedTime; // save
	[HideInInspector] public float sightFinishedTime; // save
	[HideInInspector] public float sightSideEffectFinishedTime; // save
	[HideInInspector] public float staminupFinishedTime; // save
	[HideInInspector] public int berserkIncrement; // save
	[HideInInspector] public const int PATCH_BERSERK = 1;
	[HideInInspector] public const int PATCH_DETOX = 2;
	[HideInInspector] public const int PATCH_GENIUS = 4;
	[HideInInspector] public const int PATCH_MEDI = 8;
	[HideInInspector] public const int PATCH_REFLEX = 16;
	[HideInInspector] public const int PATCH_SIGHT = 32;
	[HideInInspector] public const int PATCH_STAMINUP = 64;
	[HideInInspector] public int patchActive;  // bitflag carrier for active patches // save
	private static StringBuilder s1 = new StringBuilder(100);

	[Inject] private Const _consts;
	[Inject] private GUIState _guiState;
	[Inject] private Inventory _inventory;
	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerHealth _playerHealth;
	[Inject] private PlayerMovement _playerMovement;

	// Patches stack so multiple can be used at once
	// For instance, berserk + staminup + medi = 1 + 64 + 8 = 73
	// This is turning on bits in the int patchActive so above would be: 01001001,
	// meaning 3 patches are enabled out of the 7 types (short integer has 8 bits
	// but the 7th bit can be used for sign +/-)

	public void Initialize () {
		mediFinishedTime = -1f;
		reflexFinishedTime = -1f;
		sightFinishedTime = -1f;
		sightLight.enabled = false;
		BerserkDisable();
	}

	public void ActivatePatch(int index) { // Expects the usableItems index
		bool depleted = false;
		switch (index) {
		case 14:
			// Berserk Patch
			_inventory.patchCounts[2]--;
			if (_inventory.patchCounts[2] <= 0) { depleted = true; }
			if (!(Utils.CheckFlags(patchActive, PATCH_BERSERK))) patchActive += PATCH_BERSERK;
			berserkFinishedTime = _pauseScript.relativeTime + Const.berserkTime;
			float berserkIncrementTime = Const.berserkTime/5f;
			if (berserkIncrementFinishedTime > _pauseScript.relativeTime) {
				berserkIncrementFinishedTime += berserkIncrementTime; // berserk effect stacks
			} else {
				berserkIncrementFinishedTime = _pauseScript.relativeTime + berserkIncrementTime;
			}
			break;
		case 15:
			// Detox Patch
			_inventory.patchCounts[6]--;
			if (_inventory.patchCounts[6] <= 0) { depleted = true; }
			DisableAllPatches(); // remove all other effects, even medipatch
			patchActive = PATCH_DETOX; // overwrite all other active patches
			detoxFinishedTime = _pauseScript.relativeTime + Const.detoxTime; // detox doesn't stack, it cancels itself lol
			break;
		case 16:
			// Genius Patch
			_inventory.patchCounts[5]--;
			if (_inventory.patchCounts[5] <= 0) { depleted = true; }
			if (!(Utils.CheckFlags(patchActive, PATCH_GENIUS))) patchActive += PATCH_GENIUS;
			if (geniusFinishedTime > _pauseScript.relativeTime) {
				geniusFinishedTime += Const.geniusTime; // genius effect stacks
			} else {
				geniusFinishedTime = _pauseScript.relativeTime + Const.geniusTime;
			}
			break;
		case 17:
			// Medi Patch
			if (hm.health >=255) {
				_consts.sprint(_consts.stringTable[304],_mouseLookScript.player);
				return;
			}
			_inventory.patchCounts[3]--;
			if (_inventory.patchCounts[3] <= 0) { depleted = true; }
			if (!(Utils.CheckFlags(patchActive, PATCH_MEDI))) patchActive += PATCH_MEDI;
			_playerHealth.mediPatchPulseCount = 0;
			if (mediFinishedTime > _pauseScript.relativeTime) {
				mediFinishedTime += Const.mediTime; // medipatch effect stacks
			} else {
				mediFinishedTime = _pauseScript.relativeTime + Const.mediTime;
			}
			break;
		case 18:
			// Reflex Patch
			_inventory.patchCounts[4]--;
			if (_inventory.patchCounts[4] <= 0) { depleted = true; }
			Time.timeScale = Const.reflexTimeScale;
			if (!(Utils.CheckFlags(patchActive, PATCH_REFLEX))) patchActive += PATCH_REFLEX;
			if (reflexFinishedTime > Time.realtimeSinceStartup ) {
				reflexFinishedTime += Const.reflexTime; // reflex effect stacks
			} else {
				reflexFinishedTime = Time.realtimeSinceStartup + Const.reflexTime;
			}
			break;
		case 19:
			// Sight Patch
			_inventory.patchCounts[1]--;
			if (_inventory.patchCounts[1] <= 0) { depleted = true; }
			sightLight.enabled = true; // enable vision enhancement
			sightSideEffectFinishedTime = -1f;  // reset side effect timer from previous patch
			sightDimming.enabled = false; // deactivate side effect from previous patch
			if (!(Utils.CheckFlags(patchActive, PATCH_SIGHT))) patchActive += PATCH_SIGHT;
			if (sightFinishedTime > _pauseScript.relativeTime) {
				sightFinishedTime += Const.sightTime; // sight effect stacks
			} else {
				sightFinishedTime = _pauseScript.relativeTime + Const.sightTime;
			}
			break;
		case 20:
			// Staminup Patch
			_inventory.patchCounts[0]--;
			if (_inventory.patchCounts[0] <= 0) depleted = true;
			_playerMovement.staminupActive = true;
			if (!(Utils.CheckFlags(patchActive, PATCH_STAMINUP))) patchActive += PATCH_STAMINUP;
			if (staminupFinishedTime > _pauseScript.relativeTime) {
				staminupFinishedTime += Const.staminupTime; // staminup effect stacks
			} else {
				staminupFinishedTime = _pauseScript.relativeTime + Const.staminupTime;
			}

			break;
		}

		if (depleted) {
			_inventory.PatchCycleDown(false);
			_consts.sprint((_consts.stringTable[590]
						 + _consts.stringTable[index + 326]
						 + _consts.stringTable[589]),_mouseLookScript.player);
		} else {
			_consts.sprint((_consts.stringTable[index + 326]
						 + _consts.stringTable[589]),_mouseLookScript.player);
		}

		Utils.PlayUIOneShotSavable(_consts,89);
		_guiState.ClearOverButton();
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			// ================================== DETOX PATCH =========================
			if (Utils.CheckFlags(patchActive, PATCH_DETOX)) {
				// ---Disable Patch---
				if (detoxFinishedTime < _pauseScript.relativeTime) {
					patchActive -= PATCH_DETOX; // Back to full force radiation effects, if present.  All normal.
				} else {
					// ***Patch Effect***
					patchActive = PATCH_DETOX; // Lets health script know to ameliorate the effects of radiation.
				}
			}

			// ================================== MEDI PATCH =========================
			if (Utils.CheckFlags(patchActive, PATCH_MEDI)) {
				// ---Disable Patch---
				if (mediFinishedTime < _pauseScript.relativeTime && mediFinishedTime != -1) {
					patchActive -= PATCH_MEDI;
					mediFinishedTime = -1;
				}
			}

			// ================================== REFLEX PATCH =======================
			if (Utils.CheckFlags(patchActive, PATCH_REFLEX)) {
				// ---Disable Patch---
				if (reflexFinishedTime < Time.realtimeSinceStartup && reflexFinishedTime != -1) {
					patchActive -= PATCH_REFLEX;
					Time.timeScale = Const.defaultTimeScale;
					reflexFinishedTime = -1;
				} else {
					// ***Patch Effect***
					if (Time.timeScale != Const.reflexTimeScale) {
						Time.timeScale = Const.reflexTimeScale;
					}
				}
			} else {
			    if (Time.timeScale != Const.defaultTimeScale) {
					Time.timeScale = Const.defaultTimeScale;
				} 
			}

			// ================================== BERSERK PATCH =======================
			if (Utils.CheckFlags(patchActive, PATCH_BERSERK)) {
				// ---Disable Patch---
				if (berserkFinishedTime < _pauseScript.relativeTime) {
					berserkIncrement = 0;
					patchActive -= PATCH_BERSERK;
					BerserkDisable();
				} else {
					// ***Patch Effect***
					BerserkEnable();
					if (berserkIncrementFinishedTime < _pauseScript.relativeTime) {
						berserkIncrement++;
						switch (berserkIncrement) {
							case 0: berserk.swapTexture = b1; break;
							case 1: berserk.swapTexture = b2; berserk.IncrementStrength(); break;
							case 2: berserk.swapTexture = b3; break;
							case 3: berserk.swapTexture = b4; berserk.IncrementStats(); break;
							case 4: berserk.swapTexture = b5; break;
							case 5: berserk.swapTexture = b6; berserk.IncrementStats(); break;
							case 6: berserk.swapTexture = b7; berserk.IncrementStats(); break;
						}
						//gunCamBerserk.swapTexture = berserk.swapTexture;
						//gunCamBerserk.effectStrength = berserk.effectStrength;
						float berserkIncrementTime = Const.berserkTime/5f;
						berserkIncrementFinishedTime = _pauseScript.relativeTime + berserkIncrementTime;
					}
				}
			}

			// ================================== GENIUS PATCH ========================
			if (Utils.CheckFlags(patchActive, PATCH_GENIUS)) {
				// ---Disable Patch---
				if (geniusFinishedTime < _pauseScript.relativeTime) {
					_mouseLookScript.geniusActive = false;
					patchActive -= PATCH_GENIUS;
					wirePuzzle.geniusActive = false;
				} else {
					// ***Patch Effect***
					_mouseLookScript.geniusActive = true;  // so that LH/RH are swapped for mouse look
					wirePuzzle.geniusActive = true;
				}
			}

			// ================================== SIGHT PATCH =========================
			if (Utils.CheckFlags(patchActive, PATCH_SIGHT)) {
				// [[[Enable Side Effect]]]
				if (sightFinishedTime < _pauseScript.relativeTime && sightFinishedTime != -1f) {
					sightFinishedTime = -1f;
					sightSideEffectFinishedTime = _pauseScript.relativeTime + Const.sightSideEffectTime;
					sightLight.enabled = false;
					sightDimming.enabled = true;
				}

				// ---Disable Patch---
				if (sightSideEffectFinishedTime < _pauseScript.relativeTime && sightSideEffectFinishedTime != -1f) {
					sightSideEffectFinishedTime = -1f;
					sightFinishedTime = -1f;
					sightDimming.enabled = false;
					sightLight.enabled = false;
					patchActive -= PATCH_SIGHT;
				}
			}

			// ================================== STAMINUP PATCH ======================
			if (Utils.CheckFlags(patchActive, PATCH_STAMINUP)) {
				// ---Disable Patch---
				if (staminupFinishedTime < _pauseScript.relativeTime) {
					_playerMovement.staminupActive = false;
					_playerMovement.fatigue = 100f;  // side effect
					patchActive -= PATCH_STAMINUP;
				} else {
					// ***Patch Effect***
					_playerMovement.fatigue = 0f;
					_playerMovement.staminupActive = true;
				}
			}
		}
	}

	void BerserkEnable() {
		berserk.enabled = true;
		sensaroundCamCenterBerserk.enabled = true;
		sensaroundCamLeftBerserk.enabled = true;
		sensaroundCamRightBerserk.enabled = true;
	}

	void BerserkDisable() {
		berserk.Reset();
		berserk.enabled = false;
		sensaroundCamCenterBerserk.Reset();
		sensaroundCamCenterBerserk.enabled = false;
		sensaroundCamLeftBerserk.Reset();
		sensaroundCamLeftBerserk.enabled = false;
		sensaroundCamRightBerserk.Reset();
		sensaroundCamRightBerserk.enabled = false;
	}

	public void DisableAllPatches() {
		berserkFinishedTime = -1f;
		berserkIncrementFinishedTime =  -1f;
		berserkIncrement = 0;
		BerserkDisable();
		detoxFinishedTime =  -1f;
		geniusFinishedTime =  -1f;
		_mouseLookScript.geniusActive = false;
		wirePuzzle.geniusActive = false;
		mediFinishedTime =  -1f;
		reflexFinishedTime =  -1f;
		Time.timeScale = Const.defaultTimeScale; // normal time speed
		sightFinishedTime =  -1f;
		sightSideEffectFinishedTime =  -1f;
		sightDimming.enabled = false;
		sightLight.enabled = false;
		staminupFinishedTime =  -1f;
		_playerMovement.staminupActive = false;
		patchActive = 0;
	}

	public static string Save(GameObject go) {
		PlayerPatch pp = go.GetComponent<PlayerPatch>();
		var pauseScript = pp._pauseScript;
		s1.Clear();
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.berserkFinishedTime,"berserkFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.berserkIncrementFinishedTime,"berserkIncrementFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.detoxFinishedTime,"detoxFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.geniusFinishedTime,"geniusFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.mediFinishedTime,"mediFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(pp.reflexFinishedTime - Time.realtimeSinceStartup,"reflexFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.sightFinishedTime,"sightFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.sightSideEffectFinishedTime,"sightSideEffectFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,pp.staminupFinishedTime,"staminupFinishedTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(pp.berserkIncrement,"berserkIncrement"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(pp.patchActive,"patchActive"));
		s1.Append(Utils.splitChar);

		// Grayscale saved within each SaveCamera
		// SaveCamera 2
		// BerserkEffect 3
		s1.Append(BerserkEffect.Save(pp.berserk.gameObject));
		s1.Append(Utils.splitChar);
		s1.Append(BerserkEffect.Save(pp.sensaroundCamCenterBerserk.gameObject));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveCamera(pp.sensaroundCamCenterBerserk.gameObject));
		s1.Append(Utils.splitChar);
		s1.Append(BerserkEffect.Save(pp.sensaroundCamLeftBerserk.gameObject));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveCamera(pp.sensaroundCamLeftBerserk.gameObject));
		s1.Append(Utils.splitChar);
		s1.Append(BerserkEffect.Save(pp.sensaroundCamRightBerserk.gameObject));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveCamera(pp.sensaroundCamRightBerserk.gameObject));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		PlayerPatch pp = go.GetComponent<PlayerPatch>();
		var pauseScript = pp._pauseScript;
		pp.berserkFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"berserkFinishedTime"); index++;
		pp.berserkIncrementFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"berserkIncrementFinishedTime"); index++;
		pp.detoxFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"detoxFinishedTime"); index++;
		pp.geniusFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"geniusFinishedTime"); index++;
		pp.mediFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"mediFinishedTime"); index++;
		pp.reflexFinishedTime = Utils.GetFloatFromString(entries[index],"reflexFinishedTime");
		pp.reflexFinishedTime += Time.realtimeSinceStartup; index++;
		pp.sightFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"sightFinishedTime"); index++;
		pp.sightSideEffectFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"sightSideEffectFinishedTime"); index++;
		pp.staminupFinishedTime = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"staminupFinishedTime"); index++;
		pp.berserkIncrement = Utils.GetIntFromString(entries[index],"berserkIncrement"); index++;
		pp.patchActive = Utils.GetIntFromString(entries[index],"patchActive"); index++;
		index = BerserkEffect.Load(pp.berserk.gameObject,ref entries,index);
		index = BerserkEffect.Load(pp.sensaroundCamCenterBerserk.gameObject,ref entries,index);
		index = Utils.LoadCamera(pp.sensaroundCamCenterBerserk.gameObject,ref entries,index);
		index = BerserkEffect.Load(pp.sensaroundCamLeftBerserk.gameObject,ref entries,index);
		index = Utils.LoadCamera(pp.sensaroundCamLeftBerserk.gameObject,ref entries,index);
		index = BerserkEffect.Load(pp.sensaroundCamRightBerserk.gameObject,ref entries,index);
		index = Utils.LoadCamera(pp.sensaroundCamRightBerserk.gameObject,ref entries,index);
		return index;
	}
}
