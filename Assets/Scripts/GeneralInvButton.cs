using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Zenject;

public class GeneralInvButton : MonoBehaviour {
    public int GeneralInvButtonIndex;
    public int useableItemIndex;
	public int customIndex;
	public GameObject activateButton;
	private bool reduce = false;

	[Inject] private PlayerEnergy _playerEnergy;
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private GUIState _guiState;
	[Inject] private Inventory _inventory;
	[Inject] private PlayerHealth _playerHealth;

    void Start() {
        GetComponent<Button>().onClick.AddListener(() => {
			GeneralInvClick();
		});
    }

    void GeneralInvClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		GeneralInvUse();
	}

	public void GeneralInvUse() {
        _inventory.generalInvCurrent = GeneralInvButtonIndex; //Set current
		useableItemIndex =
			_inventory.generalInventoryIndexRef[GeneralInvButtonIndex];

		// Access Cards
		if (GeneralInvButtonIndex == 0) {
			_mfdManager.SendInfoToItemTab(81);
			if (_mfdManager.lastItemSideRH) {
				_mfdManager.rightTC.SetCurrentAsLast();
			} else {
				_mfdManager.leftTC.SetCurrentAsLast();
			}
		} else {
			_mfdManager.SendInfoToItemTab(useableItemIndex,customIndex);
			if (_mfdManager.lastItemSideRH) {
				_mfdManager.rightTC.SetCurrentAsLast();
			} else {
				_mfdManager.leftTC.SetCurrentAsLast();
			}
		}
    }

    public void DoubleClick() {
        _inventory.generalInvCurrent = GeneralInvButtonIndex; //Set current
		_mfdManager.mouseClickHeldOverGUI = true;
		GeneralInvApply();
	}

	void ApplyBattery() {
		if (_playerEnergy.energy >= 255f) {
			_consts.sprint(_consts.stringTable[303]);
			reduce = false;
		}

		_playerEnergy.GiveEnergy(83f,EnergyType.Battery);
		reduce = true;
	}

	void ApplyIcadBattery() {
		if (_playerEnergy.energy >= 255f) {
			_consts.sprint(_consts.stringTable[303]);
			reduce = false;
			return;
		}

		_playerEnergy.GiveEnergy(255f,EnergyType.Battery);
		reduce = true;
	}

	void ApplyHealthkit() {
		if (_playerHealth.hm.health >= _playerHealth.hm.maxhealth) {
			_consts.sprint(_consts.stringTable[304]);
			reduce = false;
			return;
		}

		_playerHealth.hm.health = _playerHealth.hm.maxhealth;
		_mfdManager.DrawTicks(true);
		reduce = true;
	}

	public void GeneralInvApply() {
		// Access Cards button
		if (GeneralInvButtonIndex == 0) {
			_mfdManager.SendInfoToItemTab(81);
			_mfdManager.OpenTab(1,true,TabMSG.None, useableItemIndex,
								 Handedness.LH);
			return;
		}

        reduce = false;
		useableItemIndex =
			_inventory.generalInventoryIndexRef[GeneralInvButtonIndex];
		switch (useableItemIndex) {
			case 52: ApplyBattery(); break;
			case 53: ApplyIcadBattery(); break;
			case 55: ApplyHealthkit(); break;
			default:
				_mfdManager.SendInfoToItemTab(useableItemIndex,customIndex);
				_mfdManager.OpenTab(1,true,TabMSG.None, useableItemIndex,
									 Handedness.LH);

				// Set current.
				_inventory.generalInvCurrent = GeneralInvButtonIndex;
				break;
		}

		if (reduce)  {
			_inventory.generalInventoryIndexRef[GeneralInvButtonIndex] = -1;
			_guiState.ClearOverButton();
		}
	}
}
