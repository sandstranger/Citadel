using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

// Handles weapon inventory buttons so when player clicks on the weapon name
// text it selects that weapon, also changing to it to have as current.
public class WeaponButton : MonoBehaviour {
    public int useableItemIndex;
	public int WepButtonIndex;

	[Inject] private MFDManager _mfdManager;
	[Inject] private WeaponCurrent _weaponCurrent;

	public void WeaponInvClick () {
		_mfdManager.mouseClickHeldOverGUI = true;
		_weaponCurrent.WeaponChange(useableItemIndex, WepButtonIndex);
	}

	void Start() {
		GetComponent<Button>().onClick.AddListener(() => { WeaponInvClick(); });
	}
}
