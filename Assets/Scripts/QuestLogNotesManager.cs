using System.Collections;
using System.Text;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogNotesManager : MonoBehaviour {
	public GameObject[] notes;
	public Text[] labels;
	public Toggle[] checkBoxes;
	public GameObject label15_StrikeThru;
	public Door neuroSurgeryDoor;
	public Door level6elevatorDoorTo7;

	// 0 - Destroy level 1 nodes.					...Later add:  Code is #.		DONE, CHECK DONE
	// 1 - Destroy level 2 nodes.					...Later add:  Code is #.		DONE, CHECK DONE
	// 2 - Destroy level 3 nodes.					...Later add:  Code is #.		DONE, CHECK DONE
	// 3 - Destroy level 4 nodes.					...Later add:  Code is #.		DONE, CHECK DONE
	// 4 - Destroy level 5 nodes.					...Later add:  Code is #.		DONE, CHECK DONE
	// 5 - Destroy level 6 nodes.					...Later add:  Code is #.		DONE, CHECK DONE
	// 6 - Escape neurosurgery suite.				...Later add:  Keycode is 451.	DONE, CHECK DONE
	// 7 - Disengage laser safety override.											DONE, CHECK DONE
	// 8 - Activate the station energy shield.										DONE, CHECK DONE
	// 9 - Destroy the mining laser.												DONE, CHECK DONE
	// 10- Enable master jettison.													DONE, CHECK DONE
	// 11- Diagnose and repair broken relay			...Later add:: 428.				DONE, CHECK DONE
	// 12- Jettison Beta Grove.														DONE, CHECK DONE
	// 13- Destroy the four relay antennae.											DONE, CHECK DONE
	// 14- Engage reactor self-destruct.											DONE, CHECK DONE
	// 15- Escape on escape pod.													DONE, STRIKE THRU DONE
	// 16- Access the bridge.														DONE, CHECK DONE
	// 17- Destroy SHODAN. 															DONE, N/A CAN'T SEE DURING CREDITS
	
	[Inject] private Const _consts;
	[Inject] private Inventory _inventory;

	void Awake() {
		for (int i=0;i<=17;i++) {
			notes[i].SetActive(false);
		}

		if (_consts.difficultyMission == 0) {
			gameObject.SetActive(false);
			return;
		}
		labels[6].text = _consts.stringTable[554]; // Set:Escape neurosurgery suite.
		notes[6].SetActive(true);
		if (_inventory != null) _inventory.hasNewNotes = true;
	}

	public void NotifyDoorUnlock(Door d) {
		if (d == neuroSurgeryDoor) {
			checkBoxes[6].isOn = true; // Escape neurosurgery suite.
			if (_inventory != null) _inventory.hasNewNotes = true;
		}
	}

	public void NotifyLockedDoorAttempt(Door d) {
		if (d == level6elevatorDoorTo7) {
			notes[12].SetActive(true); // Jettison Beta Grove.
			labels[12].text = _consts.stringTable[565];
			if (_inventory != null) _inventory.hasNewNotes = true;
		}
	}

	public void NotifyLevelChange(int levelIndex) {
		if (levelIndex != 1) {
			notes[6].SetActive(true); // Escape neurosurgery suite.
			checkBoxes[6].isOn = true;	// Escape neurosurgery suite.
			if (_inventory != null) _inventory.hasNewNotes = true;
		}
		
		if (levelIndex == 2) {
			notes[1].SetActive(true); // Destroy level 2 nodes.
			if (_inventory != null) _inventory.hasNewNotes = true;
		} else if (levelIndex == 3) {
			notes[2].SetActive(true); // Destroy level 3 nodes.
			if (_inventory != null) _inventory.hasNewNotes = true;
		} else if (levelIndex == 4) {
			notes[3].SetActive(true); // Destroy level 4 nodes.
			if (_inventory != null) _inventory.hasNewNotes = true;
		} else if (levelIndex == 5) {
			notes[4].SetActive(true); // Destroy level 5 nodes.
			if (_inventory != null) _inventory.hasNewNotes = true;
		} else if (levelIndex == 6) {
			notes[5].SetActive(true); // Destroy level 6 nodes.
			if (_inventory != null) _inventory.hasNewNotes = true;
		} else if (levelIndex == 7) {
			notes[13].SetActive(true); // Destroy the four relay antennae.
			notes[16].SetActive(true); // Access the bridge.
			if (_inventory != null) _inventory.hasNewNotes = true;
		} else if (levelIndex == 8) {
			notes[16].SetActive(true); // Access the bridge.
			notes[17].SetActive(true); // Destroy SHODAN. 
			if (_inventory != null) _inventory.hasNewNotes = true;
		} else if (levelIndex == 9) {
			notes[16].SetActive(true); // Access the bridge.
			checkBoxes[16].isOn = true; // Access the bridge.
			if (_inventory != null) _inventory.hasNewNotes = true;
		}
	}

	public void LogAdded(int logCustomIndex) {
		if (_consts.difficultyMission == 0) return;
		// Not checking for eReader present here...best to assume so we don't
		// have to do all this later and remember it and save it in save file.

		switch (logCustomIndex) {
			case 6: // #2-4601-06.MAY.72, just rewards
				notes[6].SetActive(true); // Make sure this was on.
				_inventory.hasNewNotes = true;
				labels[6].text += _consts.stringTable[555]; // Add:  Keycode is 451.
				break;
			case 8: // Honig-11.OCT.72, Medical CPU's
				notes[0].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[0].text = _consts.stringTable[556] + "1" + _consts.stringTable[557]; // Set:Destroy level 1 nodes.
				break;
			case 10: // Stack-15.OCT.72, Shodan's Presence
				notes[0].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[0].text = _consts.stringTable[556] + "1" + _consts.stringTable[557]; // Set:Destroy level 1 nodes.
				break;
			case 15: // D'Arcy-23.OCT.72, destroying the laser
				notes[7].SetActive(true);
				notes[8].SetActive(true);
				notes[9].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[7].text = _consts.stringTable[559]; // Set:Disengage laser safety override.
				labels[8].text = _consts.stringTable[560]; // Set:Activate the station energy shield.
				labels[9].text = _consts.stringTable[561]; // Set:Destroy the mining laser.
				break;
			case 29: // D'Arcy-21.OCT.72, block the laser with the shields?
				notes[7].SetActive(true);
				notes[8].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[7].text = _consts.stringTable[559]; // Set:Disengage laser safety override.
				labels[8].text = _consts.stringTable[560]; // Set:Activate the station energy shield.
				break;
			case 42: // diagnostic-06.NOV.72, repair diagnostic
				notes[10].SetActive(true); // Enable the enable Master Jettison task, just in case it wasn't
				notes[11].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[10].text = _consts.stringTable[562]; // Set:Enable master jettison.
				labels[11].text = _consts.stringTable[563]; // Set:Diagnose and repair broken relay
				labels[11].text += _consts.stringTable[564]; // Add:: 428
				break;
			case 66: // SHODAN-07.OCT.72, new jettison procedure
				notes[10].SetActive(true); // Enable the enable Master Jettison task, just in case it wasn't
				notes[12].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[10].text = _consts.stringTable[562]; // Set:Enable master jettison.
				labels[12].text = _consts.stringTable[565]; // Set:Diagnose and repair broken relay
				break;
			case 67: // Aaron-12.OCT.72, virus experiment
				notes[10].SetActive(true); // Enable the enable Master Jettison task, just in case it wasn't
				notes[12].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[10].text = _consts.stringTable[562]; // Set:Enable master jettison.
				labels[12].text = _consts.stringTable[565]; // Set:Diagnose and repair broken relay
				break;
			case 84: // Rebecca-1, Citadel Station
				notes[9].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[9].text = _consts.stringTable[561]; // Set:Destroy the mining laser.
				break;
			case 90: // Rebecca-3, SHODAN downloading
				notes[13].SetActive(true); // Destroy the four relay antennae.
				_inventory.hasNewNotes = true;
				labels[13].text = _consts.stringTable[566]; // Set:Destroy the four relay antennae.
				break;
			case 93: // Rebecca-4, SHODAN destroying Citadel Station
				notes[14].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[14].text = _consts.stringTable[567]; // Set:Destroy the mining laser.
				break;
			case 97: // Rebecca-5, get to the bridge!
				notes[16].SetActive(true);
				notes[17].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[16].text = _consts.stringTable[569]; // Set:Access the bridge.
				labels[17].text = _consts.stringTable[570]; // Set:Destroy SHODAN.
				label15_StrikeThru.SetActive(true);
				break;
			case 120: // VMailStatus, Shield Activated
				notes[8].SetActive(true);
				_inventory.hasNewNotes = true;
				labels[8].text = _consts.stringTable[569]; // Set:Activate the station energy shield.
				checkBoxes[8].isOn = true;
				break;
		}
	}

	public void NodesDestroyed(int levelIndex) {
		if (_consts.difficultyMission == 0) return;

		switch (levelIndex) {
			case 1: // Level 1
				notes[0].SetActive(true);
				labels[0].text = _consts.stringTable[556] + "1" + _consts.stringTable[557]; // Set:Destroy level 1 nodes. // Make sure this was on.
				if (!notes[1].activeInHierarchy) { // Level 2
					notes[1].SetActive(true);
					labels[1].text = _consts.stringTable[556] + "2" + _consts.stringTable[557]; // Set:Destroy level 2 nodes.
				}
				
				checkBoxes[0].isOn = true;
				_inventory.hasNewNotes = true;
				labels[0].text += _consts.stringTable[558] + _consts.questData.lev1SecCode.ToString() + "."; // Add:  Code is , Add:#, Add:.
				break;
			case 2: // Level 2
				notes[1].SetActive(true);
				labels[1].text = _consts.stringTable[556] + "2" + _consts.stringTable[557]; // Set:Destroy level 2 nodes. // Make sure this was on.
				if (!notes[2].activeInHierarchy) { // Level 3
					notes[2].SetActive(true);
					labels[2].text = _consts.stringTable[556] + "3" + _consts.stringTable[557]; // Set:Destroy level 3 nodes.
				}
				
				checkBoxes[1].isOn = true;
				_inventory.hasNewNotes = true;
				labels[1].text += _consts.stringTable[558] + _consts.questData.lev2SecCode.ToString() + "."; // Add:  Code is , Add:#, Add:.
				break;
			case 3: // Level 3
				notes[2].SetActive(true);
				labels[2].text = _consts.stringTable[556] + "3" + _consts.stringTable[557]; // Set:Destroy level 3 nodes. // Make sure this was on.
				if (!notes[3].activeInHierarchy) { // Level 4
					notes[3].SetActive(true);
					labels[3].text = _consts.stringTable[556] + "4" + _consts.stringTable[557]; // Set:Destroy level 4 nodes.
				}
								
				checkBoxes[2].isOn = true;
				_inventory.hasNewNotes = true;
				labels[2].text += _consts.stringTable[558] + _consts.questData.lev3SecCode.ToString() + "."; // Add:  Code is , Add:#, Add:.
				break;
			case 4: // Level 4
				notes[3].SetActive(true);
				labels[3].text = _consts.stringTable[556] + "4" + _consts.stringTable[557]; // Set:Destroy level 4 nodes. // Make sure this was on.
				if (!notes[4].activeInHierarchy) { // Level 5
					notes[4].SetActive(true);
					labels[4].text = _consts.stringTable[556] + "5" + _consts.stringTable[557]; // Set:Destroy level 5 nodes.
				}
				
				checkBoxes[3].isOn = true;
				_inventory.hasNewNotes = true;
				labels[3].text += _consts.stringTable[558] + _consts.questData.lev4SecCode.ToString() + "."; // Add:  Code is , Add:#, Add:.
				break;
			case 5: // Level 5
				notes[4].SetActive(true);
				labels[4].text = _consts.stringTable[556] + "5" + _consts.stringTable[557]; // Set:Destroy level 5 nodes. // Make sure this was on.
				if (!notes[5].activeInHierarchy) { // Level 6
					notes[5].SetActive(true);
					labels[5].text = _consts.stringTable[556] + "6" + _consts.stringTable[557]; // Set:Destroy level 6 nodes.
				}
				
				checkBoxes[4].isOn = true;
				_inventory.hasNewNotes = true;
				labels[4].text += _consts.stringTable[558] + _consts.questData.lev5SecCode.ToString() + "."; // Add:  Code is , Add:#, Add:.
				break;
			case 6: // Level 6
				notes[5].SetActive(true);
				labels[5].text = _consts.stringTable[556] + "6" + _consts.stringTable[557]; // Set:Destroy level 6 nodes. // Make sure this was on.
				checkBoxes[5].isOn = true;
				_inventory.hasNewNotes = true;
				labels[5].text += _consts.stringTable[558] + _consts.questData.lev6SecCode.ToString() + "."; // Add:  Code is , Add:#, Add:.
				break;
		}
	}

	public void UpdateToNextMission (int missionIndex) {
		switch (missionIndex) {
			case 1: checkBoxes[9].isOn = true; break; // Laser destroyed
			case 2: checkBoxes[13].isOn = true; break; // Antennae destroyed
			case 3: checkBoxes[14].isOn = true; break; // Self destruct activated.
		}
	}
	
	public string Save() {
		StringBuilder s1 = new StringBuilder();
		s1.Clear();
		for (int i=0;i<18;i++) {
			s1.Append(Utils.BoolToString(notes[i].activeSelf,"notes[" + i.ToString() + "].activeSelf"));
			s1.Append(Utils.splitChar);
		}
		
		for (int i=0;i<18;i++) {
			s1.Append(Utils.SaveString(labels[i].text,"labels[" + i.ToString() + "].text"));
			s1.Append(Utils.splitChar);
		}
		
		for (int i=0;i<18;i++) {
			s1.Append(Utils.BoolToString(checkBoxes[i].isOn,"checkBoxes[" + i.ToString() + "].isOn"));
			s1.Append(Utils.splitChar);
		}
		
		s1.Append(Utils.BoolToString(label15_StrikeThru.activeSelf,"label15_StrikeThru.activeSelf"));

		return s1.ToString();
	}
	
	public int Load(ref string[] entries, int index) {
		for (int i=0;i<18;i++) {
			notes[i].SetActive(Utils.GetBoolFromString(entries[index],"notes[" + i.ToString() + "].activeSelf")); index++;
		}
		
		for (int i=0;i<18;i++) {
			labels[i].text = Utils.LoadString(entries[index],"labels[" + i.ToString() + "].text"); index++;
		}
		
		for (int i=0;i<18;i++) {
			checkBoxes[i].isOn = Utils.GetBoolFromString(entries[index],"checkBoxes[" + i.ToString() + "].isOn"); index++;
		}
		
		label15_StrikeThru.SetActive(Utils.GetBoolFromString(entries[index],"label15_StrikeThru.activeSelf")); index++;
		return index;
	}
}
