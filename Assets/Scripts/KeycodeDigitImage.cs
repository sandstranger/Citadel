using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class KeycodeDigitImage : MonoBehaviour {
	public Sprite[] digits;
	public int digitIndex;
	private Image imageDisplay;
	private int oldIndex;

	[Inject] private PauseScript _pauseScript;

	void Awake () {
		imageDisplay = gameObject.GetComponent<Image>();
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (digitIndex != oldIndex && digitIndex >= 0 && digitIndex <= digits.Length || digitIndex == -1) {
				if (digitIndex == -1) digitIndex = 10;
				imageDisplay.overrideSprite = digits[digitIndex];
				oldIndex = digitIndex;
			}
		}
	}
}
