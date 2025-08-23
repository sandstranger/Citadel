using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class EnergySlider : MonoBehaviour {
    Slider slideS;

    [Inject] private MFDManager _mfdManager;
    [Inject] private WeaponCurrent _weaponCurrent;

	void Awake() {
        slideS = GetComponent<Slider>();
	}
/*
	void Update () {
		if (_weaponCurrent.weaponCurrent != -1) {
			slideS.value =
			  _weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent];
		} else {
			slideS.value = 0;
		}
	}*/


    public void SetValue(float val) {
		if (_weaponCurrent.weaponCurrent < 0
			|| _weaponCurrent.weaponCurrent > 6) {
			return;
		}

		_mfdManager.mouseClickHeldOverGUI = true;
		if (val < 1.0f) val = val * 100f;
		if (val < 0) val = 0f;
		if (val >= 98f) val = 100f;
		slideS.value = val;
		Debug.Log("Set energy slider value to " + slideS.value.ToString()
				  + ", from " + val.ToString());
        _weaponCurrent.weaponEnergySetting[_weaponCurrent.weaponCurrent] =
			slideS.value;
    }
}
