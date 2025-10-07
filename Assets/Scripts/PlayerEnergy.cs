using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using Zenject;

public class PlayerEnergy : MonoBehaviour {
	// External references
	public Text drainText;
	public Text jpmText;
	public float energy = 54f; // save

	// Internal references
	private float tick = 0.1f;
	[HideInInspector] public float tickFinished; // save
	private float tempF;
	[HideInInspector] public float maxenergy = 255f;
	[HideInInspector] public int drainJPM = 0;
	private string jpm = " J/min";
	
	private static StringBuilder s1 = new StringBuilder(100 * 1024);

	[Inject] private LevelManager _levelManager;
	[Inject] private Const _consts;
	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;
	[Inject] private WeaponFire _weaponFire;
	[Inject] private WeaponCurrent _weaponCurrent;

	public void Start() {
		tempF = 0;
		drainJPM = 0;
		energy = 54f; //max is 255 
		tickFinished = _pauseScript.relativeTime + tick + Random.value; // random offset seed to prevent ticks lining up and causing frame hiccups
    }
    
    void TargetIdentifierSenseTargets() {
		// Automatically lock onto nearby targets.
		// Very specific variable names are good right ;)
		int lev = LevelManager.currentLevel;
		int numNPCs = _levelManager.npcsm.childrenNPCsAICs.Length;
		if (numNPCs <= 0) return;
		
		for (int i=0;i<numNPCs;i++) {
			AIController aic = _levelManager.npcsm.childrenNPCsAICs[i];
			if (aic == null) continue;
			if (aic.healthManager == null) continue;
            if (!aic.gameObject.activeInHierarchy) continue;
            if (aic.healthManager.health <= 0) continue;
            if (aic.hasTargetIDAttached) continue;
            
			// if NPC is in range....
			float far = Vector3.Distance(aic.transform.position,
			                             transform.position);
			if (far > TargetID.GetTargetIDSensingRange(_inventory,false)) continue;
			
			_weaponFire.CreateTargetIDInstance(-1f,aic.healthManager,-1f);
		}
    }

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			tempF = 1f;
			bool activeEnergyDrainers = false;
			if (tickFinished < _pauseScript.relativeTime) {
				drainJPM = 0;
				// 0 System Analyzer doesn't take energy

				// 1 = Navigation Unit doesn't take energy

				// 2 = Datareader doesn't take energy

				// 3 Drain sensaround
				if (_inventory.hardwareIsActive[3]) {
					switch (_inventory.hardwareVersion[3]) {
						case 0: tempF = 0.01535f; drainJPM += 9; break; // takes about 300s to drain full energy
						case 1: tempF = 0.03413f; drainJPM += 20; break; // takes about 300s to drain full energy
						case 2: tempF = 0.02559f; drainJPM += 15; break; // takes about 240s to drain full energy
					}
					activeEnergyDrainers = true;
					TakeEnergy(tempF);
				}

				// 4 = Target Identifier doesn't take energy
				if (_inventory.hasHardware[4]) {
				    TargetIdentifierSenseTargets();
				}

				// 5 = Energy Shield - handled by HealthManager
				if (_inventory.hardwareIsActive[5]) {
					switch (_inventory.hardwareVersionSetting[5]) {
						case 0: tempF = 0.04096f; drainJPM += 24; break;
						case 1: tempF = 0.10239f; drainJPM += 60; break;
						case 2: tempF = 0.17919f; drainJPM += 105; break;
						case 3: tempF = 0.05119f; drainJPM += 30; break;
					}
					activeEnergyDrainers = true;
					TakeEnergy(tempF);
				}

				// 6 = Biomonitor
				if (_inventory.hardwareIsActive[6]) {
					switch (_inventory.hardwareVersionSetting[6]) {
						case 0: tempF = 0.001706f; drainJPM += 1;  activeEnergyDrainers = true; break;
						case 1: tempF = 0; break; // doesn't take energy
					}
					if (tempF > 0) TakeEnergy(tempF);
				}

				// 7 = Head Mounted Lantern
				if (_inventory.hardwareIsActive[7]) {
					switch (_inventory.hardwareVersionSetting[7]) {
						case 0: tempF = 0.02559f; drainJPM += 15; break;// takes about 180s to drain full energy
						case 1: tempF = 0.04266f; drainJPM += 25; break; // takes about 120s to drain full energy
						case 2: tempF = 0.05119f; drainJPM += 30; break; // takes about 90s to drain full energy
					}
					activeEnergyDrainers = true;
					TakeEnergy(tempF);
				}

				// 8 Envirosuit - handled by HealthManager for radiation checks

				// 9 = Turbo Motion Booster - done in PlayerMovement since we only use energy on boost, no drain with skates
				if (_inventory.hardwareIsActive[9]) {
					switch (_inventory.hardwareVersionSetting[9]) {
						case 0: tempF = 0f; break;
						case 1: tempF = 0.02f; drainJPM += 16; break; // takes about 120s to drain full energy
						case 2: tempF = 0.015f; drainJPM += 12; break; // takes about 90s to drain full energy
					}
					activeEnergyDrainers = true;
					if (tempF > 0) TakeEnergy(tempF);
				}

				// 10 Jump Jet Boots - done in PlayerMovement since we only drain while jumping

				// 11 Drain nightsight
				if (_inventory.hardwareIsActive [11]) {
					tempF = 0.08533f; drainJPM += 50; // takes about 120s to drain full energy
					activeEnergyDrainers = true;
					TakeEnergy(tempF);
				}
				tickFinished = _pauseScript.relativeTime + tick;
			}

			// Turn everything off when we are out of energy
			if (activeEnergyDrainers && energy == 0) {
				DeactivateHardwareOnEnergyDepleted();
				activeEnergyDrainers = false;
				 drainJPM = 0;
			}
			if (drainJPM > 0) {
				drainText.text = drainJPM.ToString();
				jpmText.text = jpm;
			} else {
				drainText.text = System.String.Empty;
				jpmText.text = System.String.Empty;
			}
		}
	}

	void DeactivateHardwareOnEnergyDepleted() {
		_inventory.hardwareIsActive[3] = false;
		_inventory.hardwareButtonManager.SensaroundOff(); //sensaround
		if (_inventory.hardwareIsActive [6] && _inventory.hardwareVersionSetting[6] == 0) _inventory.hardwareButtonManager.BioOff(); // biomonitor, but only on v1, v2 doesn't use power
		if (_inventory.hardwareIsActive [5]) _inventory.hardwareButtonManager.ShieldOffWithEffects(); // shield
		if (_inventory.hardwareIsActive [7]) _inventory.hardwareButtonManager.LanternOff(); // lantern
		if (_inventory.hardwareIsActive [9]) _inventory.hardwareButtonManager.BoosterOff(); // turbo motion booster
		if (_inventory.hardwareIsActive [11]) _inventory.hardwareButtonManager.InfraredOff(); // infrared
	}

    public void TakeEnergy(float take) {
		float was = energy;
		if (energy == 0) return;
		if (_weaponCurrent.redbull) return; // No energy drain!

		energy -= take;
		if (energy <= 0f) {
			energy = 0f;
			Utils.PlayUIOneShotSavable(_consts,84); // energy_gone
			_consts.sprint(314); //Power supply exhausted.
			DeactivateHardwareOnEnergyDepleted();
		}
	}

	public void GiveEnergy(float give, EnergyType type) {
		energy += give;
		if (energy > maxenergy) energy = maxenergy;
        if (type == EnergyType.Battery) {
            Utils.PlayUIOneShotSavable(_consts,79); // batteryuse
        }
        if (type == EnergyType.ChargeStation) {
            Utils.PlayUIOneShotSavable(_consts,100); // chargingstation
        }
    }

	public static string Save(GameObject go) {
		PlayerEnergy pe = go.GetComponent<PlayerEnergy>();
		s1.Clear();
		s1.Append(Utils.FloatToString(pe.energy,"energy"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pe._pauseScript,pe.tickFinished,"tickFinished"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		PlayerEnergy pe = go.GetComponent<PlayerEnergy>();
		pe.energy = Utils.GetFloatFromString(entries[index],"energy"); index++;
		pe.tickFinished = Utils.LoadRelativeTimeDifferential(pe._pauseScript,entries[index],"tickFinished"); index++;
		return index;
	}
}
