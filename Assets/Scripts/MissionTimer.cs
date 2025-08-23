using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Citadel.Game;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class MissionTimer : MonoBehaviour {
	public Text text;
	public Text timerTypeText;
	public string currentMission;
	public int currentMissionIndex;

	[HideInInspector] public bool lastTimer = false;
	[HideInInspector] public float t = 6000f;
	private float minutes;
	private float seconds;
	[HideInInspector] public float timerFinished;
	[HideInInspector] public bool timesUP = false;
	[Inject] private Const _consts;
	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerHealth _playerHealth;
	[Inject] private QuestLogNotesManager _questLogNotesManager;

	private void Awake() {
		timerFinished = _pauseScript.relativeTime + 1f;
		currentMission = _consts.stringTable[504];
		currentMissionIndex = 0;
		timesUP = false;
	}

	// 25200
	// 6000 for laser mission (level 1, 2, R)
	// 10800 for download completion (level 3, 6, G1, G2, G4, 4, 7)
	// 2700 for bridge in range for finishing download (level R, 5)
	// 3000 for bridge separation countdown (level 8)
	// 2700 for biotoxin release (level 9)
    public void UpdateToNextMission(float newTimerAmount,int misTextIndex, int nextMissionIndex) {
		if (currentMissionIndex == nextMissionIndex) return;
		
		_questLogNotesManager.UpdateToNextMission(nextMissionIndex);

		if (_consts.difficultyMission < 3) return; // Don't update timer on lower skill settings.
		t = newTimerAmount;
		currentMissionIndex = nextMissionIndex;
		currentMission = _consts.stringTable[misTextIndex];
		if (currentMissionIndex == 4) lastTimer = true; // No gameover for last timer.
    }

    void Update() {
		if (_consts.difficultyMission < 3) return;
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;
		if (_mouseLookScript.inCyberSpace) return; // timer doesn't count down in cyberspace, yay!

		if (timesUP) {
			if (_playerHealth.hm.health > 0f) {
				_playerHealth.radiationArea = true;
				_playerHealth.GiveRadiation(0.1f); // Every frame! Muahaha!!!
				return;
			}
		}

		if (t <= 0) {
			if (lastTimer) {
				text.text = _consts.stringTable[869];
				timerTypeText.text = _consts.stringTable[509];
				timesUP = true;
				return;
			} else {
				_playerHealth.PlayerDeathToMenu();
				return;
			}
		}

		switch (currentMissionIndex) {
			case 0: if (_consts.questData.LaserDestroyed) UpdateToNextMission(10800f,505,1); break;
			case 1:
				if (_consts.questData.AntennaNorthDestroyed
					&& _consts.questData.AntennaSouthDestroyed
					&& _consts.questData.AntennaEastDestroyed
					&& _consts.questData.AntennaWestDestroyed) {
					UpdateToNextMission(2700f,506,2);
				}
				break;
			case 2: if (_consts.questData.SelfDestructActivated) UpdateToNextMission(3000f,507,3); break;
			case 3: if (_consts.questData.BridgeSeparated) UpdateToNextMission(2700f,506,4); break;
		}

		if (timerFinished < _pauseScript.relativeTime) {
			t -= 1f;
			minutes = Mathf.Floor(t/60f);
			seconds = t - (minutes*60);
			text.text = (minutes.ToString("00") + ":" + seconds.ToString("00"));
			timerTypeText.text = currentMission;
			timerFinished = _pauseScript.relativeTime + 1f;
		}
    }
}
