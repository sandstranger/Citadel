using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class LogTextReaderManager : MonoBehaviour {
	public GameObject moreButton;
	public GameObject logTextOutput;
	public Text moreButtonText;
	public GameObject backButton;
	public LogBackButton logBackButton;
	public int refIndex = -1;
	
	[Inject] private Const _consts;
	[Inject] private PauseScript _pauseScript;

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (logTextOutput.GetComponent<Text>().text.Length > 568) {
				moreButtonText.text = _consts.stringTable[26];
				if (backButton.activeSelf) backButton.SetActive(false);
			} else {
				moreButtonText.text = _consts.stringTable[27];
				if (!backButton.activeSelf && _consts.audioLogSpeech2Text[refIndex].Length > 568) {
					backButton.SetActive(true);
					logBackButton.refIndex = refIndex;
				}
			}
		}
	}

	public void SendTextToReader(int referenceIndex) {
		if (referenceIndex < 0) {
			Debug.Log("BUG: Audiolog index was less than 0. Report from "
					  + "LogTextReaderManager.");
			return;
		}

		logTextOutput.GetComponent<Text>().text = _consts.audioLogSpeech2Text[referenceIndex];
		refIndex = referenceIndex;
		if (_consts.audioLogSpeech2Text[referenceIndex].Length > 568) {
			logBackButton.refIndex = referenceIndex;
		}
	}
}
