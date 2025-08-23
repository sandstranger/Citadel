using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

[System.Serializable]
public class LogCountsText : MonoBehaviour {
	Text text;
	int tempint;
	public int countsSlotnum = 0;
	
	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;

	void Start () {
		text = GetComponent<Text>();
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			tempint = _inventory.numLogsFromLevel[countsSlotnum];
			if (tempint > 0)text.text = _inventory.numLogsFromLevel[countsSlotnum].ToString();
			else 			text.text = " ";  // Blank out the text
		}
	}
}