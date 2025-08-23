using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class EmailContentsButtonsManager : MonoBehaviour {
	public GameObject[] EmailButtons;
	public MultiMediaLogButton[] mmLBs;

	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			for (int i=0; i<EmailButtons.Length; i++) {
				// Only show category buttons for levels we have logs from
				if (mmLBs[i].logReferenceIndex == -1) continue;
				if (_inventory.hasLog[mmLBs[i].logReferenceIndex]) {
					if (!EmailButtons[i].activeSelf) EmailButtons[i].SetActive(true);
				} else {
					if (EmailButtons[i].activeSelf) EmailButtons[i].SetActive(false);
				}
			}
		}
	}
}
