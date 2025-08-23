using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class QuestBitRelay : MonoBehaviour {
	[Header("Bits to enable or disable when this GameObject is Targetted, or To Test Against")]
	public int lev1SecCode;
	public int lev2SecCode;
	public int lev3SecCode;
	public int lev4SecCode;
	public int lev5SecCode;
	public int lev6SecCode;
	public bool RobotSpawnDeactivated;
	public bool IsotopeInstalled;
	public bool ShieldActivated;
	public bool LaserSafetyOverriden;
	public bool LaserDestroyed;
	public bool BetaGroveCyberUnlocked;
	public bool GroveAlphaJettisonEnabled;
	public bool GroveBetaJettisonEnabled;
	public bool GroveDeltaJettisonEnabled;
	public bool MasterJettisonBroken;
	public bool Relay428Fixed;
	public bool MasterJettisonEnabled;
	public bool BetaGroveJettisoned;
	public bool AntennaNorthDestroyed;
	public bool AntennaSouthDestroyed;
	public bool AntennaEastDestroyed;
	public bool AntennaWestDestroyed;
	public bool SelfDestructActivated;
	public bool BridgeSeparated;
	public bool IsolinearChipsetInstalled;
	public string target;
	public string targetIfFalse;
	public string argvalue;
	public string argvalueIfFalse;

	[Inject] private Const _consts;
	[Inject] private LevelManager _levelManager;
	[Inject] private QuestLogNotesManager _questLogNotesManager;
	
    public void EnableBits() {
		if (RobotSpawnDeactivated) {
			_consts.questData.RobotSpawnDeactivated = true;
			Debug.Log("Bit set RobotSpawnDeactivated: "
					  + _consts.questData.RobotSpawnDeactivated.ToString());
		}

		if (IsotopeInstalled) {
			_consts.questData.IsotopeInstalled = true;
			Debug.Log("Bit set IsotopeInstalled: "
					  + _consts.questData.IsotopeInstalled.ToString());
		}

		if (ShieldActivated) {
			_consts.questData.ShieldActivated = true;
			_levelManager.exterior_shield.SetActive(true);
			Debug.Log("Bit set ShieldActivated: "
					  + _consts.questData.ShieldActivated.ToString());

			_questLogNotesManager.notes[8].SetActive(true);
			_questLogNotesManager.checkBoxes[8].isOn = 
				_consts.questData.ShieldActivated;

			_questLogNotesManager.labels[8].text = _consts.stringTable[560];
		}

		if (LaserSafetyOverriden) {
			_consts.questData.LaserSafetyOverriden = true;
			Debug.Log("Bit set LaserSafetyOverriden: "
					  + _consts.questData.LaserSafetyOverriden.ToString());

			_questLogNotesManager.notes[7].SetActive(true);
			_questLogNotesManager.checkBoxes[7].isOn =
				_consts.questData.LaserSafetyOverriden;

			_questLogNotesManager.labels[7].text = _consts.stringTable[559];
		}
		if (LaserDestroyed) { 
			_consts.questData.LaserDestroyed = true;
			Debug.Log("Bit set LaserDestroyed: "
					  + _consts.questData.LaserDestroyed.ToString());

			if (AutoSplitterData.missionSplitID == 1) {
				AutoSplitterData.missionSplitID++;
			}

			_questLogNotesManager.notes[9].SetActive(true);
			_questLogNotesManager.checkBoxes[9].isOn =
				_consts.questData.LaserDestroyed;

			_questLogNotesManager.labels[9].text = _consts.stringTable[561];
		}

		if (BetaGroveCyberUnlocked) {
			_consts.questData.BetaGroveCyberUnlocked = true;
			Debug.Log("Bit set BetaGroveCyberUnlocked: "
					  + _consts.questData.BetaGroveCyberUnlocked.ToString());
			
			_questLogNotesManager.notes[12].SetActive(true);
		}

		if (GroveAlphaJettisonEnabled) {
			_consts.questData.GroveAlphaJettisonEnabled = true;
			Debug.Log("Bit set GroveAlphaJettisonEnabled: "
					  + _consts.questData.GroveAlphaJettisonEnabled.ToString());
			
			_questLogNotesManager.notes[12].SetActive(true);
		}

		if (GroveBetaJettisonEnabled) {
			_consts.questData.GroveBetaJettisonEnabled = true;
			Debug.Log("Bit set GroveBetaJettisonEnabled: "
					  + _consts.questData.GroveBetaJettisonEnabled.ToString());
			
			_questLogNotesManager.notes[12].SetActive(true);
		}

		if (GroveDeltaJettisonEnabled) {
			_consts.questData.GroveDeltaJettisonEnabled = true;
			Debug.Log("Bit set GroveDeltaJettisonEnabled: "
					  + _consts.questData.GroveDeltaJettisonEnabled.ToString());
			
			_questLogNotesManager.notes[12].SetActive(true);
		}

		if (MasterJettisonBroken) {
			_consts.questData.MasterJettisonBroken = true;
			Debug.Log("Bit set MasterJettisonBroken: "
					  + _consts.questData.MasterJettisonBroken.ToString());

			if (AutoSplitterData.missionSplitID == 2) {
				AutoSplitterData.missionSplitID++;
			}

			_questLogNotesManager.notes[12].SetActive(true);
			_questLogNotesManager.notes[11].SetActive(true);
			_questLogNotesManager.labels[11].text =
				_consts.stringTable[563];// Set:Diagnose and repair broken relay
		}
		if (Relay428Fixed) {
			_consts.questData.Relay428Fixed = true;
			Debug.Log("Bit set Relay428Fixed: "
					  + _consts.questData.Relay428Fixed.ToString());

			_questLogNotesManager.notes[11].SetActive(true);
			_questLogNotesManager.checkBoxes[11].isOn =
				_consts.questData.Relay428Fixed;

			_questLogNotesManager.labels[11].text =
				_consts.stringTable[563]; // Set:Diagnose and repair broken relay

			// Add:: 428.
			_questLogNotesManager.labels[11].text += _consts.stringTable[564];
		}
		if (MasterJettisonEnabled) {
			_consts.questData.MasterJettisonEnabled = true;
			Debug.Log("Bit set MasterJettisonEnabled: "
					  + _consts.questData.MasterJettisonEnabled.ToString());

			if (AutoSplitterData.missionSplitID == 3) {
				AutoSplitterData.missionSplitID++;
			}

			_questLogNotesManager.notes[10].SetActive(true);
			_questLogNotesManager.checkBoxes[10].isOn =
				_consts.questData.MasterJettisonEnabled;

			_questLogNotesManager.labels[10].text = _consts.stringTable[562];
		}
		if (BetaGroveJettisoned) {
			_consts.questData.BetaGroveJettisoned = true;
			Debug.Log("Bit set BetaGroveJettisoned: "
					  + _consts.questData.BetaGroveJettisoned.ToString());

			if (AutoSplitterData.missionSplitID == 4) { 
				AutoSplitterData.missionSplitID++;
			}

			_questLogNotesManager.notes[12].SetActive(true);
			_questLogNotesManager.checkBoxes[12].isOn =
				_consts.questData.BetaGroveJettisoned;

			_questLogNotesManager.labels[12].text = _consts.stringTable[565];
			_questLogNotesManager.notes[13].SetActive(true);
			_questLogNotesManager.labels[13].text = _consts.stringTable[566];
		}
		if (AntennaNorthDestroyed) {
			_consts.questData.AntennaNorthDestroyed = true;
			Debug.Log("Bit set AntennaNorthDestroyed: "
					  + _consts.questData.AntennaNorthDestroyed.ToString());
			
			_questLogNotesManager.notes[13].SetActive(true);
		}

		if (AntennaSouthDestroyed) {
			_consts.questData.AntennaSouthDestroyed = true;
			Debug.Log("Bit set AntennaSouthDestroyed: "
					  + _consts.questData.AntennaSouthDestroyed.ToString());
			
			_questLogNotesManager.notes[13].SetActive(true);
		}

		if (AntennaEastDestroyed) {
			_consts.questData.AntennaEastDestroyed = true;
			Debug.Log("Bit set AntennaEastDestroyed: "
					  + _consts.questData.AntennaEastDestroyed.ToString());
			
			_questLogNotesManager.notes[13].SetActive(true);
		}

		if (AntennaWestDestroyed) {
			_consts.questData.AntennaWestDestroyed = true;
			Debug.Log("Bit set AntennaWestDestroyed: "
					  + _consts.questData.AntennaWestDestroyed.ToString());
			
			_questLogNotesManager.notes[13].SetActive(true);
		}

		if (SelfDestructActivated) {
			_consts.questData.SelfDestructActivated = true;
			Debug.Log("Bit set SelfDestructActivated: "
					  + _consts.questData.SelfDestructActivated.ToString());

			_questLogNotesManager.notes[0].SetActive(true);
			_questLogNotesManager.notes[1].SetActive(true);
			_questLogNotesManager.notes[2].SetActive(true);
			_questLogNotesManager.notes[3].SetActive(true);
			_questLogNotesManager.notes[4].SetActive(true);
			_questLogNotesManager.notes[5].SetActive(true);
			_questLogNotesManager.notes[6].SetActive(true);
			_questLogNotesManager.notes[7].SetActive(true);
			_questLogNotesManager.notes[8].SetActive(true);
			_questLogNotesManager.notes[9].SetActive(true);
			_questLogNotesManager.notes[10].SetActive(true);
			_questLogNotesManager.notes[11].SetActive(true);
			_questLogNotesManager.notes[12].SetActive(true);
			_questLogNotesManager.notes[13].SetActive(true);
			_questLogNotesManager.notes[14].SetActive(true); // Self destruct
			_questLogNotesManager.notes[15].SetActive(true); // Escape pod
			_questLogNotesManager.notes[16].SetActive(true); // Access the bridge
			_questLogNotesManager.checkBoxes[14].isOn =
				_consts.questData.SelfDestructActivated;

			// Set:Engage reactor self-destruct.
			_questLogNotesManager.labels[14].text = _consts.stringTable[567];

			// Set:Escape on escape pod.
			_questLogNotesManager.labels[15].text = _consts.stringTable[568];
		}

		if (BridgeSeparated) {
			_consts.questData.BridgeSeparated = true;
			Debug.Log("Bit set BridgeSeparated: "
					  + _consts.questData.BridgeSeparated.ToString());

			_questLogNotesManager.notes[0].SetActive(true);
			_questLogNotesManager.notes[1].SetActive(true);
			_questLogNotesManager.notes[2].SetActive(true);
			_questLogNotesManager.notes[3].SetActive(true);
			_questLogNotesManager.notes[4].SetActive(true);
			_questLogNotesManager.notes[5].SetActive(true);
			_questLogNotesManager.notes[6].SetActive(true);
			_questLogNotesManager.notes[7].SetActive(true);
			_questLogNotesManager.notes[8].SetActive(true);
			_questLogNotesManager.notes[9].SetActive(true);
			_questLogNotesManager.notes[10].SetActive(true);
			_questLogNotesManager.notes[11].SetActive(true);
			_questLogNotesManager.notes[12].SetActive(true);
			_questLogNotesManager.notes[13].SetActive(true);
			_questLogNotesManager.notes[14].SetActive(true); // Self destruct
			_questLogNotesManager.checkBoxes[14].isOn =
				_consts.questData.SelfDestructActivated;

			// Set:Engage reactor self-destruct.
			_questLogNotesManager.labels[14].text = _consts.stringTable[567];
			_questLogNotesManager.notes[16].SetActive(true);
			_questLogNotesManager.notes[17].SetActive(true);
			_questLogNotesManager.checkBoxes[16].isOn = true;

			// Set:Access the bridge.
			_questLogNotesManager.labels[16].text = _consts.stringTable[569];

			// Set:Destroy SHODAN.
			_questLogNotesManager.labels[17].text = _consts.stringTable[570];
		}
		if (IsolinearChipsetInstalled) {
			_consts.questData.IsolinearChipsetInstalled = true;
			Debug.Log("Bit set IsolinearChipsetInstalled: "
					  + _consts.questData.IsolinearChipsetInstalled.ToString());
		}
	}

	public void DisableBits() {
		if (RobotSpawnDeactivated) {
			_consts.questData.RobotSpawnDeactivated = false;
		}

		if (IsotopeInstalled) _consts.questData.IsotopeInstalled = false;
		if (ShieldActivated) {
			_consts.questData.ShieldActivated = false;
			_levelManager.exterior_shield.SetActive(false);
			Debug.Log("Bit unset ShieldActivated: "
					  + _consts.questData.ShieldActivated.ToString());

			_questLogNotesManager.checkBoxes[8].isOn =
				_consts.questData.ShieldActivated;
		}
		if (LaserSafetyOverriden) {
			_consts.questData.LaserSafetyOverriden = false;
			_questLogNotesManager.checkBoxes[7].isOn = _consts.questData.LaserSafetyOverriden;
		}
		if (LaserDestroyed) {
			_consts.questData.LaserDestroyed = false;
			_questLogNotesManager.checkBoxes[9].isOn = _consts.questData.LaserDestroyed;
		}
		if (BetaGroveCyberUnlocked) _consts.questData.BetaGroveCyberUnlocked = false;
		if (GroveAlphaJettisonEnabled) _consts.questData.GroveAlphaJettisonEnabled = false;
		if (GroveBetaJettisonEnabled) _consts.questData.GroveBetaJettisonEnabled = false;
		if (GroveDeltaJettisonEnabled) _consts.questData.GroveDeltaJettisonEnabled = false;
		if (MasterJettisonBroken) _consts.questData.MasterJettisonBroken = false;
		if (Relay428Fixed) {
			_consts.questData.Relay428Fixed = false;
			_questLogNotesManager.checkBoxes[11].isOn = _consts.questData.Relay428Fixed;
		}
		if (MasterJettisonEnabled) {
			_consts.questData.MasterJettisonEnabled = false;
			_questLogNotesManager.checkBoxes[10].isOn = _consts.questData.MasterJettisonEnabled;
		}
		if (BetaGroveJettisoned) {
			_consts.questData.BetaGroveJettisoned = false;
			_questLogNotesManager.checkBoxes[12].isOn = _consts.questData.BetaGroveJettisoned;
		}
		if (AntennaNorthDestroyed) _consts.questData.AntennaNorthDestroyed = false;
		if (AntennaSouthDestroyed) _consts.questData.AntennaSouthDestroyed = false;
		if (AntennaEastDestroyed) _consts.questData.AntennaEastDestroyed = false;
		if (AntennaWestDestroyed) _consts.questData.AntennaWestDestroyed = false;
		if (SelfDestructActivated) {
			_consts.questData.SelfDestructActivated = false;
			_questLogNotesManager.checkBoxes[14].isOn = _consts.questData.SelfDestructActivated;
		}
		if (BridgeSeparated) _consts.questData.BridgeSeparated = false;
		if (IsolinearChipsetInstalled) _consts.questData.IsolinearChipsetInstalled = false;
	}

    public void ToggleBits() {
		if (RobotSpawnDeactivated) _consts.questData.RobotSpawnDeactivated = !_consts.questData.RobotSpawnDeactivated;
		if (IsotopeInstalled) _consts.questData.IsotopeInstalled = !_consts.questData.IsotopeInstalled;
		if (ShieldActivated) {
			_consts.questData.ShieldActivated = !_consts.questData.ShieldActivated;
			_levelManager.exterior_shield.SetActive(_consts.questData.ShieldActivated);
			_questLogNotesManager.checkBoxes[8].isOn = _consts.questData.ShieldActivated;
			if (_consts.questData.ShieldActivated) {
				_questLogNotesManager.notes[8].SetActive(true);
				_questLogNotesManager.labels[8].text = _consts.stringTable[560];
			}
		}
		if (LaserSafetyOverriden) {
			_consts.questData.LaserSafetyOverriden = !_consts.questData.LaserSafetyOverriden;
			_questLogNotesManager.checkBoxes[7].isOn = _consts.questData.LaserSafetyOverriden;
			if (_consts.questData.LaserSafetyOverriden) {
				_questLogNotesManager.notes[7].SetActive(true);
				_questLogNotesManager.labels[7].text = _consts.stringTable[559];
			}
		}
		if (LaserDestroyed) {
			_consts.questData.LaserDestroyed = !_consts.questData.LaserDestroyed;
			if (AutoSplitterData.missionSplitID == 1) { AutoSplitterData.missionSplitID++; }
			_questLogNotesManager.checkBoxes[9].isOn = _consts.questData.LaserDestroyed;
			if (_consts.questData.LaserDestroyed) {
				_questLogNotesManager.notes[9].SetActive(true);
				_questLogNotesManager.labels[9].text = _consts.stringTable[561];
			}
		}
		if (BetaGroveCyberUnlocked) _consts.questData.BetaGroveCyberUnlocked = !_consts.questData.BetaGroveCyberUnlocked;
		if (GroveAlphaJettisonEnabled) _consts.questData.GroveAlphaJettisonEnabled = !_consts.questData.GroveAlphaJettisonEnabled;
		if (GroveBetaJettisonEnabled) _consts.questData.GroveBetaJettisonEnabled = !_consts.questData.GroveBetaJettisonEnabled;
		if (GroveDeltaJettisonEnabled) _consts.questData.GroveDeltaJettisonEnabled = !_consts.questData.GroveDeltaJettisonEnabled;
		if (MasterJettisonBroken) {
			_consts.questData.MasterJettisonBroken = !_consts.questData.MasterJettisonBroken;
			if (_consts.questData.MasterJettisonBroken) {
				_questLogNotesManager.notes[11].SetActive(true); // Diagnose and repair broken relay
				_questLogNotesManager.labels[11].text = _consts.stringTable[563];// Set:Diagnose and repair broken relay
			}
		}
		if (Relay428Fixed) {
			_consts.questData.Relay428Fixed = !_consts.questData.Relay428Fixed;
			_questLogNotesManager.checkBoxes[11].isOn = _consts.questData.Relay428Fixed;
			if (_consts.questData.Relay428Fixed) {
				_questLogNotesManager.notes[11].SetActive(true);
				_questLogNotesManager.labels[11].text = _consts.stringTable[563]; // Set:Diagnose and repair broken relay
				_questLogNotesManager.labels[11].text += _consts.stringTable[564]; // Add:: 428.
			}
		}
		if (MasterJettisonEnabled) {
			_consts.questData.MasterJettisonEnabled = !_consts.questData.MasterJettisonEnabled;
			_questLogNotesManager.checkBoxes[10].isOn = _consts.questData.MasterJettisonEnabled;
			if (_consts.questData.MasterJettisonEnabled) {
				_questLogNotesManager.notes[10].SetActive(true);
				_questLogNotesManager.labels[10].text = _consts.stringTable[562];
			}
		}
		if (BetaGroveJettisoned) {
			_consts.questData.BetaGroveJettisoned = !_consts.questData.BetaGroveJettisoned;
			_questLogNotesManager.checkBoxes[12].isOn = _consts.questData.BetaGroveJettisoned;
			if (_consts.questData.BetaGroveJettisoned ) {
				_questLogNotesManager.notes[12].SetActive(true);
				_questLogNotesManager.labels[12].text = _consts.stringTable[565];
				_questLogNotesManager.notes[13].SetActive(true);
				_questLogNotesManager.labels[13].text = _consts.stringTable[566];
			}
		}
		if (AntennaNorthDestroyed) _consts.questData.AntennaNorthDestroyed = !_consts.questData.AntennaNorthDestroyed;
		if (AntennaSouthDestroyed) _consts.questData.AntennaSouthDestroyed = !_consts.questData.AntennaSouthDestroyed;
		if (AntennaEastDestroyed) _consts.questData.AntennaEastDestroyed = !_consts.questData.AntennaEastDestroyed;
		if (AntennaWestDestroyed) _consts.questData.AntennaWestDestroyed = !_consts.questData.AntennaWestDestroyed;
		if (SelfDestructActivated) {
			_consts.questData.SelfDestructActivated = !_consts.questData.SelfDestructActivated;
			if (_consts.questData.SelfDestructActivated) {
				_questLogNotesManager.notes[14].SetActive(true);
				_questLogNotesManager.notes[15].SetActive(true); // Escape pod
				_questLogNotesManager.labels[14].text = _consts.stringTable[567];// Set:Engage reactor self-destruct.
				_questLogNotesManager.labels[15].text = _consts.stringTable[568];// Set:Escape on escape pod.
			}
		}
		if (BridgeSeparated) {
			_consts.questData.BridgeSeparated = !_consts.questData.BridgeSeparated;
			if (_consts.questData.BridgeSeparated) {
				_questLogNotesManager.notes[16].SetActive(true);
				_questLogNotesManager.notes[17].SetActive(true);
				_questLogNotesManager.checkBoxes[16].isOn = true;
				_questLogNotesManager.labels[16].text = _consts.stringTable[569]; // Set:Access the bridge.
				_questLogNotesManager.labels[17].text = _consts.stringTable[570]; // Set:Destroy SHODAN.
			}
		}
		if (IsolinearChipsetInstalled) _consts.questData.IsolinearChipsetInstalled = !_consts.questData.IsolinearChipsetInstalled;
	}

	public void TestBits(bool testIfTrue, UseData ud, TargetIO tio) {
		if (RobotSpawnDeactivated && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.RobotSpawnDeactivated, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (IsotopeInstalled && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.IsotopeInstalled, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (ShieldActivated && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.ShieldActivated, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (LaserSafetyOverriden && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.LaserSafetyOverriden, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (LaserDestroyed && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.LaserDestroyed, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (BetaGroveCyberUnlocked && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.BetaGroveCyberUnlocked, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (GroveAlphaJettisonEnabled && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.GroveAlphaJettisonEnabled, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (GroveBetaJettisonEnabled && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.GroveBetaJettisonEnabled, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (GroveDeltaJettisonEnabled && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.GroveDeltaJettisonEnabled, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (MasterJettisonBroken && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.MasterJettisonBroken, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (Relay428Fixed && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.Relay428Fixed, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (MasterJettisonEnabled && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.MasterJettisonEnabled, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (BetaGroveJettisoned && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.BetaGroveJettisoned, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (AntennaNorthDestroyed && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.AntennaNorthDestroyed, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (AntennaSouthDestroyed && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.AntennaSouthDestroyed, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (AntennaEastDestroyed && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.AntennaEastDestroyed, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (AntennaWestDestroyed && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.AntennaWestDestroyed, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (SelfDestructActivated && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.SelfDestructActivated, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (BridgeSeparated && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.BridgeSeparated, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
		if (IsolinearChipsetInstalled && (!string.IsNullOrWhiteSpace(target) || !string.IsNullOrWhiteSpace(targetIfFalse))) _consts.questData.TargetOnGatePassed(_consts.questData.IsolinearChipsetInstalled, testIfTrue, ud, tio, target, argvalue, targetIfFalse, argvalueIfFalse);
	}
}
