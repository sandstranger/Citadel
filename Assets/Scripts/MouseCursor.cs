using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using Citadel.Game;
using Zenject;

public class MouseCursor : MonoBehaviour {
    public GameObject playerCamera;
	public GameObject uiCamera;
	private Camera uiCameraCam;
	public bool liveGrenade = false;
	public string toolTip = "";
	public bool toolTipHasText = false;
	public Camera mainCamera;
	public RectTransform centerMFDPanel;
	public GameObject inventoryAddHelper;
    public float cursorSize = 24f;
    public Sprite cursorImage;
	public Image cursorUIImage;
	public Canvas canvas;
	public Canvas cursorCanvas;
	private RectTransform canvasRectTransform;
	private RectTransform cursorCanvasRectTransform;
	private RectTransform cursorRectTransform;
	public CanvasScaler canvasScaler;
    private float offsetX;
    private float offsetY;
	public bool justDroppedItemInHelper = false;
	public Rect drawTexture;
	public List<RectTransform> uiRaycastRects;
	public List<GameObject> uiRaycastRectGOs;
	public Vector2 cursorPosition;
	public GUIStyle liveGrenadeStyle;
	public GUIStyle toolTipStyle;
	public GUIStyle toolTipStyleLH;
	public GUIStyle toolTipStyleRH;
	public GameObject tooltipCenter;
	public GameObject tooltipLeft;
	public GameObject tooltipRight;
	public GameObject tooltipLiveGrenade;
	public Text tooltipCenterText;
	public Text tooltipLeftText;
	public Text tooltipRightText;
	public Text tooltipLiveGrenadeText;
	public Handedness toolTipType;
	public Sprite cursorDefaultTexture;
	public Sprite cyberspaceCursor;
	public Sprite cursorLHTexture;
	public Sprite cursorRHTexture;
	public Sprite cursorDNTexture;
	private Sprite tooltipTexture;
	public Sprite cursorGUI;
	public RectTransform energySliderRect;
	public float cursorScreenPercentage = 0.02f;
	private float halfFactor = 0.5f;
	public float deltaX;
	public float deltaY;
	public Vector2 lastMousePos;
	public GraphicRaycaster raycaster;
	private List<RaycastResult> graphicCastResults;
	private PointerEventData pev;

	[Inject] private Const _consts;
	[Inject] private GUIState _guiState;
	[Inject] private MinigameCursor _miniGameCursor;
	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private PauseScript _pauseScript;
	[Inject] private WeaponCurrent _weaponCurrent;
	[Inject] private readonly ITexturesStorage _texturesStorage;
	
	private void Awake() {
		uiCameraCam = uiCamera.GetComponent<Camera>();
		cursorSize = Screen.width * cursorScreenPercentage;
		drawTexture = new Rect((Screen.width * halfFactor) - offsetX, (Screen.height * halfFactor) - cursorSize,
			cursorSize, cursorSize);
		deltaX = deltaY = 0;
		lastMousePos = cursorPosition = Input.mousePosition;
		pev = new PointerEventData(EventSystem.current);
		graphicCastResults = new List<RaycastResult>();
		canvasRectTransform = canvas.gameObject.GetComponent<RectTransform>();
		if (canvasRectTransform == null) Debug.LogError("Can't access RectTransform on canvas!");

		cursorCanvasRectTransform = cursorCanvas.gameObject.GetComponent<RectTransform>();
		if (cursorCanvasRectTransform == null) Debug.LogError("Can't access RectTransform on cursorCanvas!");

		cursorRectTransform = cursorUIImage.GetComponent<RectTransform>();
		if (cursorRectTransform == null) Debug.LogError("Can't access RectTransform on Cursor!");
	}

	#if UNITY_STANDALONE_LINUX
		[DllImport("libX11")]
		static extern IntPtr XOpenDisplay(string display);

		[DllImport("libX11")]
		static extern int XCloseDisplay(IntPtr display);

		[DllImport("libX11")]
		static extern int XWarpPointer(IntPtr display, IntPtr src_w, 
									   IntPtr dest_w, int src_x, int src_y,
									   uint src_width, uint src_height, 
									   int dest_x, int dest_y);
	#elif UNITY_STANDALONE_WIN
		[DllImport("user32.dll")]
		public static extern bool SetCursorPos(int X, int Y);
	#endif

	public static void SetCursorPosInternal(int x, int y) {
		// TODO: Still experiencing issues, best to live with this bug a while
		// yet as it is better to have consistent behavior rather than what
		// this does.

		//#if UNITY_STANDALONE_LINUX
		//	IntPtr display = XOpenDisplay(null);
		//	if (display == IntPtr.Zero) {
		//		throw new Exception("Failed to open display");
		//	}

		//	Debug.Log("warping pointer to " + x.ToString() + ", " + y.ToString());
		//	XWarpPointer(display, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, x, y);
		//	XCloseDisplay(display);
		//#elif UNITY_STANDALONE_WIN
		//	SetCursorPos((int)(Screen.width * 0.5f),(int)(Screen.height * 0.5f));
		//#endif
	}

	public void RegisterRaycastRect(GameObject go, RectTransform rectToAdd) {
		uiRaycastRects.Add(rectToAdd);
		uiRaycastRectGOs.Add(go);
	}
	
	private void SetCursorPositionMovable() {
		drawTexture.Set(Input.mousePosition.x - offsetX,Screen.height - Input.mousePosition.y - offsetY,cursorSize,cursorSize);
		cursorUIImage.rectTransform.anchoredPosition = new Vector2((Input.mousePosition.x / Screen.width) * canvasRectTransform.sizeDelta.x,(((-1f * (Screen.height - (Input.mousePosition.y))) / Screen.height) * canvasRectTransform.sizeDelta.y) + canvasRectTransform.sizeDelta.y);
		cursorUIImage.rectTransform.sizeDelta = new Vector2(cursorSize * halfFactor,cursorSize * halfFactor); // Pivot is 0.5,0.5
	}
	
	private void SetCursorPositionAtCenter() {
		drawTexture.Set((Screen.width * halfFactor) - offsetX, (Screen.height * halfFactor) - cursorSize - offsetY, cursorSize, cursorSize);
		cursorUIImage.rectTransform.anchoredPosition = new Vector2((halfFactor * canvasRectTransform.sizeDelta.x),(halfFactor * canvasRectTransform.sizeDelta.y));
		cursorUIImage.rectTransform.sizeDelta = new Vector2(cursorSize * halfFactor,cursorSize * halfFactor); // Pivot is 0.5,0.5
	}
	
	private void EnableTooltips() {
		if (toolTipHasText && !_pauseScript.Paused() && !_pauseScript.MenuActive() && (_mouseLookScript.inventoryMode || liveGrenade)) {
			switch(toolTipType) {
				case Handedness.LH:
					tooltipLeft.SetActive(true);
					tooltipLeftText.text = toolTip;
					tooltipTexture = cursorLHTexture;
					break;
				case Handedness.RH:
					tooltipRight.SetActive(true);
					tooltipRightText.text = toolTip;
					tooltipTexture = cursorRHTexture;
					break;
				default: // Handedness.Center
					tooltipCenter.SetActive(true);
					tooltipCenterText.text = toolTip;
					tooltipTexture = cursorDNTexture;
					break;
			}
		} else {
			DisableTooltips();
		}
	}
	
	private void DisableTooltips() {
		tooltipCenter.SetActive(false);
		tooltipLeft.SetActive(false);
		tooltipRight.SetActive(false);
	}
	
	private void DisableLiveGrenadeTooltip() {
		tooltipLiveGrenade.SetActive(false);
	}
	
	private void EnableLiveGrenadeTooltip() {
		tooltipLiveGrenade.SetActive(true); // Display "live" next to cursor
		tooltipLiveGrenadeText.text = _consts.stringTable[586];
	}

	void Update() {
		if (_consts.noHUD) {
			cursorSize = Screen.width * cursorScreenPercentage * 0.1f;// 1 pixel "beauty" cursor.
		} else {
			cursorSize = Screen.width * cursorScreenPercentage;
		}
		
		cursorCanvasRectTransform.sizeDelta = canvasRectTransform.sizeDelta;
		offsetX = cursorSize * halfFactor;
		offsetY = offsetX;
		cursorPosition = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
		cursorPosition.x = Mathf.Clamp(cursorPosition.x,0,Screen.width);
		cursorPosition.y = Mathf.Clamp(cursorPosition.y,0,Screen.height);
		lastMousePos = Input.mousePosition;
		UpdateSafeZone();
		UpdateEventSystemPointerStatus();
		CheckIfOutOfScreenBounds();
		UpdateInventoryAddHelper();

		// Maintain cursor mode.
		if (_pauseScript.Paused() || _pauseScript.MenuActive()) {
			Cursor.lockState = CursorLockMode.None;
		} else if (_mouseLookScript.inventoryMode) {
// 			#if UNITY_EDITOR
				Cursor.lockState = CursorLockMode.None;
// 			#else	
// 				Cursor.lockState = CursorLockMode.Confined;
// 			#endif

			if (_guiState.overButton || _guiState.overButtonType != ButtonType.None) {
				_guiState.isBlocking = true;
			}
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			_guiState.isBlocking = false;
		}
		
		bool hideCursorForMinigame = false;
		if (_miniGameCursor != null) {
			if (_miniGameCursor.mouseOverPanel) hideCursorForMinigame = true;
		}

		if (_pauseScript.Paused() || _pauseScript.MenuActive()) {
            // Pause / Menu Cursor
 			SetCursorPositionMovable();
			DisableTooltips();
			DisableLiveGrenadeTooltip();
			cursorImage = cursorGUI;
			if (!cursorUIImage.gameObject.activeSelf) cursorUIImage.gameObject.SetActive(true);
			if (cursorUIImage.sprite != cursorImage) cursorUIImage.sprite = cursorImage;
			return;
		}

		if (hideCursorForMinigame) {
			if (cursorUIImage.gameObject.activeSelf) cursorUIImage.gameObject.SetActive(false);
        } else {
			if (!cursorUIImage.gameObject.activeSelf) cursorUIImage.gameObject.SetActive(true);
		}
		
		if (_mouseLookScript.inventoryMode) {
            // Inventory Mode Cursor
 			SetCursorPositionMovable();
			if (toolTipHasText && _guiState.isBlocking) EnableTooltips();
			else                                         DisableTooltips();
			
			if (liveGrenade) EnableLiveGrenadeTooltip();
			else             DisableLiveGrenadeTooltip();
			
			if (_mouseLookScript.inCyberSpace) {
				if (_guiState.isBlocking) {
					if (toolTipHasText) {
						cursorImage = tooltipTexture;
					} else {						
						cursorImage = cursorGUI;
					}
				} else {
					cursorImage = cyberspaceCursor;
				}
				
				DisableLiveGrenadeTooltip();
			} else {
				if (_mouseLookScript.vmailActive)
				{
					cursorImage = _texturesStorage.GetItemFrobIcon(108); // vmail
				} else if (_guiState.isBlocking && !_mouseLookScript.holdingObject) {
					cursorImage = toolTipHasText ? tooltipTexture : cursorGUI;
				} else if (_mouseLookScript.holdingObject && _mouseLookScript.heldObjectIndex >= 0) {
					cursorImage = _mouseLookScript.holdingObject && _mouseLookScript.heldObjectIndex >= 0 ? 
						_texturesStorage.GetItemFrobIcon(_mouseLookScript.heldObjectIndex) : GetWeaponCursor();
				} 
			}
        } else {
			// Shoot Mode Cursor
			SetCursorPositionAtCenter();
			DisableTooltips();
			if (liveGrenade) EnableLiveGrenadeTooltip();
			else             DisableLiveGrenadeTooltip();
			
			if (_mouseLookScript.inCyberSpace) {				
				cursorImage = cyberspaceCursor;
				DisableLiveGrenadeTooltip();
			} else {
				if (_mouseLookScript.holdingObject && _mouseLookScript.heldObjectIndex >= 0)
				{
					cursorImage = _texturesStorage.GetItemFrobIcon(_mouseLookScript.heldObjectIndex);
				} else {
					cursorImage = GetWeaponCursor();
				}
			}
        }
		
		// Actually set the cursor texure now:
		if (cursorUIImage.sprite != cursorImage) cursorUIImage.sprite = cursorImage;
	}
	
	private Sprite GetWeaponCursor()
	{
		return _texturesStorage.GetWeaponCursor(_weaponCurrent.weaponIndex);
	}

	void UpdateSafeZone() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		if (cursorPosition.x < (0.96925f * Screen.width) && cursorPosition.x > (0.029282f * Screen.width)
			&& cursorPosition.y > (0.13541f * Screen.height) && cursorPosition.y < (0.70703f * Screen.height)) {
			_guiState.isBlocking = false; // in the safe zone!
		}
	}

	void UpdateEventSystemPointerStatus() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		pev.position = cursorPosition;
		graphicCastResults.Clear();
		if (!_mouseLookScript.inventoryMode) return;
		
		raycaster.Raycast(pev, graphicCastResults);
		if (graphicCastResults.Count > 0) {
			_guiState.isBlocking = true;
			EventSystem.current.SetSelectedGameObject(graphicCastResults[0].gameObject);
			EventTrigger evt = graphicCastResults[0].gameObject.GetComponent<EventTrigger>();
			if (evt != null) {
				RectTransform recTr = graphicCastResults[0].gameObject.GetComponent<RectTransform>();
				if (recTr != null) {
					if (RectTransformUtility.RectangleContainsScreenPoint(recTr,cursorPosition,uiCameraCam)) {
						evt.OnPointerEnter(pev);
					} else {
						evt.OnPointerExit(pev);
					}
				}
			}
			if (Input.GetMouseButtonDown(0)) {
				ExecuteEvents.Execute(graphicCastResults[0].gameObject, pev, ExecuteEvents.submitHandler);
			}
		} else {
			_guiState.isBlocking = false;
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	void CheckIfOutOfScreenBounds() {
		if (_pauseScript.MenuActive()) return;
		if (_pauseScript.Paused()) return;

		if (cursorPosition.y > Screen.height || cursorPosition.y < 0
			|| cursorPosition.x < 0 || cursorPosition.x > Screen.width) {
			_guiState.isBlocking = true; // outside the screen, don't shoot we're innocent!
		}
	}

	void UpdateInventoryAddHelper() {
		if (_pauseScript.Paused()) return;
		if (_pauseScript.MenuActive()) return;

		if (cursorPosition.y > (0.13541f*Screen.height)
			&& cursorPosition.y < (0.70703f*Screen.height)
			&& cursorPosition.x < (0.96925f*Screen.width)
			&& cursorPosition.x > (0.029282f*Screen.width)) {
			_guiState.isBlocking = false; // in the safe zone!
		}

		if (_mouseLookScript.inventoryMode && _mouseLookScript.holdingObject) {
			// Be sure to pass the camera to the 3rd parameter if using
			// "Screen Space - Camera" on the Canvas, otherwise use "null"
			if (RectTransformUtility.RectangleContainsScreenPoint(centerMFDPanel,cursorPosition,uiCameraCam)) {
				if (!inventoryAddHelper.activeInHierarchy) inventoryAddHelper.SetActive(true);
				_guiState.isBlocking = true;
			} else {
				if (inventoryAddHelper.activeInHierarchy) inventoryAddHelper.SetActive(false);
				if (justDroppedItemInHelper) {
					_guiState.ClearOverButton();
					justDroppedItemInHelper = false; // only disable blocking state once, not constantly
				}
			}
		} else {
			if (justDroppedItemInHelper) {
				justDroppedItemInHelper = false; // only disable blocking state once, not constantly
				inventoryAddHelper.SetActive(false);
				_guiState.ClearOverButton();
			}
		}
	}

	public Vector3 GetCursorScreenPointForRay() {
		Vector3 retval = cursorRectTransform.anchoredPosition3D; // retval = new Vector2(426.6665,240);
		retval.x = (retval.x / canvasRectTransform.sizeDelta.x) * Screen.width; // retval.x = (426.6665 / 853.7501) * 1366
		retval.y = (retval.y / canvasRectTransform.sizeDelta.y) * Screen.height; // retval.y = ((240 - (1366 * 0.05)) / 480) * 1
		if (retval.x > Screen.width) retval.x = Screen.width;
		if (retval.x < 0) retval.x = 0;
		if (retval.y > Screen.height) retval.y = Screen.height;
		if (retval.y < 0) retval.y = 0;
		return retval;
	}
}
