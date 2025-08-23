using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class AmbientRegistration : MonoBehaviour {
	[HideInInspector] public AudioSource SFX;
    public float normalVolume = 1.0f;

    [Inject] private PauseScript _pauseScript;

    void Start() {
        SFX = GetComponent<AudioSource>();
        normalVolume = SFX.volume;
		_pauseScript.AddAmbientToRegistry(this);
    }
}
