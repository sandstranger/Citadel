using UnityEngine;
using System.Collections;
using Zenject;

public class CyberDecoy : MonoBehaviour {
	[Inject] private Const _consts;
	
	void OnEnable() {
		_consts.decoyActive = true;
	}

	void OnDisable() {
		_consts.decoyActive = false;
	}
}