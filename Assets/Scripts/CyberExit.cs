using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class CyberExit : MonoBehaviour {

	[Inject] private MouseLookScript _mouseLookScript;

	void  OnTriggerEnter (Collider col) {
		if (col.gameObject.CompareTag("Player")) {
			PlayerMovement pm = col.gameObject.GetComponent<PlayerMovement>(); // Prevent retrigger by other parts of player.
			if (pm != null) {
				_mouseLookScript.ExitCyberspace();
			}
		}
	}
}
