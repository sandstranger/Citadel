using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class CyberItem : MonoBehaviour {
	public SoftwareType type;
	public int version;
	
	private GameObject explosionEffect;

	[Inject] private Const _consts;
	[Inject] private Inventory _inventory;
	
	void Start() {
		if (_consts.difficultyMission == 0) {
			// Disable data objects when Mission difficulty is 0.
			if (type == SoftwareType.Data) this.gameObject.SetActive(false);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			PlayerMovement pm = other.gameObject.GetComponent<PlayerMovement>();
			if (pm == null) return;

			if (!_inventory.AddSoftwareItem(type,version)) return;

			explosionEffect = null;
			explosionEffect = _consts.GetObjectFromPool(PoolType.CyberDissolve);
			if (explosionEffect != null) {
				explosionEffect.SetActive(true);

				// Put vaporization effect at raycast center.
				explosionEffect.transform.position = transform.position; 
			}

			// We've been picked up, quick hide like you were.
			this.gameObject.SetActive(false);
		}
	}
}
