using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class Radiation : MonoBehaviour {
	public float radiationAmount = 11f;

	private float intervalTime = 1f;
	private float radFinished = 0f;

	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerHealth _playerHealth;

	void Start() {
		radFinished = _pauseScript.relativeTime + (intervalTime * 2);
	}

	void OnTriggerEnter (Collider col) {
		if (col.gameObject.CompareTag("Player")) {
			if (_playerHealth.hm.health > 0f) {
				_playerHealth.radiationArea = true;
				_playerHealth.GiveRadiation(radiationAmount);
				radFinished = _pauseScript.relativeTime + (intervalTime*Random.Range(1f,1.5f));
			}
		}
	}

	void  OnTriggerStay (Collider col) {
		if (col.gameObject.CompareTag("Player")) {
			if (_playerHealth.hm.health > 0f && (radFinished < _pauseScript.relativeTime)) {
				_playerHealth.radiationArea = true;
				_playerHealth.GiveRadiation(radiationAmount);
				radFinished = _pauseScript.relativeTime + (intervalTime*Random.Range(1f,1.5f));
			}
		}
	}

	void OnTriggerExit (Collider col) {
		if (col.gameObject.CompareTag("Player")) { 
			if (_playerHealth.hm.health > 0f) {
				_playerHealth.radiationArea = false;
				radFinished = _pauseScript.relativeTime;  // reset so re-triggering is instant
			}
		}
	}
	
	void OnDisable() {
		_playerHealth.radiationArea = false;
	}
}
