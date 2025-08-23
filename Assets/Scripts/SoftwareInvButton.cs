using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class SoftwareInvButton : MonoBehaviour {
	public int index = 0;

	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private GUIState _guiState;
	[Inject] private Inventory _inventory;
	[Inject] private MouseLookScript _mouseLookScript;

	public void DoubleClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		SoftInvClick();
	}

    public void SoftInvClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		switch(index) {
			case 0:
					// Drill
					_inventory.pulserButtonText.Select(false);
					_inventory.drillButtonText.Select(true);
					_inventory.isPulserNotDrill = false;
					Utils.PlayUIOneShotSavable(_consts,80); // changeweapon
					break;
			case 1:
					// Pulser
					_inventory.pulserButtonText.Select(true);
					_inventory.drillButtonText.Select(false);
					_inventory.isPulserNotDrill = true;
					Utils.PlayUIOneShotSavable(_consts,80); // changeweapon
					break;
			case 2:
					// CyberShield
					if (_mouseLookScript.inCyberSpace) {
						_consts.sprint(_consts.stringTable[461],_consts.Player);
					} else {
						_consts.sprint(_consts.stringTable[460],_consts.Player);
					}
					break;
			case 3:
					// Turbo
					if (_mouseLookScript.inCyberSpace) {
						_inventory.UseTurbo();
						_guiState.ClearOverButton();
					} else {
						_consts.sprint(_consts.stringTable[460],_consts.Player);
					}
					break;
			case 4:
					// Decoy
					if (_mouseLookScript.inCyberSpace) {
						_inventory.UseDecoy();
						_guiState.ClearOverButton();
					} else {
						_consts.sprint(_consts.stringTable[460],_consts.Player);
					}
					break;
			case 5:
					// Recall
					if (_mouseLookScript.inCyberSpace) {
						_inventory.UseRecall();
						_guiState.ClearOverButton();
					} else {
						_consts.sprint(_consts.stringTable[460],_consts.Player);
					}
					break;
			case 6:
					// Games
					if (_mouseLookScript.inCyberSpace) {
						_consts.sprint(_consts.stringTable[443],_consts.Player);
					} else {
						_mfdManager.OpenMinigames();
						_consts.sprint(_consts.stringTable[309],_consts.Player); // Trioptimum Funpack Module, don't play on company time!
					}
					break;
		}
	}
}
