using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class LogContentsButtonsManager : MonoBehaviour {
	public GameObject[] LogButtons;
	public Text[] LogButtonsText;
	public int currentLevelFolder;
	public string[] logNames;
	public int[] retrievedIndices;
	/*[DTValidator.Optional] */public MultiMediaLogButton[] logRefButtons; //DT optional because it's empty until initialized below during Start

	[Inject] private Const _consts;
	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;
	
	void Start() {
		InitializeLogsFromLevelIntoFolder();
	}

	public void InitializeLogsFromLevelIntoFolder() {
		retrievedIndices = new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
		logNames = GetLogNamesFromLevel(currentLevelFolder);
		for (int i=0; i<15; i++) {
			logRefButtons[i] = LogButtons[i].GetComponent<MultiMediaLogButton>();
		}
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			for (int i=0; i<15; i++) {
				LogButtonsText[i].text = logNames[i];
				logRefButtons[i].logReferenceIndex = retrievedIndices[i];
				if (_inventory.hasLog[retrievedIndices[i]]) {
					LogButtons[i].SetActive(true);
				} else {
					LogButtons[i].SetActive(false);
				}
			}
		}
	}

	string[] GetLogNamesFromLevel (int index) {
		string[] retval = {"","","","","","","","","","","","","","",""};
		int indexingVal = 0;
		for (int i=0;i<134;i++) {
			if ((_consts.audioLogLevelFound[i] == index)) {
				retval[indexingVal] = _consts.audiologNames[i];
				retrievedIndices[indexingVal] = i;
				indexingVal++;
				if (indexingVal > 14)
					break; // No need to iterate through remaining list if we already have 14 values
			}
		}
		return retval;
	}
}
