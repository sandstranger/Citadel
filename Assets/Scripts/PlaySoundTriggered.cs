using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class PlaySoundTriggered : MonoBehaviour {
	public int SFXClip = -1;
	public bool loopingAmbient = false;
	public bool playEverywhere = false;
	public bool playSoundOnParticleEmit = false;
	
	private AudioSource SFX;
	[HideInInspector] public bool currentlyPlaying = false;
	[HideInInspector] public int numparticles = 0;
	[HideInInspector] public int burstemittcnt1 = 15;
	[HideInInspector] public int burstemittcnt2 = 30;
	private bool justPaused;
	private ParticleSystem psys;

	[Inject] private Const _consts;
	[Inject] private PauseScript _pauseScript;

    void Start() {
		if (SFX == null) SFX = GetComponent<AudioSource>();
		SFX.playOnAwake = false;
		SFX.loop = false;
		if (SFXClip > 0) SFX.clip = _consts.sounds[SFXClip];
		else Debug.Log("Unassigned clip index on PlaySoundTriggered at " + transform.position.ToString() + " for " + gameObject.name);
		if (playEverywhere) {
			SFX.spatialBlend = 0.0f;
		} else {
			SFX.spatialBlend = 1.0f;
		}
		
		if (loopingAmbient) {
			if (SFX != null) SFX.loop = true;
			currentlyPlaying = true;
			if (SFX != null) SFX.Play();
		}

		if (playSoundOnParticleEmit) {
			psys = GetComponent<ParticleSystem>();
			if (psys == null) Debug.Log("ERROR: missing ParticleSystem for PlaySoundTriggered");
			loopingAmbient = false; //only play when triggered by the psys emission
			numparticles = 0;
		}
    }

	// For ambient noises
	void OnEnable() {
		if (SFX == null) SFX = GetComponent<AudioSource>();
		if (loopingAmbient) {
			if (SFX != null) SFX.loop = true;
			if (SFX != null) SFX.clip = _consts.sounds[SFXClip];
			if (SFX != null) SFX.Play();
		}
	}

	void Update() {
		if (currentlyPlaying) {
			if (_pauseScript.Paused() || _pauseScript.MenuActive()) {
				if (SFX != null) SFX.Pause();
				justPaused = true;
			} else {
				if (justPaused) {
					if (SFX != null) SFX.UnPause();
					justPaused = false;
				}
			}
		}

		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (playSoundOnParticleEmit){
				int count = psys.particleCount;
				if (count > numparticles && (count == burstemittcnt1 || count == burstemittcnt2)) {
					Utils.PlayOneShotSavable(SFX,_consts.sounds[SFXClip]);
				}
				numparticles = count;
			}
		}
	}

    public void PlaySoundEffect() {
		if (SFX != null) SFX.loop = false;
		Utils.PlayOneShotSavable(SFX,_consts.sounds[SFXClip]);
	}
	
	public void StopSoundEffect() {
		if (SFX != null) SFX.Stop();
		currentlyPlaying = false;
	}
}
