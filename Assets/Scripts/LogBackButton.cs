using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class LogBackButton : MonoBehaviour {
	public GameObject logTextOutput;
	private string remainder = System.String.Empty;
	public int refIndex = -1;

	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	void LogBackButtonClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		if (refIndex < 0) return;

		logTextOutput.GetComponent<Text>().text = _consts.audioLogSpeech2Text[refIndex];
		refIndex = -1;
	}

	void Start() {
		refIndex = -1;
		GetComponent<Button>().onClick.AddListener(() => { LogBackButtonClick(); });
	}
}
