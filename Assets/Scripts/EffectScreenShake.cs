using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class EffectScreenShake : MonoBehaviour {
	[Inject] private Const _consts;
	
	[Tooltip("Distance in world units, set to -1 to use global default")] public float distance = 15f;
	[Tooltip("Force of shaking, higher is more violent, set to -1 to use global default")] public float force = 10f;
	public void Shake() {
		_consts.Shake(true,distance,force);
	}
}
