using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class CyborgConversionToggle : MonoBehaviour {
	private AudioSource SFX;
	[Inject] 
	private LevelManager _levelManager;
	[Inject] private Const _consts;
	
	void Awake() {
		SFX = GetComponent<AudioSource>();
	}

	public void PlayVoxMessage() {
		SFX.Stop();
		int lindex = LevelManager.currentLevel != -1 ? LevelManager.currentLevel : 0;
		if (_levelManager.ressurectionActive[lindex]) {
			Utils.PlayOneShotSavable(SFX,_consts.sounds[183]); // "vox_cybconvcancelled"
			_consts.sprint(_consts.stringTable[591]); // "Cyborg conversion cancelled.  Healing normal."
		} else {
			Utils.PlayOneShotSavable(SFX,_consts.sounds[184]); // "vox_cybconvenabled"
			_consts.sprint(_consts.stringTable[592]); // "Cyborg conversion reactivated."
		}
	}
}
