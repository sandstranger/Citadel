using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Citadel.Game;
using Cysharp.Threading.Tasks;
using Zenject;

public class MouseLookScript : MonoBehaviour {
    // External references
	public GameObject player;
	public GameObject canvasContainer;
	public GameObject compassContainer;
	public GameObject automapContainerLH;
	public GameObject automapContainerRH;
	public GameObject compassMidpoints;
	public GameObject compassLargeTicks;
	public GameObject compassSmallTicks;
    [Tooltip("Game object that houses the MFD tabs")] public GameObject tabControl;
	[Tooltip("Text in the data tab in the MFD that displays when searching an object containing no items")] public Text dataTabNoItemsText;
	public LogContentsButtonsManager logContentsManager;
	public GameObject[] hardwareButtons;
	public PuzzleWire puzzleWire;
	public PuzzleGrid puzzleGrid;
	public GameObject shootModeButton;
	public HealthManager hm;
	public GameObject playerRadiationTreatmentFlash;
	public Vector2 lastMousePos;
	
    // Internal references
    [HideInInspector] public bool inventoryMode;
	public bool holdingObject;
    [HideInInspector] public Vector2 cursorHotspot;
    [HideInInspector] public Vector3 cameraFocusPoint;
	[HideInInspector] public GameObject currentButton;
	[HideInInspector] public GameObject currentSearchItem;
    public int heldObjectIndex; // save
	public int heldObjectCustomIndex; // save
	public int heldObjectAmmo; // save
	public int heldObjectAmmo2; // save
	public bool heldObjectLoadedAlternate; // save
	[HideInInspector] public bool firstTimePickup;
	[HideInInspector] public bool firstTimeSearch;
	public bool grenadeActive;
	public bool inCyberSpace;
    public float yRotation;
	[HideInInspector] public Vector3 cyberspaceReturnPoint; // save
	[HideInInspector] public Vector3 cyberspaceReturnCameraLocalRotation; // save
	[HideInInspector] public Vector3 cyberspaceReturnPlayerCapsuleLocalRotation; // save
	[HideInInspector] public int cyberspaceReturnLevel; // save
	[HideInInspector] public Vector3 cyberspaceRecallPoint; // save
	[HideInInspector] public bool vmailActive = false;
	[HideInInspector] public bool geniusActive = false;
	private float keyboardTurnSpeed = 15f; // Speed multiplier for turning the view with the keyboard.
    private float tossOffset = 0.5f; // Distance from player origin to spawn objects when tossing them.
    private float tossForce = 10f; // Force given to spawned objects when tossing them.
	private float[] cameraDistances;
    [HideInInspector] public float xRotation; // save
    private float zRotation;
    private float yRotationV;
    private float xRotationV;
    private float zRotationV;
    private float currentZRotation;
    private string mlookstring1;
    [HideInInspector,Inject] public Camera playerCamera;
	private Quaternion tempQuat;
	private Vector3 tempVec;
    private RaycastHit tempHit;
	private Vector3 cameraRecoilLerpPos;
	private float cyberSpinSensitivity = 0.6f;
	private float shakeFinished;
	private float shakeForce;
	private string f9 = "f9";
	private string f6 = "f6";
	private string qsavename = "quicksave";
	private string mouseX = "Mouse X";
	private string mouseY = "Mouse Y";
	private Vector3 cursorPoint;
	private float headBobTimeShift;
	private float headBobX;
	private float headBobY;
	private float headBobZ;
	private float rotSpeedX = 0f;
	private float rotSpeedY = 0f;
	private Transform playerCapsuleTransform;
	[HideInInspector] public float returnFromCyberspaceFinished;
	private float dropFinished;
	[HideInInspector] public float randomShakeFinished;
	[HideInInspector] public float randomKlaxonFinished;
	public Vector2 debugRT;
	public Vector2 debugAng;
	public float joyXStartTime;
	public float joyYStartTime;
	public float joyXSignLast;
	public float joyYSignLast;
	public float headBobShiftFinished;
	private float bobTarget;
	private float headBobXVel;
	private float headBobYVel;
	[Inject] private LevelManager _levelManager;
	[Inject] private Const _consts;
	[Inject] private MFDManager _mfdManager;
	[Inject] private Automap _automap;
	[Inject] private GUIState _guiState;
	[Inject] private GetInput _getInput;
	[Inject] private Inventory _inventory;
	[Inject] private MainMenuHandler _mainMenuHandler;
	[Inject] private MissionTimer _missionTimer;
	[Inject] private MouseCursor _mouseCursor;
	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerMovement _playerMovement;
	[Inject] private WeaponFire _weaponFire;
	[Inject] private WeaponCurrent _weaponCurrent;
	[Inject] private DynamicCulling _dynamicCulling;
	[Inject] private readonly ITexturesStorage _texturesStorage;
	[Inject] private readonly IResourcesLoader _resourcesLoader;
	[Inject] private readonly Utils _utils;
	
	private int _prefabIndexToInstantiate;
	
	private static readonly StringBuilder s1 = new StringBuilder(500 * 1024);
    
    void Start (){
		ResetHeldItem();
		Cursor.lockState = CursorLockMode.None;
		inventoryMode = false; // Start with inventory mode turned off.
		if (Application.platform == RuntimePlatform.Android) {
			ForceInventoryMode();
			shootModeButton.SetActive(true);
		} else {
			shootModeButton.SetActive(false);
		}

		cameraDistances = new float[32];
		SetCameraCullDistances();
		playerCamera.depthTextureMode = DepthTextureMode.Depth;
		grenadeActive = false;
		yRotation = 0;
		xRotation = 0;
		canvasContainer.SetActive(true); // Enable UI.
		firstTimePickup = true;
		firstTimeSearch = true;
		inCyberSpace = false;
		shakeFinished = _pauseScript.relativeTime;
		returnFromCyberspaceFinished = 0;
		dropFinished = 0;

		// PlayerCapsule
		// -> LeanTransform
        // -> -> MainCamera: MouseLookScript component.
		playerCapsuleTransform = transform.parent.transform.parent.transform;

		randomShakeFinished = _pauseScript.relativeTime;
		randomKlaxonFinished = _pauseScript.relativeTime;
		headBobShiftFinished = _pauseScript.relativeTime;
		bobTarget = 0.3f;
    }
    
    void OnPreCull() {
		_dynamicCulling.Cull(false); // Update dynamic culling system.
	}

	async void Update() {
		// Allow quick load straight from the menu or pause.
		if (Input.GetKeyUp(f9)) {
			if (inCyberSpace) {
				_consts.sprint(_consts.stringTable[1023]); // "Cannot load in cyberspace"
				return;
			}

			_mainMenuHandler.LoadGame(7);
		}

        if (_pauseScript.MenuActive()) {
			// Ignore mouselook and turn off camera when main menu is up.
			if (!_mainMenuHandler.fileBrowserOpen) Cursor.visible = false;
			else Cursor.visible = true;

			if (playerCamera.enabled) playerCamera.enabled = false;
			return;
		}

		if (_pauseScript.Paused()) return;
		if (_playerMovement.ressurectingFinished > _pauseScript.relativeTime) return;

		Utils.EnableCamera(playerCamera);

		// Unpaused, normal functions::
		// ====================================================================
		if (Input.GetKeyUp(f6)) {
			if (inCyberSpace) {
				_consts.sprint(_consts.stringTable[602]); // Cannot save in cyberspace
				return;
			}
			
			if (_missionTimer.timesUP) {
				return;
			}

			_consts.StartSave(7,qsavename);
		}

		// Toggle inventory mode<->shoot mode
		if(_getInput.ToggleMode()) ToggleInventoryMode();

		if (_consts.questData.SelfDestructActivated
			&& LevelManager.CurrentLevel != 13   // Not Cyberspace
			&& LevelManager.CurrentLevel != 9) { // Not the bridge, separated

			if (randomShakeFinished < _pauseScript.relativeTime) {
				randomShakeFinished = _pauseScript.relativeTime
				                      + UnityEngine.Random.Range(5f,20f);
				ScreenShake(3f,2f);
			}
			
			if (randomKlaxonFinished < _pauseScript.relativeTime) {
				randomKlaxonFinished = _pauseScript.relativeTime
				                       + UnityEngine.Random.Range(10f,20f);

				Utils.PlayUIOneShotSavable(_consts,104); // klaxon
			}
		}

		RecoilAndRest(); // Spring Back to Rest from Recoil
		keyboardTurnSpeed = 15f * _consts.MouseSensitivity;
		if (Application.platform == RuntimePlatform.Android) {
			if (_consts.MouseSensitivity == 100f) _consts.MouseSensitivity = 20f;
		} 
		KeyboardTurn();
		KeyboardLookUpDn();
		if (inCyberSpace) { // Barrel roll!
			if (_getInput.LeanLeft()) {
				playerCapsuleTransform.RotateAround(
					playerCapsuleTransform.transform.position,
					playerCapsuleTransform.transform.forward,
					cyberSpinSensitivity * Time.deltaTime * 100f
				);
			}

			if (_getInput.LeanRight()) {
				playerCapsuleTransform.RotateAround(
					playerCapsuleTransform.transform.position,
					playerCapsuleTransform.transform.forward,
					cyberSpinSensitivity * Time.deltaTime * -1f * 100f
				);
			}
		} else {
			if (compassContainer.activeInHierarchy) {
				// Update automap player icon orientation.
				compassContainer.transform.rotation =
					Quaternion.Euler(0f, -yRotation + 180f, 0f);
			}
		}

		if (!inventoryMode) Mouselook(); // Only do mouselook in Shoot Mode.
		if(_getInput.Use()) await Frob(); // Frob what is under our cursor.
	}

	public async UniTask Frob() {
		if (vmailActive && !inCyberSpace) {
			_inventory.DeactivateVMail(); vmailActive = false;
			return;
		}

		if (!_guiState.isBlocking && !inCyberSpace) {
			if (dropFinished < Time.time) {
				currentButton = null; // Force this to reset.
				if (holdingObject) {
					if (!FrobWithHeldObject()) await DropHeldItem();
				} else FrobEmptyHanded();
			}
		} else {
			//We are holding cursor over the GUI
			if (holdingObject && !inCyberSpace) {
				AddItemToInventory(heldObjectIndex,heldObjectCustomIndex);
				_mouseCursor.liveGrenade = false;
				ResetHeldItem();
			} else InventoryButtonUse();
		}
	}

	public void Mouselook() {
		if (returnFromCyberspaceFinished >= Time.time) return; // Not yet.

		returnFromCyberspaceFinished = 0;

		// PROCESS INPUT SIGNALS
		// ----------------------------------------------------------------
		float angX = 0f; // Angle change for X.
		float angY = 0f; // Angle change for Y.
		// Handle mouse input from a standard mouse.
		float deltaX = Input.GetAxisRaw(mouseX) * _consts.MouseSensitivity * _consts.GraphicsFOV;
		float deltaY = Input.GetAxisRaw(mouseY) * _consts.MouseSensitivity * _consts.GraphicsFOV;

		// Handle thumbstick input from a controller.
		Vector2 rightThumbstick = new Vector2(Input.GetAxisRaw("JoyAxis4"), // Horizontal Left < 0, Right > 0
											  Input.GetAxisRaw("JoyAxis5") * -1f); // Vertical Down > 0, Up < 0 Inverted
		// X
		float signX = rightThumbstick.x > 0.0f ? 1.0f
					  : rightThumbstick.x < 0.0f ? -1.0f : 0f;
		if (signX != joyXSignLast) {
			joyXStartTime = Time.time; // Zero crossing.
			rotSpeedX = 0f; // Reset integrator windup.
		}

		joyXSignLast = signX;
		rightThumbstick.x *= _consts.MouseSensitivity * 20f;
		rotSpeedX += rightThumbstick.x; // Integrate to give fine initial.
		if (rightThumbstick.x == 0f) rotSpeedX = 0f;
		if (rotSpeedX != 0f) deltaX = rotSpeedX;

		// Y
		float signY = rightThumbstick.y > 0.0f ? 1.0f
					  : rightThumbstick.y < 0.0f ? -1.0f : 0f;
		if (signY != joyYSignLast) {
			joyYStartTime = Time.time; // Zero crossing.
			rotSpeedY = 0f; // Reset integrator windup.
		}

		joyYSignLast = signY;
		rightThumbstick.y *= _consts.MouseSensitivity * 20f;
		rotSpeedY += rightThumbstick.y; // Integrate to give fine initial.
		if (rightThumbstick.y == 0) rotSpeedY = 0f;
		if (rotSpeedY != 0f) deltaY = rotSpeedY;

		// Apply input delta from mouse or controller to angles.
		if (signX != 0f || signY != 0f) {
			// Using controller, use integrated deltas.
			angX = deltaX;
			angY = deltaY;
		} else {
			// Using mouse, map input to deg per screen half / screen.
			angX = deltaX * ((_consts.GraphicsFOV / 2f) / Screen.width / 2f);
			angY = deltaY * ((_consts.GraphicsFOV / 2f) / Screen.height / 2f);
		}

		// For my inspector viewing pleasure.
		debugRT = new Vector2(deltaX,deltaY);
		debugAng = new Vector2(angX,angY);

		// High pass filter to prevent jumpy behavior.
		if (angX > _consts.GraphicsFOV) angX = _consts.GraphicsFOV;
		if (angY > _consts.GraphicsFOV) angY = _consts.GraphicsFOV;

		// APPLY MOUSE LOOK
		// --------------------------------------------------------------------
		if (inCyberSpace) {
			// CYBER MOUSE LOOK
			if (_consts.InputInvertCyberspaceLook) xRotation = -angY;
			else xRotation = angY;

			xRotation = Clamp0360(xRotation); // Limit up/down to within 360°.
			yRotation = angX;
			playerCapsuleTransform.RotateAround(
				playerCapsuleTransform.transform.position,
				playerCapsuleTransform.transform.up,yRotation
			);

			playerCapsuleTransform.RotateAround(
				playerCapsuleTransform.transform.position,
				playerCapsuleTransform.transform.right,-xRotation
			);
		} else {
			// NORMAL MOUSE LOOK
			if (_consts.InputInvertLook) xRotation += angY;
			else xRotation -= angY;

			xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limit up/down.
			yRotation += angX;

			// Apply the mouselook. Left/Right component applied to capsule.
			playerCapsuleTransform.localRotation = Quaternion.Euler(0f,
																	yRotation,
																	0f);

			// Up down component only applied to camera.  Must be 0 for others
			// or else movement will go in wrong direction!
			transform.localRotation = Quaternion.Euler(xRotation,0f,0f);
			float xCenter = (float)Screen.width * 0.5f;
			float yCenter = (float)Screen.height * 0.5f;
			float xOffset = ((float)Input.mousePosition.x - xCenter);
			float yOffset = ((float)Input.mousePosition.y - yCenter);
			if (xOffset > 2f || yOffset > 2f) {
				MouseCursor.SetCursorPosInternal((int)xCenter,(int)yCenter);
			}
		}
	}

	public void EnterCyberspace(Vector3 entryPoint) {
		cyberspaceRecallPoint = entryPoint;
		playerRadiationTreatmentFlash.SetActive(true);
		cyberspaceReturnPoint = _playerMovement.transform.position;
		cyberspaceReturnCameraLocalRotation = transform.localRotation.eulerAngles;
		cyberspaceReturnPlayerCapsuleLocalRotation = playerCapsuleTransform.localRotation.eulerAngles;
		cyberspaceReturnLevel = LevelManager.CurrentLevel;
		_mfdManager.EnterCyberspace();
		_levelManager.LoadLevel(13,cyberspaceRecallPoint);
		_playerMovement.inCyberSpace = true;
		_playerMovement.leanCapsuleCollider.enabled = false;
		hm.inCyberSpace = true;
		inCyberSpace = true;
		playerCamera.useOcclusionCulling = false;
		_mfdManager.DrawTicks(true);
		SetCameraCullDistances();
		Utils.PlayUIOneShotSavable(_consts,81); // cyber
	}

	public void ExitCyberspace() {
		playerRadiationTreatmentFlash.SetActive(true);
		_mfdManager.ExitCyberspace();
		_levelManager.LoadLevel(cyberspaceReturnLevel,cyberspaceReturnPoint);

		// Left/right component applied to capsule.
		playerCapsuleTransform.localRotation = Quaternion.Euler(0f,
			cyberspaceReturnPlayerCapsuleLocalRotation.y,0f);

		transform.localRotation = // Up down component applied to camera
			Quaternion.Euler(cyberspaceReturnCameraLocalRotation.x,
							 cyberspaceReturnCameraLocalRotation.y,
							 cyberspaceReturnCameraLocalRotation.z);

		xRotation = cyberspaceReturnCameraLocalRotation.x;
		yRotation = cyberspaceReturnPlayerCapsuleLocalRotation.y;

		returnFromCyberspaceFinished = Time.time + 0.1f; // Prevent mouselook
														 // messing it up.
		_playerMovement.inCyberSpace = false;
		_playerMovement.rbody.linearVelocity = _consts.vectorZero;
		_playerMovement.leanCapsuleCollider.enabled = true;
		hm.inCyberSpace = false;
		inCyberSpace = false;
		playerCamera.useOcclusionCulling = true;
		_consts.decoyActive = false;
		_mfdManager.DrawTicks(true);
		Utils.PlayUIOneShotSavable(_consts,81); // cyber
		SetCameraCullDistances();
	}

	// Draw line from cursor - used for projectile firing, e.g. magpulse/stugngun/railgun/plasma
	public void SetCameraFocusPoint() {
		cursorPoint = _mouseCursor.GetCursorScreenPointForRay();
        if (Physics.Raycast(playerCamera.ScreenPointToRay(cursorPoint), out tempHit, Mathf.Infinity)) cameraFocusPoint = tempHit.point;
	}

	// Clamp cyberspace up/down look rotation to with in +/- 360f.
	float Clamp0360(float val) {
		return (val - (Mathf.CeilToInt(val*(1f/360f)) * 360f)); // Subtract out 360 times the number of times 360 fits within val.
	}

	public void SetCameraCullDistances() {
		if (cameraDistances == null) cameraDistances = new float[32];
		else if (cameraDistances.Length < 32) cameraDistances = new float[32];
		
		if (inCyberSpace) {
			for (int i=0;i<32;i++) { cameraDistances[i] = 3350f; } // Increased from 2400 to fit the Saturn's rings without overlapping with star sphere.
		} else {
			for (int i=0;i<32;i++) { cameraDistances[i] = 79f; } // Can't see further than this.  31 * 2.56 - player radius 0.48 = 78.88f rounded up to be careful..longest line of sight is the crawlway on level 6
			cameraDistances[0]  = 45.1f; // Default, most static objects and some dynamic.
			cameraDistances[1]  = 16f;   // TransparentFX, for mist and drips
			cameraDistances[4]  = 30f;   // Water, used for effects like steam
										 // and well, spraying water.
			cameraDistances[14] = 30f;   // PhysObjects, patches, carts, barrels
			cameraDistances[15] = 3350f; // Sky is visible, only exception.
		}
		playerCamera.layerCullDistances = cameraDistances; // Cull anything beyond 79f except for sky layer.
	}
	
	void TouchLook() {
		return;
	    Vector2 rightTouchstick = Vector2.zero;
	    if (rightTouchstick.x < 0f) {
			yRotation -= keyboardTurnSpeed * rightTouchstick.x;
			playerCapsuleTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
		} else if (rightTouchstick.x > 0f) {
			yRotation += keyboardTurnSpeed * rightTouchstick.x;
			playerCapsuleTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
		}
		
		if (rightTouchstick.y < 0f) {
			if ((inCyberSpace && _consts.InputInvertCyberspaceLook) || (!inCyberSpace && _consts.InputInvertLook))
				xRotation -= keyboardTurnSpeed;
			else
				xRotation += keyboardTurnSpeed;

			if (!inCyberSpace) xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Limit up and down angle.
			transform.localRotation = Quaternion.Euler(xRotation,0f,
													   transform.localRotation.z);
		} else if (rightTouchstick.y > 0f) {
			if ((inCyberSpace && _consts.InputInvertCyberspaceLook) || (!inCyberSpace && _consts.InputInvertLook))
				xRotation += keyboardTurnSpeed * rightTouchstick.y;
			else
				xRotation -= keyboardTurnSpeed * rightTouchstick.y;

			if (!inCyberSpace) xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Limit up and down angle.
			transform.localRotation = Quaternion.Euler(xRotation, 0f,
													   transform.localRotation.z);
		}
	}

	void KeyboardTurn() {
		if (inCyberSpace) {
			float angX = 0f;
			if (_getInput.TurnLeft()) {
				// Modulate input to deg per screen half / screen.
				angX = -keyboardTurnSpeed * 18f * ((_consts.GraphicsFOV / 2f) / Screen.width / 2f);
				yRotation = angX;
				playerCapsuleTransform.RotateAround(
					playerCapsuleTransform.transform.position,
					playerCapsuleTransform.transform.up,yRotation
				);
			} else if (_getInput.TurnRight()) {
				angX = keyboardTurnSpeed * 18f * ((_consts.GraphicsFOV / 2f) / Screen.width / 2f);
				yRotation = angX;
				playerCapsuleTransform.RotateAround(
					playerCapsuleTransform.transform.position,
					playerCapsuleTransform.transform.up,yRotation
				);
			}
		} else {
			if (_getInput.TurnLeft()) {
				yRotation -= keyboardTurnSpeed;
				playerCapsuleTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
			} else if (_getInput.TurnRight()) {
				yRotation += keyboardTurnSpeed;
				playerCapsuleTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
			}
		}
	}

	void KeyboardLookUpDn() {
		if (inCyberSpace) {
			float angY = 0f;
			if (_getInput.LookDown()) {
				// Modulate input to deg per screen half / screen.
				angY = -keyboardTurnSpeed * 18f * ((_consts.GraphicsFOV / 2f) / Screen.height / 2f);
				if (_consts.InputInvertCyberspaceLook) xRotation = -angY;
				else xRotation = angY;
			
				xRotation = Clamp0360(xRotation); // Limit up/down to within 360°.
				playerCapsuleTransform.RotateAround(
					playerCapsuleTransform.transform.position,
					playerCapsuleTransform.transform.right,-xRotation
				);
			} else if (_getInput.LookUp()) {
				angY = keyboardTurnSpeed * 18f * ((_consts.GraphicsFOV / 2f) / Screen.height / 2f);
				if (_consts.InputInvertCyberspaceLook) xRotation = -angY;
				else xRotation = angY;
			
				xRotation = Clamp0360(xRotation); // Limit up/down to within 360°.
					playerCapsuleTransform.RotateAround(
					playerCapsuleTransform.transform.position,
					playerCapsuleTransform.transform.right,-xRotation
				);
			}
		} else {
			// Cyberspace...more like a plane so giving the option to invert it separately.
			if (_getInput.LookDown()) {
				if ((inCyberSpace && _consts.InputInvertCyberspaceLook) || (!inCyberSpace && _consts.InputInvertLook))
					xRotation -= keyboardTurnSpeed;
				else
					xRotation += keyboardTurnSpeed;

				if (!inCyberSpace) xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Limit up and down angle.
				transform.localRotation = Quaternion.Euler(xRotation,0f,
														transform.localRotation.z);
			} else if (_getInput.LookUp()) {
				if ((inCyberSpace && _consts.InputInvertCyberspaceLook) || (!inCyberSpace && _consts.InputInvertLook))
					xRotation += keyboardTurnSpeed;
				else
					xRotation -= keyboardTurnSpeed;

				if (!inCyberSpace) xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Limit up and down angle.
				transform.localRotation = Quaternion.Euler(xRotation, 0f,
														transform.localRotation.z);
			}
		}
	}

	bool RayOffset() {
		bool successfulRay = false;
		successfulRay = Physics.Raycast(playerCamera.ScreenPointToRay(cursorPoint), out tempHit,Const.frobDistance,_consts.layerMaskPlayerFrob);
// 		Debug.DrawRay(playerCamera.ScreenPointToRay(cursorPoint).origin,playerCamera.ScreenPointToRay(cursorPoint).direction * Const.frobDistance, Color.green,1f,true);
		if (successfulRay) {
			successfulRay = (tempHit.collider != null);
			if (successfulRay) {
				successfulRay = (tempHit.collider.CompareTag("Usable") || tempHit.collider.CompareTag("Searchable") || tempHit.collider.CompareTag("NPC"));
			}
		}
		return successfulRay;
	}

	bool TargetIDFrob(Vector3 cP) {
		if (Application.platform == RuntimePlatform.Android) {
			if (inCyberSpace) {
				_weaponFire.FireCyberWeapon();
				return true;
			}
		}

		if (inCyberSpace) return false;

		float dist = TargetID.GetTargetIDSensingRange(_inventory,true);
		bool successfulRay = Physics.Raycast(playerCamera.ScreenPointToRay(cP),
											 out tempHit,dist,
											 _consts.layerMaskPlayerTargetIDFrob);

		// Success here means hit a useable something.
		// If a ray hits a wall or other unusable something, that's not success
		if (successfulRay) {
			successfulRay = (tempHit.collider != null);
			if (successfulRay) {
				successfulRay = tempHit.collider.CompareTag("NPC");
			}
		}

		if (!successfulRay) return false;

		// Say we can't use enemy and give enemy name.
		AIController aic = tempHit.collider.gameObject.GetComponent<AIController>();
		if (aic == null) return false;

		HealthManager hm = Utils.GetMainHealthManager(tempHit);
		if (hm != null) {
			if (hm.health <= 0 && aic.searchColliderGO != null) {
				currentSearchItem = aic.searchColliderGO;
				SearchObject(currentSearchItem.GetComponent<SearchableItem>().lookUpIndex);
				return true; // True = do and check nothing further this frob.
			}
		}

		if (_inventory.hasHardware[4] && _inventory.hardwareVersion[4] > 1) {
			if (!aic.hasTargetIDAttached) {
				_weaponFire.CreateTargetIDInstance(-1f,aic.healthManager,-1f);
				if (Application.platform != RuntimePlatform.Android) {
					return true;
				}
			}
		}

		if (Application.platform == RuntimePlatform.Android) {
			// Cyber handled just above, normal fire condition only here.
			int constDex = _weaponCurrent.weaponIndex;
			int wepdex = WeaponFire.Get16WeaponIndexFromConstIndex(constDex);
			_weaponFire.StartNormalAttack(wepdex);
			return true;
		}

		// "Can't use <enemy>"
		_consts.sprint(_consts.stringTable[29] + _consts.nameForNPC[aic.index],
					 player);

		return true;
	}

	void FrobEmptyHanded() {
		if (holdingObject) return;

		RaycastHit firstHit;
		float offset = Screen.height * 0.02f;
		cursorPoint = _mouseCursor.GetCursorScreenPointForRay();
		if (TargetIDFrob(cursorPoint)) return;

		Ray castDir = playerCamera.ScreenPointToRay(cursorPoint);
		bool successfulRay = Physics.Raycast(castDir, out tempHit,
											 Const.frobDistance,
											 _consts.layerMaskPlayerFrob);

// 		Debug.DrawRay(playerCamera.ScreenPointToRay(cursorPoint).origin,
// 					  playerCamera.ScreenPointToRay(cursorPoint).direction
// 					    * Const.frobDistance, Color.green,1f,true);

		firstHit = tempHit;
		// Success here means hit a useable something.
		// If a ray hits a wall or other unusable something,
		// that's not success and print "Can't use <something>".
		if (successfulRay) {
			successfulRay = (tempHit.collider != null);
			if (successfulRay) {
				successfulRay = (tempHit.collider.CompareTag("Usable")
								 || tempHit.collider.CompareTag("Searchable"));
			}
		}

		// Shoot rays in a pattern like this
		// * * *
		// * + *
		// * * *

		// In an order like this:
		// 8 3 6
		// 5 1 4
		// 7 2 9
		// To kind of walk around the center point to hopefully minimize rays
		// we try and tighten our lug nuts properly so the wheels don't fall 
		// off this thing.

		if (!successfulRay) { // Try down
			cursorPoint.y -= offset;
			successfulRay = RayOffset();
			cursorPoint.y += offset;
		}
		if (!successfulRay) { // Try up
			cursorPoint.y += offset;
			successfulRay = RayOffset();
			cursorPoint.y -= offset;
		}
		if (!successfulRay) { // Try to the right
			cursorPoint.x += offset;
			successfulRay = RayOffset();
			cursorPoint.x -= offset;
		}
		if (!successfulRay) { // Try to the left
			cursorPoint.x -= offset;
			successfulRay = RayOffset();
			cursorPoint.x += offset;
		}
		if (!successfulRay) { // Try up and to the right
			cursorPoint.x += offset;
			cursorPoint.y += offset;
			successfulRay = RayOffset();
			cursorPoint.x -= offset;
			cursorPoint.y -= offset;
		}
		if (!successfulRay) { // Try down and to the left
			cursorPoint.x -= offset;
			cursorPoint.y -= offset;
			successfulRay = RayOffset();
			cursorPoint.x += offset;
			cursorPoint.y += offset;
		}
		if (!successfulRay) { // Try up and to the left, cupid shuffle
			cursorPoint.x -= offset;
			cursorPoint.y += offset;
			successfulRay = RayOffset();
			cursorPoint.x += offset;
			cursorPoint.y -= offset;
		}
		if (!successfulRay) { // Try down and to the right
			cursorPoint.x += offset;
			cursorPoint.y -= offset;
			successfulRay = RayOffset();
			cursorPoint.x -= offset;
			cursorPoint.y += offset;
		}

		if (!successfulRay) tempHit = firstHit;

		// Okay we've checked first center, then in a box patter of 8, surely
		// we've hit something the player was reasonably aiming at by now.
		if (successfulRay) {
			if (tempHit.collider.CompareTag("Usable")) { // Use
				UseData ud = new UseData ();
				ud.owner = player;
				UseHandler uh = tempHit.collider.gameObject.GetComponent<UseHandler>();
				if (uh != null) {
					uh.Use(ud);
				} else {
					UseHandlerRelay uhr = tempHit.collider.gameObject.GetComponent<UseHandlerRelay>();
					if (uhr != null) {
						if (uhr.referenceUseHandler != null) {
							uhr.referenceUseHandler.Use(ud);
						}
					} else {
						Debug.Log("BUG: Attempting to use a useable without a UseHandler or UseHandlerRelay!");
					}
				}
			} else if (tempHit.collider.CompareTag("Searchable")) { // Search
				currentSearchItem = tempHit.collider.gameObject;
				SearchObject(currentSearchItem.GetComponent<SearchableItem>().lookUpIndex);
			} else {
				_consts.sprint(29); // "Can't use "
			}
		} else { // Frobbed into empty space, so whatever it is is too far.
			if (tempHit.collider != null) {
				// Can't use <something>
				UseName.UseNameSprint(_consts,tempHit.collider.gameObject);
			} else {
				// You are too far away from that
				_consts.sprint(_consts.stringTable[30],player);
			}
		}
	}

	bool FrobWithHeldObject() {
		if (heldObjectIndex < 0) {
			Debug.Log("BUG: Attempting to frob with held object, but "
					  + "heldObjectIndex < 0.");
			return false; // Invalid item will be dropped, wasn't used up.
		}

		bool frobUser = (heldObjectIndex == 54 || heldObjectIndex == 56
						 || heldObjectIndex == 57 || heldObjectIndex == 61
						 || heldObjectIndex == 64 || heldObjectIndex == 92
						 || heldObjectIndex == 93 || heldObjectIndex == 94);

		if (!frobUser) return false;

		cursorPoint = _mouseCursor.GetCursorScreenPointForRay();
		if (!Physics.Raycast(playerCamera.ScreenPointToRay(cursorPoint),
							 out tempHit, Const.frobDistance)) {
			return false; // Can't use it on something, go ahead and drop it.
		}

		// Cannot notify of attempt to frob with different index since this is
		// how we normally drop items.
		GameObject go = tempHit.collider.gameObject;
		if (go == null) return false;
		if (!tempHit.collider.CompareTag("Usable")) return false;

		UseData ud = new UseData();
		ud.owner = player;
		ud.mainIndex = heldObjectIndex;
		ud.customIndex = heldObjectCustomIndex;
		UseHandler uh = go.GetComponent<UseHandler>();
		bool playedSound = false;
		if (uh != null) {
			Utils.PlayUIOneShotSavable(_consts,91); // searchsound
			playedSound = true;
			uh.Use(ud);
			return true; // Item can get absorbed, not dropped.
		}

		UseHandlerRelay uhr = go.GetComponent<UseHandlerRelay>();
		if (uhr != null) {
			
			if (!playedSound) Utils.PlayUIOneShotSavable(_consts,91); // searchsound
			uhr.referenceUseHandler.Use(ud);
			return true; // Item can get absorbed, not dropped.
		}

		Debug.Log("BUG: Attempting to frob use a useable " + go.name
				  + " without a UseHandler or UseHandlerRelay!");

		return false;
	}

	void PutObjectInHand(int useableConstdex, int customIndex, int ammo1,
						 int ammo2, bool loadedAlt, bool fromButton) {
		if (useableConstdex < 0) return;

		holdingObject = true;
		heldObjectIndex = useableConstdex;
		heldObjectCustomIndex = customIndex;
		heldObjectAmmo = ammo1;
		heldObjectAmmo2 = ammo2;
		heldObjectLoadedAlternate = loadedAlt;
		if (fromButton) _guiState.ClearOverButton();
		ForceInventoryMode();
	}

	void RemoveWeapon() {
		// Take weapon out of inventory, removing weapon, remove weapon and any
		// other strings I need to CTRL+F my way to this buggy code!
		WeaponButton wepbut = currentButton.GetComponent<WeaponButton>();
		int indexPriorToRemoval = wepbut.useableItemIndex;
		int am1 = _weaponCurrent.currentMagazineAmount[wepbut.WepButtonIndex];
		_weaponCurrent.currentMagazineAmount[wepbut.WepButtonIndex] = 0;
		int am2 = _weaponCurrent.currentMagazineAmount2[wepbut.WepButtonIndex];
		_weaponCurrent.currentMagazineAmount2[wepbut.WepButtonIndex] = 0;
		bool loadAlt = false;
		if (am2 > 0) loadAlt = true;
		PutObjectInHand(indexPriorToRemoval,-1,am1,am2,loadAlt,true);
		_weaponCurrent.RemoveWeapon(wepbut.WepButtonIndex);
		_inventory.RemoveWeapon(wepbut.WepButtonIndex);
		_mfdManager.SetAmmoIcons(-1,false) ; // Clear the ammo icons.
		_mfdManager.HideAmmoAndEnergyItems();
		wepbut.useableItemIndex = -1;
		wepbut = _mfdManager.wepbutMan.wepButtonsScripts[0];
		_weaponCurrent.WeaponChange(wepbut.useableItemIndex,
									 wepbut.WepButtonIndex);
	}

	// Because Unity does not see fit for their Button class to support right
	// click behavior...or any other reasonable mouse button interaction.
	void InventoryButtonUse() {
		if (holdingObject) return;
		if (!_guiState.overButton) return;
		if (_guiState.overButtonType == ButtonType.None) return;
		if (currentButton == null) return;

		int indexPriorToRemoval = -1;
		int customIndexPrior = -1;
		switch(_guiState.overButtonType) {
			case ButtonType.Weapon: RemoveWeapon(); break;
			case ButtonType.Grenade:
				GrenadeButton grenbut = currentButton.GetComponent<GrenadeButton>();
				indexPriorToRemoval = grenbut.useableItemIndex;
				_inventory.grenAmmo[grenbut.GrenButtonIndex]--;
				_inventory.GrenadeCycleDown();
				//_inventory.grenadeCurrent = -1; This was up here, and seemed fine.  Might need to revert line 473 add.
				if (_inventory.grenAmmo[grenbut.GrenButtonIndex] <= 0) {
					_inventory.grenAmmo[grenbut.GrenButtonIndex] = 0;
					_inventory.grenadeCurrent = -1;
					for (int i = 0; i < 7; i++) {
						if (_inventory.grenAmmo[i] > 0) {
							_inventory.grenadeCurrent = i;
						}
					}

					_mfdManager.SendInfoToItemTab(_inventory.grenadeCurrent);
					if (_inventory.grenadeCurrent < 0) {
						_inventory.grenadeCurrent = 0;
					}
				}

				grenadeActive = true;
				PutObjectInHand(indexPriorToRemoval,-1,0,0,false,true);
				break;
			case ButtonType.Patch:
				PatchButton patbut = currentButton.GetComponent<PatchButton>();
				indexPriorToRemoval = patbut.useableItemIndex;
				_inventory.patchCounts[patbut.PatchButtonIndex]--;
				if (_inventory.patchCounts[patbut.PatchButtonIndex] <= 0) {
					_inventory.patchCounts[patbut.PatchButtonIndex] = 0;
					_inventory.patchCurrent = -1;
					_guiState.ClearOverButton();
					for (int i = 0; i < 7; i++) {
						if (_inventory.patchCounts[i] > 0) _inventory.patchCurrent = i;
					}
					_mfdManager.SendInfoToItemTab(_inventory.patchCurrent);
					if (_inventory.patchCurrent < 0) {
						_inventory.patchCurrent = 0;
					}
				}
				PutObjectInHand(indexPriorToRemoval,-1,0,0,false,true);
				break;
			case ButtonType.GeneralInv:
				GeneralInvButton genbut = 
					currentButton.GetComponent<GeneralInvButton>();

				// Access Cards button
				if (genbut.GeneralInvButtonIndex == 0) {
					_mfdManager.OpenLastItemSide();
					_mfdManager.SendInfoToItemTab(81);
					return;
				}

				indexPriorToRemoval = genbut.useableItemIndex;
				customIndexPrior = genbut.customIndex;
				_inventory.generalInventoryIndexRef[genbut.GeneralInvButtonIndex] = -1;
				_inventory.generalInvCurrent = -1;
				for (int i = 0; i < 7; i++) {
					if (_inventory.generalInventoryIndexRef[i] >= 0) {
						_inventory.generalInvCurrent = i;
					}
				}
				int referenceIndex = -1;
				if (_inventory.generalInvCurrent >= 0) {
					referenceIndex = _inventory.genButtons[_inventory.generalInvCurrent].transform.GetComponent<GeneralInvButton>().useableItemIndex;
				}

				if (referenceIndex < 0 || referenceIndex > 110) {
					_mfdManager.ResetItemTab();
				} else {
					_mfdManager.SendInfoToItemTab(referenceIndex,genbut.customIndex);
				}
				PutObjectInHand(indexPriorToRemoval,customIndexPrior,0,0,false,true);
				break;
			case ButtonType.Search:
				SearchButton sebut = currentButton.GetComponentInParent<SearchButton>();
				int tempButtonindex = currentButton.GetComponent<SearchContainerButton>().refIndex;
				SearchButtonClick(tempButtonindex,sebut);
				break;
			case ButtonType.PGrid:
				PuzzleUIButton puib = currentButton.GetComponent<PuzzleUIButton>();
				if (puib != null) puzzleGrid.OnGridCellClick(puib.buttonIndex);
				break;
			case ButtonType.PWire:
				PuzzleUIButton wpuib = currentButton.GetComponent<PuzzleUIButton>();
				if (wpuib != null) {
					if (wpuib.isRH)
						puzzleWire.ClickRHNode(wpuib.buttonIndex);
					else
						puzzleWire.ClickLHNode(wpuib.buttonIndex);
				}
				break;
			case ButtonType.Vaporize:
				VaporizeButton vapB = currentButton.GetComponent<VaporizeButton>();
				if (vapB != null) {
					vapB.OnVaporizeClick();
				}
				break;
			case ButtonType.ShootMode:
				ForceShootMode();
				_guiState.ClearOverButton();
				break;
			case ButtonType.GrenadeTimerSlider:
				Button btn = currentButton.GetComponent<Button>();
				Debug.Log("GrenadeTimerSlider invoke");
				break;
		}
	}
	
	public void SearchButtonClick(int index, SearchButton sebut) {
		holdingObject = true;
		heldObjectIndex = sebut.contents[index];
		heldObjectCustomIndex = sebut.customIndex[index];
		if (currentSearchItem != null) {
			SearchableItem sitem = currentSearchItem.GetComponent<SearchableItem>();
			sitem.contents[index] = -1;
			sitem.customIndex[index] = -1;
		}
		
		sebut.contents[index] = -1;
		sebut.customIndex[index] = -1;
		_mfdManager.DisableSearchItemImage(index);
		sebut.CheckForEmpty();
		_guiState.ClearOverButton();
		if (_consts.InputQuickItemPickup) {
			AddItemToInventory(heldObjectIndex,heldObjectCustomIndex);
			ResetHeldItem();
		} else {
			_consts.sprint(_consts.stringTable[heldObjectIndex + 326] + _consts.stringTable[319],player);
			ForceInventoryMode();
		}	
	}

	void RecoilAndRest() {
		float targetY = _consts.playerCameraOffsetY
						* _playerMovement.currentCrouchRatio;
		float targetX = 0f;
		if (_playerMovement.relSideways > 0) targetX += 0.12f;
		if (_playerMovement.relSideways < 0) targetX -= 0.12f;
		if (_playerMovement.relForward != 0) targetY -= 0.08f;

		// If not shaking or bobbing, this will stay this to lerp to normal.
// 		headBobY = _consts.playerCameraOffsetY
// 				   * _playerMovement.currentCrouchRatio;
		if (shakeFinished > _pauseScript.relativeTime) {
			headBobX = transform.localPosition.x
					   + UnityEngine.Random.Range(shakeForce * -0.17f,
												  shakeForce * 0.17f);

			headBobY = transform.localPosition.y
					   + UnityEngine.Random.Range(shakeForce * -0.08f,
												  shakeForce * 0.08f);

			headBobZ = transform.localPosition.z
					   + UnityEngine.Random.Range(shakeForce * -0.17f,
												  shakeForce * 0.17f);
		} else {
			headBobZ = 0f;
			Vector3 vel = _playerMovement.rbody.linearVelocity;
			vel.y = 0f;
			if (_playerMovement.relForward + _playerMovement.relSideways != 0
				&& _consts.HeadBob) {

				if (headBobShiftFinished < _pauseScript.relativeTime) {
					headBobShiftFinished = _pauseScript.relativeTime + 0.2f;
					if (!_playerMovement.isSprinting) {
						headBobShiftFinished += 0.1f;
					}

					bobTarget = Const.HeadBobAmount * -1f
								* Mathf.Sign(bobTarget);
				}

				if (_playerMovement.rbody.linearVelocity.magnitude > 0.1f){
					headBobY = Mathf.SmoothDamp(headBobY,targetY + bobTarget,ref headBobYVel,Const.HeadBobRate);
				}

				headBobX = Mathf.SmoothDamp(headBobX,targetX,ref headBobXVel,Const.HeadBobRate);
			} else {
				headBobX = Mathf.SmoothDamp(headBobX,0f,ref headBobXVel,Const.HeadBobRate);
				headBobY = Mathf.SmoothDamp(headBobY,_consts.playerCameraOffsetY * _playerMovement.currentCrouchRatio,ref headBobYVel,Const.HeadBobRate);
			}
		}
		
		if (inCyberSpace) {
			headBobX = 0f;
			headBobY = 0f;
			headBobZ = 0f;
		}
		
		transform.localPosition = new Vector3(headBobX,headBobY,headBobZ);
	}

	void AddItemFail(int index) { // Expects usableItem index
		DropHeldItem();
		_consts.sprint(_consts.stringTable[32] + _consts.stringTable[index + 326]
					 + _consts.stringTable[318],player); // Inventory full.
	}

	public void AddItemToInventory(int index, int customIndex) {
		_mfdManager.mouseClickHeldOverGUI = true; // Prevent gun shooting.
		if (index < 0) index = 0; // Good check on paper.
		if (index > 110) index = 94; // Way to get a head.
		if ((index >= 0 && index <= 5)
             || index == 33
             || index == 35
             || (index >= 52 && index < 59)
             || (index >= 61 && index <= 64)
             || (index >= 92 && index <= 101)) {
			if (!_inventory.AddGeneralObjectToInventory(index,customIndex)) {
				AddItemFail(index);
			}
		} else if (index == 6) {
			_inventory.AddAudioLogToInventory(heldObjectCustomIndex);
		} else if (index >= 36 && index <= 51) {
			if (!_inventory.AddWeaponToInventory(index,heldObjectAmmo,
												  heldObjectAmmo2,
												  heldObjectLoadedAlternate)) {
				AddItemFail(index);
			}
		} else if (index == 34 || index == 81 || (index >= 83 && index <= 91) || index == 110) {
			_inventory.AddAccessCardToInventory(index);
		} else {
			switch (index) {
				case 7:  _inventory.AddGrenadeToInventory(0,index); break; // Frag
				case 8:  _inventory.AddGrenadeToInventory(3,index); break; // Concussion
				case 9:  _inventory.AddGrenadeToInventory(1,index); break; // EMP
				case 10: _inventory.AddGrenadeToInventory(6,index); break; // Earth Shaker
				case 11: _inventory.AddGrenadeToInventory(4,index); break; // Land Mine
				case 12: _inventory.AddGrenadeToInventory(5,index); break; // Nitropak
				case 13: _inventory.AddGrenadeToInventory(2,index); break; // Gas
				case 14: _inventory.AddPatchToInventory(2,index); break;
				case 15: _inventory.AddPatchToInventory(6,index); break;
				case 16: _inventory.AddPatchToInventory(5,index); break;
				case 17: _inventory.AddPatchToInventory(3,index); break;
				case 18: _inventory.AddPatchToInventory(4,index); break;
				case 19: _inventory.AddPatchToInventory(1,index); break;
				case 20: _inventory.AddPatchToInventory(0,index); break;
				case 21: _inventory.AddHardwareToInventory(0,index,customIndex,true); break;
				case 22: _inventory.AddHardwareToInventory(1,index,customIndex,true); break;
				case 23: _inventory.AddHardwareToInventory(2,index,customIndex,true); break;
				case 24: _inventory.AddHardwareToInventory(3,index,customIndex,true); break;
				case 25: _inventory.AddHardwareToInventory(4,index,customIndex,true); break;
				case 26: _inventory.AddHardwareToInventory(5,index,customIndex,true); break;
				case 27: _inventory.AddHardwareToInventory(6,index,customIndex,true); break;
				case 28: _inventory.AddHardwareToInventory(7,index,customIndex,true); break;
				case 29: _inventory.AddHardwareToInventory(8,index,customIndex,true); break;
				case 30: _inventory.AddHardwareToInventory(9,index,customIndex,true); break;
				case 31: _inventory.AddHardwareToInventory(10,index,customIndex,true); break;
				case 32: _inventory.AddHardwareToInventory(11,index,customIndex,true); break;
				case 60: _inventory.AddAmmoToInventory(12,index, _consts.magazinePitchCountForWeapon[12], false); break; // rubber slugs
				case 65: _inventory.AddAmmoToInventory(8,index, _consts.magazinePitchCountForWeapon2[8], true); break; // magpulse cartridge super
				case 66: _inventory.AddAmmoToInventory(2,index, _consts.magazinePitchCountForWeapon[2], false); break; // needle darts
				case 67: _inventory.AddAmmoToInventory(2,index, _consts.magazinePitchCountForWeapon2[2], true); break; // tranquilizer darts
				case 68: _inventory.AddAmmoToInventory(9,index, _consts.magazinePitchCountForWeapon[9], false); break; // standard bullets
				case 69: _inventory.AddAmmoToInventory(9,index, _consts.magazinePitchCountForWeapon2[9], true); break; // teflon bullets
				case 70: _inventory.AddAmmoToInventory(7,index, _consts.magazinePitchCountForWeapon[7], false); break; // hollow point rounds
				case 71: _inventory.AddAmmoToInventory(7,index, _consts.magazinePitchCountForWeapon2[7], true); break; // slug rounds
				case 72: _inventory.AddAmmoToInventory(0,index, _consts.magazinePitchCountForWeapon[0], false); break; // magnesium tipped slugs
				case 73: _inventory.AddAmmoToInventory(0,index, _consts.magazinePitchCountForWeapon2[0], true); break; // penetrator slugs
				case 74: _inventory.AddAmmoToInventory(3,index, _consts.magazinePitchCountForWeapon[3], false); break; // hornet clip
				case 75: _inventory.AddAmmoToInventory(3,index, _consts.magazinePitchCountForWeapon2[3], true); break; // splinter clip
				case 76: _inventory.AddAmmoToInventory(11,index, _consts.magazinePitchCountForWeapon[11], false); break; // rail rounds
				case 77: _inventory.AddAmmoToInventory(13,index, _consts.magazinePitchCountForWeapon[13], false); break; // slag magazine
				case 78: _inventory.AddAmmoToInventory(13,index, _consts.magazinePitchCountForWeapon2[13], true); break; // large slag magazine
				case 79: _inventory.AddAmmoToInventory(8,index, _consts.magazinePitchCountForWeapon[8], false); break; // magpulse cartridges
				case 80: _inventory.AddAmmoToInventory(8,index, _consts.magazinePitchCountForWeapon2[8], false); break; // small magpulse cartridges
			}
		}

		Utils.PlayUIOneShotSavable(_consts,87); // frob_item
		int numberFoundContents = 0;
		if (currentSearchItem != null) {
			SearchableItem curSearchScript = currentSearchItem.GetComponent<SearchableItem>();
			if (curSearchScript != null) {
				int[] resultContents = {-1,-1,-1,-1};  // create blanked container for search results
				for (int i=3;i>=0;i--) {
					resultContents[i] = curSearchScript.contents[i];
					if (resultContents[i] > -1) numberFoundContents++; // if something was found, add 1 to count
				}
			}
	    	if (numberFoundContents == 0) {
				currentSearchItem = null;
				_mfdManager.ReturnTabsFromSearch();
			}
		}
		firstTimePickup = false;
	}

	public async UniTask DropHeldItem() {
		dropFinished = Time.time + 0.2f; // Prevent immediate regrab at high fps
		if (heldObjectIndex < 0 || heldObjectIndex > 110) { 
			Debug.Log("BUG: Attempted to DropHeldItem with index out of bounds (<0 or >110) and heldObjectIndex = " + heldObjectIndex.ToString(),player);
			ResetHeldItem();
			return;
		}

		if (!grenadeActive)
		{
			_prefabIndexToInstantiate = heldObjectIndex + 307;
		}
		
		GameObject tossObject = null;
		bool freeObjectInPoolFound = false;
		GameObject levelDynamicContainer = _levelManager.GetCurrentDynamicContainer();

		// Find any free inactive objects within the level's Levelnumber.Dynamic container and activate those before instantiating
		if (!grenadeActive) {
			for (int i=0;i<levelDynamicContainer.transform.childCount;i++) {
				Transform tr = levelDynamicContainer.transform.GetChild(i);
				GameObject go = tr.gameObject;
				UseableObjectUse reference = go.GetComponent<UseableObjectUse>();
				if (reference != null) {
					if (reference.useableItemIndex == heldObjectIndex && go.activeSelf == false) {
						reference.customIndex = heldObjectCustomIndex;
						tossObject = go;
						freeObjectInPoolFound = true;
						break;
					}
				}
			}

			if (freeObjectInPoolFound) {
				if (tossObject == null) {
					_consts.sprint("BUG: Failed to get freeObjectInPool for object being dropped!",player);
					ResetHeldItem();
					return;
				} else {
					tossObject.transform.position = (transform.position + (transform.forward * tossOffset));
				}
			} else {
				// Debug.Log("WARNING: Failed to get freeObjectInPool for object " + heldObject.ToString() + "being dropped! MouseLookScript DropHeldItem.",player);
				tossObject = await _resourcesLoader.InstantiatePrefabAsync(_prefabIndexToInstantiate,(transform.position + (transform.forward * tossOffset)),
					_consts.quaternionIdentity);  //effect
				if (tossObject == null) {
					_consts.sprint("BUG: Failed to instantiate object being dropped!",player);
					ResetHeldItem();
					return;
				}
			}
			if (tossObject.activeSelf != true) tossObject.SetActive(true);
			if (levelDynamicContainer != null) {
				tossObject.transform.SetParent(levelDynamicContainer.transform,true);
			}

			Vector3 tossDir = _mouseCursor.GetCursorScreenPointForRay();
			tossDir = playerCamera.ScreenPointToRay(tossDir).direction;
			Rigidbody rbody = tossObject.GetComponent<Rigidbody>();
			if (rbody != null) {
				rbody.isKinematic = false;
				rbody.useGravity = true;
				rbody.linearVelocity = tossDir * tossForce;
			}

			UseableObjectUse uou = tossObject.GetComponent<UseableObjectUse>();
			uou.customIndex = heldObjectCustomIndex;
			uou.ammo = heldObjectAmmo;
			uou.ammo2 = heldObjectAmmo2;
			uou.heldObjectLoadedAlternate = heldObjectLoadedAlternate;
		} else {
			// Throw an active grenade
			grenadeActive = false;
			_mfdManager.mouseClickHeldOverGUI = true; // Prevent shooting it.
			tossObject = await _resourcesLoader.InstantiatePrefabAsync(_prefabIndexToInstantiate,(transform.position + (transform.forward * tossOffset)),
				_consts.quaternionIdentity);  //effect
			if (tossObject == null) {
				_consts.sprint("BUG: Failed to instantiate object being dropped!",player);
				ResetHeldItem();
				return;
			}

            _consts.grenadesThrown++;
			if (levelDynamicContainer != null){
				tossObject.transform.SetParent(levelDynamicContainer.transform,true);
			}
			tossObject.layer = 11; // Set to player bullets layer to prevent collision and still be visible.
			Vector3 tossDir = _mouseCursor.GetCursorScreenPointForRay();
			tossDir = playerCamera.ScreenPointToRay(tossDir).direction;
			Rigidbody rbody = tossObject.GetComponent<Rigidbody>();
			if (rbody != null) {
				rbody.isKinematic = false;
				rbody.useGravity = true;
				rbody.linearVelocity = tossDir * tossForce;
			}
			GrenadeActivate ga = tossObject.GetComponent<GrenadeActivate>();
			if (ga != null) ga.Activate(); // Time to boom!
			_mouseCursor.liveGrenade = false;
		}
		ResetHeldItem();
	}

	public void ResetHeldItem() {
		heldObjectIndex = -1;
		heldObjectCustomIndex = -1;
		heldObjectAmmo = 0;
		heldObjectAmmo2 = 0;
		heldObjectLoadedAlternate = false;
		holdingObject = false;
		grenadeActive = false;
		_mouseCursor.justDroppedItemInHelper = true;
	}

	public void ToggleInventoryMode() {
		if (inventoryMode)	ForceShootMode();
		else				ForceInventoryMode();
	}

	public void ForceShootMode() {
		if (_consts.NoShootMode) return; // We are being like the original now!

		_guiState.ClearOverButton();
		_mfdManager.mouseClickHeldOverGUI = false;
		_automap.CloseFullmap();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		inventoryMode = false;
		if (Application.platform == RuntimePlatform.Android) {
			shootModeButton.SetActive(true);
		} else {
			shootModeButton.SetActive(false);
		}

		if (vmailActive) {
			_inventory.DeactivateVMail();
			vmailActive = false;
		}
	}

	public void ForceInventoryMode() {
		if (inventoryMode) return;

		_guiState.ClearOverButton();
		if (_pauseScript.MenuActive() || _pauseScript.Paused()) {
			Cursor.lockState = CursorLockMode.None;
		} else {
			#if UNITY_EDITOR
				Cursor.lockState = CursorLockMode.None;
			#else	
				Cursor.lockState = CursorLockMode.Confined;
			#endif
		}
		MouseCursor.SetCursorPosInternal((int)(Screen.width * 0.5f),(int)(Screen.height * 0.5f));
		Cursor.visible = false;
		_mouseCursor.deltaX = 0;
		_mouseCursor.deltaY = 0;
		_mouseCursor.cursorPosition.x = (Screen.width / 2);
		_mouseCursor.cursorPosition.y = (Screen.height / 2);
		inventoryMode = true;
		if (!_consts.noHUD) shootModeButton.SetActive(true);
		else shootModeButton.SetActive(false);
	}

	void SearchObject (int index){
		if (currentSearchItem == null) { Debug.Log("BUG: Early exit from SearchObject, currentSearchItem was null!"); return;}

		bool useFX = true;
		SearchableItem curSearchScript = currentSearchItem.GetComponent<SearchableItem>();
		if (curSearchScript.searchableInUse) {
			for (int i=0;i<4;i++) {
				if (curSearchScript.contents[i] >= 0)
				{
					_mouseCursor.cursorImage = _texturesStorage.GetItemFrobIcon(curSearchScript.contents[i]);
					var objectIndex = curSearchScript.contents[i];
					heldObjectIndex = objectIndex;
					heldObjectCustomIndex = objectIndex;
					curSearchScript.contents[i] = -1;
					curSearchScript.customIndex[i] = -1;
					if (heldObjectIndex != -1) holdingObject = true;
					_consts.sprint(_consts.stringTable[heldObjectIndex + 326]
								 + _consts.stringTable[319],player); // picked up

					_mfdManager.DisableSearchItemImage(i);
					useFX = false;
					break;
				}
			}
		} else {
			Utils.PlayUIOneShotSavable(_consts,91); // searchsound
		}

		curSearchScript.searchableInUse = true;

		// Search through array to see if any items are in the container
		int numberFoundContents = 0;
		int[] resultContents = {-1,-1,-1,-1};  // create blanked container for search results
		int[] resultCustomIndex = {-1,-1,-1,-1};  // create blanked container for search results custom indices
		for (int i=3;i>=0;i--) {
			resultContents[i] = curSearchScript.contents[i];
			resultCustomIndex[i] = curSearchScript.customIndex[i];
			// If something was found, add 1 to count.
			if (resultContents[i] > -1) numberFoundContents++;
		}

		if (firstTimeSearch) {
			firstTimeSearch = false;
			_mfdManager.OpenTab (4, true, TabMSG.Search, -1,Handedness.LH);
		}
		_mfdManager.SendSearchToDataTab(curSearchScript.objectName,
										 numberFoundContents,resultContents,
										 resultCustomIndex,
										 currentSearchItem.transform.position,
										 curSearchScript, useFX);
		ForceInventoryMode();
	}

	public void UseGrenade (int index) {
		if (holdingObject) { _consts.sprint(_consts.stringTable[311],player); return; } // Can't use grenade, hands full
		if (index < 7 || index > 13) { Debug.Log("BUG: index outside of 7 to 13 passed to UseGrenade() in MouseLookScript.cs"); return; }

		ForceInventoryMode();  // Inventory mode is turned on when picking something up.
		ResetHeldItem();
		_mouseCursor.liveGrenade = true;
		grenadeActive = true;
		_consts.sprint(_consts.stringTable[index + 326]
					 + _consts.stringTable[320],player); // activated, grenade is LIVE!

		switch(index) { // Subtract one from the correct grenade inventory
			case 7:  _prefabIndexToInstantiate = 370; _inventory.RemoveGrenade(0); break; // Frag
			case 8:  _prefabIndexToInstantiate = 372; _inventory.RemoveGrenade(3); break; // Concussion
			case 9:  _prefabIndexToInstantiate = 387; _inventory.RemoveGrenade(1); break; // EMP
			case 10: _prefabIndexToInstantiate = 389; _inventory.RemoveGrenade(6); break; // Earth Shaker
			case 11: _prefabIndexToInstantiate = 402; _inventory.RemoveGrenade(4); break; // Land Mine
			case 12: _prefabIndexToInstantiate = 403; _inventory.RemoveGrenade(5); break; // Nitropak
			case 13: _prefabIndexToInstantiate = 404; _inventory.RemoveGrenade(2); break; // Gas
		}
		_mfdManager.ResetItemTab();
		PutObjectInHand(index,-1,0,0,false,true);
	}

	public void ScreenShake (float force, float duration) {
		shakeFinished = _pauseScript.relativeTime + duration;
		if (force < 0.48f) shakeForce = force;
		else shakeForce = 0.48f;
	}

	public static string Save(GameObject go) {
		MouseLookScript ml = go.GetComponent<MouseLookScript>();
		var pauseScript = ml._pauseScript;
        s1.Clear();
		s1.Append(Utils.BoolToString(ml.gameObject.activeSelf,"MouseLookScript.gameObject.activeSelf"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.playerCamera.enabled,"playerCamera.enabled"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.inventoryMode,"inventoryMode"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.holdingObject,"holdingObject"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(ml.heldObjectIndex,"heldObjectIndex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(ml.heldObjectCustomIndex,"heldObjectCustomIndex"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(ml.heldObjectAmmo,"heldObjectAmmo"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(ml.heldObjectAmmo2,"heldObjectAmmo2"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.heldObjectLoadedAlternate,"heldObjectLoadedAlternate"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.firstTimePickup,"firstTimePickup"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.firstTimeSearch,"firstTimeSearch"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.grenadeActive,"grenadeActive"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.inCyberSpace,"inCyberSpace"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.yRotation,"yRotation"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.geniusActive,"geniusActive"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.xRotation,"xRotation"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(ml.vmailActive,"vmailActive"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnPoint.x,"cyberspaceReturnPoint.x"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnPoint.y,"cyberspaceReturnPoint.y"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnPoint.z,"cyberspaceReturnPoint.z"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnCameraLocalRotation.x,"cyberspaceReturnCameraLocalRotation.x"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnCameraLocalRotation.y,"cyberspaceReturnCameraLocalRotation.y"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnCameraLocalRotation.z,"cyberspaceReturnCameraLocalRotation.z"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnPlayerCapsuleLocalRotation.x,"cyberspaceReturnPlayerCapsuleLocalRotation.x"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnPlayerCapsuleLocalRotation.y,"cyberspaceReturnPlayerCapsuleLocalRotation.y"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceReturnPlayerCapsuleLocalRotation.z,"cyberspaceReturnPlayerCapsuleLocalRotation.z"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceRecallPoint.x, "cyberspaceRecallPoint.x"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceRecallPoint.y,"cyberspaceRecallPoint.y"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(ml.cyberspaceRecallPoint.z,"cyberspaceRecallPoint.z"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(ml.cyberspaceReturnLevel,"cyberspaceReturnLevel"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,ml.returnFromCyberspaceFinished,"returnFromCyberspaceFinished"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,ml.randomShakeFinished,"randomShakeFinished"));
        s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,ml.randomKlaxonFinished,"randomKlaxonFinished"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveRelativeTimeDifferential(pauseScript,ml.shakeFinished,"shakeFinished"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		MouseLookScript ml = go.GetComponent<MouseLookScript>();
		var pauseScript = ml._pauseScript;
		float readFloatx, readFloaty, readFloatz;
		ml.gameObject.SetActive(Utils.GetBoolFromString(entries[index],"MouseLookScript.gameObject.activeSelf")); index++;
		ml.playerCamera.enabled = Utils.GetBoolFromString(entries[index],"playerCamera.enabled"); index++;
		ml.inventoryMode = !Utils.GetBoolFromString(entries[index],"inventoryMode"); index++; // Take opposite because we are about to opposite again...
		ml.ToggleInventoryMode(); // ...correctly set cursor lock state, and opposite again, now it is what was saved
		pauseScript.previousInvMode = ml.inventoryMode; // Prevent it changing it inadvertently after load unpauses.
		ml.holdingObject = Utils.GetBoolFromString(entries[index],"holdingObject"); index++;
		ml.heldObjectIndex = Utils.GetIntFromString(entries[index],"heldObjectIndex"); index++;
		ml.heldObjectCustomIndex = Utils.GetIntFromString(entries[index],"heldObjectCustomIndex"); index++;
		ml.heldObjectAmmo = Utils.GetIntFromString(entries[index],"heldObjectAmmo"); index++;
		ml.heldObjectAmmo2 = Utils.GetIntFromString(entries[index],"heldObjectAmmo2"); index++;
		ml.heldObjectLoadedAlternate = Utils.GetBoolFromString(entries[index],"heldObjectLoadedAlternate"); index++;
		ml.firstTimePickup = Utils.GetBoolFromString(entries[index],"firstTimePickup"); index++;
		ml.firstTimeSearch = Utils.GetBoolFromString(entries[index],"firstTimeSearch"); index++;
		ml.grenadeActive = Utils.GetBoolFromString(entries[index],"grenadeActive"); index++;
		ml.inCyberSpace = Utils.GetBoolFromString(entries[index],"inCyberSpace"); index++;
		ml.yRotation = Utils.GetFloatFromString(entries[index],"yRotation"); index++;
		ml.geniusActive = Utils.GetBoolFromString(entries[index],"geniusActive"); index++;
		ml.xRotation = Utils.GetFloatFromString(entries[index],"xRotation"); index++;
		ml.vmailActive = Utils.GetBoolFromString(entries[index],"vmailActive"); index++;
		readFloatx = Utils.GetFloatFromString(entries[index],"cyberspaceReturnPoint.x"); index++;
		readFloaty = Utils.GetFloatFromString(entries[index],"cyberspaceReturnPoint.y"); index++;
		readFloatz = Utils.GetFloatFromString(entries[index],"cyberspaceReturnPoint.z"); index++;
		ml.cyberspaceReturnPoint = new Vector3(readFloatx,readFloaty,readFloatz);
		readFloatx = Utils.GetFloatFromString(entries[index],"cyberspaceReturnCameraLocalRotation.x"); index++;
		readFloaty = Utils.GetFloatFromString(entries[index],"cyberspaceReturnCameraLocalRotation.y"); index++;
		readFloatz = Utils.GetFloatFromString(entries[index],"cyberspaceReturnCameraLocalRotation.z"); index++;
		ml.cyberspaceReturnCameraLocalRotation = new Vector3(readFloatx,readFloaty,readFloatz);

		 // Euler Angles, only 3
		readFloatx = Utils.GetFloatFromString(entries[index],"cyberspaceReturnPlayerCapsuleLocalRotation.x"); index++;
		readFloaty = Utils.GetFloatFromString(entries[index],"cyberspaceReturnPlayerCapsuleLocalRotation.y"); index++;
		readFloatz = Utils.GetFloatFromString(entries[index],"cyberspaceReturnPlayerCapsuleLocalRotation.z"); index++;
		ml.cyberspaceReturnPlayerCapsuleLocalRotation = new Vector3(readFloatx,readFloaty,readFloatz);
		readFloatx = Utils.GetFloatFromString(entries[index],"cyberspaceRecallPoint.x"); index++;
		readFloaty = Utils.GetFloatFromString(entries[index],"cyberspaceRecallPoint.y"); index++;
		readFloatz = Utils.GetFloatFromString(entries[index],"cyberspaceRecallPoint.z"); index++;
		ml.cyberspaceRecallPoint = new Vector3(readFloatx,readFloaty,readFloatz);
		ml.cyberspaceReturnLevel = Utils.GetIntFromString(entries[index],"cyberspaceReturnLevel"); index++;
		ml.returnFromCyberspaceFinished = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"returnFromCyberspaceFinished"); index++;
		ml.randomShakeFinished = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"randomShakeFinished"); index++;
		ml.randomKlaxonFinished = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"randomKlaxonFinished"); index++;
		ml.shakeFinished = Utils.LoadRelativeTimeDifferential(pauseScript,entries[index],"shakeFinished"); index++;

		// Prevent picking up first item immediately. Not currently possible to
		// save references (without a lot of work, aherm).
		ml.currentSearchItem = null;

		// Left/Right component applied to capsule.
		Transform capsuleTr = ml.transform.parent.transform.parent.transform;
		capsuleTr.localRotation = Quaternion.Euler(0f, ml.yRotation, 0f);

		// Up/Down component only applied to camera.
		Transform cameraTr = ml.transform;
		cameraTr.localRotation = Quaternion.Euler(ml.xRotation,0f,0f);
		return index;
	}
}
