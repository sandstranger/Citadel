using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class ObjectImpact : MonoBehaviour {
	// External values set per prefab instance, optional.
	public float minVolumeSpeed = 2f;
	public float maxVolumeSpeed = 10f;
	public int impactSFXIndex = 523;

	// Internal references, required
	[HideInInspector] public AudioSource SFXSource;
	[HideInInspector] public Rigidbody rbody;
	[HideInInspector] public Vector3 oldVelocity;

	[Inject] private Const _consts;
	[Inject] private PauseScript _pauseScript;

	void Start () {
		rbody = GetComponent<Rigidbody>();
		if (rbody == null) this.enabled = false;
		SFXSource = GetComponent<AudioSource>();
	}

	void OnCollisionEnter(Collision collision) {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;
		if (collision == null) return;

		if (collision.relativeVelocity.sqrMagnitude > (minVolumeSpeed * minVolumeSpeed)) {
			if (SFXSource != null) {
				SFXSource.pitch = (UnityEngine.Random.Range(0.8f,1.2f));
				float vol = (collision.relativeVelocity.magnitude/maxVolumeSpeed) * 0.3f;
				Utils.PlayOneShotSavable(SFXSource,_consts.sounds[impactSFXIndex],vol); // Play sound when object changes velocity significantly enough that it must have hit something
			}
		}
	}
}
