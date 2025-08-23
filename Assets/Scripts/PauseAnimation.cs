using UnityEngine;
using System.Collections;
using Zenject;

public class PauseAnimation : MonoBehaviour {
	private Animation anim;

	[Inject] private Const _consts;
	
	void Awake () {
		Initialize();
	}

	void Initialize() {
		anim = GetComponent<Animation>();
		if (!_consts.panimsList.Contains(this)) _consts.panimsList.Add(this);
	}

	void OnEnable () {
		if (anim == null) Initialize();
	}
		
	public void Pause () {
		if (anim != null && this.enabled && gameObject.activeInHierarchy) {
            anim.Stop(); // Pause the animation.
        }
	}

	public void UnPause () {
		if (anim != null && this.enabled && gameObject.activeInHierarchy) {
            anim.Play(); // Resume the animation.
        }
	}
}
