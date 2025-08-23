using System;
using UnityEngine;
using System.Collections;
using Zenject;

public class PooledItemDestroy : MonoBehaviour {
	public float itemLifeTime = 3.00f;
	public bool onlyOnce = false;
	private bool doneYet = false;
	private float timerFinished = 9999999f;

	[Inject] private PauseScript _pauseScript;

	void OnEnable () {
		timerFinished = _pauseScript.relativeTime + itemLifeTime;
	}

	private void OnDisable()
	{
		if (onlyOnce)
		{
			doneYet = true;
		}
	}

	void Update() {
		if (onlyOnce && doneYet) return;

		if (timerFinished < _pauseScript.relativeTime) {
			timerFinished = 9999999f;
			if (onlyOnce) doneYet = true;
			gameObject.SetActive(false);
		}
	}
}
