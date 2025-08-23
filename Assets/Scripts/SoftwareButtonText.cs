using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

[System.Serializable]
public class SoftwareButtonText : MonoBehaviour {
	Text text;
	public int slotnum = 0;
	
	[Inject] private Const _consts;
	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;

	private void Start() {
		text = GetComponent<Text>();
	}

	private void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (slotnum == _inventory.currentCyberItem) {
				text.color = _consts.ssYellowText; // Yellow
			} else {
				text.color = _consts.ssGreenText; // Green
			}
		}
	}
}