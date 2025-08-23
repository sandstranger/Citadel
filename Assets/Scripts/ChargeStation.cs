using UnityEngine;
using System.Collections;
using System.Text;
using Zenject;

public class ChargeStation : MonoBehaviour {
	// Externally modified per prefab instance
	public float amount = 170;  //default to 2/3 of 255, the total energy player can have
	public float resetTime = 150; //150 seconds
	public bool requireReset;
	public float minSecurityLevel = 100;
	public float damageOnUse = 0f; 
	public string target;
	public string argvalue;
	public int rechargeMsgLingdex = 1;
	public int usedMsgLingdex = 0;

	// Internal references
	[HideInInspector] public float nextthink; // save, stores the time after which this will be usable again.  Soem charge stations must recharge.
	private static readonly StringBuilder s1 = new StringBuilder(100 * 1024);

	[Inject] 
	private LevelManager _levelManager;
	[Inject] private PlayerEnergy _playerEnergy;
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerHealth _playerHealth;

	void Awake() {
		nextthink = _pauseScript.relativeTime;
	}

	public void Use (UseData ud) {
		if (_levelManager.GetCurrentLevelSecurity() > minSecurityLevel) { 
			_mfdManager.BlockedBySecurity (transform.position);
			return;
		}
		
		if (nextthink < _pauseScript.relativeTime) {
			if (_playerEnergy.energy >= _playerEnergy.maxenergy) {
				_consts.sprint(303);
				return;
			} else {
				_playerEnergy.GiveEnergy(amount, EnergyType.ChargeStation);
				_mfdManager.energySurge.SetActive(true);
			}

			if (damageOnUse > 0f) {
				DamageData dd = new DamageData(_consts);

				// Don't ever kill the player from this, way too cheap.
				dd.damage = Mathf.Min(damageOnUse,_playerHealth.hm.health - 1);

				// No impact force here, it's a zap.  Ouch, it zapped me...that
				// really hurt Chargie, that hurt my finger, owhow, OW! ow,
				// hahahow ow! OWW!  Chargie zapped my finger (it helps if you
				// use a British accent and refer to Charlie Bit My Finger).
				if (dd.damage > 0) _playerHealth.hm.TakeDamage(dd);
			}

			_consts.sprint(usedMsgLingdex);
			if (requireReset) nextthink = _pauseScript.relativeTime + resetTime;
			ud.argvalue = argvalue;
			_consts.UseTargets(gameObject,ud,target);
		} else {
			_consts.sprint(rechargeMsgLingdex);
		}
	}

	public void ForceRecharge() {
		nextthink = 0;
	}

	public static string Save(GameObject go) {
		ChargeStation chg = go.GetComponent<ChargeStation>();
		s1.Clear();
		s1.Append(Utils.SaveRelativeTimeDifferential(chg._pauseScript,chg.nextthink,"nextthink")); // float - time before recharged
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(chg.amount,"amount"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(chg.resetTime,"resetTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(chg.requireReset,"requireReset"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(chg.minSecurityLevel,"minSecurityLevel"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(chg.damageOnUse,"damageOnUse"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(chg.target,"target"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(chg.argvalue,"argvalue"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(chg.rechargeMsgLingdex,"rechargeMsgLingdex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(chg.usedMsgLingdex,"usedMsgLingdex"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		ChargeStation chg = go.GetComponent<ChargeStation>();
		if (chg == null) {
			Debug.Log("ChargeStation.Load failure, chg == null");
			return index + 12;
		}

		if (index < 0) {
			Debug.Log("ChargeStation.Load failure, index < 0");
			return index + 12;
		}

		if (entries == null) {
			Debug.Log("ChargeStation.Load failure, entries == null");
			return index + 12;
		}

		chg.nextthink = Utils.LoadRelativeTimeDifferential(chg._pauseScript,entries[index],"nextthink"); index++; // float - time before recharged
		chg.amount  = Utils.GetFloatFromString(entries[index],"amount"); index++;
		chg.resetTime  = Utils.GetFloatFromString(entries[index],"resetTime"); index++;
		chg.requireReset  = Utils.GetBoolFromString(entries[index],"requireReset"); index++;
		chg.minSecurityLevel  = Utils.GetFloatFromString(entries[index],"minSecurityLevel"); index++;
		chg.damageOnUse  = Utils.GetFloatFromString(entries[index],"damageOnUse"); index++;
		chg.target = Utils.LoadString(entries[index],"target"); index++;
		chg.argvalue = Utils.LoadString(entries[index],"argvalue"); index++;
		chg.rechargeMsgLingdex = Utils.GetIntFromString(entries[index],"rechargeMsgLingdex"); index++;
		chg.usedMsgLingdex = Utils.GetIntFromString(entries[index],"usedMsgLingdex"); index++;
		return index;
	}
}
