using UnityEngine;
using System.Collections;
using Zenject;

public class HealingBed : MonoBehaviour {
	public float amount = 170;  //default to 2/3 of 255, the total player can have
	public int minSecurityLevel = 0;
	public bool broken = false;
	private float give;

	private AudioSource SFXSource;
	[Inject] 
	private PlayerReferenceManager _playerReference;
	[Inject] 
	private LevelManager _levelManager;
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	
	void Awake () {
		SFXSource = GetComponent<AudioSource>();
	}

	public void Use (UseData ud) {
		if (_levelManager.GetCurrentLevelSecurity() <= minSecurityLevel) {
			if (!broken) {
				_playerReference.HealthManager.HealingBed(amount,true);
				_playerReference.PlayerHealth.radiationArea = false;
				_playerReference.PlayerHealth.radiated = 0f;
				_consts.sprint(_consts.stringTable[23],ud.owner);
				Utils.PlayOneShotSavable(SFXSource,_consts.sounds[103]);
			} else {
				_consts.sprint(_consts.stringTable[24],ud.owner);
			}
		} else {
			_mfdManager.BlockedBySecurity(transform.position);
		}
	}
}
