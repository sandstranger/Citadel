using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class LogMoreButton : MonoBehaviour {
	public GameObject logTextOutput;
	public GameObject multiMediaTab;
	private string remainder = System.String.Empty;

	[Inject] private MFDManager _mfdManager;
	
	void LogMoreButtonClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		remainder = logTextOutput.GetComponent<Text>().text;
		if (remainder.Length>568) {
			// MORE BUTTON
			remainder = remainder.Remove(0,568);
			logTextOutput.GetComponent<Text>().text = remainder;
		} else {
			// CLOSE BUTTON
			_mfdManager.ResetMultiMediaTabs();
			_mfdManager.ClearDataTab(true);
			_mfdManager.ClearDataTab(false);
			_mfdManager.leftTC.ReturnToLastTab();
			_mfdManager.rightTC.ReturnToLastTab();
			_mfdManager.CenterTabButtonClickSilent(0,true);
			GetComponent<UIButtonMask>().PtrExit(); // Force mouse cursor out of UI.
		}
	}

	void Start() {
		GetComponent<Button>().onClick.AddListener(() => { LogMoreButtonClick(); });
	}
}
