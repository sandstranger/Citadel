using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Zenject;

public class MultiMediaLogTableButton : MonoBehaviour {
	public int logTableButtonIndex;

	[Inject] private MFDManager _mfdManager;
	[Inject] private Inventory _inventory;
	void LogTableButtonClick() {
		_inventory.hardwareIsActive[2] = true;
		_mfdManager.OpenEReaderInItemsTab();
		_mfdManager.mouseClickHeldOverGUI = true;
		_mfdManager.OpenLogsLevelFolder(logTableButtonIndex);
	}

	void Start() {
		GetComponent<Button>().onClick.AddListener(() => { LogTableButtonClick(); });
	}
}
