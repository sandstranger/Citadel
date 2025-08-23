using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeTimerSlider : MonoBehaviour {
    Slider slideS;
	public Slider actualSlider;
	public Text valueText;

	[Inject] private MFDManager _mfdManager;
	[Inject] private Inventory _inventory;
	void Awake () {
        slideS = GetComponent<Slider>();
	}

	void Update () {
		if (_inventory.grenadeCurrent != -1) {
			if (_inventory.grenadeCurrent == 5) {
				valueText.text = _inventory.nitroTimeSetting.ToString("0.0");
			} else if (_inventory.grenadeCurrent == 6) {
				valueText.text = _inventory.earthShakerTimeSetting.ToString("0.0");
			}
		}

	}

    public void SetValue() {
		if (_inventory == null) return;
		if (_inventory.grenadeCurrent != 5
			&& _inventory.grenadeCurrent != 6) {

			return;
		}

		_mfdManager.mouseClickHeldOverGUI = true;
		float val = actualSlider.value;
		if (val >= 60f) val = 60f;
		if (_inventory.grenadeCurrent == 5) {
			if (val < 2f) val = 2f;
			_inventory.nitroTimeSetting = val;
		} else if (_inventory.grenadeCurrent == 6) {
			if (val < 4f) val = 4f;
			_inventory.earthShakerTimeSetting = val;
		}

		slideS.value = val;
		valueText.text = slideS.value.ToString("0.0");
		Slider slidLH =
	 	  _mfdManager.itemTabLH.grenadeTimerSliderSlider.GetComponent<Slider>();

		Slider slidRH =
		  _mfdManager.itemTabRH.grenadeTimerSliderSlider.GetComponent<Slider>();

		if (_inventory.grenadeCurrent == 5) {
			if (slidLH != actualSlider) slidLH.value = _inventory.nitroTimeSetting;
			if (slidRH != actualSlider) slidRH.value = _inventory.nitroTimeSetting;
		} else if (_inventory.grenadeCurrent == 6) {
			if (slidLH != actualSlider) slidLH.value = _inventory.earthShakerTimeSetting;
			if (slidRH != actualSlider) slidRH.value = _inventory.earthShakerTimeSetting;
		}
    }
}
