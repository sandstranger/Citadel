using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class MusicTrigger : MonoBehaviour {
	public float tick = 0.1f;
	private float tickFinished;
	public TrackType trackType;
	public MusicType musicType;

	[Inject] private Music _music;
	[Inject] private PauseScript _pauseScript;

	void Awake() {
		tickFinished = _pauseScript.relativeTime + 2f;
	}

	void OnTriggerEnter(Collider other) {
		if (tickFinished < _pauseScript.relativeTime) {
			if (other.gameObject.CompareTag("Player")) {
				_music.PlayTrack(LevelManager.CurrentLevel,trackType,musicType);
				_music.NotifyZone(trackType);
			}
			tickFinished = _pauseScript.relativeTime + tick;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			_music.Stop(); // return to normal upon leaving the trigger
		}
	}
}
