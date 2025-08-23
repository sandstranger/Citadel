using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class CyberDataFragment : MonoBehaviour {
	public int textIndex = 0;

	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	
	void OnTriggerEnter(Collider col) {
		if (col.gameObject.CompareTag("Player")) {
			PlayerMovement pm = col.gameObject.GetComponent<PlayerMovement>();
			if (pm != null) {
				_mfdManager.CyberSprint(_consts.stringTable[textIndex]);
			}
		}
	}
}
