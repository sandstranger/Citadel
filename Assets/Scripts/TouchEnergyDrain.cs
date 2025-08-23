using UnityEngine;
using System.Collections;
using Zenject;

public class TouchEnergyDrain : MonoBehaviour {
	public float drainage = 1; // assign in the editor
	public float tick = 0.1f;
	private float tickFinished;

	[Inject] private BiomonitorGraphSystem _biomonitorGraphSystem;
	[Inject] private PauseScript _pauseScript;

	private void Awake() {
		tickFinished = _pauseScript.relativeTime + UnityEngine.Random.Range(1f,2f);
	}

	private void OnCollisionEnter (Collision col) {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		if (tickFinished < _pauseScript.relativeTime) {
			if (col.gameObject.CompareTag("Player")) {
				PlayerEnergy pe = col.gameObject.GetComponent<PlayerEnergy>();
				if (pe != null) {
					pe.TakeEnergy(drainage);
					_biomonitorGraphSystem.EnergyPulse(drainage);
				}
			}
			tickFinished = _pauseScript.relativeTime + tick;
		}
	}

}