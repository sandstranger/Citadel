using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class CyberTimer : MonoBehaviour {
	private float t;
	public Text text;
	private float minutes;
	private float seconds;
	[HideInInspector]
	public float timerFinished; // save

	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private PauseScript _pauseScript;

	void Awake() {
		t = 60f * 10f;
		timerFinished = _pauseScript.relativeTime + 1f;
	}

    public void Reset(int diff) {
		switch(diff) {
			case 0: t = 10f * 60f; break;
			case 1: t = 5f * 60f; break;
			case 2: t = 4f * 60f; break;
			case 3: t = 3f * 60f; break;
		}
    }

    void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (t <= 0) _mouseLookScript.ExitCyberspace();
			if (timerFinished < _pauseScript.relativeTime) {
				t -= 1f;
				minutes = Mathf.Floor(t/60f);
				seconds = t - (minutes*60);
				text.text = (minutes.ToString("00") + ":" + seconds.ToString("00"));
				timerFinished = _pauseScript.relativeTime + 1f;
			}
		}
    }
}
