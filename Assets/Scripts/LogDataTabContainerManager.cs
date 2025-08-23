using System;
using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class LogDataTabContainerManager : MonoBehaviour {
	public Text logName;
	public Text logSender;
	public Text logSubject;
	public Image logImage;

	[Inject] private Const _consts;
	
	public void SendLogData(int referenceIndex, bool isRH) {
		//Debug.Log("SendLogData received referenceIndex of " + referenceIndex.ToString());
		logName.text = _consts.audiologNames[referenceIndex];
		if (!isRH) {
			logSender.text = _consts.stringTable[893]
							 + _consts.audiologSenders[referenceIndex];

			logSubject.text = _consts.stringTable[894] + Environment.NewLine
							  + _consts.audiologSubjects[referenceIndex];

			logImage.overrideSprite = _consts.logImages[_consts.audioLogImagesRefIndicesLH[referenceIndex]];
		} else {
			logSender.text = System.String.Empty; // blank on RH side
			logSubject.text = System.String.Empty; // blank on RH side
			logImage.overrideSprite = _consts.logImages[_consts.audioLogImagesRefIndicesRH[referenceIndex]];
		}
	}
}
