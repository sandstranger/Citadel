using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class SkyRotate : MonoBehaviour {
	public float rotateSpeed = 0.015f;
	public float defaultSpeed = 0.015f;
	private float timeIncrement = 0.016f;
	private float nextThink;
	private Vector3 rot;

	[Inject] private PauseScript _pauseScript;

	// Start instead of Awake to allow PauseScript to initialize.
	void Start() {
		nextThink = _pauseScript.relativeTime + timeIncrement;
		rot = new Vector3(0,rotateSpeed,0);
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (nextThink < _pauseScript.relativeTime) transform.Rotate(new Vector3(0,rotateSpeed,0));
		}
	}
}
