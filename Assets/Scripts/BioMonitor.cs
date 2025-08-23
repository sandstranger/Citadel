using System.Collections;
using System.Collections.Generic;
using System.Text;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class BioMonitor : MonoBehaviour {
	// External references, required
	public Text heartRate;
	public Text patchEffects;
	public Text heartRateText;
	public Text header;
	public Text patchesActiveText;
	public Text bpmText;
	public Text fatigueDetailText;
	public Text fatigue;

	// Internal references
	private const float beatTick = 0.5f;
	private StringBuilder tempStr;
	private float beatFinished; // Visual only, Time.time controlled

	[Inject] private Const _consts;
	[Inject] private PlayerPatch _playerPatch;
	[Inject] private Inventory _inventory;
	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerMovement _playerMovement;

	void Start() {
		beatFinished = Time.time + beatTick;
		tempStr = new StringBuilder();
		tempStr.Clear();
	}

    void Update() {
		if (_pauseScript.Paused() || _pauseScript.MenuActive()) return;
		if (!_inventory.hardwareIsActive[6]) return;
		if (beatFinished >= Time.time) return;

		beatFinished = Time.time + beatTick;
		header.text = _consts.stringTable[526];
		heartRateText.text = _consts.stringTable[527];
		bpmText.text = _consts.stringTable[529];
		fatigueDetailText.text = _consts.stringTable[531];
		tempStr.Clear();
		if (_playerMovement.fatigue >= 80f) {
			tempStr.Append(_consts.stringTable[532]); // High!
		} else if (_playerMovement.fatigue < 80f
				   && _playerMovement.fatigue > 30f) {
			tempStr.Append(_consts.stringTable[533]); // Moderate
		} else {
			tempStr.Append(_consts.stringTable[534]); // Low
		}

		fatigue.text = tempStr.ToString();
		tempStr.Clear();
		float bpm = (70f +((_playerMovement.fatigue/100f) * 110f));
		bpm *= Random.Range(0.95f,1.05f);
		bpm = Mathf.Floor(bpm);
		heartRate.text = bpm.ToString();
		if (_inventory.BioMonitorVersion() > 1
			&& Utils.CheckFlags(_playerPatch.patchActive, 127)) {
			patchesActiveText.text = _consts.stringTable[528];
			if (Utils.CheckFlags(_playerPatch.patchActive,
								 PlayerPatch.PATCH_MEDI)) {

				tempStr.Append(_consts.stringTable[520]); tempStr.Append(" ");
			}
			if (Utils.CheckFlags(_playerPatch.patchActive,
								 PlayerPatch.PATCH_STAMINUP)) {

				tempStr.Append(_consts.stringTable[521]); tempStr.Append(" ");
			}
			if (Utils.CheckFlags(_playerPatch.patchActive,
								 PlayerPatch.PATCH_SIGHT)) {

				tempStr.Append(_consts.stringTable[522]); tempStr.Append(" ");
			}

			if (Utils.CheckFlags(_playerPatch.patchActive,
								 PlayerPatch.PATCH_GENIUS)) {

				tempStr.Append(_consts.stringTable[523]); tempStr.Append(" ");
			}

			if (Utils.CheckFlags(_playerPatch.patchActive,
								 PlayerPatch.PATCH_BERSERK)) {

				tempStr.Append(_consts.stringTable[524]); tempStr.Append(" ");
			}

			if (Utils.CheckFlags(_playerPatch.patchActive,
								 PlayerPatch.PATCH_REFLEX)) {

				tempStr.Append(_consts.stringTable[525]); tempStr.Append(" ");
			}

			if (Utils.CheckFlags(_playerPatch.patchActive,
								 PlayerPatch.PATCH_DETOX)) {

				tempStr.Append(_consts.stringTable[530]);
			}

			patchEffects.text = tempStr.ToString();
		} else {
			patchesActiveText.text = System.String.Empty;
			patchEffects.text = System.String.Empty;
		}
    }
}
