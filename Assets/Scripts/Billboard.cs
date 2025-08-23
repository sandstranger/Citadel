using UnityEngine;
using System.Collections;
using Zenject;

// Used by the TargetID instances to face the 3D text towards the player's camera.
public class Billboard : MonoBehaviour {
	// External references, optional
	public bool flip = false;

	// Internal references
	private Vector3 tempDir;

	[Inject] private MouseLookScript _mouseLookScript;

	void Update(){
		if (_mouseLookScript.playerCamera.enabled == true) {
			tempDir = _mouseLookScript.playerCamera.transform.forward;
			if (flip) tempDir = tempDir * -1f;
			transform.rotation = Quaternion.LookRotation(-tempDir);
		}
	}

	public void DestroySprite() {
		Utils.SafeDestroy(gameObject);
	}
}
