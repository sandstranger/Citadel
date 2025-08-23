using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;

public class SearchButton : MonoBehaviour {
	public bool isRH = false;
	public int[] contents;
	public int[] customIndex;

	[Inject] private MFDManager _mfdManager;
	[Inject] private GUIState _guiState;
	[Inject] private MouseLookScript _mouseLookScript;

	void Awake () {
		for (int i=0;i<=3;i++) {
			contents[i] = -1;
			customIndex[i] = -1;
		}
	}

	public void CheckForEmpty () {
		if (contents[0] == -1 && contents[1] == -1 && contents[2] == -1 && contents[3] == -1) {
			_mfdManager.ReturnToLastTab(isRH);
		}
	}

	public void SearchButtonClick (int buttonIndex) {
		_mfdManager.mouseClickHeldOverGUI = true;
		_mouseLookScript.SearchButtonClick(buttonIndex,this);
		_guiState.ClearOverButton();
	}
}
