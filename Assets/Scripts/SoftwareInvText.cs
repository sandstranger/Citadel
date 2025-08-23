using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

[System.Serializable]
public class SoftwareInvText : MonoBehaviour {
	Text text;
	public int slotnum = 0;
	/*[DTValidator.Optional] */public Text versionText; // Not needed for game player
	
	[Inject] private Const _consts;

	void Start () {
		text = GetComponent<Text>();
		if (text == null) UnityEngine.Debug.Log("BUG: Missing Text component for SoftwareInvText");
	}

	public void Select(bool thisOne) {
		if (thisOne) {
			if (text != null) text.color = _consts.ssYellowText; // Yellow
			if (versionText != null) versionText.color = _consts.ssYellowText; // Yellow
		} else {
			if (text != null) text.color = _consts.ssGreenText; // Green
			if (versionText != null) versionText.color = _consts.ssGreenText; // Green
		}
	}
}