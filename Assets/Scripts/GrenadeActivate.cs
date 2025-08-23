using UnityEngine;
using System.Collections;
using System.Text;
using Zenject;

// Used on the physical grenade iteslf.
public class GrenadeActivate : MonoBehaviour {
	public int constIndex = -1; // Useable Item Index (NOT the master index)s
	public float nearforce;
	public float nearradius;
	public float damage = 11f;
	public float penetration = 20f;
	public float offense = 3f;
	public AttackType attackType = AttackType.Projectile;
	public bool proxSensed = false;
	public bool useProx = false; // save
	public PoolType explosionType = PoolType.GrenadeFragExplosions;
	public bool active = false;
	
	[HideInInspector] public float timeFinished; // save
	[HideInInspector] public bool explodeOnContact = false; // save
	[HideInInspector] public bool useTimer = false; // save
	private GameObject explosionEffect;
	private Rigidbody rbody;
	private static StringBuilder s1 = new StringBuilder(100 * 500);
	[Inject] private Const _consts;
	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerHealth _playerHealth;
	[Inject] private WeaponFire _weaponFire;

	void Awake () {
		rbody = GetComponent<Rigidbody>();
		if (constIndex == 11) {
			GameObject childGO = transform.GetChild(0).gameObject;
			if (childGO != null) {
				childGO.layer = gameObject.layer;
			}
		}
	}

	public void AwakeFromLoad(float health) {
		if (health > 0) {
			rbody.useGravity = true;
			Utils.EnableCollision(gameObject);
		} else {
			rbody.useGravity = false;
			Utils.DisableCollision(gameObject);
		}
	}

	void Update() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;
		if (!active) return;

		// Plastique or other explosive device:
		if (constIndex == 14 && active && gameObject.activeInHierarchy) {
			Explode();
			return;
		}

		// Standard grenade explode route:
		if ((useTimer && timeFinished < _pauseScript.relativeTime)
			|| (useProx && proxSensed)) {

			Explode();
		}
	}

	// Index = _consts.useableItemsFrobIcon index.
	public void Activate() {
		switch(constIndex) {
			case 7: explodeOnContact = true; break; // Fragmentation Grenade
			case 8: explodeOnContact = true; break; // Concussion Grenade
			case 9: explodeOnContact = true; break; // EMP Grenade
			case 10: timeFinished = _pauseScript.relativeTime + _inventory.earthShakerTimeSetting;
					 useTimer = true; break;        // Earthshaker Bomb
			case 11: useProx = true; explodeOnContact = false; break; // Land Mine
			case 12: timeFinished = _pauseScript.relativeTime + _inventory.nitroTimeSetting; 
					 useTimer = true; break;        // Nitropack Explosive
			case 13: explodeOnContact = true; break; // Gas Grenade
			default: return;
		}
		active = true;
	}

	void OnCollisionStay(Collision col) {
		if (explodeOnContact) Explode();
	}

	public bool IsNPCMine() {
		if (gameObject.layer == 11) return false; // Bullet
		return true;
	}

	public void Explode() {
		Debug.Log("Grenade exploded");
		Utils.DisableCollision(gameObject);
		DamageData dd = new DamageData(_consts);
		dd.damage = damage;
		dd.attackType = attackType;
		dd.penetration = penetration;
		dd.offense = offense;
		dd.impactVelocity = damage * 1.5f;
		if (!IsNPCMine()) {
			dd.owner = _consts.player1Capsule;
			_playerHealth.makingNoise = true;
			_playerHealth.noiseFinished = _pauseScript.relativeTime + 2f;
		}
		
		Utils.ApplyImpactForceSphere(_consts,dd,transform.position,nearradius,1.0f);
		GameObject explosionEffect = _consts.GetObjectFromPool(explosionType);
		if (explosionEffect != null) {
			explosionEffect.SetActive(true);
			explosionEffect.transform.position = transform.position;
			int soundIndex = 60; // attack1_explode
			switch(constIndex) {
				case 7:  soundIndex = 64; _weaponFire.fogFac += 5; break; // frag, explosion1
				case 8:  soundIndex = 60; _weaponFire.fogFac += 7; break; // conc, attack1_explode
				case 9:  soundIndex = 67; break; // emp, hit2
				case 10: soundIndex = 60; _weaponFire.fogFac += 7; break; // earth, attack1_explode
				case 11: soundIndex = 64; _weaponFire.fogFac += 5; break; // mine, explosion1
				case 12: soundIndex = 60; _weaponFire.fogFac += 6; break; // nitro, attack1_explode
				case 13: soundIndex = 63; _weaponFire.fogFac += 10; break; // gas, explode_minor
			}
			
			Utils.PlayTempAudio(_consts,transform.position,_consts.sounds[soundIndex]);
		}

		_consts.Shake(true,-1,-1);
		gameObject.SetActive(false);
	}

	// Live grenades - These should only be up in the air or active running timer, but still...or it's a landmine
	public static string Save(GameObject go) {
		s1.Clear();
		GrenadeActivate ga = go.GetComponent<GrenadeActivate>();
		s1.Append(Utils.UintToString(ga.constIndex,"constIndex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ga.active,"active"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ga.useTimer,"useTimer"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(ga._pauseScript,ga.timeFinished,"timeFinished"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ga.explodeOnContact,"explodeOnContact"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ga.useProx,"useProx"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ga.IsNPCMine(),"IsNPCMine"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		GrenadeActivate ga = go.GetComponent<GrenadeActivate>();
		if (entries == null) {
			Debug.Log("GrenadeActivate.Load failure, entries == null");
			return index + 7;
		}

		ga.constIndex = Utils.GetIntFromString(entries[index],"constIndex"); index++;
		ga.active = Utils.GetBoolFromString(entries[index],"active"); index++;
		ga.useTimer = Utils.GetBoolFromString(entries[index],"useTimer"); index++;
		ga.timeFinished = Utils.LoadRelativeTimeDifferential(ga._pauseScript,entries[index],"timeFinished"); index++;
		ga.explodeOnContact = Utils.GetBoolFromString(entries[index],"explodeOnContact"); index++;
		ga.useProx = Utils.GetBoolFromString(entries[index],"useProx"); index++;
		bool isNPC = Utils.GetBoolFromString(entries[index],"IsNPCMine"); index++;
		if (isNPC) {
			go.layer = 24; // NPCBullet
		} else {
			go.layer = 11; // Bullet (player's)
		}

		if (ga.constIndex == 11) {
			if (isNPC) ga.active = true;
			GameObject childGO = ga.transform.GetChild(0).gameObject;
			if (childGO != null) {
				childGO.layer = go.layer;
			}
		}

		return index;
	}
}
