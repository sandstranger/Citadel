using UnityEngine;
using System.Collections;
using Zenject;

public class PauseParticleSystem : MonoBehaviour {
	private ParticleSystem psys;
	private Vector3 previousVelocity;
	private bool previousUseGravity;
	private bool previousKinematic;
	private CollisionDetectionMode previouscolDetMode;

	[Inject] private Const _consts;
	
	void Awake () {
		Initialize();
	}

	void Initialize() {
		if (psys == null)
		{
			psys = GetComponent<ParticleSystem>();
		}
		if (!_consts.psys.Contains(this)) _consts.psys.Add(this);
	}

	void OnEnable () {
		if (psys == null) Initialize();
	}
		
	public void Pause () {
		if (psys != null && this.enabled && gameObject.activeInHierarchy) psys.Pause(true); // Pause the particles.
	}

	public void UnPause () {
		if (psys != null && this.enabled && gameObject.activeInHierarchy) psys.Play(true); // Resume the particles.
	}
	
	public void Hide() {
		Pause();
		gameObject.layer = 26; // Clip
	}
	
	public void Unhide() {
		UnPause();
		gameObject.layer = 1; // TransparentFX		
	}
	
	void OnDestroy() {
		if (_consts == null) return;
		if (_consts.psys == null) return;
		if (this == null) return;
		
		if (_consts.psys.Contains(this)) _consts.psys.Remove(this);
	}
}
