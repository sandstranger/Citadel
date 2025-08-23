using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class EReaderSectionsButtons : MonoBehaviour {
	public EReaderSectionsButtonHighlight ersbh0;
	public EReaderSectionsButtonHighlight ersbh1;
	public EReaderSectionsButtonHighlight ersbh2;
	public EReaderSectionsButtonHighlight ersbh3;

	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private Inventory _inventory;
	
	void OnEnable() {
		if (_consts.difficultyMission == 0) ersbh3.gameObject.SetActive(false);
		else ersbh3.gameObject.SetActive(true);

		HighlightOthers();
	}

	public void HighlightOthers() {
		_inventory.CheckForUnreadLogs();
		if (_inventory.hasNewEmail) ersbh0.HighlightButton();
		if (_inventory.hasNewLogs) ersbh1.HighlightButton();
		if (_inventory.hasNewData) ersbh2.HighlightButton();
		if (_inventory.hasNewNotes) ersbh3.HighlightButton();
	}

	public void OnClick(int index) {
		_mfdManager.mouseClickHeldOverGUI = true;

		SetEReaderSectionsButtonsHighlights(index);
		switch (index) {
			case 0: _mfdManager.OpenEmailTableContents(); break;
			case 1: _mfdManager.OpenLogTableContents(); break;
			case 2: _mfdManager.OpenDataTableContents(); break;
			case 3: _mfdManager.OpenNotesTableContents(); break;
		}
	}

	public void SetEReaderSectionsButtonsHighlights(int index) {
		switch (index) {
			case 0: ersbh0.Highlight();   ersbh1.DeHighlight(); ersbh2.DeHighlight(); ersbh3.DeHighlight(); break;
			case 1: ersbh0.DeHighlight(); ersbh1.Highlight();   ersbh2.DeHighlight(); ersbh3.DeHighlight(); break;
			case 2: ersbh0.DeHighlight(); ersbh1.DeHighlight(); ersbh2.Highlight();   ersbh3.DeHighlight(); break;
			case 3: ersbh0.DeHighlight(); ersbh1.DeHighlight(); ersbh2.DeHighlight(); ersbh3.Highlight();   break;
		}

		HighlightOthers();
	}
}
