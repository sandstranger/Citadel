using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class WeaponMagazineCounter : MonoBehaviour {
	public Sprite[] indicatorSprites;
	public Image onesIndicator;
	public Image tensIndicator;
	public Image hunsIndicator;
	private int tempi;
	private int checkcount;
	private int[] tempis = new int[] {0,0,0};

	[Inject] private Inventory _inventory;
	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private PauseScript _pauseScript;
	[Inject] private WeaponCurrent _weaponCurrent;

	public void UpdateDigits (int newamount) {
		tempi = newamount;
		tempis[0] = 10;
		tempis[1] = 10;
		tempis[2] = 10;

		//if (tempi > 99) {
			tempis[2] = tempi % 10; // ones
		if (newamount > 9) {
			tempi /= 10;
			tempis[1] = tempi % 10; // tens
		}
		if (newamount > 99) {
			tempi /= 10;
			tempis[0] = tempi % 10; // huns
		}

		onesIndicator.overrideSprite = indicatorSprites[tempis[2]];
		tensIndicator.overrideSprite = indicatorSprites[tempis[1]];
		hunsIndicator.overrideSprite = indicatorSprites[tempis[0]];
	}

	void Update() {
		if (_pauseScript.Paused() || _pauseScript.MenuActive()) return;
		
		int index = _weaponCurrent.weaponCurrent; // 0 to 6, 7 slots
		// Changed from this:
		// Get16WeaponIndexFromConstIndex(_weaponCurrent.weaponIndex); 0 to 15
		if (index < 0) return;

		if (_weaponCurrent.weaponIndex == -1
		    || _weaponCurrent.weaponIndex == 41
			|| _weaponCurrent.weaponIndex == 42
			|| _mouseLookScript.inCyberSpace
			|| _weaponCurrent.weaponCurrentPending >= 0) {
				tempis[0] = 10; // blank
				tempis[1] = 10; // blank
				tempis[2] = 10; // blank
				onesIndicator.overrideSprite = indicatorSprites[tempis[2]];
				tensIndicator.overrideSprite = indicatorSprites[tempis[1]];
				hunsIndicator.overrideSprite = indicatorSprites[tempis[0]];
				return;
		}

		if (_inventory.wepLoadedWithAlternate[_weaponCurrent.weaponCurrent]) {
			UpdateDigits(_weaponCurrent.currentMagazineAmount2[index]);
		} else {
			UpdateDigits(_weaponCurrent.currentMagazineAmount[index]);
		}
	}
}
