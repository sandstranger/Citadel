using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class WeaponButtonsManager : MonoBehaviour {
	public WeaponButton[] wepButtonsScripts;
	public GameObject[] wepCountsText;

	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;
	[Inject] private WeaponFire _weaponFire;
	[Inject] private WeaponCurrent _weaponCurrent;

	public void WeaponCycleUp() {
		if (_weaponFire.reloadFinished > _pauseScript.relativeTime) return;

		int initialIndex = _weaponCurrent.weaponCurrent;
		if (initialIndex < 0) initialIndex = 0;
		if (initialIndex > 6) initialIndex = 0;
		int nextIndex = initialIndex + 1; // add 1 to get slot above this
		if (nextIndex > 6) nextIndex = 0; // wraparound to bottom
		int countCheck = 0;
		bool buttonNotValid = (_inventory.weaponInventoryIndices[nextIndex] == -1);
		while (buttonNotValid) {
			countCheck++;
			if (countCheck > 13) {
				return; // no weapons!  don't runaway loop
			}
			nextIndex++;
			if (nextIndex > 6) nextIndex = 0;
			buttonNotValid = (_inventory.weaponInventoryIndices[nextIndex] == -1);
		}

		if (wepButtonsScripts[nextIndex].gameObject.activeSelf
			&& nextIndex != initialIndex) {
			_weaponCurrent.WeaponChange(wepButtonsScripts[nextIndex].useableItemIndex,
										 wepButtonsScripts[nextIndex].WepButtonIndex);
		}
	}

	public void WeaponCycleDown() {
		if (_weaponFire.reloadFinished > _pauseScript.relativeTime) return;

		int initialIndex = _weaponCurrent.weaponCurrent;
		if (initialIndex < 0) initialIndex = 0;
		if (initialIndex > 6) initialIndex = 0;
		int nextIndex = initialIndex - 1; // add 1 to get slot above this
		if (nextIndex < 0) nextIndex = 6; // wraparound to top
		int countCheck = 0;
		bool buttonNotValid = (_inventory.weaponInventoryIndices[nextIndex] == -1);
		while (buttonNotValid) {
			countCheck++;
			if (countCheck > 13) {
				return; // no weapons!  don't runaway loop
			}
			nextIndex--;
			if (nextIndex < 0) nextIndex = 6;
			buttonNotValid = (_inventory.weaponInventoryIndices[nextIndex] == -1);
		}

		if (wepButtonsScripts[nextIndex].gameObject.activeSelf
			&& nextIndex != initialIndex) {
			_weaponCurrent.WeaponChange(wepButtonsScripts[nextIndex].useableItemIndex,
										 wepButtonsScripts[nextIndex].WepButtonIndex);
		}
	}
}
