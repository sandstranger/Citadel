using UnityEngine;
using System.Collections;
using Zenject;

public class PauseRigidbody : MonoBehaviour {
	[HideInInspector] public Rigidbody rbody;
	public Vector3 previousVelocity;
	public bool previousUseGravity;
	public bool previousKinematic;
	public CollisionDetectionMode previouscolDetMode;
	public bool previousSet = false;

	[Inject] private Const _consts;
	[Inject] private PauseScript _pauseScript;
	
	void Awake() {
		Initialize();
	}

	void Initialize() {
		if (rbody == null) rbody = GetComponent<Rigidbody>();
		if (rbody == null) rbody = gameObject.AddComponent<Rigidbody>();
		if (!_consts.prb.Contains(this)) _consts.prb.Add(this);
		if (rbody.isKinematic && rbody.collisionDetectionMode != CollisionDetectionMode.ContinuousSpeculative) Debug.Log(gameObject.name + " has isKinematic true on initialize when not using ContinuousSpeculative!");
		SetPreviousValues();
	}

	void SetPreviousValues() {
		previousVelocity = rbody.linearVelocity;
		if (previousSet) return;
		
		previousUseGravity = rbody.useGravity;
		previousKinematic = rbody.isKinematic;
		previouscolDetMode = rbody.collisionDetectionMode;
		previousSet = true;
	}

	void OnEnable() {
		if (rbody == null) Initialize();
		if (_pauseScript.MenuActive()) Pause();
	}
		
	public void Pause() {
		if (rbody != null) {
			SetPreviousValues();
			rbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			rbody.useGravity = false;
			rbody.isKinematic = true;
		}
	}

	public void UnPause() {
		if (rbody == null) rbody = GetComponent<Rigidbody>();
		
		rbody.isKinematic = previousKinematic;
		rbody.useGravity = previousUseGravity;
		
		if (previouscolDetMode == CollisionDetectionMode.ContinuousSpeculative
			&& !rbody.isKinematic && GetComponent<AIController>() == null) {
			
			previouscolDetMode = CollisionDetectionMode.ContinuousDynamic;
		}
		
		if (rbody.isKinematic && previouscolDetMode != CollisionDetectionMode.ContinuousSpeculative) {
			rbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		} else {
			rbody.collisionDetectionMode = previouscolDetMode;
		}
		
		rbody.linearVelocity = previousVelocity;
	}
}
