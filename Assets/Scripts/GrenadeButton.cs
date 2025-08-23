using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Zenject;

public class GrenadeButton : MonoBehaviour {
	public int GrenButtonIndex;
	public int useableItemIndex;

	private int itemLookup;
	private EventTrigger evenT;
	private bool pointerEntered;

	[Inject] private Const _consts;
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
            pointerEnter.callback.AddListener((data) => { OnPointerEnterDelegate((PointerEventData)data); });
            evenT.triggers.Add(pointerEnter);

            // Create a new entry for the PointerExit event
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnPointerExitDelegate((PointerEventData)data); });
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

		_guiState.PtrHandler(true,true,ButtonType.Grenade,gameObject);
		_mouseLookScript.currentButton = gameObject;
		pointerEntered = true;
	}

	public void PtrExit () {
		if (!pointerEntered) return;

		_guiState.ClearOverButton();
		pointerEntered = false;
    }

	void DoubleClick() {
		_mfdManager.mouseClickHeldOverGUI = true;

		// Put grenade in the player's hand (cursor)
		_mouseLookScript.UseGrenade(useableItemIndex);
	}

	public void GrenadeInvClick () {
		_mfdManager.mouseClickHeldOverGUI = true;
		GrenadeInvSelect();
	}

	public void GrenadeInvSelect() {
		_mfdManager.SendInfoToItemTab(useableItemIndex);
		_inventory.grenadeCurrent = GrenButtonIndex; // Set current
		Utils.PlayUIOneShotSavable(_consts,80); //changeweapon
	}

	void Start() {
		GetComponent<Button>().onClick.AddListener(() => { GrenadeInvClick();});
	}
}
