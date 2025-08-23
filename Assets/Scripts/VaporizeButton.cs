using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Should only exist on the Item Tab.  When clicked, deletes one useless item
// from the general inventory, namely the currently highlighted one.
public class VaporizeButton : MonoBehaviour {
	public Image ico;
	public Text ict;
	private EventTrigger evenT;
	private bool pointerEntered;

	[Inject] private MFDManager _mfdManager;
	[Inject] private GUIState _guiState;
	[Inject] private Inventory _inventory;
	[Inject] private MouseLookScript _mouseLookScript;

	void Awake() {
		pointerEntered = false;
		evenT = GetComponent<EventTrigger>();
		if (evenT == null) evenT = gameObject.AddComponent<EventTrigger>();
		if (evenT != null) {
			// Create a new entry for the PointerEnter event
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => {
				OnPointerEnterDelegate((PointerEventData)data);
			});

            evenT.triggers.Add(pointerEnter);

            // Create a new entry for the PointerExit event
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => {
				OnPointerExitDelegate((PointerEventData)data);
			});

            evenT.triggers.Add(pointerExit);
		} else Debug.Log("Failed to add EventTrigger to " + gameObject.name);
	}

	void OnEnable() {
		pointerEntered = false;
	}

	// Handle OnPointerEnter event, replaces OnMouseEnter
    public void OnPointerEnterDelegate(PointerEventData data) { PtrEnter(); }

	// Handle OnPointerExit event, replaces OnMouseExit
    public void OnPointerExitDelegate(PointerEventData data) { PtrExit(); }

	public void PtrEnter () {
		if (pointerEntered) return;

		_guiState.PtrHandler(true,true,ButtonType.Generic,gameObject);
		_mouseLookScript.currentButton = gameObject;
		pointerEntered = true;
	}

	public void PtrExit () {
		if (!pointerEntered) return;

		_guiState.ClearOverButton();
		pointerEntered = false;
	}

	public void OnVaporizeClick() {
		_mfdManager.mouseClickHeldOverGUI = true;
		if (_inventory == null) return;
		if (_inventory.generalInvCurrent == 0) return; // Access Cards index.

		int cur = _inventory.generalInvCurrent;
		_inventory.generalInventoryIndexRef[cur] = -1; // Remove item
		_inventory.generalInvCurrent -= 1;
		if (_inventory.generalInvCurrent < 0) {
			_inventory.generalInvCurrent = 0; // Bound to lowest, but only
		}									   // since it is Access Cards.


		cur = _inventory.generalInvCurrent;
		if (_inventory.generalInventoryIndexRef[cur] < 0) {
			for (int i=13; i >= 0; i--) {
				if (_inventory.generalInventoryIndexRef[i] >= 0) {
					_inventory.generalInvCurrent = i;
					break; // Found last item in inventory.
				}
			}
		}

		cur = _inventory.generalInvCurrent;
		int indexRef = _inventory.generalInventoryIndexRef[cur];
		if (_inventory.generalInvCurrent == 0) {
			if (_inventory.HasAnyAccessCards()) {
				_mfdManager.SendInfoToItemTab(indexRef);
			} else {
				// If no access cards, reset item tab to show nothing.
				_mfdManager.SendInfoToItemTab(-1);
				PtrExit();
			}
		} else {
			GeneralInvButton genbut = _inventory.genButtons[cur].GetComponent<GeneralInvButton>();
			_mfdManager.SendInfoToItemTab(indexRef,genbut.customIndex);
		}
	}
}
