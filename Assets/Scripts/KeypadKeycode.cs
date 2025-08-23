using UnityEngine;
using System.Collections;
using System.Text;
using Zenject;

public class KeypadKeycode : MonoBehaviour {
	public int securityThreshhold = 100; // If security level is not below this level, this is unusable.
	public int keycode; // the access code
	public bool locked = false; // save
	public string target;
	public string argvalue;
	public string lockedTarget;
	public int successMessageLingdex = -1;
	public int lockedMessageLingdex = -1;
	public bool solved = false; // save
	public bool useQuestKeycode1 = false;
	public bool useQuestKeycode2 = false;
	
	[HideInInspector] public bool padInUse = false; // save
	private GameObject playerCamera;
	[Inject] 
	private PlayerReferenceManager _playerReference;
	[Inject] 
	private LevelManager _levelManager;
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private MouseLookScript _mouseLookScript;

	private static readonly StringBuilder s1 = new(100 * 1024);

	void Start () {
		padInUse = false;
		playerCamera = _playerReference.playerCapsuleMainCamera;
	}

	public void Use (UseData ud) {
	    if (_levelManager.superoverride || _consts.difficultyMission == 0) {
	        locked = false; // SHODAN can go anywhere!  Full security override!
		} else if (_levelManager.GetCurrentLevelSecurity() > securityThreshhold) {
		    _mfdManager.BlockedBySecurity(transform.position);
		    return;
		}

		if (locked) {
			_consts.sprint(lockedMessageLingdex);

			// Target something because we are locked like a Vox message to
			// say we're locked, e.g. "Non emergency life pods disabled."
			ud.argvalue = argvalue;
			_consts.UseTargets(gameObject,ud,lockedTarget); 
			return;
		}

		if (useQuestKeycode1) {
			if (_consts.questData.lev1SecCode != -1) {
				if (_consts.questData.lev2SecCode != -1) {
					if (_consts.questData.lev3SecCode != -1) {
						int tempones = _consts.questData.lev3SecCode;
						int temptens = _consts.questData.lev2SecCode * 10;
						int temphuns = _consts.questData.lev1SecCode * 100;
						
						// Decode digits into keycode from levels 1, 2, and 3
						// in order huns, tens, ones.
						keycode = temphuns + temptens + tempones;
					}
				}
			} else {
				_consts.sprint(289);
				return;
			}
		}

		if (useQuestKeycode2) {
			if (_consts.questData.lev4SecCode != -1) {
				if (_consts.questData.lev5SecCode != -1) {
					if (_consts.questData.lev6SecCode != -1) {
						int tempones = _consts.questData.lev6SecCode;
						int temptens = _consts.questData.lev5SecCode * 10;
						int temphuns = _consts.questData.lev4SecCode * 100;
						
						// Secode digits into keycode from levels 4, 5, and 
						// in order huns, tens, ones.
						keycode = temphuns + temptens + tempones;
					}
				}
			} else {
				_consts.sprint(290);
				return;
			}
		}

		padInUse = true;
		Utils.PlayUIOneShotSavable(_consts,91);
		_mouseLookScript.ForceInventoryMode();
		_mfdManager.SendKeypadKeycodeToDataTab(keycode,transform.position,
		                                        this,solved);
	}

	public void UseTargets () {
		UseData ud = new UseData();
		ud.owner = playerCamera;
		ud.argvalue = argvalue;
		_consts.UseTargets(gameObject,ud,target);
		_consts.sprint(successMessageLingdex);
	}

	public static string Save(GameObject go) {
		KeypadKeycode kk = go.GetComponent<KeypadKeycode>();
		s1.Clear();
		s1.Append(Utils.IntToString(kk.securityThreshhold,"securityThreshhold"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(kk.keycode,"keycode"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(kk.locked,"locked"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(kk.target,"target"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(kk.argvalue,"argvalue"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(kk.lockedTarget,"lockedTarget"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(kk.successMessageLingdex,"successMessageLingdex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(kk.lockedMessageLingdex,"lockedMessageLingdex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(kk.padInUse,"padInUse"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(kk.solved,"solved"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(kk.useQuestKeycode1,"useQuestKeycode1"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(kk.useQuestKeycode2,"useQuestKeycode2"));
		return s1.ToString();
	}

	public static int Load(MFDManager mfdManager,GameObject go, ref string[] entries, int index) {
		KeypadKeycode kk = go.GetComponent<KeypadKeycode>();
        kk.securityThreshhold = Utils.GetIntFromString(entries[index],"securityThreshhold"); index++;
        kk.keycode = Utils.GetIntFromString(entries[index],"keycode"); index++;
		kk.locked = Utils.GetBoolFromString(entries[index],"locked"); index++;
		kk.target = Utils.LoadString(entries[index],"target"); index++;
		kk.argvalue = Utils.LoadString(entries[index],"argvalue"); index++;
		kk.lockedTarget = Utils.LoadString(entries[index],"lockedTarget"); index++;
		kk.successMessageLingdex = Utils.GetIntFromString(entries[index],"successMessageLingdex"); index++;
        kk.lockedMessageLingdex = Utils.GetIntFromString(entries[index],"lockedMessageLingdex"); index++;
		kk.padInUse = Utils.GetBoolFromString(entries[index],"padInUse"); index++;
		kk.solved = Utils.GetBoolFromString(entries[index],"solved"); index++;
		kk.useQuestKeycode1 = Utils.GetBoolFromString(entries[index],"useQuestKeycode1"); index++;
		kk.useQuestKeycode2 = Utils.GetBoolFromString(entries[index],"useQuestKeycode2"); index++;
		if (kk.padInUse) {
		    mfdManager.SendKeypadKeycodeToDataTab(kk.keycode,kk.transform.position,kk,kk.solved);
		} else {
		    mfdManager.SendKeypadKeycodeToDataTab(-1,Vector3.zero,null,false);
		}
		return index;
	}
}
