using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class SystemAnalyzer : MonoBehaviour {
	public Text descSecurity;
	public Text security;
	public Text descLaser;
	public Text laser;
	public Text descLifepod;
	public Text lifepod;
	public Text descShield;
	public Text shield;
	public Text descReactor;
	public Text reactor;
	public Text descProcessor;
	public Text processor;
	public Text descProgram;
	public Text program;
	public Text descAlpha;
	public Text alpha;
	public Text descBeta;
	public Text beta;
	public Text descGamma;
	public Text gamma;
	public Text descDelta;
	public Text delta;

	[Inject] private Const _consts;
	[Inject] private LevelManager _levelManager;
	[Inject] private MFDManager _mfdManager;
	[Inject] private GUIState _guiState;

	public void Close() {
		_mfdManager.sysAnalyzerLH.SetActive(false);
		_mfdManager.sysAnalyzerRH.SetActive(false);
		_mfdManager.mouseClickHeldOverGUI = true;
		_guiState.ClearOverButton();
	}
    // Start is called before the first frame update
    void Update() {
		descSecurity.text = _consts.stringTable[474];
		security.text = _levelManager.levelSecurity[LevelManager.currentLevel] + _consts.stringTable[307];
		descLaser.text = _consts.stringTable[475];
		laser.text = _consts.questData.LaserDestroyed ? _consts.stringTable[486] : _consts.stringTable[485];
		descLifepod.text = _consts.stringTable[476];
		lifepod.text = _consts.questData.SelfDestructActivated ? _consts.stringTable[488] : _consts.stringTable[487];
		descShield.text = _consts.stringTable[477];
		shield.text = _consts.questData.ShieldActivated ? _consts.stringTable[490] : _consts.stringTable[489];
		descReactor.text = _consts.stringTable[478];
		reactor.text = _consts.questData.SelfDestructActivated ? _consts.stringTable[491] : _consts.stringTable[492];
		descProcessor.text = _consts.stringTable[479];
		int nodeCount = 0;
		for (int i=0;i<14;i++) {
			nodeCount += _levelManager.levelSmallNodeCount[i];
			nodeCount += _levelManager.levelLargeNodeCount[i];
			nodeCount -= _levelManager.levelSmallNodeDestroyedCount[i];
			nodeCount -= _levelManager.levelLargeNodeDestroyedCount[i];
		}
		processor.text = nodeCount.ToString();
		descProgram.text = _consts.stringTable[480];
		if (!_consts.questData.LaserDestroyed) {
			program.text = _consts.stringTable[494];
		} else {
			if (!_consts.questData.BetaGroveJettisoned) {
				program.text = _consts.stringTable[495];
			} else {
				if (!(_consts.questData.AntennaNorthDestroyed && _consts.questData.AntennaSouthDestroyed && _consts.questData.AntennaWestDestroyed && _consts.questData.AntennaEastDestroyed)) {
					program.text = _consts.stringTable[496];
				} else {
					if (!_consts.questData.BridgeSeparated) {
						program.text = _consts.stringTable[497];
					} else {
						program.text = _consts.stringTable[498];
					}
				}
			}
		}
		descAlpha.text = _consts.stringTable[481];
		alpha.text = _consts.stringTable[492];
		descBeta.text = _consts.stringTable[482];
		beta.text = _consts.questData.BetaGroveJettisoned ? _consts.stringTable[493] : _consts.stringTable[492];
		descGamma.text = _consts.stringTable[483];
		gamma.text = _consts.stringTable[493];
		descDelta.text = _consts.stringTable[484];
		delta.text = _consts.stringTable[492];
	}
}
