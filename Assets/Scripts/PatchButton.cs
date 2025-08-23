using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Zenject;

public class PatchButton: MonoBehaviour {
	public int PatchButtonIndex;
	public int useableItemIndex;

	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private PlayerPatch _playerPatch;
	[Inject] private Inventory _inventory;
	
	public void DoubleClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		PatchUse();
	}

	public void PatchUse() {
		_playerPatch.ActivatePatch(useableItemIndex);
	}

	public void PatchInvClick (bool useSound) {
		_mfdManager.mouseClickHeldOverGUI = true;
		PatchSelect(useSound);
	}

	public void PatchSelect(bool useSound) {
		_mfdManager.SendInfoToItemTab(useableItemIndex);
		_inventory.patchCurrent = PatchButtonIndex; // Set current.
		for (int i = 0; i < 7; i++) {
			_inventory.patchCountTextObjects [i].color = _consts.ssGreenText;
		}
		_inventory.patchCountTextObjects[PatchButtonIndex].color = _consts.ssYellowText;
		if (useSound) Utils.PlayUIOneShotSavable(_consts,80); //changeweapon
	}

    void Start() {
        GetComponent<Button>().onClick.AddListener(() => { PatchInvClick(true); });
    }
}
