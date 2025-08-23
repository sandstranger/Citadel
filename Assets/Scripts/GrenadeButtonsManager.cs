using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class GrenadeButtonsManager : MonoBehaviour {
	public GameObject[] grenButtons;
	public GameObject[] grenCountsText;

	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;
	
	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			for (int i=0; i<7; i++) {
				if (_inventory.grenAmmo[i] > 0) {
					if (!grenButtons[i].activeInHierarchy) grenButtons[i].SetActive(true);
					if (!grenCountsText[i].activeInHierarchy) grenCountsText[i].SetActive(true);
				} else {
					if (grenButtons[i].activeInHierarchy) grenButtons[i].SetActive(false);
					if (grenCountsText[i].activeInHierarchy) grenCountsText[i].SetActive(false);
				}
			}
		}
	}
}