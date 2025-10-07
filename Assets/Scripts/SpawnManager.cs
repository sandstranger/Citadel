using System.Collections;
using System.Collections.Generic;
using System.Text;
using Zenject;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
	public int index; // Global constindex
	public int numberToSpawn = 1;
	public int numberActive; // save
	public bool active = false; // save
	public Transform[] spawnLocations;
	private GameObject dynamicObjectsContainer;
	public bool NPCSpawner = true;
	public float minDelayBetweenSpawns = 15f; // default values for generic level spawners for repopulating hallways
	public float maxDelayBetweenSpawns = 45f;
	public float allSpawnedResetDelay = 120f;
	[HideInInspector]
	public float delayFinished; // save
	public bool alertEnemiesOnAwake;
	public bool countOnlySameIndex = false; // Not one of us.

	[Inject] private Const _consts;
	[Inject] private LevelManager _levelManager;
	[Inject] private ConsoleEmulator _consoleEmulator;
	[Inject] private PauseScript _pauseScript;

	private static readonly StringBuilder s1 = new(100 * 500);

	void Start() {
		delayFinished = _pauseScript.relativeTime;
		if (_consts.difficultyCombat == 1) {
			numberToSpawn = (int) Mathf.Floor(numberToSpawn*0.5f);
			if (numberToSpawn < 1) numberToSpawn = 1;
		}

		if (_consts.difficultyCombat == 3) {
			numberToSpawn = (int) Mathf.Floor(numberToSpawn*1.5f);
			if (numberToSpawn < 1) numberToSpawn = 1;
		}

		if (_consts.difficultyCombat > 3) {
			numberToSpawn = (int) Mathf.Floor(numberToSpawn*5f); // Hehe :)
		}
	}

	public void Activate(bool alertEnemies) {
		alertEnemiesOnAwake = alertEnemies;
		active = true;
		delayFinished = _pauseScript.relativeTime;
	}

	void Update() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;
		if (!active) return;

		NPCSubManager subM = _levelManager.npcsm;
		int numNPCs = subM.childrenNPCsAICs.Length;		
		if (numNPCs > 300) return;

		int count = 0;
		for (int i=0;i<numNPCs;i++) {
			AIController aic = subM.childrenNPCsAICs[i];
			if (aic == null) continue; // Expected condition from them being blown up.
            if (!aic.gameObject.activeInHierarchy) continue;
            if (aic.healthManager.health <= 0) continue;
            if (aic.index != (index - 419) && countOnlySameIndex) continue;

			count++;
		}
		if (numberActive != count) numberActive = count;

		if (numberActive >= numberToSpawn) return;
		if (delayFinished >= _pauseScript.relativeTime) return; // Not yet.

		delayFinished = _pauseScript.relativeTime
						+ Random.Range(minDelayBetweenSpawns,
									   maxDelayBetweenSpawns);

		Spawn(index); // spawn then wait randomized amount of time
		count++;
		if (count >= numberToSpawn) {
			delayFinished = _pauseScript.relativeTime + allSpawnedResetDelay;
		}
	}

	void Spawn(int index) {
		if (_consts.difficultyCombat == 0) return; // Not on combat diff 0

		Debug.Log("Spawning new enemy " + index.ToString());
		dynamicObjectsContainer = _levelManager.GetCurrentDynamicContainer();
		Vector3 spot = GetRandomLocation();
		if (spot.x == 0 && spot.y == 0 && spot.z == 0) return;

		GameObject instGO = _consoleEmulator.SpawnDynamicObject(
			index,LevelManager.currentLevel,false,null,-1
		);

		if (instGO == null) {
			Debug.Log("BUG: Could not spawn NPC index " + index.ToString());
		} else {
			instGO.transform.position = spot;
			AIController aic = instGO.GetComponent<AIController>();
			if (aic == null) return;

			if (!alertEnemiesOnAwake) {
				if (aic.index != 14) aic.wandering = true;
				return;
			}

			aic.SetEnemy(_consts.player1Capsule,_consts.player1TargettingPos);
		}
	}

	Vector3 GetRandomLocation() {
		int randpos;
		randpos = Random.Range(0,(spawnLocations.Length-1));
		Vector3 retval = spawnLocations[randpos].position;
		if (!AreaClear(retval)) return new Vector3(0,0,0);
		if (!AreaHidden(retval)) return new Vector3(0,0,0);
		return retval;
	}

	// CapsuleCast using largest NPC's bounding capsule to check area is clear.
	bool AreaClear(Vector3 spot) {
		RaycastHit hit = new RaycastHit();
		if (Physics.CapsuleCast(spot + new Vector3(0,0.52f,0),
								spot + new Vector3(0,-0.52f,0),0.48f,
								_consts.vectorZero,out hit,0.02f,
								_consts.layerMaskNPCCollision)) {
			return false;
		} else {
			return true;
		}
	}

	bool AreaHidden(Vector3 spot) {
		Vector3 plyPos = _consts.player1Capsule.transform.position;
		float range = 50f;
		if (Vector3.Distance(plyPos,spot) > range) return true;

		int mask = _consts.layerMaskNPCAttack;
		Vector3 ray = (plyPos - spot).normalized;
		RaycastHit tempHit;
		if (Physics.Raycast(spot,ray,out tempHit,range,mask)) {
			if (tempHit.collider.CompareTag("Player")) return false;
		}
		return true;
	}

	public static string Save(GameObject go) {
		SpawnManager sm = go.GetComponent<SpawnManager>();
		s1.Clear();
		s1.Append(Utils.BoolToString(sm.active,"SpawnManager.active"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(sm.numberActive,"numberActive"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(sm._pauseScript,sm.delayFinished,"delayFinished"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		SpawnManager sm = go.GetComponent<SpawnManager>();
		// Spawn martin.  Ok dang, this is such a vague reference it makes
		// vague references look specific.  See source code for a Quake mod
		// called vigil (the source code, not referenced as Martin in the mod
		// though that's what your friend's name is, requires decompiling to
		// see that it's called spawnmartin() as it isn't available separately,
		// goodness gracious!)
		sm.active = Utils.GetBoolFromString(entries[index],"SpawnManager.active"); index++;
		sm.numberActive = Utils.GetIntFromString(entries[index],"numberActive"); index++;
		sm.delayFinished = Utils.LoadRelativeTimeDifferential(sm._pauseScript,entries[index],"delayFinished"); index++;
		return index;
	}
}
