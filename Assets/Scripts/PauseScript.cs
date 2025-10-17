using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using Citadel.Game;
using Citadel.SceneManagement;
using Zenject;

public class PauseScript : MonoBehaviour {
	public GameObject pauseText;
	public GameObject[] disableUIOnPause;
	public GameObject saltTheFries;
	public GameObject[] enableUIOnPause;
	public GameObject mainMenu;
	public GameObject saveDialog;
	public GameObject hardSaveDialog;

	[HideInInspector] public bool paused = false;
	[HideInInspector] public bool previousInvMode = true;
	public float relativeTime;
	public float absoluteTime;

	[SerializeField] 
	private GameObject _loadingScreen;

	private readonly List<AmbientRegistration> _ambientRegistry = new();
	[Inject] private ConsoleEmulator _consoleEmulator;
	[Inject] private Const _consts;
	[Inject] private GetInput _getInput;
	[Inject] private Inventory _inventory;
	[Inject] private MainMenuHandler _mainMenuHandler;
	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private PlayerMovement _playerMovement;

	private bool menuActive = true; // Store the state of the main menu
	                                // gameobject active state so that we don't
									// have to do a gameobject engine call more
									// than once on every Update all over the
									// code.

	public bool OnSaveDialog => hardSaveDialog.activeSelf || saveDialog.activeSelf;
									
	private void Awake()
	{
		ScenesLoader.OnStartLoadScene += OnStartLoadScene;
	}

	private void OnDestroy()
	{
		ScenesLoader.OnStartLoadScene -= OnStartLoadScene;
	}

	private void OnStartLoadScene(string sceneName)
	{
		_ambientRegistry.Clear();
	}
	
	// The whole point right here:
	public bool Paused() { return paused || _consts.loading; }
	public bool MenuActive() { return menuActive; }

	void Update() {
	    if (relativeTime > 0f) absoluteTime += Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.F12)) TakeScreenshot();

		menuActive = mainMenu.activeSelf;
		if (!menuActive) {
			if (!_mouseLookScript.playerCamera.enabled) _mouseLookScript.playerCamera.enabled = true;
			if (_getInput.Menu()) {
				if (OnSaveDialog)
					ExitSaveDialog();
				else
					PauseToggle();
			}

			if (Input.GetKeyDown(KeyCode.Home)
			    || Input.GetKeyDown(KeyCode.Menu)) {

			    PauseEnable();
			}

			CheckForSuperWinCmdKey();
			//if (!Paused()) RaycastAudioOcclusion(); TODO setting, 2.8ms cpu cost!!!!
		}

		if (!Paused()) relativeTime += Time.deltaTime;
	}

	public void ConsoleEntryEnterDelegate() {
		_consoleEmulator.ConsoleEntryEnter();
	}

	public void RaycastAudioOcclusion() {
		// Raytraced Audio Occlusion with no bounce ;)
		int hitCount = 0;
		float newVolume = 1.0f;
		RaycastHit[] results = new RaycastHit[6];
		for (int i=0;i<_ambientRegistry.Count;i++) {
			if (_ambientRegistry[i] == null) continue;

			hitCount = Physics.RaycastNonAlloc(
						_mouseLookScript.transform.position,
						_ambientRegistry[i].transform.position
						- _mouseLookScript.transform.position,
						results,32f,_consts.layerMaskPlayerFrob,
						QueryTriggerInteraction.UseGlobal);

			_ambientRegistry[i].SFX.volume =
				_ambientRegistry[i].normalVolume;

			if (hitCount > 0) {
				if (hitCount > 5) {
					newVolume = _ambientRegistry[i].normalVolume * 0.40f;
				} else if (hitCount == 5) {
					newVolume = _ambientRegistry[i].normalVolume * 0.50f;
				} else if (hitCount == 4) {
					newVolume = _ambientRegistry[i].normalVolume * 0.60f;
				} else if (hitCount == 3) {
					newVolume = _ambientRegistry[i].normalVolume * 0.70f;
				} else if (hitCount == 2) {
					newVolume = _ambientRegistry[i].normalVolume * 0.80f;
				} else {
					newVolume = _ambientRegistry[i].normalVolume * 0.90f;
				}

				_ambientRegistry[i].SFX.volume = newVolume;
			}
		}

		_consts.NPCAudioOcclusion();
	}

/*
    void FixedUpdate() {
		Debug.Log("ObjectContainmentSystem active floor chunks: "
				  + ObjectContainmentSystem.ActiveFloorChunks.Count.ToString());

		GameObject go = null; // Contain the currently checked floor.
		Rigidbody rb = null; // Contain the currently checked rigidbody.
		PauseRigidbody pb = null; // Reference to all the rigidbodies each.
		Vector3 flrPos = null;
		Vector3 objPos = null;
		float x, y, z;

        // Iterate through each floor and check for overlapping rigidbodies
        for (int i=0;i < ObjectContainmentSystem.ActiveFloorChunks.Count;i++) {
			go = ObjectContainmentSystem.ActiveFloorChunks[i];
			flrPos = go.transform.position;
			for (int k=0;k<_consts.prb.Count;k++) {
				pb = _consts.prb[k];
				if (!pm.gameObject.activeInHierarchy) continue;

				objPos = pb.gameObject.transform.position;
				if (!PhysObjAffectedByFloor(objPos, flrPos)) continue;

                Rigidbody rb = pb.rbody;
                if (flrPos.y - objPos.y > 1.28f)  {
                    // If the rigidbody falls below the barrier height, set its position to the barrier height
                    rb.position = new Vector3(rb.position.x, barrierHeight, rb.position.z);
                    // If the rigidbody has a velocity in the downward direction, set its velocity to zero
                    if (rb.velocity.y < 0f)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                    }
				}
			}



            Vector3 barrierCenter = new Vector3(barrierPositions[i].x * gridSize + gridSize / 2f, barrierHeight, barrierPositions[i].y * gridSize + gridSize / 2f);
            float barrierSize = gridSize / 2f;
            List<Rigidbody> rigidbodiesInBarrier = rigidbodyOctree.GetObjectsInRange(barrierCenter, barrierSize);
            for (int j = 0; j < rigidbodiesInBarrier.Count; j++)
            {

            }
        }
    }*/

	private bool PhysObjAffectedByFloor(Vector3 objpos, Vector3 floorpos) {
		if (objpos.x - floorpos.x > 1.28f && objpos.z - floorpos.z > 1.28f) {
			return true;
		}

		return false;
	}

	void CheckForSuperWinCmdKey() {
		if (   Input.GetKeyDown(KeyCode.LeftCommand)     // Apple / Linux
			|| Input.GetKeyDown(KeyCode.RightCommand)    // Apple / Linux
			|| Input.GetKeyDown(KeyCode.LeftWindows)     // Windows
			|| Input.GetKeyDown(KeyCode.RightWindows)) { // Windows
			PauseEnable();
		}
	}

	public void PauseToggle() {
		if (Paused())	PauseDisable();
		else			PauseEnable();
	}

	public void PauseEnable() 
	{
		if (_loadingScreen.activeSelf)
		{
			return;
		}

		AudioListener.pause = true;
		PauseSystems();
		previousInvMode = _mouseLookScript.inventoryMode;
		if (_mouseLookScript.inventoryMode == false) {
			_mouseLookScript.ToggleInventoryMode();
		}
		
		if (_inventory.vmailbetajet.activeInHierarchy) _inventory.vmailbetajetVideo.Pause();
		if (_inventory.vmailbridgesep.activeInHierarchy) _inventory.vmailbridgesepVideo.Pause();
		if (_inventory.vmailcitadestruct.activeInHierarchy) _inventory.vmailcitadestructVideo.Pause();
		if (_inventory.vmailgenstatus.activeInHierarchy) _inventory.vmailgenstatusVideo.Pause();
		if (_inventory.vmaillaserdest.activeInHierarchy) _inventory.vmaillaserdestVideo.Pause();
		if (_inventory.vmailshieldsup.activeInHierarchy) _inventory.vmailshieldsupVideo.Pause();
		EnablePauseUI();
		pauseText.SetActive(true);
	}

	public void PauseDisable()
	{
		_consts.QuitAfterSavingDone = false;
		AudioListener.pause = false;
		UnpauseSystems();
		if (previousInvMode != _mouseLookScript.inventoryMode) {
			_mouseLookScript.ToggleInventoryMode();
			_mouseLookScript.SetCameraCullDistances();
		}
		DisablePauseUI();
		if (_inventory.vmailbetajet.activeInHierarchy) _inventory.vmailbetajetVideo.Play();
		if (_inventory.vmailbridgesep.activeInHierarchy) _inventory.vmailbridgesepVideo.Play();
		if (_inventory.vmailcitadestruct.activeInHierarchy) _inventory.vmailcitadestructVideo.Play();
		if (_inventory.vmailgenstatus.activeInHierarchy) _inventory.vmailgenstatusVideo.Play();
		if (_inventory.vmaillaserdest.activeInHierarchy) _inventory.vmaillaserdestVideo.Play();
		if (_inventory.vmailshieldsup.activeInHierarchy) _inventory.vmailshieldsupVideo.Play();
		pauseText.SetActive(false);
	}

	public void PauseSystems() {
		paused = true;
		for (int i=0;i<disableUIOnPause.Length;i++) {
			disableUIOnPause[i].SetActive(false);
		}

		for (int k=0;k<_consts.prb.Count;k++) _consts.prb[k].Pause();
		for (int k=0;k<_consts.psys.Count;k++) _consts.psys[k].Pause();
		for (int k=0;k<_consts.panimsList.Count;k++) {
			_consts.panimsList[k].Pause();
		}

		PauseAmbients();
	}

	public void PauseAmbients() {
		for (int u=0;u<_ambientRegistry.Count;u++) {
			if (_ambientRegistry[u].SFX != null) _ambientRegistry[u].SFX.Pause();
		}
	}

	public void UnpauseAmbients() {
		for (int u=0;u<_ambientRegistry.Count;u++) {
			if (_ambientRegistry[u].SFX != null) _ambientRegistry[u].SFX.UnPause();
		}
	}

	public void UnpauseSystems() {
		paused = false;
		for (int i=0;i<disableUIOnPause.Length;i++) {
			disableUIOnPause[i].SetActive(true);
		}

		for (int k=0;k<_consts.prb.Count;k++) {
			if (_consts.prb[k] == null) continue;
			
			_consts.prb[k].UnPause();
		}
		
		for (int k=0;k<_consts.psys.Count;k++) {
			if (_consts.psys[k] == null) continue;
			
			_consts.psys[k].UnPause();
		}
		
		for (int k=0;k<_consts.panimsList.Count;k++) {
			if (_consts.panimsList[k] == null) continue;
			
			_consts.panimsList[k].UnPause();
		}

		UnpauseAmbients();
		_playerMovement.ConsoleDisable();
	}

	public void OpenSaveDialog() {
		if (OnSaveDialog) return;

		if (_playerMovement.inCyberSpace) {
			_consts.sprint(_consts.stringTable[602]); // Cannot save in cyberspace
			OpenSaveDialogHard();
			return;
		}

		DisablePauseUI();
		saveDialog.SetActive(true);
	}

	public void OpenSaveDialogHard() {
		if (OnSaveDialog) return;

		DisablePauseUI();
		hardSaveDialog.SetActive(true);
	}

	public void ExitSaveDialog() {
		EnablePauseUI();
		saveDialog.SetActive(false);
		hardSaveDialog.SetActive(false);
	}

	public void SavePause() {
		if (_playerMovement.inCyberSpace) {
			_consts.sprint(_consts.stringTable[602]); // Cannot save in cyberspace
			return;
		}
		if (OnSaveDialog) return;

		DisablePauseUI();
		saveDialog.SetActive(false); // turn off dialog
		mainMenu.SetActive(true);
		_mainMenuHandler.GoToSaveGameSubmenu(true);
	}

	public void LoadPause() {
		if (OnSaveDialog) return;

		DisablePauseUI();
		saveDialog.SetActive(false); // turn off dialog
		mainMenu.SetActive(true);
		_mainMenuHandler.GoToLoadGameSubmenu(true);
	}

	public void SavePauseQuit() {
		DisablePauseUI();
		saveDialog.SetActive(false); // turn off dialog
		mainMenu.SetActive(true);
		_mainMenuHandler.InitialDisplay.SetActive(false);
		_consts.QuitAfterSavingDone = true;
		_mainMenuHandler.GoToSaveGameSubmenu(true);
	}

	public void NoSavePauseQuit() {
		DisablePauseUI();
		saveDialog.SetActive(false); // turn off dialog
		mainMenu.SetActive(true);
		_mainMenuHandler.GoToFrontPage();
	}

	public void PauseQuitHard() {
		mainMenu.SetActive(true);
		_mainMenuHandler.Quit();
	}

	public void EnablePauseUI() {
		for (int i=0;i<enableUIOnPause.Length;i++) {
			enableUIOnPause[i].SetActive(true);
			StartMenuButtonHighlight smbh = 
				enableUIOnPause[i].GetComponent<StartMenuButtonHighlight>();

			if (smbh != null) {
				smbh.DeHighlight(); // Prevent persisted states.
				if (i == 3 && _playerMovement.inCyberSpace) { // Save button
					smbh.enabled = false;
				} else {
					smbh.enabled = true;
				}
			}
		}
	}

	public void DisablePauseUI() {
		for (int i=0;i<enableUIOnPause.Length;i++) {
			enableUIOnPause[i].SetActive(false);
		}
	}

	public void PauseOptions () {
		if (OnSaveDialog) return;

		DisablePauseUI();
		mainMenu.SetActive(true);
		_mainMenuHandler.GoToOptionsSubmenu(true);
	}


	public void TakeScreenshot() {
		string sname = System.DateTime.UtcNow.ToString("ddMMMyyyy_HH_mm_ss")
					   + "_" + _consts.versionString + ".png";
		string spath = Utils.SafePathCombine(Application.streamingAssetsPath,
											 "Screenshots");

		// Check and recreate Screenshots folder if it was deleted.
        if (!Directory.Exists(spath)) Directory.CreateDirectory(spath);
		spath = Utils.SafePathCombine(spath,sname);
		ScreenCapture.CaptureScreenshot(spath);
		StartCoroutine(ScreenshotSprint(sname));
	}

	// Let screenshot save without putting text in it.
	public IEnumerator ScreenshotSprint(string sname) {
		yield return new WaitForSeconds(0.1f);
		_consts.sprint(_consts.stringTable[1024] + sname); // "Wrote screenshot "

	}

	// No need to clear, these are all unsaved and static.
	public void AddAmbientToRegistry(AmbientRegistration ar) {
		_ambientRegistry.Add(ar);
	}
}

// Fore use with LiveSplit or other future speedrunner utilities for doing speedruns
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public static class AutoSplitterData {
	public static long magicNumber = 0x1337133713371337;
	public static double thisRunTime = 0;
	public static bool isLoading = false;
	public static int missionSplitID = 0;
}
