using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class LogTableContentsButtonsManager : MonoBehaviour {
	public GameObject[] LogButtons;

	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			for (int i=0; i<10; i++) {
				// Only show category buttons for levels we have logs from
				if (_inventory.numLogsFromLevel[i] > 0) {
					LogButtons[i].SetActive(true);
				} else {
					LogButtons[i].SetActive(false);
				}
			}
		}
	}
}
