using UnityEngine;
using System.Collections;
using Zenject;

public class CodeScreen : MonoBehaviour {
    public int level;
    private MeshRenderer mr;
    private int matIndex = 0;
    private float tickFinished;
    
    [Inject] private Const _consts;
    [Inject] private PauseScript _pauseScript;

    void Start() {
        mr = GetComponent<MeshRenderer>();
        tickFinished = _pauseScript.relativeTime + 0.3f;
    }
    
    void Update() {
        if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		if (tickFinished - _pauseScript.relativeTime > 0.3f) {
			tickFinished = _pauseScript.relativeTime + 0.3f;
		}

        if (tickFinished > _pauseScript.relativeTime) return;
        
        tickFinished = _pauseScript.relativeTime + 0.3f;

        // Integer overload is maximum exclusive.  Confirmed maximum return
		// value is 9 and not 10 for Random.Range's below.
        switch (level) {
			case 1:
			    if (!_consts.questData.lev1SecCodeLocked) {
			        _consts.questData.lev1SecCode = UnityEngine.Random.Range(0,10);
			    }
			    
			    matIndex = _consts.questData.lev1SecCode;
			    break;
			case 2:
			    if (!_consts.questData.lev2SecCodeLocked) {
			        _consts.questData.lev2SecCode = UnityEngine.Random.Range(0,10);
			    }
			    
			    matIndex = _consts.questData.lev2SecCode;
			    break;
			case 3:
			    if (!_consts.questData.lev3SecCodeLocked) {
			        _consts.questData.lev3SecCode = UnityEngine.Random.Range(0,10);
			    }
			    
			    matIndex = _consts.questData.lev3SecCode;
			    break;
			case 4:
			    if (!_consts.questData.lev4SecCodeLocked) {
			        _consts.questData.lev4SecCode = UnityEngine.Random.Range(0,10);
			    }
			    
			    matIndex = _consts.questData.lev4SecCode;
			    break;
			case 5:
			    if (!_consts.questData.lev5SecCodeLocked) {
			        _consts.questData.lev5SecCode = UnityEngine.Random.Range(0,10);
			    }
			    
			    matIndex = _consts.questData.lev5SecCode;
			    break;
			case 6:
			    if (!_consts.questData.lev6SecCodeLocked) {
			        _consts.questData.lev6SecCode = UnityEngine.Random.Range(0,10);
			    }
			    
			    matIndex = _consts.questData.lev6SecCode;
			    break;
		}
		
		mr.material = (_consts.screenCodes[matIndex]);
    }
}
