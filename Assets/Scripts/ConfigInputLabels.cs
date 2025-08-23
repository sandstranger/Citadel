using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigInputLabels : MonoBehaviour {
	// Externally references, required
	public Text[] labels;

	[Inject] private Const _consts;
	
	void Start () {
		if (labels.Length < 40) Debug.Log("BUG: ConfigInputLabels has less than 40 assigned values.");
		for(int i=0;i<labels.Length;i++) {
			labels[i].text = _consts.InputCodes[i];
		}
	}
}
